using MetaLoop.Common.PlatformCommon.Data;
using MetaLoop.Common.PlatformCommon.Data.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{

    public class SchemaBuilder
    {
        public static List<Type> CodeFirstTypes
        {
            get
            {
                List<Type> allTypes = new List<Type>();
                allTypes.AddRange(MetaSchema.SystemsTypes);


                var allMetaObjects = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                              .Where(x => typeof(IMetaDataObject).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList();

                var priorityList = allMetaObjects.Where(y => y.GetCustomAttribute<MetaSchemaOrder>() != null).OrderBy(y => y.GetCustomAttribute<MetaSchemaOrder>().Order).ToList();
                var nonPriorityList = allMetaObjects.Where(y => y.GetCustomAttribute<MetaSchemaOrder>() == null).ToList();

                allTypes.AddRange(priorityList);

                allTypes.Add(typeof(ConsumableCost));
                allTypes.Add(typeof(ConsumableCostItem));
                allTypes.Add(typeof(RewardData));
                allTypes.Add(typeof(RewardDataItem));


                allTypes.AddRange(nonPriorityList);

                return allTypes;
            }

        }
    }
}
