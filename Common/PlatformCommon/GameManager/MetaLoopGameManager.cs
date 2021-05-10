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
    public class MetaLoopGameManager : MonoBehaviour
    {

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
                GameData.OnGameDataReady += GameData_OnGameDataReady;
                GameServiceManager.GameService.OnGameServiceEvent += GameService_OnGameServiceEvent;

                GameData.Load();


                //force reflection engine. 
                new MetaStateSettings();
#if PROD
                UnityWebRequestHandler.Instance.GetBodyFromHttp(MetaStateSettings._ProductionEndpoint + MetaStateSettings._ServerAppVersionUrl, null, (UnityWebRequest r) => AwakeFirtsInit_ServerInfoReady(r));
#else
                AwakeFirtsInit_ServerInfoReady(null);
#endif
            }


            IsFirtsStart = false;
        }

        public void RestartMetaLoop(Action onRestartMetaLoopCompleted)
        {
            if (DataLayer.Instance != null)
            {
                DataLayer.Instance.Kill();
            }
#if PROD
            UnityWebRequestHandler.Instance.GetBodyFromHttp(MetaStateSettings._ProductionEndpoint + MetaStateSettings._ServerAppVersionUrl, null, (UnityWebRequest r) => AwakeFirtsInit_ServerInfoReady(r));
#else
            AwakeFirtsInit_ServerInfoReady(null);
#endif

            this.OnRestartMetaLoopCompleted = onRestartMetaLoopCompleted;
        }


        protected virtual void AwakeFirtsInit_ServerInfoReady(UnityWebRequest serverResponse)
        {

            Debug.Log("MetaLoopGameManager Fetching AppVersion.txt Completed.");

            if (serverResponse != null && serverResponse.isDone && !serverResponse.isHttpError && !serverResponse.isNetworkError)
            {
                string majorServerVersion = serverResponse.downloadHandler.text;
                if (majorServerVersion != MetaStateSettings.GetMajorVersion())
                {
                    int localVersion = Convert.ToInt32(MetaStateSettings.GetMajorVersion().Replace(".", ""));
                    int serverVersion = 0;
                    try
                    {
                        serverVersion = Convert.ToInt32(majorServerVersion.Replace(".", ""));
                    }
                    catch
                    {
                        ShowUnavailableMessage(GameUnavailableMessageType.INTERNET_ERROR);
                        return;
                    }

                    if (localVersion > serverVersion) UseStagingForPreProdBuild = true;
                }

            }
            else if (serverResponse != null && (!serverResponse.isDone || serverResponse.isHttpError || serverResponse.isNetworkError))
            {
                ShowUnavailableMessage(GameUnavailableMessageType.INTERNET_ERROR);
                return;
            }

            PlayFabManager.Instance.BackOfficeEnvironement = BackOfficeEnvironement.Staging;

#if PROD
            PlayFabManager.Instance.BackOfficeEnvironement = BackOfficeEnvironement.Prod;
            if (UseStagingForPreProdBuild) PlayFabManager.Instance.BackOfficeEnvironement = BackOfficeEnvironement.Staging;
#endif

#if UNITY_EDITOR
            //PlayFabManager.Instance.BackOfficeEnvironement = BackOfficeEnvironement.Dev;
#endif



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

            PlayFabSettings.RequestType = WebRequestType.UnityWebRequest;


            PlayFabManager.Instance.OnDataMissMatchDetected += OnDataMissMatchDetected;


            Debug.Log("MetaLoopGameManager Setting Environement: " + PlayFabManager.Instance.BackOfficeEnvironement.ToString());



            if (IsFirtsStartInPorgress && !GameServiceManager.IsInit)
            {
                Debug.Log("MetaLoopGameManager GameData_OnGameDataReady; Starting GameServiceManager Login...");
                DOVirtual.DelayedCall(0.5f, () => GameServiceManager.GameService.Init());
                //DOVirtual.DelayedCall(15f, () => CancelGameServiceManager());

            }
            else
            {
                PlayFabManager.Instance.Login(OnPlayFabLoginSuccess, OnPlayFabLoginFailed, SystemInfo.deviceUniqueIdentifier);
                Debug.Log("GAMEMANAGER LoginPlayFab MANUAL");
            }


        }

        private void CancelGameServiceManager()
        {
            if (gameServiceResponded) return;
            GameService_OnGameServiceEvent(new GameServiceEvent(GameServiceEventType.SingInFailed));
        }

        protected virtual void GameData_OnGameDataReady(bool isNewLocalProfile = false)
        {


        }


        private void GameService_OnGameServiceEvent(GameServiceEvent e)
        {

            Debug.Log("GameService_OnGameServiceEvent - " + e.EventType.ToString());

            switch (e.EventType)
            {
                case GameServiceEventType.SignInSuccess:
                case GameServiceEventType.SingInFailed:
                    Debug.Log("MetaLoopGameManager GameData_OnGameDataReady; Login in on PlayFab.");
                    PlayFabManager.Instance.Login(OnPlayFabLoginSuccess, OnPlayFabLoginFailed, SystemInfo.deviceUniqueIdentifier);
                    gameServiceResponded = true;
                    break;
            }

            //GameServiceManager.GameService.OnGameServiceEvent -= GameService_OnGameServiceEvent;
        }

        protected virtual void OnPlayFabLoginSuccess(LoginResult obj)
        {
            Debug.Log("MetaLoopGameManager OnPlayFabLoginSuccess, Downloading TitleData...");
            DownloadTitleData();
        }
        protected virtual void DownloadTitleData()
        {
            GetTitleDataRequest getTitleDataRequest = new GetTitleDataRequest();
            getTitleDataRequest.Keys = new List<string>() { MetaStateSettings._TitleDataKey_CdnManifest, MetaStateSettings._TitleDataKey_ServerInfo, MetaStateSettings._TitleDataKey_EventManager };
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

            EventManagerState eventManagerState;
            if (result.Data.ContainsKey(MetaStateSettings._TitleDataKey_EventManager))
            {
                eventManagerState = JsonConvert.DeserializeObject<EventManagerState>(result.Data[MetaStateSettings._TitleDataKey_EventManager]);
            }
            else
            {
                eventManagerState = new EventManagerState();
            }

            // GameData.Current.EventManagerState = eventManagerState;

            Debug.Log("MetaLoopGameManager Downloading Startup Remote Assets");

            RemoteAssetsManager.Instance.DownloadStartupAssets(OnRemoteAssetsDownloaded);

        }

        protected virtual void OnRemoteAssetsDownloaded()
        {
            Debug.Log("MetaLoopGameManager Startup Assets Completed, Login in BackOffice.");
            BackOfficeLogin();
        }



        protected virtual void BackOfficeLogin()
        {
            CloudScriptMethod method = new CloudScriptMethod();
            method.Method = "PlayerLogin";
            //Use case only valid for BOTS//Impersonates
            if (!PlayFabManager.IsImpersonating)
            {
                method.Params.Add("DisplayName", PlayFabManager.Instance.PlayerName);
            }

            method.Params.Add("UniqueId", SystemInfo.deviceUniqueIdentifier);
            PlayFabManager.Instance.InvokeBackOffice(method, OnBackOfficePlayerLoginComplete);
        }

        protected virtual void OnPlayFabLoginFailed(PlayFabError obj) { }

        protected virtual void OnBackOfficePlayerLoginComplete(CloudScriptResponse response, CloudScriptMethod method)
        {
            Debug.Log("MetaLoopGameManager BackOffice Logged In, Downloading User Data...");
            List<string> keys = MetaStateSettings._UserDataToDownload;
            var request = new GetUserDataRequest { Keys = keys };
            PlayFabClientAPI.GetUserReadOnlyData(request, OnUserDataComplete, OnUserDataError);
        }
        protected virtual void OnUserDataError(PlayFabError obj)
        {
            ShowUnavailableMessage(GameUnavailableMessageType.HOST_UNREACHABLE);
        }
        protected virtual void OnUserDataComplete(GetUserDataResult result)
        {
            Debug.Log("MetaLoopGameManager User Data downloaded...");

            if (result != null && result.Data.ContainsKey(MetaStateSettings._MetaDataStateFileName))
            {
                var gameDataObject = JsonConvert.DeserializeObject(result.Data[MetaStateSettings._MetaDataStateFileName].Value, MetaStateSettings.PolymorhTypes[typeof(MetaDataStateBase)]);
                MetaDataStateBase.LoadData((MetaDataStateBase)gameDataObject);

                OnGameDataAndBackOfficeReady();

            }
            else
            {
                ShowUnavailableMessage(GameUnavailableMessageType.BACKOFFICE_ERROR);
            }
        }


        protected virtual void OnGameDataAndBackOfficeReady()
        {
            Debug.Log("MetaLoopGameManager OnGameDataAndBackOfficeReady Instancing DataLayer...");

#if UNITY_EDITOR
            DataLayer.Instance.Init(Application.streamingAssetsPath + @"/" + MetaStateSettings._DatabaseName);

            //var caca = Application.persistentDataPath + @"/" + MetaStateSettings._RemoteAssetsPersistantName + @"/" + MetaStateSettings._AssetManagerStartupFolder + MetaStateSettings._DatabaseName;
            //DataLayer.Instance.Init(caca);
            // Debug.Log(caca);
            //DataLayer.Instance.Init(Application.persistentDataPath + "/" + MetaStateSettings._RemoteAssetsPersistantName + @"/" + MetaStateSettings._AssetManagerStartupFolder + MetaStateSettings._DatabaseName);
#else
            DataLayer.Instance.Init(Application.persistentDataPath + @"/" + MetaStateSettings._RemoteAssetsPersistantName + @"/" + MetaStateSettings._AssetManagerStartupFolder + MetaStateSettings._DatabaseName);
            //DataLayer.Instance.Init(Application.persistentDataPath + MetaSettings.RemoteAssetsPersistantName + @"/" + MetaSettings.AssetManagerStartupFolder + MetaSettings.DatabaseName);
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