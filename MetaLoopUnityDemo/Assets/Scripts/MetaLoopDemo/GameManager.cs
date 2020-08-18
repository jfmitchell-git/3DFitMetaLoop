using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlatformCommon.Protocol;
using MetaLoop.Common.PlatformCommon.RemoteAssets;
using MetaLoop.Common.PlatformCommon.Server;
using MetaLoop.Common.PlatformCommon.State;
using MetaLoopDemo;
using MetaLoopDemo.Meta;
using MetaLoopDemo.Meta.Data;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    public static bool UseStagingForPreProdBuild { get; set; }
    public static bool IsFirtsStart = true;

    void Awake()
    {
        if (IsFirtsStart)
        {
#if PROD
            UnityWebRequestHandler.Instance.GetBodyFromHttp(MetaStateSettings.ServerAppVersionUrl, null, (UnityWebRequest r) => AwakeFirtsInit_ServerInfoReady(r));
#else
            AwakeFirtsInit_ServerInfoReady(null);
#endif
        }

        IsFirtsStart = false;
    }


    void AwakeFirtsInit_ServerInfoReady(UnityWebRequest serverResponse)
    {
        if (serverResponse != null && serverResponse.isDone && !serverResponse.isHttpError && !serverResponse.isNetworkError)
        {
            string majorServerVersion = serverResponse.downloadHandler.text;
            if (majorServerVersion != MetaSettings.GetMajorVersion())
            {
                int localVersion = Convert.ToInt32(MetaSettings.GetMajorVersion().Replace(".", ""));
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

        switch (PlayFabManager.Instance.BackOfficeEnvironement)
        {
            case BackOfficeEnvironement.Dev:
            case BackOfficeEnvironement.Staging:
                PlayFabSettings.TitleId = MetaSettings.PlayFabTitleId_Staging;

                break;

            case BackOfficeEnvironement.Prod:

                PlayFabSettings.TitleId = MetaSettings.PlayFabTitleId;
                break;
        }

        PlayFabSettings.RequestType = WebRequestType.UnityWebRequest;


        PlayFabManager.Instance.OnDataMissMatchDetected += OnDataMissMatchDetected;

        GameData.OnGameDataReady += GameData_OnGameDataReady;
        GameData.Load();


    }

    private void GameData_OnGameDataReady()
    {
        PlayFabManager.Instance.Login(OnPlayFabLoginSuccess, OnPlayFabLoginFailed, SystemInfo.deviceUniqueIdentifier);
    }

    private void OnPlayFabLoginSuccess(LoginResult obj)
    {
        DownloadTitleData();
    }
    private void DownloadTitleData()
    {
        GetTitleDataRequest getTitleDataRequest = new GetTitleDataRequest();
        getTitleDataRequest.Keys = new List<string>() { MetaSettings.TitleDataKey_CdnManifest, MetaSettings.TitleDataKey_ServerInfo, MetaSettings.TitleDataKey_EventManager };
        PlayFabClientAPI.GetTitleData(getTitleDataRequest, DownloadTitleData_Completed, (PlayFabError obj) => DownloadTitleData_Completed(null));
    }

    private void DownloadTitleData_Completed(GetTitleDataResult result)
    {
        if (result == null)
        {
            ShowUnavailableMessage(GameUnavailableMessageType.HOST_UNREACHABLE);
            return;
        }

        //Read ServerInfo...
        if (result.Data.ContainsKey(MetaSettings.TitleDataKey_ServerInfo))
        {
            ServerInfo serverInfo = JsonConvert.DeserializeObject<ServerInfo>(result.Data[MetaSettings.TitleDataKey_ServerInfo]);
            if (serverInfo.ServerStatus != ServerStatus.Online)
            {
                ShowUnavailableMessage(GameUnavailableMessageType.MAINTENANCE);
                return;
            }
        }

        AssetManifest assetManifest;
        if (result.Data.ContainsKey(MetaSettings.TitleDataKey_CdnManifest))
        {
            assetManifest = JsonConvert.DeserializeObject<AssetManifest>(result.Data[MetaSettings.TitleDataKey_CdnManifest]);
            RemoteAssetsManager.Init(assetManifest);
        }

        EventManagerState eventManagerState;
        if (result.Data.ContainsKey(MetaSettings.TitleDataKey_EventManager))
        {
            eventManagerState = JsonConvert.DeserializeObject<EventManagerState>(result.Data[MetaSettings.TitleDataKey_EventManager]);
        }
        else
        {
            eventManagerState = new EventManagerState();
        }

        // GameData.Current.EventManagerState = eventManagerState;

        RemoteAssetsManager.Instance.DownloadStartupAssets(OnRemoteAssetsDownloaded);

    }

    private void OnRemoteAssetsDownloaded()
    {
        BackOfficeLogin();
    }



    public void BackOfficeLogin()
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

    private void OnPlayFabLoginFailed(PlayFabError obj) { }

    private void OnBackOfficePlayerLoginComplete(CloudScriptResponse response, CloudScriptMethod method)
    {
        List<string> keys = new List<string>() { MetaSettings.MetaDataStateFileName };
        var request = new GetUserDataRequest { Keys = keys };
        PlayFabClientAPI.GetUserReadOnlyData(request, OnUserDataComplete, OnUserDataError);
    }


    private void OnUserDataError(PlayFabError obj)
    {
        ShowUnavailableMessage(GameUnavailableMessageType.HOST_UNREACHABLE);
    }



    private void OnUserDataComplete(GetUserDataResult result)
    {
        if (result != null && result.Data.ContainsKey(MetaSettings.MetaDataStateFileName))
        {
            MetaDataState metaDataState = JsonConvert.DeserializeObject<MetaDataState>(result.Data[MetaSettings.MetaDataStateFileName].Value);
            MetaDataState.LoadData(metaDataState);

            GameDataAndBackOfficeReady();

        } else
        {
            ShowUnavailableMessage(GameUnavailableMessageType.BACKOFFICE_ERROR);
        }
    }


    private void GameDataAndBackOfficeReady()
    {


#if UNITY_EDITOR
        //DataLayer.Instance.Init();
        DataLayer.Instance.Init(Application.persistentDataPath + "/" + MetaSettings.DatabaseName);
#else
        DataLayer.Instance.Init(Application.persistentDataPath + "/" + MetaSettings.DatabaseName);
#endif

        DataLayer.Instance.GetTable<TroopData>().ForEach(y => Debug.Log(y.Name));

        var troopInfo = DataLayer.Instance.GetTable<TroopData>().Where(y => y.Name == "Chicken").SingleOrDefault();

        var nextTierInfo = DataLayer.Instance.GetTable<TierData>().Where(y => (int)y.Tier == (int)troopInfo.StartTier + 1).SingleOrDefault();

        if (MetaDataState.Current.Consumables.CheckBalances(nextTierInfo.Cost.ConsumableCostItems) && true == false)
        {
            CloudScriptMethod upgradeTier = new CloudScriptMethod("UpgradeTier");
            upgradeTier.Params.Add("troopId", troopInfo.Name);
            PlayFabManager.Instance.InvokeBackOffice(upgradeTier, OnUpgradeTierResult);
        }

    }

    private void OnDataMissMatchDetected(OnDataMissMatchDetectedEventType type)
    {

    }


    // Start is called before the first frame update
    void Start()
    {


    }

    private void OnUpgradeTierResult(CloudScriptResponse r, CloudScriptMethod m)
    {
        if (r.ResponseCode == ResponseCode.Success)
        {
            //MetaDataState.Current.Consumables.SpendConsumables
            //show Upgrade Animation!!
        }
    }


    public void ShowUnavailableMessage(GameUnavailableMessageType reason)
    {
        //Show client update/unvailable message...
    }


    // Update is called once per frame
    void Update()
    {

    }
}
