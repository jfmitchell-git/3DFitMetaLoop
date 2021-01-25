using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlayFabWrapper;
using MetaLoop.GameLogic.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetaLoop.RESTApi.ApiMethods
{
    public class ClaimDailyObjective : PlayFabApiMethodBase
    {
        public override async Task<CloudScriptResponse> ExecuteAsync(CloudScriptRequest request, string[] urlArguments)
        {
            DateTime benginRequest = DateTime.UtcNow;
            if (IsClientValidRequest(request))
            {
                var cloudData = new PlayFabFileDetails(MetaSettings.MetaDataStateFileName);
         
                if (await PlayFabApiHandler.GetPlayerTitleData(request.UserId, new List<PlayFabFileDetails>() { cloudData }))
                {

                    MetaDataState metaDataState = MetaDataState.FromJson<MetaDataState>(cloudData.DataAsString);

                    DailyObjectiveType type = (DailyObjectiveType)Enum.Parse(typeof(DailyObjectiveType),request.CloudScriptMethod.Params["type"]);

                    //bool result = MissionManager.ClaimDailyObjective(metaDataState, type);
                    bool result = true;

                    cloudData.DataAsString = metaDataState.ToJson();

                    if (result && await PlayFabApiHandler.UploadPlayerTitleData(request.UserId, new List<PlayFabFileDetails>() { cloudData }))
                    {
                        var response = new CloudScriptResponse() { ResponseCode = ResponseCode.Success };
                        return response;
                    }

                }

            }

            return new CloudScriptResponse() { ResponseCode = ResponseCode.Error };

        }

    }

}
