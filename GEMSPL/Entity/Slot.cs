using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace  GEMSPL.Entity
{
    [Serializable]
    public class Slot
    {
        [XmlIgnore]
        public static Slot DefaultSlot
        {
            get
            {
                return new Slot("01-01-2014", "31-12-2014", "18:00", "06:00", "20:00", "05:00");


            }
        }
        public int id { get; set; }
        public DateTime FromTs { get; set; }
        public DateTime ToTs { get; set; }

        [XmlIgnore]
        public TimeSpan T1On { get; set; }


        [Browsable(false)]
        [XmlElement(DataType = "duration", ElementName = "T1On")]
        public string T1ONString
        {
            get
            {
                return XmlConvert.ToString(T1On);
            }
            set
            {
                T1On = string.IsNullOrEmpty(value) ?
                    TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
            }
        }

        [XmlIgnore]
        public TimeSpan T1Off { get; set; }

        [Browsable(false)]
        [XmlElement(DataType = "duration", ElementName = "T1Off")]
        public string T1OFFString
        {
            get
            {
                return XmlConvert.ToString(T1Off);
            }
            set
            {
                T1Off = string.IsNullOrEmpty(value) ?
                    TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
            }
        }


        [XmlIgnore]
        public TimeSpan T2On { get; set; }

        [Browsable(false)]
        [XmlElement(DataType = "duration", ElementName = "T2On")]
        public string T2ONString
        {
            get
            {
                return XmlConvert.ToString(T2On);
            }
            set
            {
                T2On = string.IsNullOrEmpty(value) ?
                    TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
            }
        }


        [XmlIgnore]
        public TimeSpan T2Off { get; set; }


        [Browsable(false)]
        [XmlElement(DataType = "duration", ElementName = "T2Off")]
        public string T2OffString
        {
            get
            {
                return XmlConvert.ToString(T2Off);
            }
            set
            {
                T2Off = string.IsNullOrEmpty(value) ?
                    TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
            }
        }

        public Slot()
        {
        }

        public Slot(String from, String to, String t1on, String t1off, String t2on, String t2off)
        {
            CultureInfo cli = new CultureInfo("en-NZ");
            cli.DateTimeFormat.DateSeparator = "-";
            cli.DateTimeFormat.ShortDatePattern = @"dd-MM-yyyy";

            FromTs = DateTime.ParseExact(from, "dd-MM-yyyy", CultureInfo.CurrentCulture);
            ToTs = DateTime.ParseExact(to, "dd-MM-yyyy", CultureInfo.CurrentCulture);

            T1On = TimeSpan.Parse(t1on);
            T1Off = TimeSpan.Parse(t1off);
            T2On = TimeSpan.Parse(t2on);
            T2Off = TimeSpan.Parse(t2off);
        }


    }
}
