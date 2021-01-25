using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public sealed class LootTableDataItem
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

        public int LootTable_Id { get; set; }
        public int Consumable_Id { get; set; }
        public int Amount { get; set; }
        public float Weight { get; set; }

        [Ignore, IgnoreCodeFirst]
        public Consumable Consumable
        {
            get
            {
                return Consumable.GetById(Consumable_Id);
            }
        }

    }
}
