using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.Configuration
{
    public class AzureSettings
    {


        public static string Endpoint
        {
            get
            {
#if DEV
                return MetaStateSettings.DevEndpoint;
#elif STAGING || DEBUG
                return MetaStateSettings._StagingEndpoint;
#else
                return MetaStateSettings._ProductionEndpoint;
#endif
            }
        }

        public static string StatusUrl
        {
            get
            {
                return Endpoint + "/Api/Status";
            }
        }
    }
}
