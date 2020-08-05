using System;
using System.Collections.Generic;
using System.Text;

namespace MetaBackend.Common.Data
{
    public partial class MetaSchema
    {
        public static List<Type> SystemsTypes
        {
            get
            {
                List<Type> allTypes = new List<Type>();
                allTypes.Add(typeof(DataVersion));
                return allTypes;
            }
        }
    }
}
