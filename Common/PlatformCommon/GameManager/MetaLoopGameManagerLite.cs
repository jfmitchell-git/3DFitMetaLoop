#if !BACKOFFICE
using DG.Tweening;
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
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        protected virtual void Awake()
        {
            Debug.Log("MetaLoopGameManager Awake() invoked.");
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

        public void RestartMetaLoop(Action onRestartMetaLoopCompleted)
        {
            if (DataLayer.Instance != null)
            {
                DataLayer.Instance.Kill();
            }
            this.OnRestartMetaLoopCompleted = onRestartMetaLoopCompleted;
        }

        protected virtual void GameData_OnGameDataReady()
        {
            PlayFabManager.Instance.Login(OnPlayFabLoginSuccess, OnPlayFabLoginFailed, SystemInfo.deviceUniqueIdentifier);
        }




        private void GameService_OnGameServiceEvent(GameServiceEvent e)
        {

            Debug.Log("GameService_OnGameServiceEvent - " + e.EventType.ToString());

            //switch (e.EventType)
            //{
            //    case GameServiceEventType.SignInSuccess:
            //    case GameServiceEventType.SingInFailed:
            //        Debug.Log("MetaLoopGameManager GameData_OnGameDataReady; Login in on PlayFab.");
            //        PlayFabManager.Instance.Login(OnPlayFabLoginSuccess, OnPlayFabLoginFailed, SystemInfo.deviceUniqueIdentifier);
            //        gameServiceResponded = true;
            //        break;
            //}
        }

        protected virtual void OnPlayFabLoginSuccess(LoginResult obj)
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

            Debug.Log("MetaLoopGameManager Downloading Startup Remote Assets");

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
                MetaDataStateBase onlineData = (MetaDataStateBase)JsonConvert.DeserializeObject(result.Data[MetaStateSettings._MetaDataStateFileName].Value, MetaStateSettings.PolymorhTypes[typeof(MetaDataStateBase)]);

                if (onlineData.Version > GameData.Current.MetaDataState.Version)
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

#if UNITY_EDITOR
            DataLayer.Instance.Init(Application.streamingAssetsPath + @"/" + MetaStateSettings._DatabaseName);
#else
            DataLayer.Instance.Init(Application.persistentDataPath + @"/" + MetaStateSettings._RemoteAssetsPersistantName + @"/" + MetaStateSettings._AssetManagerStartupFolder + MetaStateSettings._DatabaseName);
#endif

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


            if (OnRestartMetaLoopCompleted != null)
            {
                OnRestartMetaLoopCompleted.Invoke();
                OnRestartMetaLoopCompleted = null;
            }

            GameData.Save();
        }

        public virtual void OnDataMissMatchDetected(OnDataMissMatchDetectedEventType type)
        {

        }


        // Start is called before the first frame update
        protected virtual void Start()
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