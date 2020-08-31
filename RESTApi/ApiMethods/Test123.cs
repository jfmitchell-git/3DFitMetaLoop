using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlayFabWrapper;
using MetaLoopDemo.Meta;
using MetaLoopDemo.Meta.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetaLoop.RESTApi.ApiMethods
{
    public class Test123 : PlayFabApiMethodBase
    {
        public override async Task<CloudScriptResponse> ExecuteAsync(CloudScriptRequest request, string[] urlArguments)
        {


            CloudScriptResponse result = new CloudScriptResponse();
            result.Params.Add("TroopOfTheDay", DataLayer.Instance.GetTable<TroopData>().OrderBy(y => Guid.NewGuid()).First().Name);
            result.ResponseCode = ResponseCode.Success;
            return result;
        }
    }

}
