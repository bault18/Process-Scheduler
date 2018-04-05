using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessScheduler
{
    class LoadSharing : Dispatcher
    {
        #region Member Variables
        private Queue<Process> scheduleQueue;

        List<Proccessor> cores;

        private int nextOpenCore;
        #endregion

        #region Constructor
        public LoadSharing(List<Process> processes)
        {
            scheduleQueue = new Queue<Process>();
            cores = new List<Proccessor>();

            for(int core = 0; core < 4; core++)
            {
                Proccessor newCore = new Proccessor();
                newCore.endTime = -1;
                newCore.CoreID = core;

                cores.Add(newCore);
            }

            nextOpenCore = 0;

            foreach (Process proc in processes)
                arrivalQueue.Enqueue(proc);

            arrivalQueue.OrderBy<Process, int>(p => p.arrivalTime);
        }

        #endregion





        public override void addNewProcess()
        {
            while (arrivalQueue.Count > 0) //Do not run if no procs to queue
            {
                Process proc = arrivalQueue.Peek(); //Look at top item in queue without removing from queue

                if (proc.arrivalTime <= CPUTime)    //If proc arrived
                {
                    scheduleQueue.Enqueue(proc);
                    arrivalQueue.Dequeue();
                }
                else        //No procs left to arrive
                    break;
            }
        }

        public override void swapProcesses()
        {
            if (scheduleQueue.Count > 0)
            {
                currProcess = scheduleQueue.Dequeue();

                //Set response time
                if (currProcess.responseTime < 0)
                    currProcess.responseTime = CPUTime;

                int burstTime = currProcess.remainingEvents.First.Value;  //Get current CPU burst time
                currProcess.remainingEvents.RemoveFirst(); //And remove the first node from the queue

                removeCoreProc(nextOpenCore);
                addCoreProc(nextOpenCore, burstTime);
                findNextOpenCore();

                checkBlockedQueue();
            }
            else
                CPUTime++;
        }

        public override void checkBlockedQueue()
        {
            for (int proc = 0; proc < blockedQueue.Count(); proc++)
            {
                if (blockedQueue[proc].blockExitTime <= CPUTime)
                {
                    scheduleQueue.Enqueue(blockedQueue[proc]);  //Put process back into scheduler
                    blockedQueue.RemoveAt(proc);                //Remove process from blocked queue
                    proc--;                                     //Prevent memory access errors
                }
            }
        }

        public void removeCoreProc(int core)
        {
            //check for IO as we remove process
            Proccessor currCore = cores[core];
            if(currCore.currProc.remainingEvents.Count() > 0)
            {
                blockProcess(currCore.currProc);
            }
            else
            {
                currCore.currProc.completedTime = CPUTime;
                completedProcesses.Add(currCore.currProc);
            }

            contextSwitch();
        }

        public void addCoreProc(int core, int burstTime)
        {
            Proccessor currCore = cores[core];
            currCore.endTime= CPUTime + burstTime;
            currCore.currProc.totalProcessingTime += burstTime;

            currCore.currProc = currProcess;
        }

        public void findNextOpenCore()
        {
            foreach(Proccessor core in cores)
            {
                if (core.endTime < cores[nextOpenCore].endTime)
                    nextOpenCore = core.CoreID;
            }
        }
    }
    


    public struct Proccessor
    {
        public Process currProc;
        public int endTime;
        public int CoreID;
    }
}
