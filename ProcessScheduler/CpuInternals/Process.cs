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
        private Queue<int> RemainingEvents;
        public int blockExitTime; //Time process exits block queue


        public int schedulingTime;  //Total time spend in process scheduler
        public int blockedTime;     //Time blocked by I/O
        public int completedTime;   //Time of process completion
        public int totalProcessingTime;
        public int responseTime;    //Time (raw) first hit processor

        #endregion

        #region Constructors
        public Process(int ID, int arriveTime)
        {
            pid = ID;
            ArrivalTime = arriveTime;
            schedulingTime = 0;
            blockedTime = 0;
            responseTime = -1;
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

        public Queue<int> remainingEvents
        {
            get { return RemainingEvents; }
            set { RemainingEvents = value; }
        }
        #endregion
    }
}
