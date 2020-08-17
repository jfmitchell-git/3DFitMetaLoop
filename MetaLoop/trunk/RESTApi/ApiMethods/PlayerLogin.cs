﻿using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlayFabWrapper;
using MetaLoopDemo.Meta;
using MetaLoopDemo.Meta.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetaLoop.RESTApi.ApiMethods
{
    public class PlayerLogin : PlayFabApiMethodBase
    {
        public override async Task<CloudScriptResponse> ExecuteAsync(CloudScriptRequest request, string[] urlArguments)
        {
            if (IsClientValidRequest)
            {
                var cloudData = new PlayFabFileDetails(MetaSettings.MetaDataStateFileName);
                MetaDataState metaDataState = null;


                if (await PlayFabApiHandler.GetEntityFiles(CurrentEntity, new List<PlayFabFileDetails>() { cloudData }))
                {
                    //if file does not exist yet, create default for content, otherwise perfom Login Activies.
                    if (cloudData.ExistOnServer)
                    {
                        metaDataState = MetaDataState.FromJson(cloudData.DataAsString);
                    }
                    else
                    {
                        metaDataState = new MetaDataState();
                        metaDataState.Consumables.AddConsumable(Consumable.GetByName(MetaSettings.HardCurrencyId), 100);
                        metaDataState.Consumables.AddConsumable(Consumable.GetByName(MetaSettings.SoftCurrencyId), 1000);
                        metaDataState.Consumables.AddConsumable(Consumable.GetByName(MetaSettings.EnergyId), 70);
                        metaDataState.CreationDate = DateTime.UtcNow;
                        metaDataState.MetaTimeZone = MetaTimeZone.UTC;
                        metaDataState.ApplyDailyReset();
                    }

                    metaDataState.SyncLoginCalendar();
                    metaDataState.ServerDateTime = DateTime.UtcNow;
                    cloudData.DataAsString = metaDataState.ToJson();

                    if (await PlayFabApiHandler.UploadEntityFiles(CurrentEntity, new List<PlayFabFileDetails>() { cloudData }))
                    {
                        return new CloudScriptResponse() { ResponseCode = ResponseCode.Success };
                    }

                }

            }

            return new CloudScriptResponse() { ResponseCode = ResponseCode.Error };

        }

    }

}
