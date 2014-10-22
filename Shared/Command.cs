using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Shared
{
    public abstract class Command
    {
        public enum Result { FAILURE = 0 , SUCCESS = 1, TIMEOUT = 2 , ERROR = 3 };

        Timer  Timer;

        public Command(double timeout)
        {
            Timer = new Timer(timeout);
            Timer.Elapsed += Timer_Elapsed;
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
        }

        public abstract Result Execute();

        public abstract void Abort();
    }
}
