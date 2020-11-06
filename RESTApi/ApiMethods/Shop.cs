using dryginstudios.bioinc.meta;
using dryginstudios.bioinc.meta.Data;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlayFabWrapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetaLoop.RESTApi.ApiMethods
{
    public class Shop : PlayFabApiMethodBase
    {
        private const string ServerMethodSuffix = "_Server";
        public override async Task<CloudScriptResponse> ExecuteAsync(CloudScriptRequest request, string[] urlArguments)
        {
            DateTime benginRequest = DateTime.UtcNow;
            if (IsClientValidRequest)
            {
                var cloudData = new PlayFabFileDetails(MetaSettings.MetaDataStateFileName);

                if (await PlayFabApiHandler.GetPlayerTitleData(CurrentUserId, new List<PlayFabFileDetails>() { cloudData }))
                {
                    MetaDataState state = MetaDataState.FromJson(cloudData.DataAsString);
                    cloudData.DataAsString = state.ToJson();

                    string methodName = urlArguments[0] + ServerMethodSuffix;

                    Type shopManager = typeof(ShopManager);
                    ShopRequestResult shopRequestResult = shopManager.GetMethod(methodName).Invoke(null, new object[] { state, request.CloudScriptMethod }) as ShopRequestResult;

                    cloudData.DataAsString = state.ToJson();

                    if (await PlayFabApiHandler.UploadPlayerTitleData(CurrentUserId, new List<PlayFabFileDetails>() { cloudData }))
                    {
                        CloudScriptResponse response = new CloudScriptResponse() { ResponseCode = ResponseCode.Success, Method = this.GetType().Name };
                        response.Params.Add("ShopRequestResult", JsonConvert.SerializeObject(shopRequestResult));
                        return response;
                    }
                }

            }

            return new CloudScriptResponse() { ResponseCode = ResponseCode.Error };

        }

    }

}
