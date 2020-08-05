using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLoop.Common.DataEngine
{
   public class MetaDataImportExeption : Exception
    {
        public static List<String> SchemaErrors = new List<string>();
        public MetaDataImportExeption(string message) : base(message)
        {
        }
        public static void Throw(string message, string objectId)
        {
            throw new MetaDataImportExeption(message + string.Format(@"""{0}""", objectId));
        }
        public static void Log(string message, string objectId)
        {
            SchemaErrors.Add(message + string.Format(@"""{0}""", objectId));
        }
    }

}
