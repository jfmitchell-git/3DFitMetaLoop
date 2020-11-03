using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.State
{
    [Serializable]
    public class ShopTransactionItem
    {
        public GenericRequestRewardResultType Result;
        public ShopItemType ShopType;
        public DateTime DateTime;
        public string InternalId;
        public string Arguments;
        public string ValidationId;



        public ShopTransactionItem()
        {

        }

        public ShopTransactionItem(ShopItemType shopItemType, string internalId)
        {
            InternalId = internalId;
            ShopType = shopItemType;
        }

        public void SetTransactionTime()
        {
#if BACKOFFICE
  DateTime = DateTime.UtcNow;
#else
            DateTime = MetaDataStateBase.Current.GetServerUTCDatetime();
#endif
        }

        public ShopTransactionItem(ShopItemType shopItemType, GenericRequestRewardResultType result, string internalId, string args)
        {
            InternalId = internalId;
            ShopType = shopItemType;
            Result = result;
            Arguments = args;
            SetTransactionTime();
        }
    }


    [Serializable]
    public class ShopVariableCostData
    {
        public VariableCostDataType VariableCostDataType { get; set; }
        public int UseCount { get; set; }
    }


    [Serializable]
    public class ShopState
    {
        public List<ShopTransactionItem> AllTransactions;
        public List<ShopVariableCostData> AllShopVariableCostData;

        public ShopState()
        {
            if (AllTransactions == null) AllTransactions = new List<ShopTransactionItem>();
        }

        public void CreateDefaultVariableCostValues(bool reset = false)
        {
            if (reset) AllShopVariableCostData = null;
            if (AllShopVariableCostData == null)
            {
                AllShopVariableCostData = new List<ShopVariableCostData>();
                AllShopVariableCostData.Clear(); //Odd serialisation bug?
                //AllShopVariableCostData.Add(new ShopVariableCostData() { VariableCostDataType = VariableCostDataType.CampaignEnergy, UseCount = 0 });
                //AllShopVariableCostData.Add(new ShopVariableCostData() { VariableCostDataType = VariableCostDataType.SuppliesRefresh, UseCount = 0 });
                //AllShopVariableCostData.Add(new ShopVariableCostData() { VariableCostDataType = VariableCostDataType.BlitzRefresh, UseCount = 0 });

            }
        }


        public void CleanShop()
        {
            var allOldTrans = AllTransactions.Where(y => y.ShopType != ShopItemType.Offers && y.ShopType != ShopItemType.HardCurrency && y.DateTime.AddDays(30) > DateTime.UtcNow).ToList();

            foreach (var item in allOldTrans)
            {
                AllTransactions.Remove(item);
            }
        }
        private void CreateListEntry(VariableCostDataType type)
        {
            CreateDefaultVariableCostValues();
            if (AllShopVariableCostData.Where(y => y.VariableCostDataType == type).Count() == 0)
            {
                AllShopVariableCostData.Add(new ShopVariableCostData() { VariableCostDataType = type, UseCount = 0 });
            }

        }
        public void IncreaseVariableCostData(VariableCostDataType type)
        {
            CreateListEntry(type);
            AllShopVariableCostData.Where(y => y.VariableCostDataType == type).Single().UseCount++;
        }

        public int GetVariableCostData(VariableCostDataType type)
        {
            CreateListEntry(type);
            return AllShopVariableCostData.Where(y => y.VariableCostDataType == type).Single().UseCount;
        }
    }
}
