using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessScheduler
{
    class Fcfs : Dispatcher
    {
        #region Member Variables
        public Queue<Process> scheduleQueue;
        #endregion

        #region Constructors
        public Fcfs(List<Process> processes)
        { 
            scheduleQueue = new Queue<Process>();

            foreach (Process proc in processes)
                arrivalQueue.Enqueue(proc);

            arrivalQueue = new Queue<Process>(arrivalQueue.OrderBy(p => p.arrivalTime));
        }
        #endregion

        //Check if new processes need to go into scheduling queue
        public override void addNewProcess()
        {
            while(arrivalQueue.Count > 0) //Do not run if no procs to queue
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

        public void AddProcess(Process process)
        {
            scheduleQueue.Enqueue(process);
        }

        //take top and throw in processor
        public override void swapProcesses()
        {
            if (scheduleQueue.Count > 0)
            {
                currProcess = scheduleQueue.Dequeue(); //Fetch process from front of dispatch queue

                //Set response time
                if (currProcess.responseTime < 0)
                    currProcess.responseTime = CPUTime;

                int burstTime = currProcess.remainingEvents.First.Value;  //Get current CPU burst time
                currProcess.remainingEvents.RemoveFirst();

                //Update CPU clock, update data collection
                CPUTime += burstTime;
                currProcess.totalProcessingTime += burstTime;

                //check if I/O event occurs
                if (currProcess.remainingEvents.Count() > 0) //IO event occurs
                {
                    blockProcess(currProcess);
                }
                else //Process is comlete
                {
                    currProcess.completedTime = CPUTime;
                    completedProcesses.Add(currProcess);
                }

                contextSwitch();
                checkBlockedQueue();
            }
            else
            {
                CPUTime++;
            }
        }

        public override void checkBlockedQueue()
        {
            for(int proc = 0; proc < blockedQueue.Count(); proc++)
            {
                if(blockedQueue[proc].blockExitTime <= CPUTime)
                {
                    scheduleQueue.Enqueue(blockedQueue[proc]);  //Put process back into scheduler
                    blockedQueue.RemoveAt(proc);                //Remove process from blocked queue
                    proc--;                                     //Prevent memory access errors
                }
            }
        }

        public int Count()
        {
            return scheduleQueue.Count;
        }
    }
}
