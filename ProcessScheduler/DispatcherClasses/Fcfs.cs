using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessScheduler
{
    class Fcfs : Dispatcher
    {
        Queue<Process> scheduleQueue;

        public Fcfs(List<Process> processes)
        { 
            scheduleQueue = new Queue<Process>();

            foreach (Process proc in processes)
            {
                arrivalQueue.Enqueue(proc);
            }

            arrivalQueue.OrderBy<Process, int>(p => p.arrivalTime);
        }

        //Check if new processes need to go into scheduling queue
        void addNewProcess()
        {
            foreach (Process proc in arrivalQueue)
            {
                if (proc.arrivalTime <= CPUTime)
                    scheduleQueue.Enqueue(proc);
            }
        }


        //take top and throw in processor
        void processing()
        {
            currProcess = scheduleQueue.Dequeue(); //Get process to go in processor

            //set response time
            if (currProcess.responseTime < 0)
                currProcess.responseTime = CPUTime;

            int burstTime = currProcess.remainingEvents.Dequeue();  //Get current CPU burst time

            //Update CPU clock, update data collection
            CPUTime += burstTime;
            currProcess.totalProcessingTime += burstTime;

            //check if I/O event occurs
            if (currProcess.remainingEvents.Count() > 0)
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
    }
}
