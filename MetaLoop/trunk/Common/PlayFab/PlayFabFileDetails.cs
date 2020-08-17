using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLoop.Common.PlayFabWrapper
{
    public class PlayFabFileDetails
    {
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        public string DataAsString { get; set; }
        public bool ExistOnServer { get; set; }

        public PlayFabFileDetails(string fileName)
        {
            this.FileName = fileName;
        }
        public PlayFabFileDetails(string fileName, byte[] data)
        {
            this.FileName = fileName;
            this.Data = data;
            this.DataAsString = null;
        }

        public PlayFabFileDetails(string fileName, string data)
        {
            this.FileName = fileName;
            this.DataAsString = data;
            this.Data = null;
        }
    }
}
