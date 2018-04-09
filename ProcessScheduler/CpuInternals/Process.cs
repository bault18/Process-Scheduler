using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessScheduler
{
    public class Process
    {
        #region Member Variables

        private int pid;
        private int ArrivalTime;
        private List<int> CompletedEvents; //TODO: determine if needs to be public member
        private LinkedList<int> RemainingEvents;
        public int blockExitTime; //Time process exits block queue
        public int priority; //Linux style priority. The lower the value, the higher the priority
		public double predictedBurst;

        public int schedulingTime;  //Total time spend in process scheduler TODO: do we need this? this is just = Completed - Arrival
        public int blockedTime;     //Time blocked by I/O
        public int completedTime;   //Time of process completion
        public int totalProcessingTime;
        public int responseTime;    //Time (raw) first hit processor

		public int previousBurstTime; //The last CPU burst executed by this process
		public double previousGuessBurstTime; //The last guess we made for the expected burst time

        #endregion

        #region Constructors
        public Process(int ID, int arriveTime, List<int> events)
        {
            pid = ID;
            ArrivalTime = arriveTime;
            schedulingTime = 0;
            blockedTime = 0;
            responseTime = -1;
			priority = -1;
			predictedBurst = 0;
			previousBurstTime = 0;
			RemainingEvents = new LinkedList<int>();
            CompletedEvents = new List<int>();

            foreach(int evnt in events) //Create event queue
            {
                RemainingEvents.AddLast(evnt);
            }
        }
        #endregion

        #region Get/Set Functions
        public int PID
        {
            get { return pid; }
        }
        public int arrivalTime
        {
            get { return ArrivalTime; }
        }
        public List<int> completedEvents
        {
            get { return CompletedEvents; }
            set { CompletedEvents = value; }
        }

        public LinkedList<int> remainingEvents
        {
            get { return RemainingEvents; }
            set { RemainingEvents = value; }
        }
        #endregion
    }
}
