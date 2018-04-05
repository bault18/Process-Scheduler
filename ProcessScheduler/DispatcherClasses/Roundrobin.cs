using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessScheduler.DispatcherClasses
{
    class Roundrobin : Dispatcher
    {
        int quantumTime = 5; 
        Queue<Process> scheduleQueue;
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
                currProcess = scheduleQueue.Dequeue(); //Fetch process from front of dispatch queue

                //Set response time
                if (currProcess.responseTime < 0)
                    currProcess.responseTime = CPUTime;

                int burstTime = currProcess.remainingEvents.Dequeue();  //Get current CPU burst time

                if (burstTime > quantumTime)
                {
                    burstTime -= quantumTime;
                    CPUTime += quantumTime;
                    currProcess.totalProcessingTime += quantumTime;
                    scheduleQueue.Enqueue(currProcess); 
                }
                else
                {
                    CPUTime += burstTime;
                    currProcess.totalProcessingTime += burstTime;
                    if (currProcess.remainingEvents.Count() > 0)
                    {
                        blockProcess(currProcess); 
                    }
                    else
                    {
                        currProcess.completedTime = CPUTime;
                        completedProcesses.Add(currProcess); 
                    }


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

        
    }
}


