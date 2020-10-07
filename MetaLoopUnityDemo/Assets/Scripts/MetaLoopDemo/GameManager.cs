using DG.Tweening;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.GameManager;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlatformCommon.Protocol;
using MetaLoop.Common.PlatformCommon.RemoteAssets;
using MetaLoop.Common.PlatformCommon.Server;
using MetaLoop.Common.PlatformCommon.Settings;
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

public class GameManager : MetaLoopGameManager
{

    protected override void Awake()
    {
        //Set our custom MetaDataState Type...
        MetaDataStateType = typeof(MetaDataState);
        base.Awake();
    }


    public override void OnMetaLoopReady()
    {
        //Yay!

        DataLayer.Instance.GetTable<TroopData>().ForEach(y => Debug.Log(y.Name));

        DOVirtual.DelayedCall(5f, () => UpgradeTier(DataLayer.Instance.GetTable<TroopData>().FirstOrDefault().Name));
    }

    public void UpgradeTier(string troopId)
    {
        Debug.Log("Upgrading Tier for " + troopId);
        var troopInfo = DataLayer.Instance.GetTable<TroopData>().Where(y => y.Name == troopId).SingleOrDefault();

        var playerTroopInfo = MetaDataState.GetCurrent().TroopsData.Where(y => y.TroopId == troopId).SingleOrDefault();

        TierData nextTierInfo;
        if (playerTroopInfo != null)
        {
            nextTierInfo = DataLayer.Instance.GetTable<TierData>().Where(y => (int)y.Tier == (int)troopInfo.StartTier + 1).SingleOrDefault();
        }
        else
        {
            nextTierInfo = DataLayer.Instance.GetTable<TierData>().First();
        }

        if (MetaDataState.Current.Consumables.CheckBalances(nextTierInfo.Cost.ConsumableCostItems))
        {
            CloudScriptMethod upgradeTier = new CloudScriptMethod("UpgradeTier");
            upgradeTier.Params.Add("troopId", troopId);
            PlayFabManager.Instance.InvokeBackOffice(upgradeTier, (CloudScriptResponse r, CloudScriptMethod m) => OnUpgradeTierResult(r, troopId));
        }

    }

    private void OnUpgradeTierResult(CloudScriptResponse r, string troopId)
    {
        if (r.ResponseCode == ResponseCode.Success)
        {
            Debug.Log("SUCCESS Upgrading Tier for " + troopId);

            var playerTroopInfo = MetaLoopDemo.Meta.MetaDataState.GetCurrent().TroopsData.Where(y => y.TroopId == troopId).SingleOrDefault();

            if (playerTroopInfo == null)
            {
                playerTroopInfo = new TroopDataState() { TroopId = troopId };
                MetaDataState.GetCurrent().TroopsData.Add(playerTroopInfo);
            }

            playerTroopInfo.CurrentTier = (TierType)((int)playerTroopInfo.CurrentTier + 1);

        }
    }


}
