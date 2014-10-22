using GEMSPL.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GEMSPL.Entity
{
    public class ScheduleVM : ObservableCollection<SlotVM>, INotifyPropertyChanged
    {
        #region INotifyPropetyChangedHandler
        public event PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion


        public ScheduleVM(String scheduleDate, String scheduleT1, String ScheduleT2)
        {
            String[] seperator = { "," };
            String[] dates = scheduleDate.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            String[] t1 = scheduleT1.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            String[] t2 = ScheduleT2.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

            Queue<String> from = new Queue<String>();
            Queue<String> to = new Queue<string>();
            Queue<String> t1On = new Queue<string>();
            Queue<String> t1Off = new Queue<string>();
            Queue<String> t2On = new Queue<string>();
            Queue<String> t2Off = new Queue<string>();


            foreach (String s in dates)
            {
                String temp = s.Substring(0, 4);
                temp = temp.Insert(2, "-");
                temp += "-" + DateTime.Now.Year.ToString();
                from.Enqueue(temp);

                temp = s.Substring(4, 4);
                temp = temp.Insert(2, "-");
                temp += "-" + DateTime.Now.Year.ToString();
                to.Enqueue(temp);


            }
            foreach (String s in t1)
            {
                String temp = s.Substring(0, 4);
                temp = temp.Insert(2, ":");

                t1On.Enqueue(temp);

                temp = s.Substring(4, 4);
                temp = temp.Insert(2, ":");

                t1Off.Enqueue(temp);
            }

            foreach (String s in t2)
            {
                String temp = s.Substring(0, 4);
                temp = temp.Insert(2, ":");

                t2On.Enqueue(temp);

                temp = s.Substring(4, 4);
                temp = temp.Insert(2, ":");

                t2Off.Enqueue(temp);
            }
            int i = 0;
            while (from.Count > 0)
            {
                SlotVM s = new SlotVM(i++, from.Dequeue(), to.Dequeue(), t1On.Dequeue(), t1Off.Dequeue(), t2On.Dequeue(), t2Off.Dequeue(), sl_scheduleChangedEventHandler);
                s.Validate = true;
                this.Add(s);
            }


        }

        public ScheduleVM(Schedule sch)
        {
            foreach (Slot s in sch.Slots)
                this.Add(new SlotVM(s,sl_scheduleChangedEventHandler));
        }



        public String getDate()
        {
            String date = String.Empty;
            IEnumerator item = GetEnumerator();
            int i = 0;
            while (item.MoveNext())
            {
                date += ((SlotVM)item.Current).getDate();
                date += ",";
                i++;

            }
            if (i < 12)
            {
                while (i < 11)
                {
                    date += "00000000,";
                    i++;
                }
                date += "00000000";
            }

            if (date.LastIndexOf(',') == (date.Length - 1))
                date.Remove(date.LastIndexOf(','));
            return date;
        }

        public String getT1()
        {
            String date = String.Empty;
            IEnumerator item = GetEnumerator();
            int i = 0;
            while (item.MoveNext())
            {
                date += ((SlotVM)item.Current).getT1();
                date += ",";
                i++;

            }
            if (i < 12)
            {
                while (i < 11)
                {
                    date += "00000000,";
                    i++;
                }
                date += "00000000";
            }

            if (date.LastIndexOf(',') == (date.Length - 1))
                date.Remove(date.LastIndexOf(','));
            return date;
        }


        public String getT2()
        {
            String date = String.Empty;
            IEnumerator item = GetEnumerator();
            int i = 0;
            while (item.MoveNext())
            {
                date += ((SlotVM)item.Current).getT2();
                date += ",";
                i++;

            }
            if (i < 12)
            {
                while (i < 11)
                {
                    date += "00000000,";
                    i++;
                }
                date += "00000000";
            }

            if (date.LastIndexOf(',') == (date.Length - 1))
                date.Remove(date.LastIndexOf(','));
            return date;
        }



        void sl_scheduleChangedEventHandler(object sender, ScheduleChanged e)
        {
            SlotVM sl = (SlotVM)sender;

            DateTime scheduleEnd = new DateTime(DateTime.Now.Year, 12, 31);


            if (e.To < scheduleEnd)
            {
                reschedule(e.SlotID);
                CultureInfo cli = new CultureInfo("en-NZ");
                cli.DateTimeFormat.DateSeparator = "-";
                cli.DateTimeFormat.ShortDatePattern = @"dd-MM-yyyy";

                this.Add(new SlotVM(e.SlotID + 1, e.To.AddDays(1).ToString("dd-MM-yyyy", cli), scheduleEnd.ToString("dd-MM-yyyy", cli),
                    "18:00:00", "06:00:00", "22:00:00", "04:00:00", sl_scheduleChangedEventHandler));





            }
            else if (e.To > scheduleEnd)
            {
                sl.To = scheduleEnd.ToString("dd-MM-yyyy");
                MessageBox.Show("Schdule should be within the current year", "Schedule Info", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            else
            {
                reschedule(e.SlotID);
            }

        }
        private void reschedule(int id)
        {
            List<SlotVM> newSlots = new List<SlotVM>();
            DateTime scheduleEnd = new DateTime(DateTime.Now.Year, 12, 31);
            int count = Items.Count;
            for (int i = id + 1; i < count; count--)
            {
                this.RemoveAt(i);

            }

        }


        public    Schedule getSchedule()
        {
            List<Slot> slots = new List<Slot>();
            
            foreach( SlotVM svm in this )
            {
                slots.Add(svm.getSlot());
            }
            Schedule sch = new Schedule(slots);
            return sch;
        }
    }

    public class ScheduleChanged : EventArgs
    {
        public DateTime To;
        public int SlotID;
        public ScheduleChanged(DateTime to, int id)
        {
            SlotID = id;
            To = to;
        }
    }
}
