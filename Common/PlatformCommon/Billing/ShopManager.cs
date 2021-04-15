#if !BACKOFFICE
using UnityEngine.Purchasing;
using MetaLoop.Common.PlatformCommon.Data.Billing;
#else
using MetaLoop.Common.PlatformCommon.Billing;
#endif
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlatformCommon.State;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;
using MetaLoop.Common.PlatformCommon.Utils;
using MetaLoop.Common.PlatformCommon.Settings;

namespace MetaLoop.Common.PlatformCommon.Data.Billing
{

    public class ShopRequestResult : GenericRequestRewardResult { }

    public class ShopManager
    {
        public static bool IsInitialized = false;
        public static bool PurchaseReady { get; set; }

        private static List<PurchasableItem> allPurchasableItems;


        public static void Init()
        {
            if (IsInitialized) return;

            BuildPurchasables();

#if !BACKOFFICE && !UNITY_STANDALONE
            List<PurchaserSku> allIapSkus = new List<PurchaserSku>();
            allPurchasableItems.Select(y => y.GetStoreId()).Distinct().ToList().ForEach(y => allIapSkus.Add(new PurchaserSku() { StoreSkuId = y, ProductType = UnityEngine.Purchasing.ProductType.Consumable }));
            Purchaser.Instance.InitializePurchasing(allIapSkus, OnPurchaserReady);
#endif
            IsInitialized = true;
        }
        private static void BuildPurchasables()
        {
            if (allPurchasableItems != null) allPurchasableItems.Clear();
            allPurchasableItems = new List<PurchasableItem>();

            if (MetaStateSettings.PolymorhTypes.ContainsKey(typeof(ShopHardCurrencyItem)))
            {
                allPurchasableItems.AddRange(DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(ShopHardCurrencyItem)]).Cast<PurchasableItem>().ToList());
            }

            if (MetaStateSettings.PolymorhTypes.ContainsKey(typeof(ShopOffer)))
            {
                allPurchasableItems = DataLayer.Instance.GetTable(MetaStateSettings.PolymorhTypes[typeof(ShopOffer)]).Cast<PurchasableItem>().ToList();
            }
        }

#if !BACKOFFICE && !UNITY_STANDALONE


        public static void Kill()
        {
            Purchaser.Kill();
            allPurchasableItems.Clear();
            IsInitialized = false;
        }

        private static ProductCollection AllProductsFromStore;
        public static void SyncPurchableItems()
        {
            if (AllProductsFromStore == null) return;
            BuildPurchasables();
            foreach (var item in AllProductsFromStore.all)
            {
                var allPurchasableItemWithSku = allPurchasableItems.Where(y => y.GetStoreId().ToLower() == item.definition.id.ToLower()).ToList();
                allPurchasableItemWithSku.ForEach(y => { y.RetailPrice = Convert.ToSingle(item.metadata.localizedPrice); y.RetailPriceString = item.metadata.localizedPriceString; });
            }
        }

#endif



#if !BACKOFFICE && !UNITY_STANDALONE
        private static void OnPurchaserReady(ProductCollection allProducts)
        {
            if (PurchaseReady) return;
            AllProductsFromStore = allProducts;

            UnityEngine.Debug.Log("OnPurchaserReady");
            UnityEngine.Debug.Log(allProducts);

            foreach (var item in AllProductsFromStore.all)
            {
                var allPurchasableItemWithSku = allPurchasableItems.Where(y => y.GetStoreId().ToLower() == item.definition.id.ToLower()).ToList();
                allPurchasableItemWithSku.ForEach(y => { y.RetailPrice = Convert.ToSingle(item.metadata.localizedPrice); y.RetailPriceString = item.metadata.localizedPriceString; });
            }
            PurchaseReady = true;
        }

        public static void ValidateIAP(MetaDataStateBase metaDataState, string internalId, string receiptID, bool isOffer, Action<ShopRequestResult> callBack)
        {
            ShopTransactionItem shopTransactionItem = new ShopTransactionItem(ShopItemType.HardCurrency, internalId);
            string platformType = UnityEngine.Application.platform.ToString();
            CloudScriptMethod cloudScriptMethod = new CloudScriptMethod("Shop/ValidateIAP", false);
            cloudScriptMethod.Params.Add("internalId", internalId);
            cloudScriptMethod.Params.Add("receiptID", receiptID);
            cloudScriptMethod.Params.Add("platformType", platformType);
            PlayFabManager.Instance.InvokeBackOffice(cloudScriptMethod, (CloudScriptResponse r, CloudScriptMethod m) => HandleShopResult(r, metaDataState, shopTransactionItem, callBack, true));
        }



        private static void HandleShopResult(CloudScriptResponse response, MetaDataStateBase metaDataState, ShopTransactionItem shopTransactionItem, Action<ShopRequestResult> callBack, bool isIAP = false)
        {
            ShopRequestResult result = null;

            if (response.ResponseCode == ResponseCode.Success && response.Params != null && response.Params.ContainsKey("ShopRequestResult"))
            {
                result = JsonConvert.DeserializeObject<ShopRequestResult>(response.Params["ShopRequestResult"].ToString()) as ShopRequestResult;

                if (result.Result == GenericRequestRewardResultType.Success)
                {
                    shopTransactionItem.Result = GenericRequestRewardResultType.Success;

                    if (result.GetCost() != null && result.GetCost().ConsumableCostItems.Count > 0)
                    {
                        metaDataState.Consumables.SpendConsumables(result.GetCost().ConsumableCostItems);
                    }

                    foreach (RewardDataItem entry in result.GetReward())
                    {
                        metaDataState.Consumables.AddConsumable(entry.Consumable, entry.Amount, isIAP ? "InAppPurchase" : string.Empty);
                    }
                }



            }

            shopTransactionItem.SetTransactionTime();
            metaDataState.ShopState.AllTransactions.Add(shopTransactionItem);

            //result with error code.
            if (result == null) result = new ShopRequestResult();

            callBack.Invoke(result);
        }
#endif

    }
}

