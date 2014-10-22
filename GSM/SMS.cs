using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using ias.devicedriver;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Timers;
using System.IO;
namespace GSM
{
    public partial class Messenger
    {
        static SerialPortDriver SerialInterface;
        public BlockingCollection<GSMCommand> CommandQ { get; set; }
        public BlockingCollection<GSMResponse> ResponseQ { get; set; }

        BackgroundWorker worker;

        
         AutoResetEvent transactionEvent = null;

         System.Timers.Timer transactionTimer = null;
        TraceSource _gsmSMSTrace = null;
        TextWriterTraceListener _gsmSMSTraceListener = null;

        String ATResponse = String.Empty;
        CancellationTokenSource cts;
        bool AbortTransaction = false;
        public Messenger()
        {
            CommandQ = new BlockingCollection<GSMCommand>();
            ResponseQ = new BlockingCollection<GSMResponse>();
            SerialInterface = new SerialPortDriver();
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += worker_DoWork;


            transactionTimer = new System.Timers.Timer();
            transactionTimer.Elapsed +=transactionTimer_Elapsed;
            transactionTimer.AutoReset = false;
           

            _gsmSMSTrace = new TraceSource("GSM_SMSTrace");
            _gsmSMSTrace.Switch = new SourceSwitch("GSM_SMSTraceSwitch");


            String gsmSMSTraceFile = "GSM_TRACE.txt";

            if (gsmSMSTraceFile != String.Empty && gsmSMSTraceFile != null)
            {
                _gsmSMSTraceListener = new TextWriterTraceListener(gsmSMSTraceFile);
                _gsmSMSTrace.Listeners.Add(_gsmSMSTraceListener);
                _gsmSMSTrace.Switch.Level = SourceLevels.Information;
            }
            else
            {
                _gsmSMSTrace.Switch.Level = SourceLevels.Off;
            }

            transactionEvent = new AutoResetEvent(false);

            cts = new CancellationTokenSource();
            
            worker.RunWorkerAsync(cts.Token);
        }

        public void Initialize(String port, String baud, String handshake)
        {
            SerialInterface.open(port, baud, handshake);
        }


        public void Abort()
        {
            transactionTimer.Stop();
            AbortTransaction = true;

            cts.Cancel();
            
        }

        void transactionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            transactionTimer.Stop();    //stop the timer

            try
            {
                ATResponse = SerialInterface.ReadExisting();

            }
            catch (System.TimeoutException)
            {
                ATResponse = String.Empty;
            }
            finally
            {


                transactionEvent.Set();
            }
        }



        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    GSMCommand Command = CommandQ.Take((CancellationToken)e.Argument);
                    AbortTransaction = false;
                    switch (Command.Command)
                    {
                        case GSM_COMMAND.PING:

                            GSMResponse response = new GSMResponse(GSM_COMMAND.PING);
                            if (transact(AT_CMD_STR[(int)GSM_COMMAND.PING], "", Command.Timeout) == true)
                                response.Response = "Success";
                            else
                                response.Response = "Failure";

                            ResponseQ.Add(response);
                            break;

                        case GSM_COMMAND.ECHO_OFF:

                           response = new GSMResponse(GSM_COMMAND.ECHO_OFF);
                            if (transact(AT_CMD_STR[(int)GSM_COMMAND.ECHO_OFF], "", Command.Timeout) == true)
                                response.Response = "Success";
                            else
                                response.Response = "Failure";

                            ResponseQ.Add(response);
                            break;


                        case GSM_COMMAND.SMS_CENTER_ADDRESS:

                            response = new GSMResponse(GSM_COMMAND.SMS_CENTER_ADDRESS);
                            if (transact(AT_CMD_STR[(int)GSM_COMMAND.SMS_CENTER_ADDRESS], Command.Parameters[0], Command.Timeout) == true)
                                response.Response = "Success";
                            else
                                response.Response = "Failure";

                            ResponseQ.Add(response);
                            break;

                        case GSM_COMMAND.SET_SMS_MODE:

                            response = new GSMResponse(GSM_COMMAND.SET_SMS_MODE);
                            if (transact(AT_CMD_STR[(int)GSM_COMMAND.SET_SMS_MODE], Command.Parameters[0], Command.Timeout) == true)
                                response.Response = "Success";
                            else
                                response.Response = "Failure";

                            ResponseQ.Add(response);
                            break;


                    }
                }
                catch (System.OperationCanceledException)
                {
                    transactionTimer.Stop();
                   
                    break;
                }
            }
        }


        private bool parseATResponse(String ATResponse, String atCommand)
        {
            bool result = false;

            #region TRACE_CODE

            String traceString = DateTime.Now.ToString();

            traceString += ":" + ATResponse;

            traceString += Environment.NewLine;

            _gsmSMSTrace.TraceInformation(traceString);
            foreach (TraceListener l in _gsmSMSTrace.Listeners)
            {
                l.Flush();
            }

            #endregion

            switch (atCommand)
            {
                case "":
                    if (ATResponse.Contains("OK"))
                        result = true;
                    break;
                case "E0":
                    if (ATResponse.Contains("OK"))
                        result = true;
                    break;
                case "+CSCA":
                    if (ATResponse.Contains("OK"))
                        result = true;
                    break;

                case "+CMGF":
                    if (ATResponse.Contains("OK"))
                        result = true;
                    break;

                case "+CMGS":
                    if (ATResponse.Contains(">"))
                        return true;

                    if (ATResponse.Contains("OK"))
                        result = true;
                    break;

                case "+CMGR":

                    if (ATResponse.Contains("OK"))
                    {
                        if (processMessage(ATResponse))
                            result = true;
                    }

                    break;

                default:
                    result = false;
                    break;
            }
            return result;
        }


        private bool transact(String atCommand, String data, double timeout)
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
                    if (SerialInterface.IsOpen)
                    {
                        SerialInterface.DiscardInBuffer();
                        SerialInterface.WriteToPort(command);
                    }
                }
                catch (Exception e)
                {

                    throw e;
                }
                transactionTimer.Interval = timeout*1000;
                transactionTimer.Start();           //start transaction timer 
                transactionEvent.WaitOne();         //wait for response

                if (AbortTransaction)
                    return false;

                if (ATResponse == String.Empty)               //if no response
                {
                    result = false;                 //indicate failure
                }
                else
                {

                    result = parseATResponse(ATResponse, atCommand);

                }
            }

            return result;
        }

        private bool processMessage(string ATResponse)
        {
            try
            {

                StringReader sr = new StringReader(ATResponse);

                ATResponse = ATResponse.Remove(0, sr.ReadLine().Length);
                String header = sr.ReadLine();
                ATResponse = ATResponse.Remove(0, header.Length + 3);
                char[] separator = { '\"' };

                String[] headerFields = header.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                //currentMessage = new SMSMessage();

                //currentMessage.Sender = headerFields[3];



                //currentMessage.Body = ATResponse;
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }


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
                if (SerialInterface.IsOpen)
                {
                    SerialInterface.DiscardInBuffer();
                    SerialInterface.DiscardOutBuffer();
                    SerialInterface.WriteToPort(data);
                }
            }
            catch (Exception e)
            {

                //throw new GSM_SMSException("Serial Port Write Error");
            }
            transactionTimer.Interval = 7000;
            transactionTimer.Start();           //start transaction timer 
            transactionEvent.WaitOne();         //wait for response

            if (ATResponse == String.Empty)               //if no response
            {
                result = false;                 //indicate failure
            }
            else
            {

                result = parseATResponse(ATResponse, AT_CMD_STR[(int)AT_COMMAND.SEND_SMS]);

            }

            return result;
        }


     


       
    }


   

}
