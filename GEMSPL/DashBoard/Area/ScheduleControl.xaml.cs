
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

namespace GEMSPL.DashBoard.Area
{
    /// <summary>
    /// Interaction logic for ScheduleControl.xaml
    /// </summary>
    public partial class ScheduleControl : UserControl
    {
        public event EventHandler<SchSaveEventArgs> SaveEvent;
        public event EventHandler<CancelEventArgs> CancelEvent;
        String BasePath = String.Empty;
        Schedule Schedule;
        ScheduleVM ScheduleVM;
        GEMSPL.Entity.Area area;
        public ScheduleControl(String path, GEMSPL.Entity.Area a)
        {
            InitializeComponent();
            BasePath = path;
            area = a;
            ScheduleVM = new ScheduleVM(a.Schedule);
            this.DataContext = ScheduleVM;
        }

        private void Save_Click_1(object sender, RoutedEventArgs e)
        {
            Schedule = ScheduleVM.getSchedule();

            if (SaveEvent != null)
            {
                area.Schedule = Schedule;
                foreach (Module m in area.Modules)
                    m.Schedule = Schedule;
                SaveEvent(this, new SchSaveEventArgs(BasePath, Schedule));
            }
        }

        private void Cancel_Click_1(object sender, RoutedEventArgs e)
        {
            if (CancelEvent != null)
                CancelEvent(this, new CancelEventArgs());

        }
    }

    public class SchSaveEventArgs : EventArgs
    {
        public String Entity { get; set; }
        public Schedule Schedule { get; set; }

        public SchSaveEventArgs(String entity, Schedule sch)
        {
            Entity = entity;
            Schedule = sch;
        }
    }

    
}
