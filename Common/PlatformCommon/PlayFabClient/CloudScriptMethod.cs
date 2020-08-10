using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.PlayFabClient
{
    [Serializable]
    public class CloudScriptMethod
    {
        public string Method { get; set; }
        public string Entity { get; set; }
        public Dictionary<string, string> Params { get; set; }
        public int Attempt { get; set; }

        [JsonIgnore]
        public bool IgnoreError { get; set; }

#if !BACKOFFICE
        public string Environement { get; set; }
#endif

        public CloudScriptMethod(string methodName, bool allowRetry = true)
        {
            Method = methodName;
            Params = new Dictionary<string, string>();
            if (!allowRetry) Attempt = -1;
        }

        public CloudScriptMethod()
        {
            Params = new Dictionary<string, string>();
        }
    }

}