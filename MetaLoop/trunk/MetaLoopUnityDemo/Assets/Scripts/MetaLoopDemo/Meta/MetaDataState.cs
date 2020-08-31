﻿using MetaLoop.Common.PlatformCommon.State;
using MetaLoopDemo.Meta.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLoopDemo.Meta
{
    [Serializable]
    public class TroopDataState { 
        public string TroopId { get; set; }
        public TierType CurrentTier { get; set; }
    }
    public class MetaDataState : MetaDataStateBase
    {
        
        public static MetaDataState GetCurrent()
        {
            return (MetaDataState)Current;
        }

        public List<TroopDataState> TroopsData;
        public MetaDataState() : base()
        {
            TroopsData = new List<TroopDataState>();
        }
        public static MetaDataState FromJson(string data) {
            return JsonConvert.DeserializeObject<MetaDataState>(data);
        }

    }
}
