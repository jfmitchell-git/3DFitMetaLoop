using System;
using System.Collections.Generic;
using System.Text;

namespace MetaConfiguration
{
    public static class ProjectSettings
    {
      
        public static string BaseUnityFolder = "";  //Setting applied from publish bot.

        public const string DownloadableFolderPath = @"\Downloadable";
        public const string DownloadableStartupPath = @"\Startup";
        public const string DownloadableOnDemandPath = @"\OnDemand";

        public static string DownloadablePath
        {
            get
            {
                return BaseUnityFolder + DownloadableFolderPath;
            }
        }
    }
}
