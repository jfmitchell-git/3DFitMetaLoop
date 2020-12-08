﻿using dryginstudios.bioinc.meta;
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
    public class RegisterOfferState : PlayFabApiMethodBase
    {
        public override async Task<CloudScriptResponse> ExecuteStackAsync(CloudScriptRequestStack request)
        {
            DateTime benginRequest = DateTime.UtcNow;
            if (IsClientValidRequest(request))
            {
                var cloudData = new PlayFabFileDetails(MetaSettings.MetaDataStateFileName);

                if (await PlayFabApiHandler.GetPlayerTitleData(request.UserId, new List<PlayFabFileDetails>() { cloudData }))
                {

                    MetaDataState metaDataState = MetaDataState.FromJson(cloudData.DataAsString);

                    bool success = true;

                    foreach (var stack in request.CloudScriptMethod)
                    {
                        ShopManager.OfferMarkAsShown_Server(metaDataState, stack);
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
