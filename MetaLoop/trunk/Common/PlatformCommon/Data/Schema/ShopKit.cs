using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public class ShopKit : IRewardObject
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
                //Load relationships here...
            }
        }
        public ShopType ShopType { get; set; }
        public int KitData_Id { get; set; }

        public LootCrateData KitData
        {
            get
            {
                return DataLayer.Instance.GetTable<LootCrateData>().Where(y => y.Id == KitData_Id).Single();
            }
        }

        public RewardObjectType RewardObjectType
        {
            get
            {
                return RewardObjectType.Undefined;
            }
        }

        public List<RewardDataItem> PotentialRewards
        {
            get
            {
                List<RewardDataItem> allPotentialRewards = new List<RewardDataItem>();
                KitData.KitDataItems.SelectMany(y => y.LootTableData.LootTableItems).ToList().ForEach(y => allPotentialRewards.Add(new RewardDataItem() { Consumable_Id = y.Consumable_Id, Amount = y.Amount }));
                return allPotentialRewards;
            }
        }

        public string DisplayName
        {
            get
            {
                return KitData.KitName;
            }
        }
        public bool IsAvailable
        {
            get
            {
                return true;
            }
        }

        public int ZOrder
        {
            get
            {
                return 1001;
            }
        }
    }
}
