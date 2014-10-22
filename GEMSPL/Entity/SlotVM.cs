using GEMSPL.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GEMSPL.Entity
{
    public class SlotVM : INotifyPropertyChanged, IEditableObject
    {
        public event EventHandler<ScheduleChanged> scheduleChangedEventHandler;
        public int ID { get; set; }

        public DateTime _from;
        public String From
        {
            get { return _from.ToString("dd-MM-yyyy"); }
            set
            {
                try
                {
                    CultureInfo cli = new CultureInfo("en-NZ");
                    cli.DateTimeFormat.DateSeparator = "-";
                    cli.DateTimeFormat.ShortDatePattern = @"dd-MM-yyyy";
                    _from = DateTime.ParseExact(value, "dd-MM-yyyy", cli);

                    OnPropertyChanged("From");
                }
                catch (System.ArgumentNullException)
                {
                    MessageBox.Show("Error in From Date ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.FormatException)
                {
                    MessageBox.Show("Error in  From Date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }


        public DateTime _to;
        public String To
        {
            get { return _to.ToString("dd-MM-yyyy"); }
            set
            {
                try
                {
                    CultureInfo cli = new CultureInfo("en-NZ");
                    cli.DateTimeFormat.DateSeparator = "-";
                    cli.DateTimeFormat.ShortDatePattern = @"dd-MM-yyyy";
                    DateTime temp = DateTime.ParseExact(value, "dd-MM-yyyy", cli);
                    if (temp == _to) return;
                    if (temp <= _from)
                    {
                        MessageBox.Show("To date cannot be lesser than From date ", "Schedule Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _to = temp;
                        OnPropertyChanged("To");
                        if (scheduleChangedEventHandler != null)
                        {
                            scheduleChangedEventHandler(this, new ScheduleChanged(_to, this.ID));
                        }
                    }
                }
                catch (System.ArgumentNullException)
                {
                    MessageBox.Show("Error in  To Date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.FormatException)
                {
                    MessageBox.Show("Error in To Date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        public TimeSpan _t1on;
        public String T1On
        {
            get { return string.Format("{0:00}:{1:00}", _t1on.Hours, _t1on.Minutes); }
            set
            {
                try
                {
                    if (Validate)
                    {
                        TimeSpan temp = TimeSpan.Parse(value);
                        if (temp >= _t2on)
                        {
                            MessageBox.Show("T1 On time should be less than T2 On ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    _t1on = TimeSpan.Parse(value);
                    OnPropertyChanged("T1On");


                }
                catch (System.ArgumentNullException)
                {
                    MessageBox.Show("Error in Input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.FormatException)
                {
                    MessageBox.Show("Error in Input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.OverflowException)
                {
                    MessageBox.Show("Error in Input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        public TimeSpan _t1off;
        public String T1Off
        {
            get { return string.Format("{0:00}:{1:00}", _t1off.Hours, _t1off.Minutes); }
            set
            {
                try
                {
                    if (Validate)
                    {
                        TimeSpan temp = TimeSpan.Parse(value);
                        if (temp <= _t2off)
                        {
                            MessageBox.Show("T1 Off time should be greater than T2 Off ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    _t1off = TimeSpan.Parse(value);
                    OnPropertyChanged("T1Off");
                }

                catch (System.ArgumentNullException)
                {
                    MessageBox.Show("Error in Input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.FormatException)
                {
                    MessageBox.Show("Error in Input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.OverflowException)
                {
                    MessageBox.Show("Error in Input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public TimeSpan _t2on;
        public String T2On
        {
            get { return string.Format("{0:00}:{1:00}", _t2on.Hours, _t2on.Minutes); }
            set
            {
                try
                {
                    if (Validate)
                    {
                        TimeSpan temp = TimeSpan.Parse(value);
                        if (temp <= _t1on)
                        {
                            MessageBox.Show("T2 On time should be greater than T1 On ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    _t2on = TimeSpan.Parse(value);
                    OnPropertyChanged("T2On");
                }

                catch (System.ArgumentNullException)
                {
                    MessageBox.Show("Error in Input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.FormatException)
                {
                    MessageBox.Show("Error in Input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.OverflowException)
                {
                    MessageBox.Show("Error in Input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public TimeSpan _t2off;
        public String T2Off
        {
            get { return string.Format("{0:00}:{1:00}", _t2off.Hours, _t2off.Minutes); }
            set
            {
                try
                {
                    if (Validate)
                    {
                        TimeSpan temp = TimeSpan.Parse(value);
                        if (temp >= _t1off)
                        {
                            MessageBox.Show("T2 Off time should be less than T1 Off ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    _t2off = TimeSpan.Parse(value);
                    OnPropertyChanged("T2Off");
                }
                catch (System.ArgumentNullException)
                {
                    MessageBox.Show("Error in Input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.FormatException)
                {
                    MessageBox.Show("Error in Input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.OverflowException)
                {
                    MessageBox.Show("Error in Input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public bool Validate = false;

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

        #region IEditableObjectMembers
        void IEditableObject.BeginEdit()
        {
        }
        void IEditableObject.CancelEdit()
        {
        }
        void IEditableObject.EndEdit()
        {
        }
        #endregion

        public SlotVM(int id, String from, String to, String t1on, String t1off, String t2on, String t2off)
        {

            ID = id;
            this.From = from;
            this.To = to;
            this.T1On = t1on;
            this.T1Off = t1off;
            this.T2On = t2on;
            this.T2Off = t2off;
            Validate = true;
        }

        public SlotVM(int id, String from, String to, String t1on, String t1off, String t2on, String t2off, EventHandler<ScheduleChanged> eventHandler)
        {
            ID = id;
            this.From = from;
            this.To = to;
            this.T1On = t1on;
            this.T1Off = t1off;
            this.T2On = t2on;
            this.T2Off = t2off;
            this.scheduleChangedEventHandler = eventHandler;
            Validate = true;
        }

        public SlotVM(Slot s, EventHandler<ScheduleChanged> eventHandler)
        {
            ID = s.id;
            _from = s.FromTs;
            _to = s.ToTs;
            _t1on = s.T1On;
            _t1off = s.T1Off;
            _t2on = s.T2On;
            _t2off = s.T2Off;
            this.scheduleChangedEventHandler = eventHandler;
          
        }


        public String getDate()
        {
            String date = _from.Day.ToString("D2") + _from.Month.ToString("D2") + _to.Day.ToString("D2") + _to.Month.ToString("D2");
            return date;
        }

        public String getT1()
        {
            String t1 = _t1on.Hours.ToString("D2") + _t1on.Minutes.ToString("D2") + _t1off.Hours.ToString("D2") + _t1off.Minutes.ToString("D2");
            return t1;
        }

        public String getT2()
        {
            String t2 = _t2on.Hours.ToString("D2") + _t2on.Minutes.ToString("D2") + _t2off.Hours.ToString("D2") + _t2off.Minutes.ToString("D2");
            return t2;
        }



        public Slot getSlot()
        {
            Slot s = new Slot();
            s.id = ID;
            s.FromTs = _from;
            s.ToTs = _to;
            s.T1On = _t1on;
            s.T1Off = _t1off;
            s.T2On = _t2on;
            s.T2Off = _t2off;
            return s;

        }


        
    }
}
