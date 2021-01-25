using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public sealed class RewardDataItem
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

        public string SkuId { get; set; }
        public int Reward_Id { get; set; }
        public int Consumable_Id { get; set; }
        public int Amount { get; set; }
        public int DropRate { get; set; } 
        public bool FirstTimeOnly { get; set; }

        public void FlushCache()
        {
            consumable = null;
        }

        private Consumable consumable;
        [IgnoreCodeFirst, Ignore]
        public Consumable Consumable
        {
            get
            {
                if (consumable == null)
                    consumable = Consumable.GetById(Consumable_Id);
                return consumable;
            }

       
        }

    }
}
