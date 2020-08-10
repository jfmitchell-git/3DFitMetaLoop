using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlatformCommon.State;
using MetaLoopDemo;
using MetaLoopDemo.Meta;
using MetaLoopDemo.Meta.Data;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    void Awake()
    {
        PlayFabManager.Instance.BackOfficeEnvironement = BackOfficeEnvironement.Dev;

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

        PlayFabManager.Instance.Login(OnPlayFabLoginSuccess, OnPlayFabLoginFailed, SystemInfo.deviceUniqueIdentifier);


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

    private void OnBackOfficePlayerLoginComplete(CloudScriptResponse response, CloudScriptMethod method)
    {
        
    }

    private void OnPlayFabLoginFailed(PlayFabError obj)
    {
    
    }

    private void OnPlayFabLoginSuccess(LoginResult obj)
    {
        BackOfficeLogin();
    }



    private void OnDataMissMatchDetected(OnDataMissMatchDetectedEventType type)
    {
        
    }


    // Start is called before the first frame update
    void Start()
    {

        MetaDataState.LoadData(new MetaDataStateBase());

     
        Debug.Log(MetaLoop.Common.PlatformCommon.Settings.MetaStateSettings._DateTimeFormat);

        DataLayer.Instance.Init();

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

    private void OnUpgradeTierResult(CloudScriptResponse r, CloudScriptMethod m)
    {
        if (r.ResponseCode == ResponseCode.Success)
        {
            //MetaDataState.Current.Consumables.SpendConsumables
            //show Upgrade Animation!!
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
