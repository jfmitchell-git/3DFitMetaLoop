using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public class LootCrateDataItem
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
                    lootTableData = DataLayer.Instance.GetTable<LootTableData>().Where(y => y.Id == LootTableData_Id).SingleOrDefault();
                }
                return lootTableData;
            }
        }

    }
}
