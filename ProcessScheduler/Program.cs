using System;
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

        static void Main(string[] args)
        {
            //Create Excel Doc
            var excelApp = new Excel.Application();
            excelApp.Visible = true;
            excelApp.Workbooks.Add();

            //BEGIN RUNS
            for(int x = 1; x < 10; x++)
            {
                //Bring in process input files
                List<Process> processes = getProcesses("\\1_BaseDataSet\\set" + x.ToString() + ".txt");

                Dispatcher LS = new Fcfs(processes);
                LS.run();
                Console.WriteLine("Run" + x.ToString() + " complete");


                //OUTPUT RESULTS TO EXCEL DOC
                Excel._Worksheet workSheet;
                if (x != 1) //Add new worksheets
                {
                    excelApp.Worksheets.Add();
                    workSheet = (Excel.Worksheet)excelApp.ActiveSheet;
                }
                else //Create first worksheet
                    workSheet = (Excel.Worksheet)excelApp.ActiveSheet;

                //Add values to sheet
                workSheet.Cells[1, "A"] = "PID";
                workSheet.Cells[1, "B"] = "Arrival Time";
                workSheet.Cells[1, "C"] = "Response Time (Raw)";
                workSheet.Cells[1, "D"] = "Completion Time";

                workSheet.Cells[1, "F"] = "Turnaround Time";
                for (int i = 0; i < processes.Count(); i++)
                {
                    workSheet.Cells[i + 2, "A"] = processes[i].PID;
                    workSheet.Cells[i + 2, "B"] = processes[i].arrivalTime;
                    workSheet.Cells[i + 2, "C"] = processes[i].responseTime;
                    workSheet.Cells[i + 2, "D"] = processes[i].completedTime;

                    workSheet.Cells[i + 2, "F"] = processes[i].responseTime - processes[i].arrivalTime;
                }

                workSheet.Cells[1, "H"] = "Avg Turnaround";
                workSheet.Cells[2, "H"] = "=AVERAGE(F2:F1001)";
            }

            
        }
    }
}
