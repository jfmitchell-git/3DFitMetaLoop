using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Server
{
    public enum ServerStatus
    {
        Offline = 0,
        Online = 1,
        DownForMaintenance = 2
    }
    public class ServerInfo
    {
        public ServerStatus ServerStatus { get; set; }
        public string MaintenanceMessage { get; set; }
        public string AppVersion { get; set; }
        public int CacheVersion { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
