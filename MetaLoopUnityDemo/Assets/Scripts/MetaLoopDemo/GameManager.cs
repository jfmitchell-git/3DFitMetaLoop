using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlatformCommon.State;
using MetaLoopDemo;
using MetaLoopDemo.Meta;
using MetaLoopDemo.Meta.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
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
