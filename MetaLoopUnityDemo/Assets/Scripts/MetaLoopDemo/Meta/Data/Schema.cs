using MetaLoop.Common.PlatformCommon.Data;
using MetaLoop.Common.PlatformCommon.Data.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MetaLoopDemo.Meta.Data
{

    public class Schema
    {
        public static List<Type> CodeFirstTypes
        {
            get
            {
                List<Type> allTypes = SchemaBuilder.CodeFirstTypes;

                //Add any custom integration here..

                return allTypes;
            }

        }
    }
}
