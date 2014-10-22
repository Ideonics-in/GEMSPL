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

namespace GEMSPL.DashBoard.TimerStatus
{
    /// <summary>
    /// Interaction logic for TimerStatus.xaml
    /// </summary>
    public partial class TimerStatusView : UserControl
    {
        public event EventHandler<CancelEventArgs> CancelEvent;
        public String TagContent { get; set; }
        public TimerStatusView(String tag,String status)
        {
            InitializeComponent();
            TagContent = tag;
            StatusLabel.Content = status;

            switch (status)
            {
                case "ON":
                    StatusLabel.Background = Brushes.Red;
                    break;

                case "OFF":
                    StatusLabel.Background = Brushes.Green;
                    break;
                default:
                    StatusLabel.Background = Brushes.Gray;
                    break;
            }

            DataContext = this;
        }

        private void CancelButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (CancelEvent != null)
                CancelEvent(this, new CancelEventArgs());
        }
    }

    public class CancelEventArgs : EventArgs
    {

    }
}
