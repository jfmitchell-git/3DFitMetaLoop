
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetaLoop.Common.PlatformCommon.PlayFabClient
{
    public class CloudScriptRequest
    {
        public string UserId { get; set; }
        public string EntityId { get; set; }
        public CloudScriptMethod CloudScriptMethod { get; set; }
    }

    public class CloudScriptRequestStack
    {
        public string UserId { get; set; }
        public string EntityId { get; set; }
        public List<CloudScriptMethod> CloudScriptMethods { get; set; }


    }
}
