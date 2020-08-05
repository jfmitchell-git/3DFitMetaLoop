using GameLogic.DataImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DataImportTool
{
    class Program
    {

        public static ManualResetEvent resetEvent = new ManualResetEvent(false);
        public static bool manualInput = false;
        public static List<string> allFunctions = new List<string>() { "exit", "importdb"};
        static void Main(string[] args)
        {
            Console.WriteLine("MetaBackend Automation Tools.");

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
            ThreadPool.QueueUserWorkItem(arg => DataImport.ImportData());
            resetEvent.WaitOne();
        }

        public static void example(List<string> args)
        {
            ThreadPool.QueueUserWorkItem(arg => { });
            resetEvent.WaitOne();
        }

        public static void exit(List<string> args) { Environment.Exit(0); }


    }
}
