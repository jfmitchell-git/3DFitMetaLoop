using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.State;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{

    public class VariableCostData
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
        public int Amount { get; set; }
        public int PrimaryConsumable_Id { get; set; }
        public int SecondaryConsumable_Id { get; set; }


        [Ignore, IgnoreCodeFirst]
        public Consumable PrimaryConsumable
        {
            get
            {
                if (PrimaryConsumable_Id > 0)
                {
                    return Consumable.GetById(PrimaryConsumable_Id);
                }
                return null;
            }
        }

        [Ignore, IgnoreCodeFirst]
        public Consumable SecondaryConsumable
        {
            get
            {
                if (SecondaryConsumable_Id > 0)
                {
                    return Consumable.GetById(SecondaryConsumable_Id);
                }
                return null;
            }
        }

        public static VariableCostData GetVariableCostData(VariableCostDataType costType, int count)
        {
            if (count == 0) count = 1;

            VariableCostData costData;
            costData = DataLayer.Instance.GetTable<VariableCostData>().Where(y => y.PurchaseType == costType && y.PurchaseCount == count).SingleOrDefault();

            if (costData == null)
            {
                costData = DataLayer.Instance.GetTable<VariableCostData>().Where(y => y.PurchaseType == costType).OrderBy(y => y.PurchaseCount).Last();
            }

            return costData;
        }
        public static ConsumableCost GetVariableCost(VariableCostDataType costType, MetaDataStateBase state)
        {
            VariableCostData costData;
            int purchaseCount = state.ShopState.GetVariableCostData(costType) + 1;

            costData = DataLayer.Instance.GetTable<VariableCostData>().Where(y => y.PurchaseType == costType && y.PurchaseCount == purchaseCount).SingleOrDefault();

            if (costData == null)
            {
                costData = DataLayer.Instance.GetTable<VariableCostData>().Where(y => y.PurchaseType == costType).OrderBy(y => y.PurchaseCount).Last();
            }

            ConsumableCost result = new ConsumableCost(true);

            if (costData.SecondaryConsumable == null)
            {
                result.ConsumableCostItems.Add(new ConsumableCostItem() { ConsumableId = costData.PrimaryConsumable.Id, Ammount = costData.Amount });
            }
            else
            {
                if (state.Consumables.CheckBalance(costData.PrimaryConsumable, costData.Amount) >= 0)
                {
                    result.ConsumableCostItems.Add(new ConsumableCostItem() { ConsumableId = costData.PrimaryConsumable.Id, Ammount = costData.Amount });
                }
                else
                {
                    result.ConsumableCostItems.Add(new ConsumableCostItem() { ConsumableId = costData.SecondaryConsumable.Id, Ammount = costData.Amount });
                }
            }

            return result;

        }



    }
}
