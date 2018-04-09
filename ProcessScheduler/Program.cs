﻿using System;
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
            string dataset = "1_BaseDataSet";
            Dispatcher rr = new Roundrobin(3);
            Dispatcher mlf = new MultiLevelFeedback(1,5,10,15);
            
            CPU RR = new CPU(rr);
            CPU MLF = new CPU(mlf);


            List<Thread> threads = new List<Thread>();
            
            threads.Add(new Thread(delegate () { RR.runAlg(dataset); }));
            threads.Add(new Thread(delegate () { MLF.runAlg(dataset); }));

            foreach (Thread currthread in threads)
                currthread.Start();
        }

        public static void highTimeQuanta()
        {
            string dataset = "1_BaseDataSet";
            Dispatcher rr = new Roundrobin(3);
            Dispatcher mlf = new MultiLevelFeedback(10,20,30,40);

            CPU RR = new CPU(rr);
            CPU MLF = new CPU(mlf);


            List<Thread> threads = new List<Thread>();

            threads.Add(new Thread(delegate () { RR.runAlg(dataset); }));
            threads.Add(new Thread(delegate () { MLF.runAlg(dataset); }));

            foreach (Thread currthread in threads)
                currthread.Start();
        }

        static void Main(string[] args)
        {

            string dataset = "1_BaseDataSet";
            Dispatcher fcfs = new Fcfs();
            Dispatcher rr = new Roundrobin();
            Dispatcher spn = new Spn();
            Dispatcher mlf = new MultiLevelFeedback();
            Dispatcher ls = new LoadSharing();

            CPU FCFS = new CPU(fcfs);
            CPU RR = new CPU(rr);
            CPU SPN = new CPU(spn);
            CPU MLF = new CPU(mlf);
            CPU LS = new CPU(ls);

            FCFS.runAlg(dataset);
            RR.runAlg(dataset);
            SPN.runAlg(dataset);
            MLF.runAlg(dataset);
            LS.runAlg(dataset);

            //TODO: Uncomment
            /*
            collectData("\\2_LowIOProbability");
            collectData("\\3_HighIOProbability");

            collectData("\\4_ShortJobs");
            collectData("\\5_LongJobs");
            */
        }
    }

}
