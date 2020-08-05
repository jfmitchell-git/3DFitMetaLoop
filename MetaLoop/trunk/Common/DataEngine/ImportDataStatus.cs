using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.Common.DataEngine
{
    public class ImportDataStatus
    {

        public static DateTime? lastOperation;
        public static double TotalSeconds;

        public static void WriteStatus(string status)
        {
            if (lastOperation != null)
            {
                TotalSeconds += DateTime.Now.Subtract(lastOperation.Value).TotalSeconds;
                int diffInSeconds = (int)DateTime.Now.Subtract(lastOperation.Value).TotalSeconds;
                Console.Write("Completed in " + diffInSeconds + " sec." + Environment.NewLine);
            }

            Console.Write(status);
            lastOperation = DateTime.Now;
        }

        public static void Reset()
        {
            lastOperation = null;
            TotalSeconds = 0;
        }

    }
}
