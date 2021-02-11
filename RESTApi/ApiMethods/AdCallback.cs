using DryGinStudios.PuzzleFit3D.Meta;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlatformCommon.State;
using MetaLoop.Common.PlayFabWrapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetaLoop.RESTApi.ApiMethods
{
    public class AdCallback : PlayFabApiMethodBase
    {
        public override async Task<CloudScriptResponse> ExecuteAsync(CloudScriptRequest request, string[] urlArguments)
        {
            DateTime benginRequest = DateTime.UtcNow;

            string playFabId = urlArguments[0];
            string placementId = urlArguments[1];
            string eventId = urlArguments[2];

            var cloudData = new PlayFabFileDetails(MetaSettings.MetaDataStateFileName);

            if (await PlayFabApiHandler.GetPlayerTitleData(playFabId, new List<PlayFabFileDetails>() { cloudData }))
            {
                MetaDataState state = MetaDataStateBase.FromJson<MetaDataState>(cloudData.DataAsString);

                AdPlacementType adPlacementType = (AdPlacementType)(Enum.Parse(typeof(AdPlacementType), placementId));
                //state.AdState.RegisterAdWatch(adPlacementType, state, () => AdData.GetAdRewardForTypeMethod(adPlacementType, state));

                cloudData.DataAsString = state.ToJson();

                if (await PlayFabApiHandler.UploadPlayerTitleData(playFabId, new List<PlayFabFileDetails>() { cloudData }))
                {
                    CloudScriptResponse cloudScriptResponse = new CloudScriptResponse() { ResponseCode = ResponseCode.Success, Method = this.GetType().Name };
                    cloudScriptResponse.Params.Add("status", eventId + ":OK");
                    return cloudScriptResponse;
                }
            }

            return new CloudScriptResponse() { ResponseCode = ResponseCode.Error };

        }

    }

}
