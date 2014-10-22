using GEMSPL.Clock;
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

namespace GEMSPL.DashBoard.Modules
{
    /// <summary>
    /// Interaction logic for ModuleClockView.xaml
    /// </summary>
    public partial class ModuleClockView : UserControl
    {
        public event EventHandler<CancelEventArgs> CancelEvent;
        public ModuleClockView( String timestamp)
        {
            InitializeComponent();
            ModuleClockGrid.Children.Add(new ClockDisplay(timestamp));
        }
        private void CancelButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (CancelEvent != null)
                CancelEvent(this, new CancelEventArgs());
        }

    }
  
}
