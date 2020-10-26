using dryginstudios.bioinc.meta;
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
using Newtonsoft.Json;

namespace MetaLoop.RESTApi.ApiMethods
{
    public class RegisterMission : PlayFabApiMethodBase
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

                    string missionId = request.CloudScriptMethod.Params["missionId"];
                    List<string> boosterUsed = JsonConvert.DeserializeObject<List<string>>(request.CloudScriptMethod.Params["boosterUsed"]);
                    int difficultyId = Convert.ToInt32(request.CloudScriptMethod.Params["difficultyId"]);

                    var missionData = DataLayer.Instance.GetTable<MissionData>().Where(y => y.MissionId.ToString() == missionId && y.DifficultyId == difficultyId).Single();

                    List<BoosterData> boosterDataList = new List<BoosterData>();
                    if (boosterUsed != null)
                    {
                        boosterUsed.ForEach(y => boosterDataList.Add(DataLayer.Instance.GetTable<BoosterData>().Where(x => x.GetUniqueId() == y).Single()));
                    }
                   
                    bool result = MissionManager.RegisterCurrentMission(metaDataState, missionData, boosterDataList);

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
