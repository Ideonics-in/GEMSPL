using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ias.devicedriver
{
    public partial class GSM_SMS
    {
        String cmdPrefix = "AT";
        //String cmdSuffix = new String((char)13, 1);
        String cmdSuffix = "\r";

        enum AT_COMMAND
        {
            NONE = 0,
            MESSAGE_MODE,
            SEND_SMS, 
            SMSC_ADDRESS,
            ECHO_OFF,
            READ_MESSAGE,
            DELETE_MESSAGE,
            READ_ALL
        }
        String[] AT_CMD_STR = {
                                  "",
                                  "+CMGF",
                                  "+CMGS",
                                  "+CSCA",
                                  "E0",
                                  "+CMGR",
                                  "+CMGD",
                                  "+CMGL"

    };

       
    }
}
