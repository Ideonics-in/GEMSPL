using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSM
{
    public partial class Messenger
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
                                  "E0",
                                  "+CSCA",
                                  "+CMGF",
                                  "+CMGS",
                                  "+CMGR",
                                  "+CMGD",
                                  "+CMGL"

    };
    }
}
