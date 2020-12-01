﻿using dryginstudios.bioinc.meta;
using dryginstudios.bioinc.meta.Data;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlayFabWrapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using dryginstudios.bioinc.stage;
using dryginstudios.bioinc.stageinfo;
using Newtonsoft.Json;

namespace MetaLoop.RESTApi.ApiMethods
{
    public class RegisterMissionComplete : PlayFabApiMethodBase
    {
        public override async Task<CloudScriptResponse> ExecuteAsync(CloudScriptRequest request, string[] urlArguments)
        {
            DateTime benginRequest = DateTime.UtcNow;
            if (IsClientValidRequest)
            {
                var cloudData = new PlayFabFileDetails(MetaSettings.MetaDataStateFileName);

                if (await PlayFabApiHandler.GetPlayerTitleData(CurrentUserId, new List<PlayFabFileDetails>() { cloudData }))
                {
                    MetaDataState metaDataState = MetaDataState.FromJson(cloudData.DataAsString);
                    StageHistory stageInfo = JsonConvert.DeserializeObject<StageHistory>(request.CloudScriptMethod.Params["stageInfo"]);

                    bool result = MissionManager.RegisterMissionComplete(metaDataState, stageInfo);

                    if (result)
                    {
                        cloudData.DataAsString = metaDataState.ToJson();

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