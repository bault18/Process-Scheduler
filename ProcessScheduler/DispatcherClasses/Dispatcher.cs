﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessScheduler
{
    public abstract class Dispatcher
    {
        public Queue<Process> arrivalQueue;    //Holds list of all processes created
        public List<Process> completedProcesses;
        public List<Process> blockedQueue; //Holds list of all processes blocked by I/O

        public Process currProcess; //current process being executed by CPU

        public int CPUTime; //Total time executed by cpu
        int contextSwtichTime;
        int numContextSwitch;

        public Dispatcher() {
            CPUTime = 0;
            numContextSwitch = 0;
            contextSwtichTime = 2;
            blockedQueue = new List<Process>();
            completedProcesses = new List<Process>();
            arrivalQueue = new Queue<Process>();
        }

        public void blockProcess(Process process) {
            //set exit time
            int IOBurst = process.remainingEvents.First.Value;
            process.remainingEvents.RemoveFirst();
            process.blockExitTime = IOBurst + CPUTime;

            //remove I/O event, place in comleted event
            process.completedEvents.Add(IOBurst);

            //update queue time
            process.schedulingTime += IOBurst;
            process.blockedTime += IOBurst;

            //Add to blocoked queue
            blockedQueue.Add(process);
        }

        public void contextSwitch()
        {
            CPUTime += contextSwtichTime;
            numContextSwitch++;
        }

        public void run()
        {
            while (completedProcesses.Count() < 999) //TODO: Fix this static number
            {
                addNewProcess();
                swapProcesses();
            }
        }

        public abstract void addNewProcess();
        public abstract void swapProcesses();
        public abstract void checkBlockedQueue();
    }
}
