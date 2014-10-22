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
using System.Timers;


namespace GEMSPL.DashBoard
{
    /// <summary>
    /// Interaction logic for DashBoardView.xaml
    /// </summary>

    public partial class DashBoardView : UserControl
    {
        Module CurrentModule;
        GEMSPL.Entity.Area CurrentArea;
        
        PortSettings PortSettings;

        BackgroundWorker worker;

        public Boolean Admin = false;

      

        public  DashBoardView(String baseDir, Users users,String currentUser, PortSettings Settings)
        {
            InitializeComponent();
            BaseDir = baseDir;
            CurrentUser = currentUser;
            Users = users;

            PortSettings = Settings;

            GetULBs();

            extendConstructor();


            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerAsync(new WorkArgs(ACTIVITY.INITIALIZE,null));


           

            
            
           
        }

        partial void  extendConstructor();

        



        private void UserControl_Unloaded_1(object sender, RoutedEventArgs e)
        {
            

        }

     

        

      
       

        
  

       

       

       
    }
}
