using MetaLoop.Common.DataEngine;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Settings;
using MetaLoop.GameLogic;
using MetaLoop.GameLogic.DataImport;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;

namespace MetaLoop.AutomationsTools
{
    class Program
    {

        public static ManualResetEvent resetEvent = new ManualResetEvent(false);
        public static bool manualInput = false;
        public static List<string> allFunctions = new List<string>() { "exit", "importdb" };
        static void Main(string[] args)
        {
            _MetaStateSettings.Init();

            Console.WriteLine("MetaLoop Automation Tools.");

            if (args != null && args.ToList().ElementAtOrDefault(0) != null && args.ToList().ElementAtOrDefault(0) != string.Empty)
            {
                Console.WriteLine("Executing command from arguments.");
                RunCommandFromArgs(args.ToList(), true);
            }
            else
            {
                manualInput = true;
                PromptForCommandName();
            }

        }

        private static void PromptForCommandName()
        {
            Console.Write("Type command:");
            var inputArgs = Console.ReadLine().Split(' ').ToList();
            RunCommandFromArgs(inputArgs, true);

        }

        private static void RunCommandFromArgs(List<string> args, bool retry = false)
        {
            if (allFunctions.Contains(args.ElementAtOrDefault(0)))
            {
                var allArgumentsWithoutCommand = args.ToList();
                allArgumentsWithoutCommand.RemoveAt(0);
                var method = typeof(Program).GetMethod(args.ElementAtOrDefault(0));
                method.Invoke(null, new object[] { allArgumentsWithoutCommand });
                Console.WriteLine(string.Empty);
                PromptForCommandName();
            }
            else
            {
                if (retry)
                {
                    Console.WriteLine("Invalid command, possible option are " + string.Join(", ", allFunctions.ToArray()));
                    PromptForCommandName();
                }
            }
        }

        public static void PromptForExit()
        {
            if (manualInput)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        public static void importdb(List<string> args)
        {
            var pathArg = args.Where(y => y.StartsWith("-BaseUnityFolder:")).SingleOrDefault();
            if (pathArg != null)
            {
                MetaStateSettings._BaseUnityFolder = pathArg.Replace("-BaseUnityFolder:", string.Empty);
            }

            string excelFileName = Path.GetFullPath(MetaStateSettings._BaseUnityFolder + @"\" + args.First(), Directory.GetCurrentDirectory());
            string databaseFileName = Path.GetFullPath(MetaStateSettings._BaseUnityFolder + @"\" + MetaStateSettings._DatabaseFileName, Directory.GetCurrentDirectory());


            if (!File.Exists(databaseFileName))
            {
                Console.WriteLine("Error, could not locate file: " + databaseFileName);
            }
            else
            {
                DataLayer.Instance.Init(databaseFileName, true);
            }

            if (File.Exists(excelFileName))
            {
                ExcelReader reader = new ExcelReader();
                DataSet dataSet = null;

                try
                {
                    dataSet = reader.ReadExcelFile(excelFileName);
                    ThreadPool.QueueUserWorkItem(arg => DataImport.ImportData(dataSet, resetEvent));
                    resetEvent.WaitOne();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while parsing Excel file.");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
            else
            {
                Console.WriteLine("Error, could not locate file: " + excelFileName);
            }

        }

        public static void example(List<string> args)
        {
            ThreadPool.QueueUserWorkItem(arg => { });
            resetEvent.WaitOne();
        }

        public static void exit(List<string> args) { Environment.Exit(0); }


    }
}
