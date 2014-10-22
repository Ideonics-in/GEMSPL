using System;
using System.Collections.Generic;
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

namespace GEMSPL.Reports.InstallationReport
{
    /// <summary>
    /// Interaction logic for InstallationReportControl.xaml
    /// </summary>
    public partial class InstallationReportControl : UserControl
    {
        public event EventHandler<GenerateEventArgs> GenerateEvent;
        public event EventHandler<CancelEventArgs> CancelEvent;
        String BasePath = String.Empty;
        public InstallationReportControl()
        {
            InitializeComponent();
        }

        private void Generate_Click_1(object sender, RoutedEventArgs e)
        {
            if (GenerateEvent != null)
            {
                InstallationParameters parameters = new InstallationParameters(CapacityTB.Text,
                    InstallationDP.SelectedDate.Value, ConsumptionTB.Text, TimeSpan.Parse(OnTimeTB.Text),
                    TimeSpan.Parse(OffTimeTB.Text), Convert.ToInt32(InitialReadingTB.Text));
                GenerateEvent(this, new GenerateEventArgs(parameters));
            }
        }

        private void Cancel_Click_1(object sender, RoutedEventArgs e)
        {
            if (CancelEvent != null)
                CancelEvent(this, new CancelEventArgs());
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            CapacityTB.Focus();
        }
    }
    public class GenerateEventArgs : EventArgs
    {
        public InstallationParameters Parameters{ get; set; }

        public GenerateEventArgs(InstallationParameters p)
        {
            Parameters = p;
        }
    }

    public class CancelEventArgs : EventArgs
    {

    }

    public class InstallationParameters
    {
        public String Capacity { get; set; }
        public DateTime InstallationDate { get; set; }
        public String StandardConsumption { get; set; }
        public TimeSpan Ontime { get; set; }
        public TimeSpan Offtime { get; set; }
        public int InitialReading { get; set; }


        public InstallationParameters(String capacity, DateTime date, String stdCon, TimeSpan on, TimeSpan off, int iReading)
        {
            Capacity = capacity;
            InstallationDate = date;
            StandardConsumption = stdCon;
            Ontime = on;
            Offtime = off;
            InitialReading = iReading;
        }


    }
}
