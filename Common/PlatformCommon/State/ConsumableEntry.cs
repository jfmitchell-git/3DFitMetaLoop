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
                return Amount;
            }
        }

        [JsonIgnore]
        public Consumable Consumable
        {
            get
            {
                if (cache != null) return cache;
                cache = MetaLoop.Common.PlatformCommon.Data.Schema.Consumable.GetByName(Id);
                return cache;
            }
        }

    }
}
