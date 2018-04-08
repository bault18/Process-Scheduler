using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessScheduler
{
    class Roundrobin : Dispatcher
    {
        public string Name = "RoundRobin";
        int quantumTime; 
		
        public Queue<Process> scheduleQueue;

        public Roundrobin(List<Process> processes)
        {
            quantumTime = 5;

            scheduleQueue = new Queue<Process>();

            foreach (Process proc in processes)
                arrivalQueue.Enqueue(proc);

            arrivalQueue = new Queue<Process>(arrivalQueue.OrderBy(p => p.arrivalTime));
        }

        public Roundrobin(List<Process> processes, int quantum)
        {
            quantumTime = quantum;

            scheduleQueue = new Queue<Process>();

            foreach (Process proc in processes)
                arrivalQueue.Enqueue(proc);

            arrivalQueue = new Queue<Process>(arrivalQueue.OrderBy(p => p.arrivalTime));
        }



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

		public void AddProcess(Process process) {
			scheduleQueue.Enqueue(process);
		}

        public override void swapProcesses()
        {
            if (scheduleQueue.Count > 0)
            {
                currProcess = scheduleQueue.Dequeue(); //Fetch process from front of dispatch queue

                //Set response time
                if (currProcess.responseTime < 0)
                    currProcess.responseTime = CPUTime;

                int burstTime = currProcess.remainingEvents.First.Value;  //Get current CPU burst time


                if (burstTime > quantumTime)
                {
                    burstTime -= quantumTime;
                    currProcess.remainingEvents.First.Value = burstTime;
                    CPUTime += quantumTime;
                    currProcess.totalProcessingTime += quantumTime;
                    scheduleQueue.Enqueue(currProcess); 
                }
                else
                {
                    currProcess.remainingEvents.RemoveFirst(); 
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

		public int Count()
		{
			return scheduleQueue.Count;
		}
	}
}


