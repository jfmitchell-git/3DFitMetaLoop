﻿using dryginstudios.bioinc.meta;
using dryginstudios.bioinc.meta.Data;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlayFabWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetaLoop.RESTApi.ApiMethods
{
    public class PlayerLogin : PlayFabApiMethodBase
    {
        public override async Task<CloudScriptResponse> ExecuteAsync(CloudScriptRequest request, string[] urlArguments)
        {
            DateTime benginRequest = DateTime.UtcNow;
            if (IsClientValidRequest)
            {
                var cloudData = new PlayFabFileDetails(MetaSettings.MetaDataStateFileName);

                if (await PlayFabApiHandler.GetPlayerTitleData(CurrentUserId, new List<PlayFabFileDetails>() { cloudData }))
                {
                    MetaDataState metaDataState = null;

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

                        var playerProfile = await PlayFabApiHandler.GetPlayerProfileInfo(CurrentUserId);

                        if (playerProfile != null && playerProfile.Locations != null && playerProfile.Locations.LastOrDefault() != null)
                        {
                            switch (playerProfile.Locations.LastOrDefault().ContinentCode)
                            {
                                case PlayFab.ServerModels.ContinentCode.AF:
                                case PlayFab.ServerModels.ContinentCode.AN:
                                case PlayFab.ServerModels.ContinentCode.EU:
                                    metaDataState.MetaTimeZone = MetaTimeZone.EU;
                                    break;

                                case PlayFab.ServerModels.ContinentCode.NA:
                                case PlayFab.ServerModels.ContinentCode.SA:
                                    metaDataState.MetaTimeZone = MetaTimeZone.NA;
                                    break;

                                case PlayFab.ServerModels.ContinentCode.AS:
                                case PlayFab.ServerModels.ContinentCode.OC:
                                    metaDataState.MetaTimeZone = MetaTimeZone.ASIA;
                                    break;
                            }
                            metaDataState.CountryCode = playerProfile.Locations.LastOrDefault().CountryCode.ToString();
                        }
                        metaDataState.ApplyDailyReset();
                    }

                    if (metaDataState.NextDailyReset == DateTime.MinValue) metaDataState.NextDailyReset = MetaSettings.GetNextDailyReset(metaDataState.MetaTimeZone, DateTime.UtcNow);

                    metaDataState.SyncLoginCalendar();
                    metaDataState.ServerDateTime = DateTime.UtcNow;

                 
                    if (DateTime.UtcNow > metaDataState.NextDailyReset)
                    {
                        metaDataState.ApplyDailyReset();
                    }


                    cloudData.DataAsString = metaDataState.ToJson();

                    if (await PlayFabApiHandler.UploadPlayerTitleData(CurrentUserId, new List<PlayFabFileDetails>() { cloudData }))
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
