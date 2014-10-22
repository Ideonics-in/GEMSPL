using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GEMSPL.Entity
{
    [Serializable]
    
    public class PortSettings
    {
        [XmlElement]
        public String ComPort { get; set; }
        [XmlElement]
        public String BaudRate { get; set; }
        [XmlElement]
        public String HandShake { get; set; }

        public PortSettings()
        {
            ComPort = String.Empty;
            BaudRate = String.Empty;
            HandShake = String.Empty;
        }

        public PortSettings(String com, String baud, String handshake)
        {
            ComPort = com;
            BaudRate = baud;
            HandShake = handshake;
        }
    }
}
