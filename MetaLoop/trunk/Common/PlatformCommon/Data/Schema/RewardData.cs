using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public class RewardData
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

        public RewardType RewardType { get; set; }
        public string ObjectId { get; set; }

        private List<RewardDataItem> rewardItems;

        [IgnoreCodeFirst, Ignore]
        public List<RewardDataItem> RewardItems
        {
            get
            {
                if (rewardItems == null)
                {
                    rewardItems = DataLayer.Instance.GetTable<RewardDataItem>().Where(y => y.Reward_Id == this.id).ToList();
                }
                return rewardItems;
            }
        }
    }
}
