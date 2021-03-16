#if !BACKOFFICE
using DG.Tweening;
using Facebook.Unity;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon.GameServices;
using MetaLoop.Common.PlatformCommon.HttpClient;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlatformCommon.Protocol;
using MetaLoop.Common.PlatformCommon.RemoteAssets;
using MetaLoop.Common.PlatformCommon.Server;
using MetaLoop.Common.PlatformCommon.Settings;
using MetaLoop.Common.PlatformCommon.State;
using MetaLoop.Common.PlatformCommon.UserProfile;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace MetaLoop.Common.PlatformCommon.GameManager
{
    public class MetaLoopGameManagerLite : MonoBehaviour
    {
        public bool IsOfflineMode { get; set; }

        private bool gameServiceResponded = false;
        public bool RequireMetaLoopRestart { get; set; }
        public static bool UseStagingForPreProdBuild { get; set; }
        public static bool IsFirtsStart = true;
        public static bool IsFirtsStartInPorgress = true;
        private int lastServerInfoCacheVersion;
        public bool IsGameAvailable { get; set; }

        private bool isPlayFabConfigReady = false;

        public Action OnRestartMetaLoopCompleted = null;

        public bool IsMetaLoopReady = false;
        public bool IsNewInstall { get; set; }

        public UnityEvent OnMetaLoopCompletedCallback { get; internal set; }

        private bool forceCloudVersionFromAccountSync = false;
        protected virtual void Awake()
        {
            OnMetaLoopCompletedCallback = new UnityEvent();
            Debug.Log("MetaLoopGameManager Awake() invoked.");



        }

        protected virtual void Start()
        {
            if (IsFirtsStart)
            {
                new MetaStateSettings();

                GameServiceManager.GameService.OnGameServiceEvent += GameService_OnGameServiceEvent;

                switch (PlayFabManager.Instance.BackOfficeEnvironement)
                {
                    case BackOfficeEnvironement.Dev:
                    case BackOfficeEnvironement.Staging:
                        PlayFabSettings.TitleId = MetaStateSettings._PlayFabTitleId_Staging;
                        break;

                    case BackOfficeEnvironement.Prod:
                        PlayFabSettings.TitleId = MetaStateSettings._PlayFabTitleId;
                        break;
                }

                PlayFabManager.Instance.OnDataMissMatchDetected += OnDataMissMatchDetected;
                Debug.Log("MetaLoopGameManager Setting Environement: " + PlayFabManager.Instance.BackOfficeEnvironement.ToString());

                GameData.OnGameDataReady += GameData_OnGameDataReady;
                GameData.Load();

            }


            IsFirtsStart = false;
        }

        public void RestartMetaLoop(Action onRestartMetaLoopCompleted, bool forceCloudVersionFromAccountSync = false)
        {
            this.forceCloudVersionFromAccountSync = forceCloudVersionFromAccountSync;

            if (DataLayer.Instance != null)
            {
                DataLayer.Instance.Kill();
            }

            this.OnRestartMetaLoopCompleted = onRestartMetaLoopCompleted;

            GameData_OnGameDataReady(false);
        }

        protected virtual void GameData_OnGameDataReady(bool isNewProfile)
        {
            IsNewInstall = isNewProfile;



            switch (GameData.Current.LoginType)
            {
                case LoginType.Undefined:
                case LoginType.Device:
                    LoginToPlayFabAsDefault();
                    break;

                case LoginType.GameService:
                    GameServiceManager.GameService.Init();
                    break;


                case LoginType.Facebook:

                    if (FB.IsLoggedIn)
                    {
                        FB.Mobile.RefreshCurrentAccessToken();
                    }


                    if (Facebook.Unity.AccessToken.CurrentAccessToken != null)
                    {
                        PlayFabManager.Instance.LoginWithFacebookAccount(Facebook.Unity.AccessToken.CurrentAccessToken.TokenString, OnPlayFabLoginSuccess, OnPlayFabLoginFailed);
                        Debug.Log("Facebook Rapid login success: " + Facebook.Unity.AccessToken.CurrentAccessToken.UserId);
                    } else
                    {
                        LoginToPlayFabAsDefault();
                    }

                    break;
            }


        }


        private void LoginToPlayFabAsDefault()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
#if UNITY_EDITOR
            deviceId = UserProfileManager.Instance.UserProfileData.Id;
#endif
            PlayFabManager.Instance.Login(OnPlayFabLoginSuccess, OnPlayFabLoginFailed, deviceId);
        }

       

        private void GameService_OnGameServiceEvent(GameServiceEvent e)
        {

            Debug.Log("GameService_OnGameServiceEvent - " + e.EventType.ToString());

            switch (e.EventType)
            {
                case GameServiceEventType.SignInSuccess:

                    Debug.Log("MetaLoopGameManager GameData_OnGameDataReady; Login in on PlayFab.");
                    PlayFabManager.Instance.Login(OnPlayFabLoginSuccess, OnPlayFabLoginFailed, SystemInfo.deviceUniqueIdentifier);
                    gameServiceResponded = true;
                    break;

                case GameServiceEventType.SingInFailed:
                    LoginToPlayFabAsDefault();
                    break;
            }
        }

        protected virtual void OnPlayFabLoginSuccess(PlayFab.ClientModels.LoginResult obj)
        {
            Debug.Log("MetaLoopGameManager OnPlayFabLoginSuccess, Downloading TitleData...");
            DownloadTitleData();
        }
        protected virtual void DownloadTitleData()
        {
            GetTitleDataRequest getTitleDataRequest = new GetTitleDataRequest();
            getTitleDataRequest.Keys = new List<string>() { MetaStateSettings._TitleDataKey_CdnManifest, MetaStateSettings._TitleDataKey_ServerInfo, MetaStateSettings._TitleDataKey_EventManager, MetaStateSettings._TitleDataKey_RemoteConfig };
            PlayFabClientAPI.GetTitleData(getTitleDataRequest, DownloadTitleData_Completed, (PlayFabError obj) => DownloadTitleData_Completed(null));
        }

        protected virtual void DownloadTitleData_Completed(GetTitleDataResult result)
        {
            Debug.Log("MetaLoopGameManager Downloading TitleData Completed...");
            if (result != null)
            {
                if (!ReadTitleData(result, false))
                {
                    return;
                }
            }
            else
            {
                ShowUnavailableMessage(GameUnavailableMessageType.HOST_UNREACHABLE);
                return;
            }

            AssetManifest assetManifest;
            if (result.Data.ContainsKey(MetaStateSettings._TitleDataKey_CdnManifest))
            {
                assetManifest = JsonConvert.DeserializeObject<AssetManifest>(result.Data[MetaStateSettings._TitleDataKey_CdnManifest]);
                RemoteAssetsManager.Init(assetManifest);
            }


            if (result.Data.ContainsKey(MetaStateSettings._TitleDataKey_RemoteConfig))
            {
                RemoteConfigData.Load(result.Data[MetaStateSettings._TitleDataKey_RemoteConfig]);
            }
            else
            {
                RemoteConfigData.Load(null);
            }


            EventManagerState eventManagerState;
            if (result.Data.ContainsKey(MetaStateSettings._TitleDataKey_EventManager))
            {
                eventManagerState = JsonConvert.DeserializeObject<EventManagerState>(result.Data[MetaStateSettings._TitleDataKey_EventManager]);
            }
            else
            {
                eventManagerState = new EventManagerState();
            }

            Debug.Log("MetaLoopGameManager Downloading User Data...");

            DownloadUserData();


        }

        public void DownloadUserData()
        {
            Debug.Log("MetaLoopGameManager BackOffice Logged In, Downloading User Data...");
            List<string> keys = MetaStateSettings._UserDataToDownload;
            var request = new GetUserDataRequest { Keys = keys };
            PlayFabClientAPI.GetUserData(request, OnUserDataComplete, OnUserDataError);
        }
        protected virtual void OnPlayFabLoginFailed(PlayFabError obj)
        {
            IsOfflineMode = true;
            MetaDataStateBase.LoadData(GameData.Current.MetaDataState);
            OnGameDataAndBackOfficeReady();

        }

        protected virtual void OnUserDataError(PlayFabError obj)
        {
            ShowUnavailableMessage(GameUnavailableMessageType.HOST_UNREACHABLE);
        }
        protected virtual void OnUserDataComplete(GetUserDataResult result)
        {
            Debug.Log("MetaLoopGameManager User Data downloaded...");

            if (result != null && result.Data.ContainsKey(MetaStateSettings._MetaDataStateFileName) && !string.IsNullOrEmpty(result.Data[MetaStateSettings._MetaDataStateFileName].Value))
            {
                //being able to retreive userData confirm that user can't be a new user.
                IsNewInstall = false;

                MetaDataStateBase onlineData = (MetaDataStateBase)JsonConvert.DeserializeObject(result.Data[MetaStateSettings._MetaDataStateFileName].Value, MetaStateSettings.PolymorhTypes[typeof(MetaDataStateBase)]);

                if (onlineData.Version > GameData.Current.MetaDataState.Version || forceCloudVersionFromAccountSync)
                {
                    MetaDataStateBase.LoadData(onlineData);
                    GameData.Current.OverwriteMetaDataState(onlineData);
                }
                else
                {
                    MetaDataStateBase.LoadData(GameData.Current.MetaDataState);
                }
            }
            else
            {
                MetaDataStateBase.LoadData(GameData.Current.MetaDataState);
            }

            OnGameDataAndBackOfficeReady();
        }


        protected virtual void OnGameDataAndBackOfficeReady()
        {


            Debug.Log("MetaLoopGameManager Instancing DataLayer...");

            DataLayer.Instance.InitFromStreamingAssets(MetaStateSettings._DatabaseName);

            OnMetaLoopReady();
        }


        public bool ReadTitleData(GetTitleDataResult obj, bool fromRoutineTaskManager = true)
        {
            ServerInfo serverInfo;
            if (obj != null && obj.Data.ContainsKey(MetaStateSettings._TitleDataKey_ServerInfo))
            {
                serverInfo = JsonConvert.DeserializeObject<ServerInfo>(obj.Data[MetaStateSettings._TitleDataKey_ServerInfo]);

                if ((serverInfo.CacheVersion < lastServerInfoCacheVersion) && IsGameAvailable) return true; //ignore this manifest.
                lastServerInfoCacheVersion = serverInfo.CacheVersion;

                if (serverInfo.ServerStatus != ServerStatus.Online)
                {
                    GameUnavailableMessageType gameUnavailableMessageType = GameUnavailableMessageType.MAINTENANCE;

                    if (!string.IsNullOrEmpty(serverInfo.MaintenanceMessage))
                    {
                        try
                        {
                            gameUnavailableMessageType = (GameUnavailableMessageType)Enum.Parse(typeof(GameUnavailableMessageType), serverInfo.MaintenanceMessage);
                        }
                        catch { }
                    }

                    ShowUnavailableMessage(gameUnavailableMessageType);

                    return false;
                }
                else if (serverInfo.ServerStatus == ServerStatus.Online)
                {
                    //"0.1" is wildcard for dev/staging env
                    if (serverInfo.AppVersion == MetaStateSettings.GetMajorVersion() || serverInfo.AppVersion == "0.10" || serverInfo.AppVersion == "0.1")
                    {
                        IsGameAvailable = true;

                        if (fromRoutineTaskManager && DataLayer.Instance.Connection != null)
                        {
                            if (obj != null && obj.Data.ContainsKey(MetaStateSettings._TitleDataKey_EventManager))
                            {
                                EventManagerState eventManagerState = JsonConvert.DeserializeObject<EventManagerState>(obj.Data[MetaStateSettings._TitleDataKey_EventManager]);
                                GameData.Current.EventManagerState = eventManagerState;
                                GameData.Current.EventManagerState.SyncState(EventData.GetAllEvents());
                            }
                        }
                    }
                    else
                    {
                        ShowUnavailableMessage(GameUnavailableMessageType.VERSION_MISMATCH);
                    }

                }
            }
            else
            {

                if (CheckInternetConnection())
                {
                    ShowUnavailableMessage(GameUnavailableMessageType.HOST_UNREACHABLE);
                }
                else
                {
                    ShowUnavailableMessage(GameUnavailableMessageType.INTERNET_ERROR);

                }

                return false;
            }

            return true;
        }

        public bool CheckInternetConnection()
        {
            return true;
        }

        public virtual void OnMetaLoopReady()
        {
            IsFirtsStartInPorgress = false;

            IsMetaLoopReady = true;


            if (OnRestartMetaLoopCompleted != null)
            {
                OnRestartMetaLoopCompleted.Invoke();
                OnRestartMetaLoopCompleted = null;
            }

            OnMetaLoopCompletedCallback.Invoke();
        }

        public virtual void OnDataMissMatchDetected(OnDataMissMatchDetectedEventType type)
        {

        }


        public virtual void ShowUnavailableMessage(GameUnavailableMessageType reason)
        {
            //Show client update/unvailable message...
        }

        // Update is called once per frame
        protected virtual void Update()
        {

        }
    }

}
#endif