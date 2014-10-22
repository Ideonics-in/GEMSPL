using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSM
{
    public enum GSM_COMMAND { PING = 0, ECHO_OFF = 1, SMS_CENTER_ADDRESS = 2, SET_SMS_MODE = 3, SEND_SMS = 4, READ_SMS = 5, DELETE_SMS = 6 };

    public  class GSMCommand
    {
        public GSM_COMMAND Command { get; set; }

        public List<String> Parameters { get; set; }

        public double Timeout { get; set; }

        public GSMCommand(GSM_COMMAND cmd, List<String> parameters, double timeout)
        {
            Command = cmd;
            Parameters = parameters;
            Timeout = timeout;
        }

    }
}
