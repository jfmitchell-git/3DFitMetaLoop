using DryGinStudios.PuzzleFit3D.Meta;
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

                    MetaDataState metaDataState = MetaDataState.FromJson<MetaDataState>(cloudData.DataAsString);

                    bool success = true;

                    foreach (var stack in request.CloudScriptMethod)
                    {
                        //ShopManager.OfferMarkAsShown_Server(metaDataState, stack);
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

        //public static ShopRequestResult OfferMarkAsShown_Server(MetaDataState metaDataState, CloudScriptMethod cloudScriptMethod)
        //{
        //    string offerId = cloudScriptMethod.Params["offerId"].ToString();
        //    ShopOfferPlacementType placementType = (ShopOfferPlacementType)Enum.Parse(typeof(ShopOfferPlacementType), cloudScriptMethod.Params["placement"].ToString());
        //    bool popup = Convert.ToBoolean(cloudScriptMethod.Params["popup"].ToString());
        //    ShopOffer item = DataLayer.Instance.GetTable<ShopOffer>().Where(y => y.InternalId == offerId).FirstOrDefault();
        //    metaDataState.OfferDataState.MarkAsShown(offerId, placementType, item, popup);
        //    return null;
        //}
        public override async Task<CloudScriptResponse> ExecuteAsync(CloudScriptRequest request, string[] urlArguments)
        {
            return null;

        }

    }

}
