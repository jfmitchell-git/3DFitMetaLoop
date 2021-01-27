using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Server
{
    [Serializable]
    public class RemoteConfigData
    {
        private static RemoteConfigData instance;
        public Dictionary<string, string> Values { get; set; }
        public static RemoteConfigData Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new Exception("Cannot access RemoteConfigData before calling the Load method.");
                }
                return instance;
            }
        }
        public string this[string settingId]
        {
            get
            {
                if (Values.ContainsKey(settingId)) return Values[settingId];
                return String.Empty;
            }
        }
        public RemoteConfigData()
        {
            if (Values == null)
            {
                Values = new Dictionary<string, string>();
            }
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static void Load(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                instance = JsonConvert.DeserializeObject<RemoteConfigData>(data);
            }
            else
            {
                instance = new RemoteConfigData();
            }

        }





    }
}
