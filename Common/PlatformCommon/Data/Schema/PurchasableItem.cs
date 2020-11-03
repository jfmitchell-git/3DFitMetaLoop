
using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public class PurchasableItem : RewardObject
    {



        public string InternalId { get; set; }
        public string iOSBillingID { get; set; }
        public string PlayBillingID { get; set; }

        public string CostConsumableName { get; set; }
        public int CostConsumableAmount { get; set; }

        public float DefaultUsdPrice { get; set; }

        [IgnoreCodeFirst, Ignore]
        public bool ReadyForPurchase
        {
            get
            {
                return retailPrice > 0;
            }
        }

        private string retailPriceString = "";
        [IgnoreCodeFirst, Ignore]
        public string RetailPriceString
        {
            get
            {
                if (RetailPrice == 0f)
                {
                    return DefaultUsdPrice.ToString("C");
                }
                else
                {
                    return retailPriceString;
                }
            }
            set
            {
                retailPriceString = value;
            }

        }

        private float retailPrice = 0f;
        [IgnoreCodeFirst, Ignore]
        public float RetailPrice
        {
            get
            {
                if (retailPrice == 0f)
                {
                    return DefaultUsdPrice;
                }
                else
                {
                    return retailPrice;
                }
            }
            set
            {
                retailPrice = value;
            }

        }


        public string GetDisplayPrice()
        {

#if UNITY_EDITOR
            return RetailPrice.ToString();
#endif
            return RetailPriceString;
        }


        [Ignore, IgnoreCodeFirst]
        public Consumable Consumable
        {
            get
            {
                if (!string.IsNullOrEmpty(CostConsumableName))
                {
                    return Consumable.GetByName(CostConsumableName);
                }
                return null;
            }
        }

        [IgnoreCodeFirst, Ignore]
        public bool IsRealMoneyTransaction
        {
            get
            {
                return iOSBillingID != string.Empty && PlayBillingID != string.Empty;
            }
        }


        public string GetStoreId()
        {

            string result = string.Empty;
#if UNITY_ANDROID
            result =  PlayBillingID;
#endif

#if UNITY_IOS
            result = iOSBillingID;
#endif

            if (string.IsNullOrEmpty(result)) result = InternalId;
            return result;
        }
        
    }
}
