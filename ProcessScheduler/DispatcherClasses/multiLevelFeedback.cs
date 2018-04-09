using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcessScheduler;

namespace ProcessScheduler
{
	class MultiLevelFeedback : Dispatcher
	{
        #region Member Variables
		Queue<Process> highPriorityQ;
		Queue<Process> mediumPriorityQ;
		Queue<Process> lowPriorityQ;
		SimpleFcfs catchQ;
        int[] quanta = { 5, 10, 15, 20 };

		#endregion

		#region Constructors
		public MultiLevelFeedback()
		{
            Name = "MultiLevelFeedback";
            highPriorityQ = new Queue<Process>();
            mediumPriorityQ = new Queue<Process>();
            lowPriorityQ = new Queue<Process>();
            catchQ = new SimpleFcfs();
        }

        //Set the quanta array via constructor
        //THE newQuanta PARAMETER MUST HAVE LENGTH 4
        public MultiLevelFeedback(int hiPriQuantum, int medPriQuantum, int lowPriQuantum, int catchQuantum)
        {
            Name = "MultiLevelFeedback";
            //Set the quanta
            quanta[0] = hiPriQuantum;
            quanta[1] = medPriQuantum;
            quanta[2] = lowPriQuantum;
            quanta[3] = catchQuantum;
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
					ShiftQueues(proc);
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
                int quantum = quanta[currProcess.priority]; //get the time quantum for the process's current queue
				int burstTime = currProcess.remainingEvents.First.Value;  //Get current CPU burst time
				currProcess.priority++; //Reduce the process's priority
				if (burstTime > quantum) //If the event will not finish this quantum
				{
                    int remainingTime = burstTime - quantum; //update the time remaining for this event
                    currProcess.remainingEvents.First.Value = remainingTime; //subtract the quantum from the current burst time
					ShiftQueues(currProcess); //And send the process to the back of the appropriate queue
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

        //Queue a process based on its priority
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
