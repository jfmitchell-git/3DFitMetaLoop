using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.State;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using MetaLoop.Common.PlatformCommon.Settings;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{

    public abstract class VariableCostData : CostObject
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

        public VariableCostDataType PurchaseType { get; set; }
        public int PurchaseCount { get; set; }

        [Ignore, IgnoreCodeFirst]
        public ConsumableCostItem PrimaryConsumable
        {
            get
            {
                return Cost.ConsumableCostItems.ElementAtOrDefault(0);
            }
        }

        [Ignore, IgnoreCodeFirst]
        public ConsumableCostItem SecondaryConsumable
        {
            get
            {
                return Cost.ConsumableCostItems.ElementAtOrDefault(1);
            }
        }

        public static VariableCostData GetVariableCostData(VariableCostDataType costType, int count)
        {
            if (count == 0) count = 1;

            VariableCostData costData;
            costData = DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(VariableCostData)]).Cast<VariableCostData>().Where(y => y.PurchaseType == costType && y.PurchaseCount == count).SingleOrDefault();

            if (costData == null)
            {
                costData = DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(VariableCostData)]).Cast<VariableCostData>().Where(y => y.PurchaseType == costType).OrderBy(y => y.PurchaseCount).Last();
            }

            return costData;
        }
        public static ConsumableCost GetVariableCost(VariableCostDataType costType, MetaDataStateBase state)
        {
            VariableCostData costData;
            int purchaseCount = state.ShopState.GetVariableCostData(costType) + 1;

            costData = DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(VariableCostData)]).Cast<VariableCostData>().Where(y => y.PurchaseType == costType && y.PurchaseCount == purchaseCount).SingleOrDefault();

            if (costData == null)
            {
                costData = DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(VariableCostData)]).Cast<VariableCostData>().Where(y => y.PurchaseType == costType).OrderBy(y => y.PurchaseCount).Last();
            }

            ConsumableCost result = new ConsumableCost(true);

            if (costData.SecondaryConsumable == null)
            {
                result.ConsumableCostItems.Add(new ConsumableCostItem() { ConsumableId = costData.PrimaryConsumable.ConsumableId, Ammount = costData.PrimaryConsumable.Ammount });
            }
            else
            {
                if (state.Consumables.CheckBalance(costData.PrimaryConsumable.Consumable, costData.PrimaryConsumable.Ammount) >= 0)
                {
                    result.ConsumableCostItems.Add(new ConsumableCostItem() { ConsumableId = costData.PrimaryConsumable.ConsumableId, Ammount = costData.PrimaryConsumable.Ammount });
                }
                else
                {
                    result.ConsumableCostItems.Add(new ConsumableCostItem() { ConsumableId = costData.SecondaryConsumable.ConsumableId, Ammount = costData.SecondaryConsumable.Ammount });
                }
            }

            return result;

        }



    }
}
