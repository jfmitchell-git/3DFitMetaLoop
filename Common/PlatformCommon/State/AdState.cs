using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.State
{
    [Serializable]
    public class AdStateItem
    {
        public AdPlacementType AdType { get; set; }
        public int UseCount { get; set; }
        public AdStateItem()
        {

        }
        public AdStateItem(AdPlacementType adType)
        {
            AdType = adType;
        }
    }


    [Serializable]
    public class AdState
    {

        public List<AdStateItem> AllAdStateItems;

        public AdState()
        {
            AllAdStateItems = new List<AdStateItem>();
        }

        public void Reset()
        {
            if (AllAdStateItems == null) AllAdStateItems = new List<AdStateItem>();
            AllAdStateItems.Clear();
        }

        public AdStateItem GetStateItem(AdPlacementType type)
        {
            var item = AllAdStateItems.Where(y => y.AdType == type).SingleOrDefault();
            if (item == null)
            {
                item = new AdStateItem(type);
                AllAdStateItems.Add(item);
            }

            return item;
        }

        public void RegisterAdWatch(AdPlacementType type, MetaDataStateBase playerState, Action getAdRewardForTypeMethod)
        {
            AdData adData = AdData.GetAdDataForPlacementType(type);
            AdStateItem adStateItem = GetStateItem(adData.PlacementType);

            if (adData != null && (adStateItem.UseCount < adData.DailyMaxUse || adData.DailyMaxUse == 0))
            {
                adStateItem.UseCount++;

                if (getAdRewardForTypeMethod != null) getAdRewardForTypeMethod.Invoke();
           
                playerState.TotalAdWatchedCount++;
            }


        }



    }
}
