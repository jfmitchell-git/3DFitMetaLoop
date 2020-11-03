using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.State
{
    [Serializable]
    public class ConsumableEntry
    {
        public ConsumableEntry() { }
        public string Id { get; set; }
        public int Amount { get; set; }

        private Consumable cache;

        [JsonIgnore]
        public int TotalAmount
        {
            get
            {
                if (Consumable.DerivedConsumable != null)
                {
                    return Amount * Consumable.DerivedAmount;
                } else
                {
                    return Amount;
                }
            }
        }

        [JsonIgnore]
        public Consumable Consumable
        {
            get
            {
                if (cache != null) return cache;

                cache = DataLayer.Instance.GetTable<Consumable>().Where(y => y.Name == Id).SingleOrDefault();
                return cache;
            }
        }

    }
}
