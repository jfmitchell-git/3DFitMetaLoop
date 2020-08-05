using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data
{

    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreCodeFirst : Attribute
    {
    }

    public class DefaultZero : Attribute
    {
    }


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
