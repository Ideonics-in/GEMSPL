using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Configuration;
using System.Timers;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.IO;

namespace ias.devicedriver
{
    public partial class GSM_SMS
    {
        SerialPortDriver spDriver = null;

        public enum SMS_MODE { PDU=0,TEXT=1 };
        


        String atResponse = String.Empty;

        String comPort = String.Empty;
        public String ComPort
        {
            get { return comPort; }
            set
            {
                comPort = value;
            }
        }


        System.Timers.Timer transactionTimer = null;
        int responseTimeout = 50; //response timeout in milliseconds


        System.Timers.Timer receiveTimer = null;
        int receiveTimeout = 10 * 1000;

        AutoResetEvent transactionEvent = null;

        TraceSource _gsmSMSTrace = null;
        TextWriterTraceListener _gsmSMSTraceListener = null;

        SMSMessage currentMessage;

        String rxStream;

        public event EventHandler<MessageReceivedEvent> messageReceived;
        Boolean InTransaction = false;

        public GSM_SMS()
        {
            //spDriver = new SerialPortDriver();
            spDriver = new SerialPortDriver(115200, 8, StopBits.One, Parity.None, Handshake.RequestToSend);

            spDriver.NewLine = Environment.NewLine;

            responseTimeout = 3000;

            transactionTimer = new System.Timers.Timer(responseTimeout);
            transactionTimer.Elapsed += new ElapsedEventHandler(transactionTimeout);
            transactionTimer.AutoReset = false;


           

            transactionEvent = new AutoResetEvent(false);


            receiveTimer = new System.Timers.Timer(receiveTimeout);
            receiveTimer.Elapsed += new ElapsedEventHandler(receiveTimer_Elapsed);
            receiveTimer.AutoReset = false;


            

            _gsmSMSTrace = new TraceSource("GSM_SMSTrace");
            _gsmSMSTrace.Switch = new SourceSwitch("GSM_SMSTraceSwitch");


            String gsmSMSTraceFile = "GSM_TRACE.txt";

            if (gsmSMSTraceFile != String.Empty && gsmSMSTraceFile!= null)
            {
                _gsmSMSTraceListener = new TextWriterTraceListener(gsmSMSTraceFile);
                _gsmSMSTrace.Listeners.Add(_gsmSMSTraceListener);
                _gsmSMSTrace.Switch.Level = SourceLevels.Information;
            }
            else
            {
                _gsmSMSTrace.Switch.Level = SourceLevels.Off;
            }
        }

        void receiveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
            if (spDriver.BytesToRead <= 0)
            {
                receiveTimer.Start();
            }
            else
            {
                while (spDriver.BytesToRead > 0)
                {
                    char[] segment = new char[spDriver.BytesToRead];
                    spDriver.Read(segment,0,spDriver.BytesToRead);

                    rxStream +=new String(segment);
                    if (rxStream.Contains("+CMTI"))
                    {
                        char[] separartor = { ',' };
                        String[] fields = rxStream.Split(separartor);
                        if (transact(AT_CMD_STR[(int)AT_COMMAND.READ_MESSAGE], "=" + fields[1],3000) == true)
                        {
                            transact(AT_CMD_STR[(int)AT_COMMAND.DELETE_MESSAGE], "=" + fields[1], 3000);
                            
                            if (messageReceived != null)
                            {
                                messageReceived(this, new MessageReceivedEvent(currentMessage));

                            }

                            
                        }

                    }
                }
                
            }



            
        }

        private void _constructorCommon(){
        }

        public void initialize()
        {
            String[] ports = SerialPort.GetPortNames();

            foreach (String port in ports)
            {
                try
                {
                    if (spDriver.open(port) == false)
                        continue;

                   // spDriver.ReadTimeout = 2000;
                    //spDriver.WriteTimeout = 2000;
                    if (transact(AT_CMD_STR[(int)AT_COMMAND.NONE], String.Empty,200) == false)
                    {
                        spDriver.Close();
                        continue;
                    }
                    if (transact(AT_CMD_STR[(int)AT_COMMAND.SMSC_ADDRESS], "?",200) == false)
                    {
                        spDriver.Close();
                        continue;
                    }

                    comPort = port;
                    break;
                }

                catch (Exception e)
                {
                    if (spDriver.IsOpen)
                        spDriver.Close();
                    continue;
                }
            }

        }

        public void initializePort(String port)
        {
            if (spDriver.open(port) == false)
                throw new GSM_SMSException("Unable to open Port:" + port);

            int i = 2;

            do
            {
                if (transact(AT_CMD_STR[(int)AT_COMMAND.NONE], string.Empty,200) == false)
                {
                    throw new GSM_SMSException("GSM Modem not found on:" + port);
                }

                if (transact(AT_CMD_STR[(int)AT_COMMAND.ECHO_OFF], String.Empty,200) == false)
                {
                    throw new GSM_SMSException("GSM Modem not found on:" + port);
                }

                if (transact(AT_CMD_STR[(int)AT_COMMAND.SMSC_ADDRESS], "?",200) == false)
                {
                    spDriver.Close();
                    throw new GSM_SMSException("Unable to find SMS centre address:" + port);
                }
                    
                
                
            } while (--i > 0);

            transact(AT_CMD_STR[(int)AT_COMMAND.DELETE_MESSAGE], "=1,4",25000);
            

            comPort = port;
        }

        public void initializePort(String port,String baud, String Handshaking)
        {
           
                spDriver.open(port, baud, Handshaking);


                int i = 2;

                do
                {
                    if (transact(AT_CMD_STR[(int)AT_COMMAND.NONE], string.Empty, 200) == false)
                    {
                        throw new GSM_SMSException("GSM Modem not found on:" + port);
                    }

                    if (transact(AT_CMD_STR[(int)AT_COMMAND.ECHO_OFF], String.Empty, 200) == false)
                    {
                        throw new GSM_SMSException("GSM Modem not found on:" + port);
                    }

                    if (transact(AT_CMD_STR[(int)AT_COMMAND.SMSC_ADDRESS], "?", 1000) == false)
                    {
                        spDriver.Close();
                        throw new GSM_SMSException("Unable to find SMS centre address:" + port);
                    }



                } while (--i > 0);

                transact(AT_CMD_STR[(int)AT_COMMAND.DELETE_MESSAGE], "=1,4", 25000);


                comPort = port;
            
  
        }

       
       

        public bool setMessageMode(SMS_MODE mode)
        {
            bool result = false;

            String cmdData = "=" + (int)mode;

            result = transact(AT_CMD_STR[(int)AT_COMMAND.MESSAGE_MODE], cmdData,5000);


            return result;
        }


        public bool sendSMS(String no, String message)
        {
            bool result = false;

            String cmdData ="="+"\""+ no +"\"" ;

            result = transact(AT_CMD_STR[(int)AT_COMMAND.SEND_SMS], cmdData,1000);

            if (result == true)
            {
                cmdData = message + new String((char)26, 1);
                result = transact(cmdData);
            }

            return result;
        }

        public void readAll()
        {
            transact(AT_CMD_STR[(int)AT_COMMAND.READ_ALL], "=\"ALL\"", 5000);
        }

        public void readSMS_Start()
        {
            receiveTimer.Start();
            
         
               

        }

        public void readSMS_Stop()
        {
            receiveTimer.Stop();
            



        }



        private bool parseATresponse(String atResponse, String atCommand)
        {
            bool result = false;

            #region TRACE_CODE

            String traceString = DateTime.Now.ToString();

            traceString += ":" + atResponse;

            traceString += Environment.NewLine;

            _gsmSMSTrace.TraceInformation(traceString);
            foreach (TraceListener l in _gsmSMSTrace.Listeners)
            {
                l.Flush();
            }

            #endregion

            switch( atCommand)
            {
                case "":
                    if( atResponse.Contains("OK"))
                        result = true;
                    break;
                case "E0":
                    if (atResponse.Contains("OK"))
                        result = true;
                    break;
                case "+CSCA":
                    if( atResponse.Contains("OK"))
                        result = true;
                    break;

                case "+CMGF":
                    if(atResponse.Contains("OK"))
                        result = true;
                    break;

                case "+CMGD":
                    if (atResponse.Contains("OK"))
                        result = true;
                    break;

                case "+CMGS":
                    if (atResponse.Contains(">"))
                        return true;
                       
                    if( atResponse.Contains("OK"))
                        result = true;
                    break;

                case "+CMGL":
                    if (atResponse.Contains("OK"))
                    {
                        
                            result = true;
                    }
                    break;

                case "+CMGR":

                    if (atResponse.Contains("OK"))
                    {
                        //if( processMessage(atResponse) )
                        result = true;
                    }

                    break;

                default:
                    result = false;
                    break;
            }
            return result;
        }


        private List<SMSMessage> processMessages(string atResponse)
        {
            try
            {
                string[] tag = { "+CMGL:" };
                if (!atResponse.Contains(tag[0])) return null;

                List<SMSMessage> Messages = new List<SMSMessage>();
                
                String[] responses = atResponse.Split(tag, StringSplitOptions.None);
                foreach (string r in responses)
                {
                    if (r.Length < 10) continue;
                    StringReader sr = new StringReader(r);

                    
                    String header = sr.ReadLine();
                    
                    char[] separator = { ',' };

                    String[] headerFields = header.Split(separator, StringSplitOptions.None);

                    currentMessage = new SMSMessage();
                    
                    currentMessage.Sender = headerFields[2].Substring(4, 10);
                    String temp= String.Empty;
                    do{
                        temp = sr.ReadLine();
                    }while( temp.Length < 3 );

                    currentMessage.Body = temp;
                    Messages.Add(currentMessage);
                    DeleteMessage(Convert.ToInt32(headerFields[0]));
                }
                return Messages;
            }
            catch (Exception e)
            {
                return null;
            }
            

        }

     

        private bool transact(String atCommand , String data,int timeout)
        {
            bool result = false;



            String command = cmdPrefix + atCommand + data + cmdSuffix;


            #region TRACE_CODE

            String traceString = DateTime.Now.ToString();

            traceString += ":" + command;

            traceString += Environment.NewLine;

            _gsmSMSTrace.TraceInformation(traceString);
            foreach (TraceListener l in _gsmSMSTrace.Listeners)
            {
                l.Flush();
            }

            #endregion



            if (command != String.Empty)
            {
                try
                {
                    if (spDriver.IsOpen)
                    {
                        spDriver.DiscardInBuffer();
                        spDriver.WriteToPort(command);
                    }
                }
                catch (Exception e)
                {

                    throw new GSM_SMSException("Serial Port Write Error");
                }
                transactionTimer = new System.Timers.Timer(timeout);
                transactionTimer.Elapsed += transactionTimeout;
                transactionTimer.Start();           //start transaction timer 
                InTransaction = true;
                transactionEvent.WaitOne();         //wait for response

                InTransaction = false;

                if (atResponse == String.Empty)               //if no response
                {
                    result = false;                 //indicate failure
                }
                else
                {

                    result = parseATresponse(atResponse, atCommand);

                }
            }

            return result;
        }


      

        private bool transact(String data)
        {
            bool result = false;


            #region TRACE_CODE

            String traceString = DateTime.Now.ToString();

            traceString += ":" + data;

            traceString += Environment.NewLine;

            _gsmSMSTrace.TraceInformation(traceString);
            foreach (TraceListener l in _gsmSMSTrace.Listeners)
            {
                l.Flush();
            }

            #endregion


            try
            {
                if (spDriver.IsOpen)
                {
                    spDriver.DiscardInBuffer();
                    spDriver.DiscardOutBuffer();
                    spDriver.WriteToPort(data);
                }
            }
            catch (Exception e)
            {

                throw new GSM_SMSException("Serial Port Write Error");
            }
            transactionTimer.Interval = 10000;
            transactionTimer.Start();           //start transaction timer 
            InTransaction = true;
            transactionEvent.WaitOne();         //wait for response

            InTransaction = false;

            if (atResponse == String.Empty)               //if no response
            {
                result = false;                 //indicate failure
            }
            else
            {

                result = parseATresponse(atResponse, AT_CMD_STR[(int)AT_COMMAND.SEND_SMS]);

            }
        
            return result;
        }


        private void transactionTimeout(object sender, ElapsedEventArgs e)
        {
            transactionTimer.Stop();    //stop the timer

            try
            {
                atResponse = spDriver.ReadExisting();

            }
            catch (Exception ex)
            {
                atResponse = String.Empty;
            }
            finally
            {


                transactionEvent.Set();
            }
        }

        public void Open(String port, String baud, String handshake)
        {
            spDriver.open(port, baud, handshake);
               // throw new GSM_SMSException("Unable to open Port:" + port);

            ComPort = port;
        }


        public bool Ping()
        {
            return transact(AT_CMD_STR[(int)AT_COMMAND.NONE], string.Empty, 200);
        }

        public bool SetEchoOff()
        {
            return transact(AT_CMD_STR[(int)AT_COMMAND.ECHO_OFF], String.Empty, 200);
        }

        public bool CheckSMSCAddress()
        {
            return transact(AT_CMD_STR[(int)AT_COMMAND.SMSC_ADDRESS], "?", 200);
        }


        public bool SetMessageMode(SMS_MODE mode)
        {
            String modeString = (mode == SMS_MODE.TEXT) ? "=1" : "=0";
            return transact(AT_CMD_STR[(int)AT_COMMAND.MESSAGE_MODE], modeString, 200);
        }

        public List<SMSMessage> ReadMessage(String type)
        {
            if (transact(AT_CMD_STR[(int)AT_COMMAND.READ_ALL], "=" + "\"" + type + "\"", 3000) == true)
                return processMessages(atResponse);
            else return null;
        }

        public bool DeleteMessage(int index)
        {
            return transact(AT_CMD_STR[(int)AT_COMMAND.DELETE_MESSAGE], "=" + index.ToString(), 2000);
        }

        public bool DeleteAllMessages()
        {
            return transact(AT_CMD_STR[(int)AT_COMMAND.DELETE_MESSAGE], "=1,4", 25000);
        }

        public void Cancel()
        {
            if (transactionTimer.Enabled)
            {
                transactionTimer.Stop();
                transactionTimer.Close();
                transactionTimer.Dispose();
                
            }
            if (InTransaction)
                transactionEvent.Set();
        }

        
        
    }

    public class SMSMessage
    {
        public String Body { get; set; }
        public String Sender { get; set; }
        public SMSMessage()
        {
        }
    }


    public class MessageReceivedEvent : EventArgs
    {
        public SMSMessage msg;

        public MessageReceivedEvent(SMSMessage msg)
        {
            this.msg = msg;
        }
    }

    public class GSM_SMSException : Exception
    {
        public String message = String.Empty;
        public GSM_SMSException(String msg)
        {
            message = msg;
        }
    }    
}
