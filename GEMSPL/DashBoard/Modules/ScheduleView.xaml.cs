using GEMSPL.Entity;
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
    /// Interaction logic for ScheduleView.xaml
    /// </summary>
    public partial class ScheduleView : UserControl
    {
        public event EventHandler<CancelEventArgs> CancelEvent;
       
        Schedule Schedule;
        ScheduleVM ScheduleVM;
        public ScheduleView(String date, String t1, String t2)
        {
            InitializeComponent();
            ScheduleVM = new ScheduleVM(date,t1,t2);
            this.DataContext = ScheduleVM;
        }

        private void Cancel_Click_1(object sender, RoutedEventArgs e)
        {
            if (CancelEvent != null)
                CancelEvent(this, new CancelEventArgs());

        }
    }
}
