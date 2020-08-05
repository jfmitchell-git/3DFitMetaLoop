using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MetaLoop.Common.PlatformCommon.RemoteAssets;
using MetaLoop.Configuration;

namespace MetaLoop.PlayFabCdnManager
{
   
    public static class LocalFileHandler
    {

        public static List<AssetFileInfo> GetLocalDownloadableFiles()
        {
            List<AssetFileInfo> result = new List<AssetFileInfo>();
            FindFileRecursive(ProjectSettings.DownloadablePath, result);
            return result;
        }

        static void FindFileRecursive(string folder, List<AssetFileInfo> appendTo)
        {
            foreach (string d in Directory.GetDirectories(folder))
            {
                foreach (string f in Directory.GetFiles(d))
                {
                    appendTo.Add(new AssetFileInfo(f, f.Replace(ProjectSettings.DownloadablePath, string.Empty)));
                }
                FindFileRecursive(d, appendTo);
            }
        }

    }
}
