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
    public class RegisterAchievement : PlayFabApiMethodBase
    {
        public override async Task<CloudScriptResponse> ExecuteStackAsync(CloudScriptRequestStack request)
        {
            DateTime benginRequest = DateTime.UtcNow;
            if (IsClientValidRequest(request))
            {
                var cloudData = new PlayFabFileDetails(MetaSettings.MetaDataStateFileName);

                if (await PlayFabApiHandler.GetPlayerTitleData(request.UserId, new List<PlayFabFileDetails>() { cloudData }))
                {

                    MetaDataState metaDataState = MetaDataState.FromJson<MetaDataState>(cloudData.DataAsString);

                    bool success = true;

                    foreach (var stack in request.CloudScriptMethod)
                    {
                        AchievementType type = (AchievementType)Enum.Parse(typeof(AchievementType), stack.Params["type"]);
                        bool result = metaDataState.AchievementDataState.RegisterAchievementCount(type);
                    }

                    cloudData.DataAsString = metaDataState.ToJson();

                    if (await PlayFabApiHandler.UploadPlayerTitleData(request.UserId, new List<PlayFabFileDetails>() { cloudData }))
                    {
                        if (success) // not being a success will trigger data mistmatch.
                        {
                            var response = new CloudScriptResponse() { ResponseCode = ResponseCode.Success };
                            return response;
                        }
                    }
                }
            }

            return new CloudScriptResponse() { ResponseCode = ResponseCode.Error };
        }
        public override async Task<CloudScriptResponse> ExecuteAsync(CloudScriptRequest request, string[] urlArguments)
        {
            return null;

        }

    }

}
