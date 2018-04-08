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

        private List<Proccessor> cores;

        private static int numCores;
        
        private int nextOpenCoreTime;
        private int nextOpenCore;
        private int numCoresUsed;
        #endregion

        #region Constructor
        public LoadSharing(List<Process> processes)
        {
            Name = "LoadSharing";
            numCores = 4;
            scheduleQueue = new Queue<Process>();
            cores = new List<Proccessor>();


            for(int core = 0; core < numCores; core++)
            {
                Proccessor newCore = new Proccessor();
                newCore.endTime = -1;
                newCore.CoreID = core;
                newCore.free = true;

                cores.Add(newCore);
            }
            
            nextOpenCoreTime = 0;
            nextOpenCore = 0;

            foreach (Process proc in processes)
                arrivalQueue.Enqueue(proc);

            arrivalQueue = new Queue<Process>(arrivalQueue.OrderBy(p => p.arrivalTime));
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
            if (scheduleQueue.Count > 0 || nextOpenCoreTime <= CPUTime) //Need to 'OR' to utilize all cores at start of scheduling
            {
                for(int core = 0; core < numCores; core++)
                {
                    //If proc completed & core hasn't been released
                    if (cores[core].endTime <= CPUTime && !cores[core].free)
                        releaseCore(core);
                    
                    //If core free, add proc
                    if (cores[core].free && scheduleQueue.Count > 0)
                        addCoreProc(core);
                }

                findNextOpenCore();
                checkBlockedQueue();
            }
            numCoresUse();
            updateClock();
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

        public void releaseCore(int core)
        {
            //check for IO as we remove process
            Proccessor currCore = cores[core];
            if (currCore.currProc != null)  //Issue where first proc added causes error to occur
            {
                if (currCore.currProc.remainingEvents.Count() > 0)
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

            currCore.free = true;

            cores[core] = currCore;
        }

        public void addCoreProc(int core)
        {
            //Get process
            currProcess = scheduleQueue.Dequeue();

            //Update process statistics
            if (currProcess.responseTime < 0)
                currProcess.responseTime = CPUTime;

            //Add process to core
            Proccessor currCore = cores[core];
            currCore.currProc = currProcess;

            //Setup end time for CPU
            int burstTime = currProcess.remainingEvents.First.Value;
            currProcess.remainingEvents.RemoveFirst(); //And remove the first node from the queue
            currProcess.completedEvents.Add(burstTime);

            currCore.endTime = CPUTime + burstTime;
            currCore.currProc.totalProcessingTime += burstTime;

            //Assign process
            currCore.currProc = currProcess;
            currCore.free = false;

            //Update core list with new core values
            cores[core] = currCore;

            if (core == nextOpenCore)
                nextOpenCoreTime = currCore.endTime;
        }


        
        public void findNextOpenCore()
        {
            foreach (Proccessor core in cores)
            {
                if (core.endTime < cores[nextOpenCore].endTime)
                {
                    nextOpenCoreTime = core.endTime;
                    nextOpenCore = core.CoreID;
                }
            }
        }

        public void updateClock()
        {
            bool allUtilized = true;
            foreach(Proccessor core in cores)
            {
                if(core.free)
                {
                    allUtilized = false;
                    break;
                }
            }

            if (allUtilized)
                CPUTime = nextOpenCoreTime;
            else
                CPUTime++;
        }

        public void numCoresUse()           //TODO: Remove
        {
            int num = 0;
            foreach(Proccessor core in cores)
            {
                if (!core.free)
                    num++;
            }
            numCoresUsed = num;
        }

    }
    


    public struct Proccessor
    {
        public Process currProc;
        public int endTime;
        public int CoreID;
        public bool free;
    }
}
