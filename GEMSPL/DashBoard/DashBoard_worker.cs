using GEMSPL.Calibration;
using GEMSPL.DashBoard.Area;
using GEMSPL.DashBoard.Modules;
using GEMSPL.DashBoard.ULBS;
using GEMSPL.DashBoard.Zones;
using GEMSPL.Entity;
using GEMSPL.Reports.InstallationReport;
using GEMSPL.Reports.SavingsReport;
using GEMSPL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using GEMSPL.DashBoard.Feedback;
using ias.devicedriver;
using System.Threading;
using GEMSPL.DashBoard.TimerStatus;

namespace GEMSPL.DashBoard
{
    public enum ACTIVITY
    {
        INITIALIZE, TIMER_STATUS,
        T1ON, T1OFF, T2ON, T2OFF, UPDATE_SCHEDULE, UPDATE_CLOCK,
        READ_CLOCK,
        READ_SCHEDULE,
        READ_ENERGY_METER
    };

    public partial class DashBoardView : UserControl
    {

        GSM_SMS SMSInterface;
        Dictionary<String, Transaction> Transactions;
        InitializationFB FBView;
        AutoResetEvent timeoutEvent;
        bool CancelTransaction = false;
        partial void extendConstructor()
        {
            Transactions = new Dictionary<string, Transaction>();
            FBView = new InitializationFB("", "");
            SMSInterface = new GSM_SMS();
            timeoutEvent = new AutoResetEvent(false);
        }


        void worker_DoWork(object sender, DoWorkEventArgs e)
        {

            try
            {
                WorkArgs wa = e.Argument as WorkArgs;
                ACTIVITY ACTIVITY = wa.Activity;
                
                switch (ACTIVITY)
                {
                    case ACTIVITY.INITIALIZE:
                        try
                        {
                            
                            if (initialize() == false)
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                             new Action(() =>
                             {

                                 MessageBox.Show("GSM Modem Error - Messaging Controls will be disabled",
                                     "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                 Transient.Children.Clear();
                             }));
                        }
                        catch (Exception exp)
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                         new Action(() =>
                         {

                             MessageBox.Show("GSM Modem Error - Messaging Controls will be disabled",
                                 "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                             Transient.Children.Clear();
                         }));
                        }

                        break;

                    case DashBoard.ACTIVITY.TIMER_STATUS:
                        
                        CancelTransaction = false;
                        timerStatus(wa.ParmeterContainer as TimerStatusArgs);
                        break;

                    case DashBoard.ACTIVITY.READ_CLOCK:
                       
                        CancelTransaction = false;
                        readClock(wa.ParmeterContainer as TimerStatusArgs);
                        break;

                    case DashBoard.ACTIVITY.UPDATE_CLOCK:
                        
                        CancelTransaction = false;
                        updateClock(wa.ParmeterContainer as TimerStatusArgs);
                        break;


                    case DashBoard.ACTIVITY.READ_SCHEDULE:
                        
                        CancelTransaction = false;
                        readSchedule(wa.ParmeterContainer as TimerStatusArgs);
                        break;

                    case DashBoard.ACTIVITY.UPDATE_SCHEDULE:
                        
                        CancelTransaction = false;
                        updateSchedule(wa.ParmeterContainer as TimerStatusArgs);
                        break;

                    case DashBoard.ACTIVITY.READ_ENERGY_METER:
                        
                        CancelTransaction = false;
                        readEnergyMeter(wa.ParmeterContainer as TimerStatusArgs);
                        break;


                }
            }

            catch (Exception exp)
            {





            }

        }
        #region ENERGY_METER

        private void readEnergyMeter(TimerStatusArgs timerStatusArgs)
        {
            int messageCount = 0;
            foreach (KeyValuePair<String, Transaction> t in Transactions)
            {
                messageCount += t.Value.Messages.Count;
            }
            int messageSent = 0;
            int responseReceived = 0;
            double totalTimeout = timerStatusArgs.Timeout;
            ICollection<EnergyParameter> parameters = null;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              FBView.Clear();
                              FBView.EnableCancellation();
                              FBView.SetHeader(timerStatusArgs.Header);
                              FBView.SetTag(timerStatusArgs.Tag);
                              FBView.CancelEvent += DashBoardView_CancelEvent;
                              Transient.Children.Clear();
                              Transient.Children.Add(FBView);

                          }));
            SMSInterface.ReadMessage("REC UNREAD");
            foreach (KeyValuePair<String, Transaction> t in Transactions)
            {
                foreach (String m in t.Value.Messages)
                {
                    if (SMSInterface.sendSMS(t.Key, m) == true)
                        messageSent++;
                    updateProgress(messageCount, messageSent, responseReceived);
                }

                if (CancelTransaction == true)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {

                                  Transient.Children.Clear();

                              }));
                    return;
                }


            }


            do
            {
                List<SMSMessage> responses = SMSInterface.ReadMessage("REC UNREAD");
                if (responses != null)
                {
                    foreach (SMSMessage m in responses)
                    {
                        if (!Transactions.Keys.Contains(m.Sender)) continue;
                        parameters = processEnergyMeterResponse(m);
                        if( parameters != null )
                        responseReceived++;

                        if (CancelTransaction == true)
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                      new Action(() =>
                                      {

                                          Transient.Children.Clear();

                                      }));
                            return;
                        }
                    }
                }
                timeoutEvent.WaitOne(timerStatusArgs.Period * 1000);
                if (CancelTransaction == true)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {

                                  Transient.Children.Clear();

                              }));
                    return;
                }
              
                totalTimeout -= timerStatusArgs.Period;

            } while ((totalTimeout > 0) && (responseReceived < messageSent) && (CancelTransaction == false));

            

            if (parameters == null)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                         new Action(() =>
                         {
                             MessageBox.Show("Unable To Read Energy Meter Data", "Energy Meter Read Failure", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                             Transient.Children.Clear();
                         }));
            }
            else
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              EnergyMeterView ev = new EnergyMeterView(parameters);
                              ev.CancelEvent += mv_CancelEvent;
                              Transient.Children.Clear();
                              Transient.Children.Add(ev);
                          }));
            }
        }

        private ICollection<EnergyParameter> processEnergyMeterResponse(SMSMessage m)
        {
            List<EnergyParameter> Parameters = null;

            char[] separator = {','};
            char[] endMarker = {'@'};
            if (m.Body.Contains("EMT"))
            {
                Parameters = new List<EnergyParameter>();
                m.Body = m.Body.TrimEnd(endMarker);
                string[] fields = m.Body.Split(separator);
                if (fields[1] == "1")
                {
                    Parameters.Add(new EnergyParameter("Total Active Energy (kWh)", fields[2]));
                    Parameters.Add(new EnergyParameter("Phase Voltage (V)", fields[3]));
                    Parameters.Add(new EnergyParameter("Phase Current (A)", fields[4]));
                    Parameters.Add(new EnergyParameter("Active Power (kW)", fields[5]));
                    if (Admin)
                    {
                        Parameters.Add(new EnergyParameter("Power Factor (COS)", fields[6]));
                    }
                    Parameters.Add(new EnergyParameter("Grid Frequency (Hz)", fields[7]));
                }
                else if (fields[1] == "3")
                {
                    Parameters.Add(new EnergyParameter("Total Active Energy (kWh)", fields[2]));

                    Parameters.Add(new EnergyParameter("Phase A Voltage (V)", fields[3]));
                    Parameters.Add(new EnergyParameter("Phase B Voltage (V)", fields[4]));
                    Parameters.Add(new EnergyParameter("Phase C Voltage (V)", fields[5]));

                    Parameters.Add(new EnergyParameter("Phase A Current (A)", fields[6]));
                    Parameters.Add(new EnergyParameter("Phase B Current (A)", fields[7]));
                    Parameters.Add(new EnergyParameter("Phase C Current (A)", fields[8]));

                    Parameters.Add(new EnergyParameter("Phase A Active Power (kW)", fields[9]));
                    Parameters.Add(new EnergyParameter("Phase B Active Power (kW)", fields[10]));
                    Parameters.Add(new EnergyParameter("Phase C Active Power (kW)", fields[11]));

                    Parameters.Add(new EnergyParameter("Total Active Power (kW)", fields[12]));

                    if (Admin)
                    {

                        Parameters.Add(new EnergyParameter("Phase A Power Factor (COS)", fields[13]));
                        Parameters.Add(new EnergyParameter("Phase B Power Factor (COS)", fields[14]));
                        Parameters.Add(new EnergyParameter("Phase C Power Factor (COS)", fields[15]));


                        Parameters.Add(new EnergyParameter("Total Power Factor (COS)", fields[16]));
                    }
                    Parameters.Add(new EnergyParameter("Grid Frequency (Hz)", fields[17]));


                }
            }

            return Parameters;
        }

        #endregion

        #region MODULE_SCHEDULE

        private void updateSchedule(TimerStatusArgs timerStatusArgs)
        {
            int messageCount = 0;
            foreach (KeyValuePair<String, Transaction> t in Transactions)
            {
                messageCount += t.Value.Messages.Count;
            }
            int messageSent = 0;
            int responseReceived = 0;
            double totalTimeout = timerStatusArgs.Timeout;
            String[] responses = new string[messageCount];

            int responseIndex = -1;

            String response = String.Empty;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              FBView.Clear();
                              FBView.EnableCancellation();
                              FBView.SetHeader(timerStatusArgs.Header);
                              FBView.SetTag(timerStatusArgs.Tag);
                              FBView.CancelEvent += DashBoardView_CancelEvent;
                              Transient.Children.Clear();
                              Transient.Children.Add(FBView);

                          }));

            SMSInterface.ReadMessage("REC UNREAD");
            foreach (KeyValuePair<String, Transaction> t in Transactions)
            {
                foreach (String m in t.Value.Messages)
                {
                    if (SMSInterface.sendSMS(t.Key, m) == true)
                        messageSent++;
                    updateProgress(messageCount, messageSent, responseReceived);
                    if (CancelTransaction == true)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                  new Action(() =>
                                  {

                                      Transient.Children.Clear();

                                  }));
                        return;
                    }
                }

             
            }


            do
            {
                List<SMSMessage> responseMessages = SMSInterface.ReadMessage("REC UNREAD");
                if (responseMessages != null)
                {
                    foreach (SMSMessage m in responseMessages)
                    {
                        if (!Transactions.Keys.Contains(m.Sender)) continue;
                        response = processScheduleUpdateResponse(m, out responseIndex);
                        if (responseIndex != -1 && response != String.Empty)
                        {
                            responseReceived++;
                       

                        
                            responses[responseIndex] = response;
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                             new Action(() =>
                             {
                                 updateProgress(messageCount, messageSent, responseReceived);
                             }));
                        }
                        if (CancelTransaction == true)
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                      new Action(() =>
                                      {

                                          Transient.Children.Clear();

                                      }));
                            return;
                        }

                    }
                }
                timeoutEvent.WaitOne(timerStatusArgs.Period * 1000);
                if (CancelTransaction == true)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {

                                  Transient.Children.Clear();

                              }));
                    return;
                }
              
                totalTimeout -= timerStatusArgs.Period;

            } while ((totalTimeout > 0) && (responseReceived < messageSent) && (CancelTransaction == false));

           

            if (responseReceived == messageCount)
            {

                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              
                                    MessageBox.Show("Schedule Updated Successfully", "Schedule Update Success",
                                        MessageBoxButton.OK, MessageBoxImage.Exclamation);

                                    Transient.Children.Clear();
                                
                          }));
            }

            else
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    MessageBox.Show("Unable To Update Schedule", "Schedule Update Failure",
                                        MessageBoxButton.OK, MessageBoxImage.Exclamation);

                                    Transient.Children.Clear();
                                }));
            }
            
        }

        private string processScheduleUpdateResponse(SMSMessage m, out int responseIndex)
        {
            responseIndex = -1;
            if (m.Body.Contains("SSHDT CHANGED"))
            {
                responseIndex = 0;
                return  "SSHDT CHANGED";
            }
            else if (m.Body.Contains("SSHT1 CHANGED"))
            {
                responseIndex = 1;
                return  "SSHT1 CHANGED";
            }
            else if (m.Body.Contains("SSHT2 CHANGED"))
            {
                responseIndex = 2;
                return  "SSHT2 CHANGED";
            }
            return string.Empty;
        }




        private void readSchedule(TimerStatusArgs timerStatusArgs)
        {
            int messageCount = 0;
            foreach (KeyValuePair<String, Transaction> t in Transactions)
            {
                messageCount += t.Value.Messages.Count;
            }
            int messageSent = 0;
            int responseReceived = 0;
            double totalTimeout = timerStatusArgs.Timeout;
            String[] responses= new string[messageCount];

            int responseIndex = -1;

            String response = String.Empty;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              FBView.Clear();
                              FBView.EnableCancellation();
                              FBView.SetHeader(timerStatusArgs.Header);
                              FBView.SetTag(timerStatusArgs.Tag);
                              FBView.CancelEvent += DashBoardView_CancelEvent;
                              Transient.Children.Clear();
                              Transient.Children.Add(FBView);

                          }));

            SMSInterface.ReadMessage("REC UNREAD");
            foreach (KeyValuePair<String, Transaction> t in Transactions)
            {
                foreach (String m in t.Value.Messages)
                {
                    if (SMSInterface.sendSMS(t.Key, m) == true)
                        messageSent++;
                    updateProgress(messageCount, messageSent, responseReceived);

                    if (CancelTransaction == true)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                  new Action(() =>
                                  {

                                      Transient.Children.Clear();

                                  }));
                        return;
                    }
                }

                
            }


            do
            {
                List<SMSMessage> responseMessages = SMSInterface.ReadMessage("REC UNREAD");
                if (responseMessages != null)
                {
                    foreach (SMSMessage m in responseMessages)
                    {
                        if (!Transactions.Keys.Contains(m.Sender)) continue;
                        response = processScheduleReadResponse(m,out responseIndex);
                        if (response != String.Empty && responseIndex != -1)
                        {
                            responseReceived++;

                            responses[responseIndex] = response;
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                             new Action(() =>
                             {
                                 updateProgress(messageCount, messageSent, responseReceived);
                             }));
                        }
                        if (CancelTransaction == true)
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                      new Action(() =>
                                      {

                                          Transient.Children.Clear();

                                      }));
                            return;
                        }

                    }
                }
                timeoutEvent.WaitOne(timerStatusArgs.Period * 1000);
                if (CancelTransaction == true)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {

                                  Transient.Children.Clear();

                              }));
                    return;
                }
               
                totalTimeout -= timerStatusArgs.Period;

            } while ((totalTimeout > 0) && (responseReceived < messageSent) && (CancelTransaction == false));

           
           

            if (responseReceived == messageCount)
            {

                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              ScheduleView sv = new ScheduleView(responses[0], responses[1], responses[2]);
                              sv.CancelEvent += mv_CancelEvent;
                              Transient.Children.Clear();
                              Transient.Children.Add(sv);
                          }));
            }

            else
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    MessageBox.Show("Unable To Read Schedule", "Schedule Read Failure",
                                        MessageBoxButton.OK, MessageBoxImage.Exclamation);

                                    Transient.Children.Clear();
                                }));
            }
            
        }

        private string processScheduleReadResponse(SMSMessage m,  out int index)
        {
            index = -1;
            if (m.Body.Contains("ASHDT"))
            {
                index = 0;
                return cropResponse(m.Body, "ASHDT");
            }
            else if( m.Body.Contains("ASHT1"))
            {
                index = 1;
                return cropResponse(m.Body, "ASHT1");
            }
            else if(  m.Body.Contains("ASHT2"))
            {
                index = 2;
                return cropResponse(m.Body, "ASHT2");
            }
            return string.Empty;
        }

        #endregion

        #region MODULE_CLOCK

        private void updateClock(TimerStatusArgs timerStatusArgs)
        {
            int messageCount = 0;
            foreach (KeyValuePair<String, Transaction> t in Transactions)
            {
                messageCount += t.Value.Messages.Count;
            }
            int messageSent = 0;
            int responseReceived = 0;
            double totalTimeout = timerStatusArgs.Timeout;
            String timestamp = String.Empty;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              FBView.Clear();
                              FBView.EnableCancellation();
                              FBView.SetHeader(timerStatusArgs.Header);
                              FBView.SetTag(timerStatusArgs.Tag);
                              FBView.CancelEvent += DashBoardView_CancelEvent;
                              Transient.Children.Clear();
                              Transient.Children.Add(FBView);

                          }));

            SMSInterface.ReadMessage("REC UNREAD");

            foreach (KeyValuePair<String, Transaction> t in Transactions)
            {
                foreach (String m in t.Value.Messages)
                {
                    if (SMSInterface.sendSMS(t.Key, m) == true)
                        messageSent++;
                    updateProgress(messageCount, messageSent, responseReceived);

                    if (CancelTransaction == true)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                  new Action(() =>
                                  {

                                      Transient.Children.Clear();

                                  }));
                        return;
                    }
                }


            }


            do
            {
                List<SMSMessage> responses = SMSInterface.ReadMessage("REC UNREAD");
                if (responses != null)
                {
                    foreach (SMSMessage m in responses)
                    {
                        if (!Transactions.Keys.Contains(m.Sender)) continue;
                        timestamp = processClockUpdateResponse(m);
                        responseReceived++;
                    }
                    if (CancelTransaction == true)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                  new Action(() =>
                                  {

                                      Transient.Children.Clear();

                                  }));
                        return;
                    }
                }
                timeoutEvent.WaitOne(timerStatusArgs.Period * 1000);

                if (CancelTransaction == true)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {

                                  Transient.Children.Clear();

                              }));
                    return;
                }

                totalTimeout -= timerStatusArgs.Period;

            } while ((totalTimeout > 0) && (responseReceived < messageSent) && (CancelTransaction == false));

            if (timestamp == String.Empty)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                         new Action(() =>
                         {
                             MessageBox.Show("Unable To Update Module Clock", "Clock Update Failure", 
                                 MessageBoxButton.OK, MessageBoxImage.Exclamation);

                             Transient.Children.Clear();
                         }));
            }
            else
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              MessageBox.Show(" Module Clock Updated" , "Clock Update",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                              Transient.Children.Clear();
                              
                          }));
            }
        }

        private string processClockUpdateResponse(SMSMessage m)
        {
            if (m.Body.Contains("DTT CHANGED")) return "UPDATED";
            else return String.Empty;

        }

        private void readClock(TimerStatusArgs timerStatusArgs)
        {
            int messageCount = 0;
            foreach (KeyValuePair<String, Transaction> t in Transactions)
            {
                messageCount += t.Value.Messages.Count;
            }
            int messageSent = 0;
            int responseReceived = 0;
            double totalTimeout = timerStatusArgs.Timeout;
            String timestamp = String.Empty;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              FBView.Clear();
                              FBView.EnableCancellation();
                              FBView.SetHeader(timerStatusArgs.Header);
                              FBView.SetTag(timerStatusArgs.Tag);
                              FBView.CancelEvent += DashBoardView_CancelEvent;
                              Transient.Children.Clear();
                              Transient.Children.Add(FBView);

                          }));

            SMSInterface.ReadMessage("REC UNREAD");
            foreach (KeyValuePair<String, Transaction> t in Transactions)
            {
                foreach (String m in t.Value.Messages)
                {
                    if (SMSInterface.sendSMS(t.Key, m) == true)
                        messageSent++;
                    updateProgress(messageCount, messageSent, responseReceived);
                }
                if (CancelTransaction == true)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {

                                  Transient.Children.Clear();

                              }));
                    return;
                }

            }


            do
            {
                List<SMSMessage> responses = SMSInterface.ReadMessage("REC UNREAD");
                if (responses != null)
                {
                    foreach (SMSMessage m in responses)
                    {
                        if (!Transactions.Keys.Contains(m.Sender)) continue;
                        timestamp = processClockReadResponse(m);
                        if( timestamp != string.Empty )
                        responseReceived++;
                    }
                    if (CancelTransaction == true)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                  new Action(() =>
                                  {

                                      Transient.Children.Clear();

                                  }));
                        return;
                    }
                }
                timeoutEvent.WaitOne(timerStatusArgs.Period * 1000);

                if (CancelTransaction == true)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {

                                  Transient.Children.Clear();

                              }));
                    return;
                }
               
                totalTimeout -= timerStatusArgs.Period;

            } while ((totalTimeout > 0) && (responseReceived < messageSent) && (CancelTransaction == false));

            if (timestamp == String.Empty)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                         new Action(() =>
                         {
                             MessageBox.Show("Unable To Read Module Clock", "Clock Read Failure", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                             Transient.Children.Clear();
                         }));
            }
            else
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              ModuleClockView mv = new ModuleClockView(timestamp);
                              mv.CancelEvent += mv_CancelEvent;
                              Transient.Children.Clear();
                              Transient.Children.Add(mv);
                          }));
            }
        }

        void mv_CancelEvent(object sender, Modules.CancelEventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                         new Action(() =>
                         {
                             Transient.Children.Clear();
                         }));
        }

   

        private string processClockReadResponse(SMSMessage m)
        {
            if (!m.Body.Contains("ADMDT")) return String.Empty;
             return cropResponse(m.Body, "ADMDT");
        }
        #endregion

        #region TIMER_STATUS

        private void timerStatus(TimerStatusArgs args)
        {
            int messageCount = 0;
            foreach (KeyValuePair<String, Transaction> t in Transactions)
            {
                messageCount += t.Value.Messages.Count;
            }
            int messageSent = 0;
            int responseReceived = 0;
            double totalTimeout = args.Timeout;

            String status = "UNKNOWN";



            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              FBView.Clear();
                              FBView.EnableCancellation();
                              FBView.SetHeader(args.Header);
                              FBView.SetTag(args.Tag);
                              FBView.CancelEvent += DashBoardView_CancelEvent;
                              Transient.Children.Clear();
                              Transient.Children.Add(FBView);

                          }));

            SMSInterface.ReadMessage("REC UNREAD");

            foreach (KeyValuePair<String, Transaction> t in Transactions)
            {
                foreach (String m in t.Value.Messages)
                {
                    if (SMSInterface.sendSMS(t.Key, m) == true)
                        messageSent++;
                    updateProgress(messageCount, messageSent, responseReceived);

                    if (CancelTransaction == true)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                  new Action(() =>
                                  {

                                      Transient.Children.Clear();

                                  }));
                        return;
                    }
                
                }


            }


            do
            {
                List<SMSMessage> responses = SMSInterface.ReadMessage("REC UNREAD");
                if (responses != null)
                {
                    foreach (SMSMessage m in responses)
                    {
                        if (!Transactions.Keys.Contains(m.Sender)) continue;
                        status = processTimerStatusResponse(m);
                        if (status != String.Empty)
                        {
                            responseReceived++;
                        }
                    }
                }

                if (CancelTransaction == true)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {

                                  Transient.Children.Clear();

                              }));
                    return;
                }

                timeoutEvent.WaitOne(args.Period * 1000);

                if (CancelTransaction == true)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {

                                  Transient.Children.Clear();

                              }));
                    return;
                }

                totalTimeout -= args.Period;



            } while ((totalTimeout > 0) && (responseReceived < messageSent)&& (CancelTransaction == false ));



            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              TimerStatusView tv = new TimerStatusView(args.Tag, status);
                              tv.CancelEvent += tv_CancelEvent;
                              Transient.Children.Clear();
                              Transient.Children.Add(tv);
                          }));




        }

        private string processTimerStatusResponse(SMSMessage m)
        {
            if (m.Body.Contains("RT1ON") || m.Body.Contains("RT2ON")) return "ON";
            else if (m.Body.Contains("RT1OFF") || m.Body.Contains("RT2OFF")) return "OFF";
            else if (m.Body.Contains("MODULE IN AUTO MODE")) return m.Body;
            else return String.Empty;

        }

        void tv_CancelEvent(object sender, TimerStatus.CancelEventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                          new Action(() =>
                          {
                              Transient.Children.Clear();
                          }));
        }



        #endregion





        void DashBoardView_CancelEvent(object sender, Feedback.CancelEventArgs e)
        {
            CancelTransaction = true;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    if (Transient.Children.Count > 0)
                                    {
                                        InitializationFB fb = Transient.Children[0] as InitializationFB;
                                        fb.SetHeader("Cancelling Transaction...");
                                        fb.DisableCancellation();
                                    }
                                    
                                }));
        }
        private String cropResponse(String response, string crop)
        {
            response = response.Replace(crop, "");
            response = response.Replace("\r\n", "");
            response = response.Replace("\n", "");
            response = response.Replace("OK", "");
            response = response.Replace("@", "");
            response = response.Replace("00000000", "");
            return response;
        }


        private bool initialize()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                         new Action(() =>
                         {
                             FBView.Clear();
                             FBView.DisableCancellation();
                             FBView.SetTag("Initialization");
                             FBView.SetHeader("Initializing Modem...");
                             FBView.CancelEvent += DashBoardView_CancelEvent;
                             Transient.Children.Clear();
                             Transient.Children.Add(FBView);

                         }));
            SMSInterface.Open(PortSettings.ComPort, PortSettings.BaudRate, PortSettings.HandShake);

            if (SMSInterface.Ping() == false) return false;

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                         new Action(() =>
                         {
                             FBView.ProgessUpdate(5);
                         }));

            if (SMSInterface.SetEchoOff() == false) return false;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                         new Action(() =>
                         {
                             FBView.ProgessUpdate(10);
                         }));
            if (SMSInterface.CheckSMSCAddress() == false) return false;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                         new Action(() =>
                         {
                             FBView.ProgessUpdate(15);
                         }));

            if (SMSInterface.setMessageMode(GSM_SMS.SMS_MODE.TEXT) == false) return false;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                         new Action(() =>
                         {
                             FBView.ProgessUpdate(20);
                         }));

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
             new Action(() =>
             {
                 FBView.SetHeader("Deleting Unwanted Messages");
             }));

            for (int i = 1; i < 21; i++)
            {
                if (SMSInterface.DeleteMessage(i) == false) return false;
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                         new Action(() =>
                         {
                             FBView.ProgessUpdate(20 + i * 4);
                         }));
            }




            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
              new Action(() =>
              {
                  Transient.Children.Clear();
              }));
            return true;
        }

       




        private void updateProgress(int messageCount, int messageSent, int responseReceived)
        {
            double progress = ((messageSent + responseReceived) / (2.0 * messageCount))*100.0;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {
                                  FBView.ProgessUpdate(progress);
                              }));

        }
    }

    public class WorkArgs
    {
        public ACTIVITY Activity;
        public Object ParmeterContainer;

        public WorkArgs(ACTIVITY a, Object p)
        {
            Activity = a;
            ParmeterContainer = p;
        }
    }

    public class Transaction
    {
        
        public List<String> Messages { get; set; }
        public List<String> ValidResponses { get; set; }
        public List<String> InvalidResponses { get; set; }

        public Transaction(List<String> msg, List<String> vr, List<String> ivr)
        {
            Messages = msg;
            ValidResponses = vr;
            InvalidResponses = ivr;
        }
    }


    public class TimerStatusArgs
    {
        public String Header { get; set; }
        public String Tag { get; set; }
        public double Timeout { get; set; }
        public int Period { get; set; }

        public TimerStatusArgs(String header, String tag, double timeout,int period)
        {
            Header = header;
            Tag = tag;
            Timeout = timeout;
            Period = period;

        }

    }
   
         

}
