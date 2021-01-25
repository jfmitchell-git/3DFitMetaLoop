using MetaLoop.Common.PlatformCommon;
using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public sealed partial class ConsumableCost
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
            }
        }

        public ConsumableCost()
        {

        }

        public ConsumableCost(bool isVirtual)
        {
            if (isVirtual) consumableCostItems = new List<ConsumableCostItem>();
        }

        private List<ConsumableCostItem> consumableCostItems;
        public List<ConsumableCostItem> ConsumableCostItems
        {
            get
            {
                if (consumableCostItems == null)
                    consumableCostItems = DataLayer.Instance.GetTable<ConsumableCostItem>().Where(y => y.ConsumableCost_Id == id).ToList();
                return consumableCostItems;
            }
        }
    }
}
