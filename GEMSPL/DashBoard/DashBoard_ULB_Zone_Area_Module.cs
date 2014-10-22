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

namespace GEMSPL.DashBoard
{
    public partial class DashBoardView : UserControl
    {
        Module currentModule;
        TreeNode currentNode;
        #region ULB

        private void AddULB_Click_1(object sender, RoutedEventArgs e)
        {
            ULBControl ulbControl = new ULBControl(BaseDir);
            ulbControl.SaveEvent += ulbControl_SaveEvent;
            ulbControl.CancelEvent += ulbControl_CancelEvent;
            Transient.Children.Clear();
            Transient.Children.Add(ulbControl);
        }

        void ulbControl_CancelEvent(object sender, GEMSPL.DashBoard.ULBS.CancelEventArgs e)
        {
            Transient.Children.Clear();
        }

        void ulbControl_SaveEvent(object sender, GEMSPL.DashBoard.ULBS.SaveEventArgs e)
        {
            Directory.CreateDirectory(BaseDir + "\\" + e.ULB);


            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    GetULBs();
                                    MessageBox.Show("ULB Created", "Application Info", MessageBoxButton.OK, MessageBoxImage.Information);
                                    Transient.Children.Clear();
                                }));

        }

        # endregion


        #region Zone

        private void AddZone_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode node = getNode((MenuItem)sender);
            ZoneControl zoneControl = new ZoneControl(node.Path);
            zoneControl.SaveEvent += zoneControl_SaveEvent;
            zoneControl.CancelEvent += zoneControl_CancelEvent;
            Transient.Children.Clear();
            Transient.Children.Add(zoneControl);
        }

        void zoneControl_CancelEvent(object sender, Zones.CancelEventArgs e)
        {
            Transient.Children.Clear();
        }

        void zoneControl_SaveEvent(object sender, Zones.SaveEventArgs e)
        {
            Directory.CreateDirectory(e.Entity);


            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    GetULBs();
                                    MessageBox.Show("Zone Created", "Application Info", MessageBoxButton.OK, MessageBoxImage.Information);
                                    Transient.Children.Clear();
                                }));
        }

        # endregion

        #region Area
        private void AddArea_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode node = getNode((MenuItem)sender);
            AreaControl areaControl = new AreaControl(node.Path);
            areaControl.SaveEvent += areaControl_SaveEvent;
            areaControl.CancelEvent += areaControl_CancelEvent;
            Transient.Children.Clear();
            Transient.Children.Add(areaControl);

        }

        void areaControl_CancelEvent(object sender, Area.CancelEventArgs e)
        {
            Transient.Children.Clear();
        }

        void areaControl_SaveEvent(object sender, Area.SaveEventArgs e)
        {
            Directory.CreateDirectory(e.Entity);
            FileStream fs;
            XmlSerializer serializer = new XmlSerializer(typeof(Schedule));
            String file = System.IO.Path.Combine(e.Entity + "\\" + "Schedule");
            if (!File.Exists(file))
            {
                fs = File.Create(file);
                Encoding encoding = Encoding.GetEncoding("UTF-8");

                Schedule sch = new Schedule();
                sch.Slots.Add(Slot.DefaultSlot);
                using (StreamWriter sw = new StreamWriter(fs, encoding))
                {
                    serializer.Serialize(sw, sch);
                }
                fs.Close();
            }

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    GetULBs();
                                    MessageBox.Show("Area Created", "Application Info",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                    Transient.Children.Clear();
                                }));
        }

        private void ModifyTemplate_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode node = getNode((MenuItem)sender);
            GEMSPL.Entity.Area a = node as GEMSPL.Entity.Area;
            ScheduleControl sc = new ScheduleControl(node.Path, a);
            sc.SaveEvent += sc_SaveEvent;
            sc.CancelEvent += sc_CancelEvent;
            Transient.Children.Clear();
            Transient.Children.Add(sc);

        }

        void sc_CancelEvent(object sender, Area.CancelEventArgs e)
        {
            Transient.Children.Clear();
        }

        void sc_SaveEvent(object sender, SchSaveEventArgs e)
        {
            FileStream fs;
            XmlSerializer serializer = new XmlSerializer(typeof(Schedule));
            String file = System.IO.Path.Combine(e.Entity + "\\" + "Schedule");

            fs = File.Open(file, FileMode.Truncate);
            Encoding encoding = Encoding.GetEncoding("UTF-8");



            using (StreamWriter sw = new StreamWriter(fs, encoding))
            {
                serializer.Serialize(sw, e.Schedule);
            }
            fs.Close();

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(() =>
                    {

                        MessageBox.Show("Schedule Saved", "Application Info",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        Transient.Children.Clear();
                    }));

        }


        private void UpdateAreaSchedule_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode node = getNode((MenuItem)sender);
            GEMSPL.Entity.Area a = node as GEMSPL.Entity.Area;
        }



        #endregion

        #region Module
        private void AddModule_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode node = getNode((MenuItem)sender);
            ModuleControl ModuleControl = new ModuleControl(node.Path);
            ModuleControl.SaveEvent += ModuleControl_SaveEvent;
            ModuleControl.CancelEvent += ModuleControl_CancelEvent;
            Transient.Children.Clear();
            Transient.Children.Add(ModuleControl);
        }

        void ModuleControl_CancelEvent(object sender, Modules.CancelEventArgs e)
        {
            Transient.Children.Clear();
        }

        void ModuleControl_SaveEvent(object sender, Modules.SaveEventArgs e)
        {
            Directory.CreateDirectory(e.Module.Location);
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.UTF8;
            settings.CloseOutput = false;
            settings.CheckCharacters = true;

            String sourceImage = e.Module.Details.Image;
            e.Module.Details.Image = e.Module.Location + "\\" + "Image";
            if (sourceImage != String.Empty) 
                File.Copy(sourceImage, e.Module.Details.Image, true);

            XmlWriter w = XmlWriter.Create(sb, settings);
            String file = System.IO.Path.Combine(e.Module.Location + "\\" + "Details");
            FileStream fs;
            XmlSerializer serializer = new XmlSerializer(typeof(ModuleDetails));



            if (!File.Exists(file))
            {
                fs = File.Create(file);
                Encoding encoding = Encoding.GetEncoding("UTF-8");

                using (StreamWriter sw = new StreamWriter(fs, encoding))
                {
                    serializer.Serialize(sw, e.Module.Details);
                }
                fs.Close();
            }

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    GetULBs();
                                    MessageBox.Show("Module Created", "Application Info",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                    Transient.Children.Clear();
                                }));

        }

        private void EditModule_Click_1(object sender, RoutedEventArgs e)
        {
            currentNode = getNode((MenuItem)sender);
            currentModule = currentNode as Module;
            ModuleControl ModuleControl = new ModuleControl(currentModule, currentNode.Path);
            ModuleControl.EditEvent += ModuleControl_EditEvent;
            ModuleControl.CancelEvent += ModuleControl_CancelEvent;
            Transient.Children.Clear();
            Transient.Children.Add(ModuleControl);
        }

        void ModuleControl_EditEvent(object sender, Modules.SaveEventArgs e)
        {
            String file = System.IO.Path.Combine(e.Module.Location + "\\" + "Details");
            FileStream fs;
            XmlSerializer serializer;
            if (currentNode.Path!= e.Module.Location)
            {
                Directory.Delete(currentNode.Path, true);
                Directory.CreateDirectory(e.Module.Location);
                StringBuilder sb = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.Encoding = Encoding.UTF8;
                settings.CloseOutput = false;
                settings.CheckCharacters = true;

                String sourceImage = e.Module.Details.Image;
                e.Module.Details.Image = e.Module.Location + "\\" + "Image";
                if (sourceImage != String.Empty)
                    File.Copy(sourceImage, e.Module.Details.Image, true);

                XmlWriter w = XmlWriter.Create(sb, settings);
                
                
                serializer = new XmlSerializer(typeof(ModuleDetails));




            }
            else
            {
                String sourceImage = e.Module.Details.Image;
                e.Module.Details.Image = e.Module.Location + "\\" + "Image";
                if (sourceImage != String.Empty)
                    File.Copy(sourceImage, e.Module.Details.Image, true);

                StringBuilder sb = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.Encoding = Encoding.UTF8;
                settings.CloseOutput = false;
                settings.CheckCharacters = true;

                XmlWriter w = XmlWriter.Create(sb, settings);
                
                
                serializer = new XmlSerializer(typeof(ModuleDetails));

            }
            if (!File.Exists(file))
            {
                fs = File.Create(file);
                Encoding encoding = Encoding.GetEncoding("UTF-8");

                using (StreamWriter sw = new StreamWriter(fs, encoding))
                {
                    serializer.Serialize(sw, e.Module.Details);
                }
                fs.Close();
            }
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    GetULBs();
                                    MessageBox.Show("Module Saved", "Application Info",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                    Transient.Children.Clear();
                                }));
        }

        private void GEMSPLTree_SelectedItemChanged_1(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            if (((TreeView)sender).SelectedItem.GetType() == typeof(Module))
            {
                ModuleView mv = new ModuleView();

                Module m = ((TreeView)sender).SelectedItem as Module;
                mv.DataContext = m;
                mv.SetImage(m.Details.Image);
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                new Action(() =>
                                {
                                    Transient.Children.Clear();
                                    Transient.Children.Add(mv);
                                }));

            }
            else
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                               new Action(() =>
                               {
                                   Transient.Children.Clear();
                               }));
            }

        }

        #region TIMER_STATUS

        private void T1ON_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode node = getNode((MenuItem)sender);
            Module m = node as Module;
            List<String> msg = new List<string>();
            msg.Add("MT1ON@");

            List<String> vr = new List<string>();
            vr.Add ("RT1ON");
            vr.Add("MODULE IN AUTO MODE");
            List<String> ivr = new List<string>();
            vr.Add ("");
            Transactions.Clear();
            Transactions.Add(m.Details.SIM, new Transaction(msg,vr,ivr));
            TimerStatusArgs args = new TimerStatusArgs("Switching On Timer 1...", "Timer 1", 120, 5);
            worker.RunWorkerAsync(new WorkArgs(ACTIVITY.TIMER_STATUS,args));
        }

        private void T1OFF_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode node = getNode((MenuItem)sender);
            Module m = node as Module;
            List<String> msg = new List<string>();
            msg.Add("MT1OFF@");

            List<String> vr = new List<string>();
            vr.Add("RT1OFF");
            vr.Add("MODULE IN AUTO MODE");
            List<String> ivr = new List<string>();
            vr.Add("");
            Transactions.Clear();
            Transactions.Add(m.Details.SIM, new Transaction(msg, vr, ivr));
            TimerStatusArgs args = new TimerStatusArgs("Switching Off Timer 1...", "Timer 1", 120, 5);
            worker.RunWorkerAsync(new WorkArgs(ACTIVITY.TIMER_STATUS, args));
        }

        private void T2ON_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode node = getNode((MenuItem)sender);
            Module m = node as Module;
            List<String> msg = new List<string>();
            msg.Add("MT2ON@");

            List<String> vr = new List<string>();
            vr.Add("RT2ON");
            vr.Add("MODULE IN AUTO MODE");
            List<String> ivr = new List<string>();
            vr.Add("");
            Transactions.Clear();
            Transactions.Add(m.Details.SIM, new Transaction(msg, vr, ivr));
            TimerStatusArgs args = new TimerStatusArgs("Switching On Timer 2...", "Timer 2", 120, 5);
            worker.RunWorkerAsync(new WorkArgs(ACTIVITY.TIMER_STATUS, args));
        }

        private void T2OFF_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode node = getNode((MenuItem)sender);
            Module m = node as Module;
            List<String> msg = new List<string>();
            msg.Add("MT2OFF@");

            List<String> vr = new List<string>();
            vr.Add("RT2OFF");
            vr.Add("MODULE IN AUTO MODE");
            List<String> ivr = new List<string>();
            vr.Add("");
            Transactions.Clear();
            Transactions.Add(m.Details.SIM, new Transaction(msg, vr, ivr));
            TimerStatusArgs args = new TimerStatusArgs("Switching Off Timer 2...", "Timer 2", 120, 5);
            worker.RunWorkerAsync(new WorkArgs(ACTIVITY.TIMER_STATUS, args));
        }
        #endregion

        #region CLOCK
        private void ReadClock_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode node = getNode((MenuItem)sender);
            Module m = node as Module;
            List<String> msg = new List<string>();
            msg.Add("RDMDT@");

            List<String> vr = new List<string>();
            vr.Add("ADMDT");
            
            List<String> ivr = new List<string>();
            vr.Add("");
            Transactions.Clear();
            Transactions.Add(m.Details.SIM, new Transaction(msg, vr, ivr));
            TimerStatusArgs args = new TimerStatusArgs("Reading Module Clock...", "Clock Read", 120, 5);
            worker.RunWorkerAsync(new WorkArgs(ACTIVITY.READ_CLOCK, args));
        }

        private void UpdateClock_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode node = getNode((MenuItem)sender);
            Module m = node as Module;
            String moduleTime = DateTime.Now.Day.ToString("D2") + DateTime.Now.Month.ToString("D2") + (DateTime.Now.Year - 2000).ToString("D2")
                        + DateTime.Now.Hour.ToString("D2") + DateTime.Now.Minute.ToString("D2");

            List<String> msg = new List<string>();
            msg.Add("CHDTT" + moduleTime + "@");

            List<String> vr = new List<string>();
            vr.Add("DTT CHANGED");

            List<String> ivr = new List<string>();
            vr.Add("INVALID CMD/DATA");
            Transactions.Clear();
            Transactions.Add(m.Details.SIM, new Transaction(msg, vr, ivr));
            TimerStatusArgs args = new TimerStatusArgs("Updating Module Clock...", "Clock Update", 120, 5);
            worker.RunWorkerAsync(new WorkArgs(ACTIVITY.UPDATE_CLOCK, args));
        }
        #endregion

        #region SCHEDULE

        private void ReadSchedule_Click_1(object sender, RoutedEventArgs e)
        {

            TreeNode node = getNode((MenuItem)sender);
            Module m = node as Module;
            List<String> msg = new List<string>();
            msg.Add("RSHDT@");
            msg.Add("RSHT1@");
            msg.Add("RSHT2@");

            List<String> vr = new List<string>();
            vr.Add("ASHDT");
            vr.Add("ASHT1");
            vr.Add("ASHT2");

            List<String> ivr = new List<string>();
            vr.Add("");
            Transactions.Clear();
            Transactions.Add(m.Details.SIM, new Transaction(msg, vr, ivr));
            TimerStatusArgs args = new TimerStatusArgs("Reading Schedule...", "Schedule Read", 120, 5);
            worker.RunWorkerAsync(new WorkArgs(ACTIVITY.READ_SCHEDULE, args));
        }

        private void UpdateSchedule_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode node = getNode((MenuItem)sender);
            Module m = node as Module;
            ScheduleVM svm = new ScheduleVM(m.Schedule);
            String date = svm.getDate();
            String t1 = svm.getT1();
            String t2 = svm.getT2();

            List<String> msg = new List<string>();
            msg.Add("SSHDT," + date + "@");
            msg.Add("SSHT1," + t1 + "@");
            msg.Add("SSHT2," + t2 + "@");

            List<String> vr = new List<string>();
            vr.Add("SSHDT CHANGED");
            vr.Add("SSHT1 CHANGED");
            vr.Add("SSHT2 CHANGED");

            List<String> ivr = new List<string>();
            vr.Add("");
            Transactions.Clear();
            Transactions.Add(m.Details.SIM, new Transaction(msg, vr, ivr));
            TimerStatusArgs args = new TimerStatusArgs("Updating Schedule...", "Schedule Update", 120, 5);
            worker.RunWorkerAsync(new WorkArgs(ACTIVITY.UPDATE_SCHEDULE, args));
        }

        #endregion

        #region ENERGY_METER
        private void ReadMeter_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode node = getNode((MenuItem)sender);
            Module m = node as Module;
            List<String> msg = new List<string>();
            msg.Add("RAEMT@");

            List<String> vr = new List<string>();
            vr.Add("EMT");
           
            List<String> ivr = new List<string>();
            vr.Add("");
            Transactions.Clear();
            Transactions.Add(m.Details.SIM, new Transaction(msg, vr, ivr));
            TimerStatusArgs args = new TimerStatusArgs("Reading Energy Meter Data...", "Energy Meter Data", 120, 5);
            worker.RunWorkerAsync(new WorkArgs(ACTIVITY.READ_ENERGY_METER, args));
        }

        #endregion


        #endregion


        #region InstallationReport
        private void InstallationReport_Click_1(object sender, RoutedEventArgs e)
        {
            CurrentArea = getNode((MenuItem)sender) as GEMSPL.Entity.Area;

            InstallationReportControl irc = new InstallationReportControl();
            irc.GenerateEvent += irc_GenerateEvent;
            irc.CancelEvent += irc_CancelEvent;

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {
                                  Transient.Children.Clear();
                                  Transient.Children.Add(irc);
                              }));

        }

        void irc_CancelEvent(object sender, Reports.InstallationReport.CancelEventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                               new Action(() =>
                               {
                                   Transient.Children.Clear();
                                   CurrentModule = null;
                               }));

        }

        void irc_GenerateEvent(object sender, GEMSPL.Reports.InstallationReport.GenerateEventArgs e)
        {
            //tobe done

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {
                                  Transient.Children.Clear();
                                  CurrentModule = null;
                              }));

        }
        #endregion

        TreeNode getNode(MenuItem m)
        {

            ContextMenu mn = (ContextMenu)m.Parent;
            TextBlock tb = (TextBlock)mn.PlacementTarget;
            TreeNode tn = (TreeNode)tb.DataContext;
            return tn;
        }



        private void DeleteULB_Click_1(object sender, RoutedEventArgs e)
        {
            TreeNode tn = getNode((MenuItem)sender);
            if (MessageBox.Show("Data will lost. Are you sure, you want to delete this node ?", "Application Warning", MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Directory.Delete(tn.Path, true);
                MessageBox.Show("Node Deleted", "Application Info",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                GetULBs();
            }
        }



        private void Calibration_Click_1(object sender, RoutedEventArgs e)
        {
            CurrentModule = getNode((MenuItem)sender) as GEMSPL.Entity.Module;

            CalibrationControl cc = new CalibrationControl();

            cc.SaveEvent += cc_SaveEvent;
            cc.CancelEvent += cc_CancelEvent;
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {
                                  Transient.Children.Clear();
                                  Transient.Children.Add(cc);
                              }));


        }

        void cc_CancelEvent(object sender, Calibration.CancelEventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {
                                  Transient.Children.Clear();
                                  CurrentModule = null;
                              }));
        }

        void cc_SaveEvent(object sender, Calibration.SaveEventArgs e)
        {
            //tobe done

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {
                                  Transient.Children.Clear();
                                  CurrentModule = null;
                              }));
        }

        #region SavingsReport_Module

        private void SavingsReport_Click_1(object sender, RoutedEventArgs e)
        {
            CurrentModule = getNode((MenuItem)sender) as GEMSPL.Entity.Module;
            SavingsReportControl src = new SavingsReportControl();
            src.GenerateEvent += src_GenerateEvent;
            src.CancelEvent += src_CancelEvent;

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                            new Action(() =>
                            {
                                Transient.Children.Clear();
                                Transient.Children.Add(src);
                            }));

        }

        void src_CancelEvent(object sender, Reports.SavingsReport.CancelEventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                             new Action(() =>
                             {
                                 Transient.Children.Clear();
                                 CurrentModule = null;
                             }));
        }

        void src_GenerateEvent(object sender, Reports.SavingsReport.GenerateEventArgs e)
        {
            //tobe done

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                              new Action(() =>
                              {
                                  Transient.Children.Clear();
                                  CurrentModule = null;
                              }));
        }

        #endregion
    }

    

}
