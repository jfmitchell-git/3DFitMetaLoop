using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MetaBackend.Common.RemoteAssets
{
    [Serializable]
    public class AssetManifest
    {
        public int ManifestVersion;
        public int DataVersion;
        public List<AssetFileInfo> Files;
        public AssetManifest()
        {
            Files = new List<AssetFileInfo>();
        }

        public AssetFileInfo GetFile(string relativeName)
        {
            return Files.Where(y => y.RelativeName == relativeName).SingleOrDefault();
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [Serializable]
    public class AssetFileInfo
    {
        [JsonIgnore]
        public string FileName;
        public string RelativeName;
        public DateTime LastModified;
        public long Size;
        [JsonIgnore]
        public bool ForceSync;
        [JsonIgnore]
        public string LocalRelativeName;


        public AssetFileInfo()
        {

        }

        public AssetFileInfo(string filename, string keyName, bool external = false)
        {
            FileInfo fileInfo = new FileInfo(filename);
            this.FileName = filename;
            this.RelativeName = keyName.Replace(@"\",@"/");
            if (this.RelativeName.StartsWith(@"/")) this.RelativeName = this.RelativeName.Substring(1);

            this.LastModified = fileInfo.LastWriteTimeUtc;
            this.Size = fileInfo.Length;
        }

        public byte[] ToArray()
        {
            byte[] array = File.ReadAllBytes(this.FileName);
            return array;
        }
    }
}
