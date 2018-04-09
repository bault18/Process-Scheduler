﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Core;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;
using System.IO;
using System.Reflection;

namespace ProcessScheduler
{
    class CPU
    {
        private Dispatcher scheduler;
        private int numRuns = 4;        //TODO: set to 100
        public CPU(Dispatcher schedul)
        {
            scheduler = schedul;
        }

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

        /// <summary>
        /// Adds a sheet to an excel doc with all the run's data
        /// </summary>
        private void outputRun(ref List<Process> completeProcs, ref Excel.Application excelApp, int run)
        {

            Excel._Worksheet workSheet;

            if (run != 1) //Add new worksheets
            {
                excelApp.Worksheets.Add();
                workSheet = (Excel.Worksheet)excelApp.ActiveSheet;
                workSheet.Name = "Run" + run.ToString();
            }
            else //Create first worksheet
            {
                workSheet = (Excel.Worksheet)excelApp.ActiveSheet;
                workSheet.Name = "Run1";
            }
            //Add values to sheet
            workSheet.Cells[1, "A"] = "PID";
            workSheet.Cells[1, "B"] = "Arrival Time";
            workSheet.Cells[1, "C"] = "Response Time (Raw)";
            workSheet.Cells[1, "D"] = "Completion Time";

            workSheet.Cells[1, "F"] = "Total Processing Time";
            workSheet.Cells[1, "G"] = "Total Blocked Time";
            workSheet.Cells[1, "H"] = "Total Scheduling Queue Time";
            workSheet.Cells[1, "I"] = "Turnaround Time";

            double responseTime = 0;
            double blockedTime = 0;
            double waitTime = 0;
            for (int i = 0; i < completeProcs.Count(); i++)
            {
                workSheet.Cells[i + 2, "A"] = completeProcs[i].PID;
                workSheet.Cells[i + 2, "B"] = completeProcs[i].arrivalTime;
                workSheet.Cells[i + 2, "C"] = completeProcs[i].responseTime;
                workSheet.Cells[i + 2, "D"] = completeProcs[i].completedTime;

                workSheet.Cells[i + 2, "F"] = completeProcs[i].totalProcessingTime;
                workSheet.Cells[i + 2, "G"] = completeProcs[i].blockedTime;
                workSheet.Cells[i + 2, "H"] = (completeProcs[i].completedTime - completeProcs[i].arrivalTime) - completeProcs[i].blockedTime - completeProcs[i].totalProcessingTime; //Scheduling Time
                workSheet.Cells[i + 2, "I"] = completeProcs[i].completedTime - completeProcs[i].arrivalTime; //Turnaround time


                //Stuff for other data analysis
                responseTime += completeProcs[i].responseTime - completeProcs[i].arrivalTime;
                blockedTime += completeProcs[i].blockedTime;
                waitTime += (completeProcs[i].completedTime - completeProcs[i].arrivalTime) - completeProcs[i].totalProcessingTime;
            }

            workSheet.Cells[1, "K"] = "Avg Turnaround";
            workSheet.Cells[2, "K"] = "=AVERAGE(I2:I1001)";

            workSheet.Cells[4, "K"] = "Avg Response time";
            workSheet.Cells[5, "K"] = responseTime / completeProcs.Count();

            workSheet.Cells[7, "K"] = "Avg Blocking Time";
            workSheet.Cells[8, "K"] = blockedTime / completeProcs.Count();

            workSheet.Cells[10, "K"] = "Avg Wait Time";
            workSheet.Cells[11, "K"] = waitTime;

            workSheet.Cells[13, "K"] = "Throughput";
            workSheet.Cells[14, "K"] = "=A1000/999";

            workSheet.Cells[16, "K"] = "Num Context Switches";
            workSheet.Cells[17, "K"] = scheduler.numContextSwitch;
        }

        /// <summary>
        /// Creates a final sheet on Excel doc with statics encompassing every run
        /// </summary>
        private void finalStatistics(ref Excel.Application excelApp)
        {
            excelApp.Worksheets.Add();
            Excel._Worksheet workSheet = (Excel.Worksheet)excelApp.ActiveSheet;
            string turnaround = "=AVERAGE(Run1!K2";
            string response = "=AVERAGE(Run1!K5";
            string blocked = "=AVERAGE(Run1!K8";
            string waittime = "=AVERAGE(Run1!K11";
            string throughput = "=AVERAGE(Run1!K14";
            string contextswitch = "=AVERAGE(Run1!K17";
            for (int runNum = 2; runNum < numRuns; runNum++)
            {
                turnaround += ",Run" + runNum + "!K2";
                response += ",Run" + runNum + "!K5";
                blocked += ",Run" + runNum + "!K8";
                waittime += ",Run" + runNum + "!K11";
                throughput += ",Run" + runNum + "!K14";
                contextswitch += ",Run" + runNum + "!K17";
            }
            turnaround += ")";
            response += ")";
            blocked += ")";
            throughput += ")";
            contextswitch += ")";


            workSheet.Cells[1, "A"] = "Avg Turnaround";
            workSheet.Cells[2, "A"] = turnaround;

            workSheet.Cells[4, "A"] = "Avg Response Time";
            workSheet.Cells[5, "A"] = response;

            workSheet.Cells[7, "A"] = "Avg Blocked Time";
            workSheet.Cells[8, "A"] = blocked;

            workSheet.Cells[10, "A"] = "Avg Wait Time";
            workSheet.Cells[11, "A"] = waittime;

            workSheet.Cells[13, "A"] = "Avg Throughput";
            workSheet.Cells[14, "A"] = throughput;

            workSheet.Cells[16, "A"] = "Avg Num Context Switches";
            workSheet.Cells[17, "A"] = contextswitch;

            Console.WriteLine(scheduler.Name);
            Console.WriteLine(Directory.GetCurrentDirectory() + "\\" + scheduler.Name);
            excelApp.ActiveWorkbook.SaveAs(Directory.GetCurrentDirectory() + "\\" + scheduler.Name);


        }

        public void runAlg(string dataSet)
        {
            //Create Excel Doc
            var excelApp = new Excel.Application();
            excelApp.Visible = true;
            excelApp.Workbooks.Add();

            //BEGIN RUNS
            for (int runNum = 0; runNum < numRuns; runNum++)
            {
                //Bring in process input files
                List<Process> processes = getProcesses(dataSet + "\\set" + runNum.ToString() + ".txt");

                scheduler.setArrivalQueue(processes);
                scheduler.run();
                Console.WriteLine("Run" + runNum.ToString()  + " of " + scheduler.Name + " complete");


                //OUTPUT RESULTS TO EXCEL DOC
                outputRun(ref scheduler.completedProcesses, ref excelApp, runNum);
                scheduler.reset();
            }

            finalStatistics(ref excelApp);
            excelApp.Quit();
        }
    }
}
