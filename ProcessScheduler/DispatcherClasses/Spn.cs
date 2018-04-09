using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessScheduler
{
	//Shortest Process Next
	class Spn : Dispatcher
	{
        #region memberVariables
        double defaultPreviousServiceTime = 0;
		double defaultExpectedServiceTime = 10;
		double historicWeight = 0.5; //Between 0 and 1

		Queue<Process> scheduleQueue;
        #endregion

        #region Constructors
        public Spn(List<Process> processes)
        {
            Name = "ShortedProcessNext";
            scheduleQueue = new Queue<Process>();

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
				proc.priority = 0;
				if (proc.arrivalTime <= CPUTime)    //If proc arrived
				{
					scheduleQueue.Enqueue(proc);
					arrivalQueue.Dequeue();
				}
				else        //No procs left to arrive
					break;
			}
		}

        //take top and throw in processor
        public override void swapProcesses()
		{
			double expectedServiceTime = 0; //Expected burst time for an interactive process. Expected total execution time for a batch process
			double previousServiceTime = 0; //Previous burst time for an interactive process. Previous total execution time for a batch process
			//Since all of our processes are modeled as interactive, we can use burst times
			double counterWeight = 1 - historicWeight;

			if (scheduleQueue.Count > 0)
			{
				SortScheduleQ();
                currProcess = scheduleQueue.Dequeue(); //Fetch process from front of dispatch queue
                //If the time value is zero, use the default value instead
                previousServiceTime = currProcess.previousBurstTime > 0 ? currProcess.previousBurstTime : defaultPreviousServiceTime;
				expectedServiceTime = currProcess.predictedBurst > 0 ? currProcess.predictedBurst : defaultExpectedServiceTime; 
				
				currProcess.predictedBurst = (historicWeight * previousServiceTime) + (counterWeight * expectedServiceTime);
				//Set response time
				if (currProcess.responseTime < 0)
					currProcess.responseTime = CPUTime;

				int burstTime = currProcess.remainingEvents.First.Value;  //Get current CPU burst time
				currProcess.remainingEvents.RemoveFirst();

				//Update CPU clock, update data collection
				CPUTime += burstTime;
				currProcess.totalProcessingTime += burstTime;
                currProcess.previousBurstTime = burstTime;
                currProcess.previousGuessBurstTime = currProcess.predictedBurst;

				//check if I/O event occurs
				if (currProcess.remainingEvents.Count() > 0) //IO event occurs
				{
					currProcess.previousBurstTime = burstTime;
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
		//Sorts the schedule queue based on the 
		public void SortScheduleQ() {
			scheduleQueue.OrderBy<Process, double>(proc => proc.predictedBurst);
		}
	}
}
