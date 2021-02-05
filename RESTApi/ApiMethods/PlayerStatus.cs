using DryGinStudios.PuzzleFit3D.Meta;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlayFabWrapper;
using MetaLoop.GameLogic.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetaLoop.RESTApi.ApiMethods
{
    public class PlayerStatus : PlayFabApiMethodBase
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

                    if (metaDataState != null)
                    {
                        bool updatePlayerData = false;

                        if (AddEnergyToPlayer(metaDataState) > 0)
                        {
                            updatePlayerData = true;
                        }
                
                        if (metaDataState.SyncLoginCalendar())
                        {
                            updatePlayerData = true;
                        }

                        bool applyDailyReset = false;
                        if (DateTime.UtcNow > metaDataState.NextDailyReset)
                        {
                            metaDataState.ApplyDailyReset();
                            updatePlayerData = true;
                        }

                        if (updatePlayerData)
                        {
                            cloudData.DataAsString = metaDataState.ToJson();
                            await PlayFabApiHandler.UploadPlayerTitleData(request.UserId, new List<PlayFabFileDetails>() { cloudData });
                        }

                        CloudScriptResponse response = new CloudScriptResponse();
                        response.Method = this.GetType().Name;
                        response.ResponseCode = ResponseCode.Success;
                        response.Params.Add("EnergyBalance", metaDataState.Consumables.GetConsumableAmount(Consumable.GetByName<Consumable>(MetaSettings.EnergyId)).ToString());
                        response.Params.Add("ApplyDailyReset", applyDailyReset.ToString());
                        response.Params.Add("UniqueId", metaDataState.UniqueId);
                        return response;
                    }
                }
            }
            return new CloudScriptResponse() { ResponseCode = ResponseCode.Error };
        }

        public static int AddEnergyToPlayer(MetaDataState state)
        {
            //DateTime lastRefill = state.LastEnergyRefil;
            //TimeSpan span = DateTime.UtcNow.Subtract(lastRefill);
            //float totalMinutes = (float)span.TotalMinutes;

            //float totalEnergy = totalMinutes * MetaSettings.EnergyPerMinute;
            //float leftOver = totalEnergy - (int)totalEnergy;
            //double totalMinutesLeftOver = leftOver / MetaSettings.EnergyPerMinute;

            //bool isEnergyCapped = false;

            //// Energy not linked to player level in this case.
            //if (state.Consumables.GetConsumableAmount(Consumable.GetByName(MetaSettings.EnergyId)) >= state.EnergyCap)
            //{
            //    isEnergyCapped = true;
            //}

            //if (totalEnergy >= 1f)
            //{
            //    if (!isEnergyCapped)
            //    {
            //        int newBalance = state.Consumables.GetConsumableAmount(Consumable.GetByName(MetaSettings.EnergyId)) + (int)totalEnergy;
            //        if (newBalance >= state.EnergyCap)
            //        {
            //            totalEnergy = state.EnergyCap - state.Consumables.GetConsumableAmount(Consumable.GetByName(MetaSettings.EnergyId));
            //        }
            //        state.Consumables.AddConsumable(Consumable.GetByName(MetaSettings.EnergyId), (int)totalEnergy);
            //    }
            //    state.LastEnergyRefil = DateTime.UtcNow.AddMinutes(-totalMinutesLeftOver);
            //}

            //if (isEnergyCapped) totalEnergy = 0;

            //return (int)totalEnergy;
            return 0;
        }

    }

}
