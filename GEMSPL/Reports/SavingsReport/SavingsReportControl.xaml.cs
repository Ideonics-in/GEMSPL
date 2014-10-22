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

namespace GEMSPL.Reports.SavingsReport
{
    /// <summary>
    /// Interaction logic for SavingsReportControl.xaml
    /// </summary>
    public partial class SavingsReportControl : UserControl
    {
        public event EventHandler<GenerateEventArgs> GenerateEvent;
        public event EventHandler<CancelEventArgs> CancelEvent;
        public SavingsReportControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {

        }

        private void Generate_Click_1(object sender, RoutedEventArgs e)
        {
            if (GenerateEvent != null)
            {
                SavingsParameters parameters = new SavingsParameters(FromDP.SelectedDate.Value,
                    ToDP.SelectedDate.Value);
                    
                GenerateEvent(this, new GenerateEventArgs(parameters));
            }
        }

        private void Cancel_Click_1(object sender, RoutedEventArgs e)
        {
            if (CancelEvent != null)
                CancelEvent(this, new CancelEventArgs());
        }
    }
    public class GenerateEventArgs : EventArgs
    {
        public SavingsParameters Parameters { get; set; }

        public GenerateEventArgs(SavingsParameters p)
        {
            Parameters = p;
        }
    }

    public class CancelEventArgs : EventArgs
    {

    }

    public class SavingsParameters
    {
        
        public DateTime From { get; set; }
        public DateTime To {get;set;}


        public SavingsParameters( DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }


    }
}
