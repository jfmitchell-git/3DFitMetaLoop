
using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.Internal;
using PlayFab.ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MetaLoop.Common.PlayFabWrapper
{


    public enum EntityType
    {
        title_player_account,
        title,
        group
    }
    public static class PlayFabApiHandler
    {
        public static bool UseEntityFiles { get; set; }

        private const string title_player_account = "title_player_account";
        //public static GetEntityTokenResponse CurrentEntityToken { get; set; }

        //public static PlayFabAuthenticationContext GetPlayFabAuthenticationContext()
        //{
        //    if (CurrentEntityToken != null)
        //    {
        //        return new PlayFabAuthenticationContext()
        //        {
        //            EntityId = CurrentEntityToken.Entity.Id,
        //            EntityToken = CurrentEntityToken.EntityToken,
        //            EntityType = CurrentEntityToken.Entity.Type
        //        };
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public static async Task ValidateEntityToken()
        //{
        //    if (CurrentEntityToken == null || DateTime.UtcNow >= CurrentEntityToken.TokenExpiration.Value)
        //    {
        //        CurrentEntityToken = await GetEntityToken();
        //    }
        //}


        public static async Task<GetEntityTokenResponse> GetEntityToken(params string[] keys)
        {
            GetEntityTokenRequest requestData = new GetEntityTokenRequest();
            var result = await PlayFab.PlayFabAuthenticationAPI.GetEntityTokenAsync(requestData);
            return result.Result;
        }

        public static async Task<PlayerProfileModel> GetPlayerProfileInfo(string titlePlayerId)
        {
            var requestData = new GetPlayerProfileRequest() { PlayFabId = titlePlayerId,  };
            requestData.ProfileConstraints = new PlayerProfileViewConstraints();
            requestData.ProfileConstraints.ShowLocations = true;
            requestData.ProfileConstraints.ShowDisplayName = true;

            var result = await PlayFabServerAPI.GetPlayerProfileAsync(requestData);

            if (result.Error == null)
            {
                return result.Result.PlayerProfile;
            }
            else
            {
                return null;
            }
        }



        public static async Task<bool> UploadPlayerTitleData(string titlePlayerId, List<PlayFabFileDetails> files)
        {
            var requestData = new UpdateUserDataRequest() { PlayFabId = titlePlayerId, Permission = UserDataPermission.Private };
            requestData.Data = new Dictionary<string, string>();
            foreach (var file in files)
            {
                requestData.Data.Add(file.FileName, file.DataAsString);
            }

            var result = await PlayFabServerAPI.UpdateUserReadOnlyDataAsync(requestData);

            if (result.Error == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static async Task<bool> GetPlayerTitleData(string titlePlayerId, List<PlayFabFileDetails> files)
        {
            var requestData = new GetUserDataRequest() { PlayFabId = titlePlayerId };
            requestData.Keys = files.Select(y => y.FileName).ToList();
            var result = await PlayFabServerAPI.GetUserReadOnlyDataAsync(requestData);

            if (result.Error == null)
            {
                foreach (var file in files)
                {
                    if (result.Result.Data.ContainsKey(file.FileName))
                    {
                        file.ExistOnServer = true;
                        file.DataAsString = result.Result.Data[file.FileName].Value;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        //public static async Task<bool> GetEntityFiles(string entityId, List<PlayFabFileDetails> files, EntityType entityType = EntityType.title_player_account, int retryAttemptsOnFailure = 2)
        //{
        //    bool hasOperationError = false;

        //    var request = new PlayFab.DataModels.GetFilesRequest()
        //    {
        //        Entity = new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType.ToString() },
        //        AuthenticationContext = GetPlayFabAuthenticationContext()
        //    };

        //    var requestResult = await PlayFabDataAPI.GetFilesAsync(request);

        //    if (requestResult.Error == null)
        //    {
        //        foreach (var file in files)
        //        {
        //            if (requestResult.Result.Metadata.ContainsKey(file.FileName))
        //            {
        //                var fileInfo = requestResult.Result.Metadata[file.FileName];
        //                var client = new HttpClient();
        //                var httpGetResult = await client.GetAsync(fileInfo.DownloadUrl);

        //                if (httpGetResult.IsSuccessStatusCode)
        //                {
        //                    file.ExistOnServer = true;
        //                    file.Data = await httpGetResult.Content.ReadAsByteArrayAsync();
        //                    file.DataAsString = await httpGetResult.Content.ReadAsStringAsync();
        //                }
        //                else
        //                {
        //                    hasOperationError = true;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        hasOperationError = true;
        //    }

        //    return !hasOperationError;

        //}
        //public static async Task<bool> UploadEntityFiles(string entityId, List<PlayFabFileDetails> files, EntityType entityType = EntityType.title_player_account, int retryAttemptsOnFailure = 2)
        //{
        //    bool hasOperationError = false;

        //    var request = new PlayFab.DataModels.InitiateFileUploadsRequest
        //    {
        //        Entity = new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType.ToString() },
        //        FileNames = files.Select(y => y.FileName).ToList(),
        //        AuthenticationContext = GetPlayFabAuthenticationContext()
        //    };

        //    var requestResult = await PlayFabDataAPI.InitiateFileUploadsAsync(request);

        //    if (requestResult.Result != null)
        //    {
        //        foreach (var fileInfo in requestResult.Result.UploadDetails)
        //        {
        //            var fileUploadDetails = files.Where(y => y.FileName == fileInfo.FileName).SingleOrDefault();

        //            if (fileUploadDetails != null)
        //            {
        //                var client = new HttpClient();

        //                if (fileUploadDetails.DataAsString != null)
        //                {
        //                    StringContent data = new StringContent(fileUploadDetails.DataAsString);
        //                    var response = await client.PutAsync(fileInfo.UploadUrl, data);
        //                    if (!response.IsSuccessStatusCode)
        //                    {
        //                        hasOperationError = true;
        //                    }
        //                }
        //                else if (fileUploadDetails.Data != null)
        //                {
        //                    ByteArrayContent data = new ByteArrayContent(fileUploadDetails.Data);
        //                    var response = await client.PutAsync(fileInfo.UploadUrl, data);
        //                    if (!response.IsSuccessStatusCode)
        //                    {
        //                        hasOperationError = true;
        //                    }
        //                }
        //                else
        //                {
        //                    hasOperationError = true;
        //                }
        //            }
        //            else
        //            {
        //                hasOperationError = true;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (requestResult.Error != null && retryAttemptsOnFailure > 0)
        //        {
        //            switch (requestResult.Error.Error)
        //            {
        //                case PlayFabErrorCode.EntityFileOperationPending:
        //                    await Task.Delay(120);
        //                    return await UploadEntityFiles(entityId, files, entityType, retryAttemptsOnFailure - 1);
        //            }
        //        }

        //        hasOperationError = true;
        //    }


        //    var finalizeRequest = new PlayFab.DataModels.FinalizeFileUploadsRequest
        //    {
        //        Entity = new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType.ToString() },
        //        FileNames = files.Select(y => y.FileName).ToList(),
        //        AuthenticationContext = GetPlayFabAuthenticationContext()
        //    };


        //    await PlayFabDataAPI.FinalizeFileUploadsAsync(finalizeRequest);


        //    return !hasOperationError;

        //}



    }
}
