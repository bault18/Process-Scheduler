using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcessScheduler;

namespace ProcessScheduler.DispatcherClasses
{
	class MultiLevelFeedback : Dispatcher
	{
		#region Member Variables
		Queue<Process> scheduleQueue;
		Queue<Process> highPriorityQ;
		Queue<Process> mediumPriorityQ;
		Queue<Process> lowPriorityQ;
		SimpleFcfs catchQ;
        int[] quanta = { 5, 10, 15, 20 };
		int quantum = 5;
		#endregion

		#region Constructors
		public MultiLevelFeedback(List<Process> processes)
		{
			scheduleQueue = new Queue<Process>();

			foreach (Process proc in processes)
			{
				proc.priority = 0;              //Set the initial priority of each process to high
				arrivalQueue.Enqueue(proc);
			}
			arrivalQueue = new Queue<Process>(arrivalQueue.OrderBy(p => p.arrivalTime));
            highPriorityQ = new Queue<Process>();
            mediumPriorityQ = new Queue<Process>();
            lowPriorityQ = new Queue<Process>();
            catchQ = new SimpleFcfs();

        }
		#endregion

		//Check if new processes need to go into scheduling queue
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


		//take top and throw in processor
		public override void swapProcesses()
		{
			currProcess = null;
			//Check the queues in descending order of priority
			//Once we find one with elements, take the first element for processing
			if (highPriorityQ.Count > 0)
			{
				currProcess = highPriorityQ.Dequeue();
				//A process should always reach the processor first from the high priority queue
				//So set response time if necessary
				if (currProcess.responseTime < 0)
					currProcess.responseTime = CPUTime;
			}
			else if (mediumPriorityQ.Count > 0)
			{
				currProcess = mediumPriorityQ.Dequeue();
			}
			else if (lowPriorityQ.Count > 0)
			{
				currProcess = lowPriorityQ.Dequeue();
			}
			else if (catchQ.Count() > 0)
			{
				currProcess = catchQ.scheduleQueue.Dequeue();
			}
			if (currProcess != null)
			{
				int burstTime = currProcess.remainingEvents.First.Value;  //Get current CPU burst time
				currProcess.priority++; //Reduce the process's priority
				if (burstTime > quantum) //Then the event will not finish this quantum
				{
					currProcess.remainingEvents.First.Value -= quantum; //subtract the quantum from the current burst time
					ShiftQueues(currProcess); //And send the process to the appropriate queue
				}
				else
				{ //The event will finish this quantum
				  //Get rid of the event we have stored in burstTime
					currProcess.remainingEvents.RemoveFirst();
					if (currProcess.remainingEvents.Count() > 0) //process is blocked by IO
					{
						blockProcess(currProcess);

					}
					else //Process is complete
					{
						currProcess.completedTime = CPUTime;
						completedProcesses.Add(currProcess);
					}
				}

				//Update CPU clock, update data collection
				CPUTime += burstTime;
				currProcess.totalProcessingTime += burstTime;

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
					ShiftQueues(blockedQueue[proc]);  //Put process back into the appropriate queue
					blockedQueue.RemoveAt(proc);      //Remove process from blocked queue
					proc--;                           //Prevent memory access errors
				}
			}
		}

		private void ShiftQueues(Process currProcess)
		{
			switch (currProcess.priority) {
				case 0:
					highPriorityQ.Enqueue(currProcess);
					break;
				case 1:
					mediumPriorityQ.Enqueue(currProcess);
					break;
				case 2:
					lowPriorityQ.Enqueue(currProcess);
					break;
				default:
					catchQ.AddProcess(currProcess);
					break;
			}
		}
	}
}
