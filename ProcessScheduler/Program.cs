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
       

        


        static void Main(string[] args)
        {
            CPU dataCollector = new CPU();


            Dispatcher scheduler = Fcfs();
            dataCollector.runAlg();


            //Create Excel Doc
            var excelApp = new Excel.Application();
            excelApp.Visible = true;
            excelApp.Workbooks.Add();

            //BEGIN RUNS
            for (int runNum = 1; runNum < 5; runNum++)
            {
                //Bring in process input files
                List<Process> processes = getProcesses("\\1_BaseDataSet\\set" + runNum.ToString() + ".txt");

                Dispatcher LS = new Fcfs(processes);
                LS.run();
                Console.WriteLine("Run" + runNum.ToString() + " complete");


                //OUTPUT RESULTS TO EXCEL DOC
                outputRun(ref LS.completedProcesses, ref excelApp, runNum);
                
            }

            finalStatistics(ref excelApp);





            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            List<Thread> threads = new List<Thread>();

            for (int numThreads = 4; numThreads > 0; numThreads--)
            {

            }

        }
    }

}
