using dryginstudios.bioinc.data.meta;
using MetaLoop.Common.PlatformCommon.Data.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.State
{
    public enum GenericRequestRewardResultType
    {
        Error = 0,
        Success = 1,
    }
    public class GenericRequestRewardResult
    {
        public GenericRequestRewardResultType Result { get; set; }
        public List<KeyValuePair<string, int>> Rewards { get; set; }
        public Dictionary<string, int> Cost { get; set; }

        public GenericRequestRewardResult()
        {
            Rewards = new List<KeyValuePair<string, int>>();
            Cost = new Dictionary<string, int>();
        }

        public ConsumableCost GetCost()
        {
            if (Cost != null)
            {
                ConsumableCost result = new ConsumableCost(true);
                foreach (KeyValuePair<string, int> entry in Cost)
                {
                    result.ConsumableCostItems.Add(new ConsumableCostItem() { ConsumableId = Consumable.GetByName(entry.Key).Id, Ammount = entry.Value });
                }
                return result;

            }
            else
            {
                return null;
            }
        }

        public List<RewardDataItem> GetReward()
        {
            List<RewardDataItem> result = new List<RewardDataItem>();
            foreach (KeyValuePair<string, int> entry in Rewards)
            {
                if (entry.Value > 0)
                    result.Add(new RewardDataItem() { Consumable_Id = Consumable.GetByName(entry.Key).Id, Amount = entry.Value });
            }

            return result;
        }
    }
}
