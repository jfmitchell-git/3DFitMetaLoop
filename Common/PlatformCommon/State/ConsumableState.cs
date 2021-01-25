using MetaLoop.Common.PlatformCommon.Data.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.State
{
    [Serializable]
    public class ConsumableState
    {
        public int soft_currency_earned { get; set; }
        public int soft_currency_bought { get; set; }
        public int soft_currency_spent { get; set; }

        public int hard_currency_earned { get; set; }
        public int hard_currency_bought { get; set; }
        public int hard_currency_spent { get; set; }

        public List<ConsumableEntry> Consumables { get; set; }

        public delegate void ConsumableStateChangedEvent(Consumable consumable);

        [field: NonSerialized]
        public event ConsumableStateChangedEvent OnConsumableStateChanged;

        public ConsumableState()
        {

            Consumables = new List<ConsumableEntry>();
        }

        public ConsumableEntry GetConsumable(Consumable consumable)
        {
            return Consumables.Where(y => y.Id == consumable.Name).SingleOrDefault();
        }

        public ConsumableEntry GetConsumable(string consumableId)
        {
            return Consumables.Where(y => y.Id == consumableId).SingleOrDefault();
        }


        public void SetConsumableAmount(Consumable consumable, int amount, string source = "")
        {
            var currentEntry = GetConsumable(consumable);

            if (currentEntry == null)
            {
                AddConsumable(consumable, amount, source);
            }
            else
            {
                int diffToAdd = amount - currentEntry.Amount;
                AddConsumable(consumable, diffToAdd, source);
            }
        }

        public ConsumableEntry AddConsumable(Consumable consumable, int amount, string source = "")
        {
            ConsumableEntry consumableEntry = GetConsumable(consumable);

            if (consumableEntry == null)
            {
                consumableEntry = new ConsumableEntry();
                consumableEntry.Id = consumable.Name;
                Consumables.Add(consumableEntry);
            }

            consumableEntry.Amount += amount;

            HandleStats(consumable, amount, source);

            //event
            if (OnConsumableStateChanged != null)
                OnConsumableStateChanged.Invoke(consumable);

            return consumableEntry;
        }


        private void HandleStats(Consumable consumable, int amount, string source)
        {

            //switch (consumable.Name)
            //{
            //    case MetaStateSettings.GoldId:

            //        if (amount > 0)
            //        {
            //            switch (source)
            //            {
            //                case MetaStateSettings.ConsumableStats_Shop:
            //                    soft_currency_bought += amount;
            //                    break;

            //                default:
            //                    soft_currency_earned += amount;
            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            soft_currency_spent += Math.Abs(amount);
            //        }

            //        break;

            //    case MetaStateSettings.HardCurrencyId:
            //        if (amount > 0)
            //        {
            //            switch (source)
            //            {
            //                case MetaStateSettings.ConsumableStats_Shop:
            //                    hard_currency_bought += amount;
            //                    break;

            //                default:
            //                    hard_currency_earned += amount;
            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            hard_currency_spent += Math.Abs(amount);
            //        }

            //        break;


            //        break;

            //}
        }

        public int GetConsumableAmount(Consumable consumable)
        {

            return Consumables.Where(y => y.Consumable != null && (y.Consumable == consumable )).Sum(y => y.TotalAmount);
        }

        public int CheckBalance(Consumable consumable, int amount)
        {
            return GetConsumableAmount(consumable) - amount;
        }


        public bool SpendConsumable(Consumable consumable, int amount, string source = "")
        {
            if (CheckBalance(consumable, amount) >= 0)
            {
                GetConsumable(consumable).Amount -= amount;

                HandleStats(consumable, -amount, source);
                //event
                if (OnConsumableStateChanged != null)
                    OnConsumableStateChanged.Invoke(consumable);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void SpendConsumables(List<ConsumableCostItem> consumableCostItems)
        {
            foreach (ConsumableCostItem consumableCostItem in consumableCostItems)
            {
                SpendConsumable(consumableCostItem.Consumable, consumableCostItem.Ammount);
            }
        }

        public bool CheckBalances(List<ConsumableCostItem> consumableCostItems)
        {
            foreach (ConsumableCostItem costItem in consumableCostItems)
            {

                if (CheckBalance(costItem.Consumable, costItem.Ammount) < 0)
                {
                    return false;


                }

            }

            return true;
        }
    }
}
