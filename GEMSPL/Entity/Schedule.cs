using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GEMSPL.Entity
{
    [Serializable]
    public class Schedule
    {
        public List<Slot> Slots { get; set; }

        public Schedule()
        {
            Slots = new List<Slot>();
            
        }


        public Schedule(List<Slot> slots)
        {
            Slots = slots;
        }

     

    }
}
