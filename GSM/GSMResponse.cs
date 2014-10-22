using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSM
{


    public class GSMResponse
    {

        public GSM_COMMAND Command { get; set; }
        public Object Response { get; set; }
        public bool Timedout{get;set;}

        public GSMResponse(GSM_COMMAND cmd)
        {
            Command = cmd;
        }
    }
}
