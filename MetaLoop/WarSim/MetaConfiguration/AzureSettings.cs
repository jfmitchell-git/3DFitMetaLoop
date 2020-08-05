using System;
using System.Collections.Generic;
using System.Text;

namespace MetaConfiguration
{
    public class AzureSettings
    {
        public const string ProductionEndpoint = @"https://bioincbackofficewest2.azurewebsites.net";
        public const string StagingEndpoint = @"https://bioincbackofficewest2-staging.azurewebsites.net";
        public const string DevEndpoint = @"https://bioincbackofficewest2-staging.azurewebsites.net";

        public static string Endpoint
        {
            get
            {
#if DEV
                return DevEndpoint;
#elif STAGING || DEBUG
                return StagingEndpoint;
#else
                return ProductionEndpoint;
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
