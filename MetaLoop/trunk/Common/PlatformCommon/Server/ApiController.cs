using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetaLoop.Common.PlatformCommon.Server
{
    public class ApiController
    {

        public static Dictionary<string, IPlayFabApiMethod> ApiMethods { get; set; }

        public static void Init()
        {
            ApiMethods = new Dictionary<string, IPlayFabApiMethod>();

         
            var type = typeof(IPlayFabApiMethod);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass);

          
            foreach(Type t in types)
            {
                
                IPlayFabApiMethod instance = (IPlayFabApiMethod)Activator.CreateInstance(t);
                ApiMethods.Add(t.Name, instance);
            }

        }


    }
}