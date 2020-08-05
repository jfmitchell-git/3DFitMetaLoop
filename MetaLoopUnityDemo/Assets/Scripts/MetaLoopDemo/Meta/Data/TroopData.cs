﻿using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;

using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace MetaLoopDemo.Meta.Data
{
    [MetaAutoGenerated]
    public class TroopData : IMetaDataObject
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MetaUniqueField]
        public string Name { get; set; }
        public TierType StartTier { get; set; }

        [IgnoreCodeFirst]
        public TierData StartTierData
        {
            get
            {
                return DataLayer.Instance.GetTable<TierData>().Where(y => y.Tier == StartTier).SingleOrDefault();
            }
        }

        [MetaDependency(typeof(TroopTagData), true)]
        public int FactionTag { get; set; }

        [MetaDependency(typeof(TroopTagData), true)]
        public int RoleTag { get; set; }



    }
}
