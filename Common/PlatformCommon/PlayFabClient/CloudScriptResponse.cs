using System;
using System.Collections.Generic;
using System.Linq;


namespace MetaLoop.Common.PlatformCommon.PlayFabClient
{

    public enum ResponseCode
    {
        ProtocolError = 0,
        Error = 1,
        Success = 2,
        MismatchDetected = 3
    }


    [Serializable]
    public class CloudScriptResponse
    {
        public ResponseCode ResponseCode { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Params { get; set; }
        public string ErrorMessage { get; set; }


        public CloudScriptResponse()
        {
            Params = new Dictionary<string, string>();

        }
    }
}
