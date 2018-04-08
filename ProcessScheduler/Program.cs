﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Office.Core;
using Excel = Microsoft.Office.Interop.Excel;

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
            List<Process> processes = new List <Process> ();
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
        /// <param name="completeProcs">List of completed processes</param>
        /// <param name="excelApp">Excel doc to output to</param>
        /// <param name="run">Current Run number</param>
        static public void outputRun(ref List<Process> completeProcs, ref Excel.Application excelApp, int run)
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

            double subToResponse = 0;
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
                subToResponse += completeProcs[i].responseTime - completeProcs[i].arrivalTime;
            }

            workSheet.Cells[1, "K"] = "Avg Turnaround";
            workSheet.Cells[2, "K"] = "=AVERAGE(I2:I1001)";

            workSheet.Cells[4, "K"] = "Avg time from sumbission to first response";
            workSheet.Cells[5, "K"] = subToResponse/completeProcs.Count();
        }

        /// <summary>
        /// Creates a final sheet on Excel doc with statics encompassing every run
        /// </summary>
        /// <param name="excelApp">Excel doc to output to</param>
        static void finalStatistics(ref Excel.Application excelApp)
        {
            excelApp.Worksheets.Add();
            Excel._Worksheet workSheet = (Excel.Worksheet)excelApp.ActiveSheet;
            string turnaround = "=AVERAGE(Run1!K2";
            string subToResponse = "=AVERAGE(Run1!K5";
            for (int runNum = 2; runNum < 5; runNum++)
            {
                turnaround += ",Run" + runNum + "!K2";
                subToResponse += ",Run" + runNum + "!K5";
            }
            turnaround += ")";
            subToResponse += ")";

            workSheet.Cells[1, "A"] = "Avg Turnaround";
            workSheet.Cells[2, "A"] = turnaround;

            workSheet.Cells[3, "A"] = "Avg Submission to completion";
            workSheet.Cells[4, "A"] = subToResponse;

            
        }


        static void Main(string[] args)
        {
            //Create Excel Doc
            var excelApp = new Excel.Application();
            excelApp.Visible = true;
            excelApp.Workbooks.Add();

            //BEGIN RUNS
            for (int runNum = 1; runNum < 2; runNum++)
            {
                //Bring in process input files
                List<Process> processes = getProcesses("\\1_BaseDataSet\\set" + runNum.ToString() + ".txt");

                Dispatcher LS = new LoadSharing(processes);
                LS.run();
                Console.WriteLine("Run" + runNum.ToString() + " complete");


                //OUTPUT RESULTS TO EXCEL DOC
                outputRun(ref LS.completedProcesses, ref excelApp, runNum);
                
            }

            finalStatistics(ref excelApp);
        }
    }
}
