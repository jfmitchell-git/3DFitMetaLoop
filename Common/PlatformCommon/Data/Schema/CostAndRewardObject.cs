﻿using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
   public class CostAndRewardObject : RewardObject, ICostObject
    {

        public int ConsumableCost_Id { get; set; }
        private ConsumableCost cost;
        [IgnoreCodeFirst]
        public ConsumableCost Cost
        {
            get
            {
                if (ConsumableCost_Id > 0)
                {
                    if (cost == null)
                        cost = DataLayer.Instance.GetTable<ConsumableCost>().Where(y => y.Id == ConsumableCost_Id).Single();
                    return cost;

                }
                else
                {
                    return null;
                }
            }
        }


    }
}
