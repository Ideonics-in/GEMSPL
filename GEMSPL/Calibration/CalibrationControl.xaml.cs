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

namespace GEMSPL.Calibration
{
    /// <summary>
    /// Interaction logic for CalibrationControl.xaml
    /// </summary>
    public partial class CalibrationControl : UserControl
    {
        public event EventHandler<SaveEventArgs> SaveEvent;
        public event EventHandler<CancelEventArgs> CancelEvent;
        public CalibrationControl()
        {
            InitializeComponent();
        }

        private void Save_Click_1(object sender, RoutedEventArgs e)
        {
            if( SaveEvent != null )
            {
                SaveEvent(this, new SaveEventArgs(PrimaryTB.Text,SecondaryTB.Text ));
            }
        }

        private void Cancel_Click_1(object sender, RoutedEventArgs e)
        {
            if (CancelEvent != null)
                CancelEvent(this, new CancelEventArgs());

        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            PrimaryTB.Focus();
        }
    }

    public class SaveEventArgs : EventArgs
    {
        public String PrimaryReading { get; set; }
        public String SecondaryReading { get; set; }

        public SaveEventArgs(String pr, String sr)
        {
            PrimaryReading = pr;
            SecondaryReading = sr;
        }
    }

    public class CancelEventArgs : EventArgs
    {

    }
}
