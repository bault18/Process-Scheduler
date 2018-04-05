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
        private List<Process> processes;

        public Dispatcher dispatcher;
        #endregion

        public CPU(List<Process> procs)
        {
            processes = procs;
            Dispatcher dispatcher = new Fcfs(processes);

            dispatcher.run();

            Dispatcher LoadShare = new LoadSharing(processes);
            LoadShare.run();
            Console.WriteLine("Done");
        }
    }
}
