using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.Configuration
{
    public static class ProjectSettings
    {
        public static string DownloadablePath
        {
            get
            {
                return MetaStateSettings._BaseUnityFolder + MetaStateSettings._DownloadableFolderPath;
            }
        }
    }
}
