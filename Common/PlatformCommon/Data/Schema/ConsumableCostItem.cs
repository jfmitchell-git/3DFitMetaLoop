using SQLite4Unity3d;
using System.Linq;
using MetaLoop.Common.PlatformCommon.Data;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Settings;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public sealed class ConsumableCostItem
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

        public int ConsumableCost_Id { get; set; }
        public int ConsumableId { get; set; }

        private Consumable consumable;
        [IgnoreCodeFirst, Ignore]
        public Consumable Consumable
        {
            get
            {

                if (consumable == null)
                    consumable = Consumable.GetById(ConsumableId);
                return consumable;
            }
        }

        public int Ammount { get; set; }

    }
}
