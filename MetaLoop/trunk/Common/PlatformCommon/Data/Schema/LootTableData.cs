using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public class LootTableData
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

        public string ToolTableName { get; set; }

        private List<LootTableDataItem> lootTableItems;

        [Ignore, IgnoreCodeFirst]
        public List<LootTableDataItem> LootTableItems
        {
            get
            {
                if (lootTableItems == null)
                {
                    lootTableItems = DataLayer.Instance.GetTable<LootTableDataItem>().Where(y => y.LootTable_Id == this.Id).ToList();
                }
                return lootTableItems;
            }
        }

    }
}
