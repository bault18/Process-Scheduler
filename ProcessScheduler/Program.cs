using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Office.Core;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;

namespace ProcessScheduler
{
    class Program
    {

        /// <summary>
        /// Used to retrieve process information from files
        /// </summary>
        /// <returns>list of process objects from given file</returns>
        static public List<Process> getProcesses(string fileName)
        {
            List<Process> processes = new List<Process>();
            string filePath = Directory.GetCurrentDirectory() + "\\" + fileName;

            StreamReader file = new StreamReader(filePath);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                //Split line into each of the saved process info
                List<string> lineElements = line.Split('|').ToList();

                //Split events into list. Needs to convert to int.
                List<string> strEvents = lineElements[2].Split(',').ToList();
                strEvents[0] = strEvents[0].Replace("[", "");
                strEvents[strEvents.Count - 1] = strEvents[strEvents.Count - 1].Replace("]", "");




                int PID = Int32.Parse(lineElements[0]);
                int ArrivalTime = Int32.Parse(lineElements[1]);
                List<int> intEvents = new List<int>();

                foreach (string item in strEvents)
                    intEvents.Add(Int32.Parse(item));

                processes.Add(new Process(PID, ArrivalTime, intEvents));
            }

            return processes;
        }



        static void Main(string[] args)
        {

            string dataset = "\\1_BaseDataSet";
            Dispatcher fcfs = new Fcfs();
            Dispatcher rr = new Roundrobin();
            //Dispatcher mlf = new MultiLevelFeedback();
            Dispatcher ls = new LoadSharing();
            //Dispatcher spn = new Spn();

            CPU FCFS = new CPU(fcfs);
            CPU RR = new CPU(rr);
            //CPU MLF = new CPU(mlf);
            CPU LS = new CPU(ls);
            //CPU SPN = new CPU(spn);

            FCFS.runAlg(dataset);

            List<Thread> threads = new List<Thread>();

            //threads.Add(new Thread(delegate () { FCFS.runAlg(dataset); }));
            //threads.Add(new Thread(delegate () { RR.runAlg(dataset); }));
            //threads.Add(new Thread(delegate () { MLF.runAlg(dataset); }));
            //threads.Add(new Thread(delegate () { LS.runAlg(dataset); }));
            //threads.Add(new Thread(delegate () { SPN.runAlg(dataset); }));

            foreach (Thread currthread in threads)
                currthread.Start();

 
        }
    }

}
