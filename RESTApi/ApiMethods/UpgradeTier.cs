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
    public class UpgradeTier : PlayFabApiMethodBase
    {
        public override async Task<CloudScriptResponse> ExecuteAsync(CloudScriptRequest request, string[] urlArguments)
        {

            if (IsClientValidRequest)
            {
                var cloudData = new PlayFabFileDetails(MetaSettings.MetaDataStateFileName);
                MetaDataState metaDataState = null;

                string troopId = request.CloudScriptMethod.Params["troopId"];

                if (DataLayer.Instance.GetTable<TroopData>().Where(y => y.Name == troopId).Count() == 0)
                {
                    return new CloudScriptResponse() { ResponseCode = ResponseCode.Error, ErrorMessage = "Invalid troop name." };
                }

                if (await PlayFabApiHandler.GetPlayerTitleData(CurrentUserId, new List<PlayFabFileDetails>() { cloudData }))
                {
                    if (cloudData.ExistOnServer)
                    {
                        metaDataState = MetaDataState.FromJson(cloudData.DataAsString);

                        var troopInfo = metaDataState.TroopsData.Where(y => y.TroopId == troopId).SingleOrDefault();

                        if (troopInfo == null)
                        {
                            troopInfo = new TroopDataState() { TroopId = troopId };
                            metaDataState.TroopsData.Add(troopInfo);
                        }

                        troopInfo.CurrentTier = (TierType)((int)troopInfo.CurrentTier + 1);

                        if (await PlayFabApiHandler.UploadPlayerTitleData(CurrentUserId, new List<PlayFabFileDetails>() { cloudData }))
                        {
                            var response = new CloudScriptResponse() { ResponseCode = ResponseCode.Success };
                            return response;
                        }
                    }

                }

            }

            return new CloudScriptResponse() { ResponseCode = ResponseCode.Error };
        }
    }

}
