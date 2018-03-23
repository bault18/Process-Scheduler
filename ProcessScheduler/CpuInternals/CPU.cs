using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessScheduler
{
    class CPU
    {
        #region Member Variables
        private static int contextSwitchTime = 1;
        private int numContextSwitch;
        private List<Process> processes;

        public Dispatcher dispatcher;
        #endregion

        public CPU()
        {
            Dispatcher dispatcher = new Fcfs(processes);

        }
    }
}
