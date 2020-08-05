using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public class LootCrateData : CostObject
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

        public string KitName { get; set; }
        public int CostConsumable_Id { get; set; }


        private List<LootCrateDataItem> kitDataItems;
        [Ignore, IgnoreCodeFirst]
        public List<LootCrateDataItem> KitDataItems
        {
            get
            {
                if (kitDataItems == null)
                {
                    kitDataItems = DataLayer.Instance.GetTable<LootCrateDataItem>().Where(y => y.KitData_Id == this.id).ToList();
                }
                return kitDataItems;
            }
        }



        public string DisplayName
        {
            get
            {
                return KitName;
            }
        }

    }
}
