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
        public static void collectData(string dataset)
        {
            Dispatcher fcfs = new Fcfs();
            Dispatcher rr = new Roundrobin();
            Dispatcher mlf = new MultiLevelFeedback();
            Dispatcher ls = new LoadSharing();
            Dispatcher spn = new Spn();

            CPU FCFS = new CPU(fcfs);
            CPU RR = new CPU(rr);
            CPU MLF = new CPU(mlf);
            CPU LS = new CPU(ls);
            CPU SPN = new CPU(spn);


            List<Thread> threads = new List<Thread>();

            threads.Add(new Thread(delegate () { FCFS.runAlg(dataset); }));
            threads.Add(new Thread(delegate () { RR.runAlg(dataset); }));
            threads.Add(new Thread(delegate () { MLF.runAlg(dataset); }));
            threads.Add(new Thread(delegate () { LS.runAlg(dataset); }));
            threads.Add(new Thread(delegate () { SPN.runAlg(dataset); }));

            foreach (Thread currthread in threads)
                currthread.Start();
        }

        public static void LowTimeQuanta()
        {
            string dataset = "\\1_BaseDataSet";
            Dispatcher rr = new Roundrobin(10);
            Dispatcher spn = new Spn();
            
            CPU RR = new CPU(rr);
            CPU SPN = new CPU(spn);


            List<Thread> threads = new List<Thread>();
            
            threads.Add(new Thread(delegate () { RR.runAlg(dataset); }));
            threads.Add(new Thread(delegate () { SPN.runAlg(dataset); }));

            foreach (Thread currthread in threads)
                currthread.Start();
        }

        public static void highTimeQuanta()
        {
            string dataset = "\\1_BaseDataSet";
            Dispatcher rr = new Roundrobin(2);
            Dispatcher spn = new Spn();

            CPU RR = new CPU(rr);
            CPU SPN = new CPU(spn);


            List<Thread> threads = new List<Thread>();

            threads.Add(new Thread(delegate () { RR.runAlg(dataset); }));
            threads.Add(new Thread(delegate () { SPN.runAlg(dataset); }));

            foreach (Thread currthread in threads)
                currthread.Start();
        }

        static void Main(string[] args)
        {
            collectData("\\1_BaseDataSet");

            collectData("\\2_LowIOProbability");
            collectData("\\3_HighIOProbability");

            collectData("\\4_ShortJobs");
            collectData("\\5_LongJobs");


            
            
 
        }
    }

}
