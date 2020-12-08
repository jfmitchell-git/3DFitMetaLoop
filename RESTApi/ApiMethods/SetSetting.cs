using dryginstudios.bioinc.meta;
using dryginstudios.bioinc.meta.Data;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlayFabWrapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetaLoop.RESTApi.ApiMethods
{
    public class SetSetting : PlayFabApiMethodBase
    {
        public override async Task<CloudScriptResponse> ExecuteStackAsync(CloudScriptRequestStack request)
        {
            DateTime benginRequest = DateTime.UtcNow;
            if (IsClientValidRequest(request))
            {
                var cloudData = new PlayFabFileDetails(MetaSettings.MetaDataStateFileName);

                if (await PlayFabApiHandler.GetPlayerTitleData(request.UserId, new List<PlayFabFileDetails>() { cloudData }))
                {
                    MetaDataState state = MetaDataState.FromJson(cloudData.DataAsString);

                    foreach (var stack in request.CloudScriptMethod)
                    {
                        string settingId = stack.Params["key"];

                        switch (stack.Params["type"])
                        {
                            case "setting":

                                int value = Convert.ToInt32(stack.Params["value"]);
                                state.SettingState.SetSetting(settingId, value);
                                break;

                            case "tutorial":
                                bool tutorialProgressValue = Convert.ToBoolean(stack.Params["value"]);
                                state.SettingState.SetTutorialProgress(settingId, tutorialProgressValue);
                                break;

                            case "keyvaluepairs":
                                string keyValue = stack.Params["value"];
                                state.SettingState.SetKeyValuePairs(settingId, keyValue);
                                break;
                        }
                    }

                    cloudData.DataAsString = state.ToJson();

                    if (await PlayFabApiHandler.UploadPlayerTitleData(request.UserId, new List<PlayFabFileDetails>() { cloudData }))
                    {
                        var response = new CloudScriptResponse() { ResponseCode = ResponseCode.Success };
                        return response;
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
