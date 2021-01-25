using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using MetaLoop.Common.PlatformCommon.Settings;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public sealed class LootCrateDataItem
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

        public int KitData_Id { get; set; }
        public int LootTableData_Id { get; set; }
        public int SlotId { get; set; }
        public float Weight { get; set; }

  
        private LootTableData lootTableData;
        [Ignore, IgnoreCodeFirst]
        public LootTableData LootTableData
        {
            get
            {
                if (lootTableData == null)
                {
                    lootTableData = (LootTableData)DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(LootTableData)]).Where(y => ((LootTableData)y).Id == LootTableData_Id).SingleOrDefault();
                }
                return lootTableData;
            }
        }

    }
}
