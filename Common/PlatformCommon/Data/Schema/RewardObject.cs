﻿
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public abstract class RewardObject
    {


        [MetaAutoGenerated]
        public int RewardData_Id { get; set; }


        protected RewardData rewardData;
        [IgnoreCodeFirst, Ignore]
        public List<RewardDataItem> Rewards
        {
            get
            {
                if (rewardData == null && RewardData_Id > 0)
                {
                    rewardData = DataLayer.Instance.GetTable<RewardData>().Where(y => y.Id == this.RewardData_Id).SingleOrDefault();
                }

                if (rewardData != null)
                {
                    return rewardData.RewardItems;
                }

                return null;
            }
        }




    }
}
