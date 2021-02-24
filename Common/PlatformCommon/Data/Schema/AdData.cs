using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.State;
using MetaLoop.Common.PlatformCommon;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetaLoop.Common.PlatformCommon.Settings;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{

    public abstract partial class AdData : RewardObject
    {
        private int id;
        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        public AdPlacementType PlacementType { get; set; }
        public string PlacementId { get; set; }
        public int DailyMaxUse { get; set; }
        public bool Enabled { get; set; }

        public string GetPlacementId()
        {
            if (!string.IsNullOrEmpty(PlacementId))
            {
                return PlacementId;
            }
            else
            {
                return PlacementType.ToString();
            }
        }

        public static AdData GetAdDataForPlacementType(AdPlacementType type)
        {
            return DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(AdData)]).Cast<AdData>().Where(y => y.PlacementType == type).SingleOrDefault();
        }

        public static T GetById<T>(AdPlacementType type) where T : new()
        {
            return (T)DataLayer.Instance.GetTable<T>().Where(y => ((AdData)(object)y).PlacementType == type).SingleOrDefault();
        }


#if !BACKOFFICE

        public int AvailableCount
        {
            get
            {
                int numOfTimeUsed = 0;
                var adStateEntry = MetaDataStateBase.Current.AdState.AllAdStateItems.Where(y => y.AdType == this.PlacementType).SingleOrDefault();
                if (adStateEntry != null) numOfTimeUsed = adStateEntry.UseCount;
                int availableCount = AdData.GetAdDataForPlacementType(this.PlacementType).DailyMaxUse - numOfTimeUsed;
                return availableCount;
            }
        }

        public int UseCount
        {
            get
            {
                int numOfTimeUsed = 0;
                var adStateEntry = MetaDataStateBase.Current.AdState.AllAdStateItems.Where(y => y.AdType == this.PlacementType).SingleOrDefault();
                if (adStateEntry != null) numOfTimeUsed = adStateEntry.UseCount;
                return numOfTimeUsed;
            }
        }

#endif



    }
}