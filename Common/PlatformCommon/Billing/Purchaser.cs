#if !BACKOFFICE && !UNITY_STANDALONE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Purchasing;


namespace MetaLoop.Common.PlatformCommon.Data.Billing
{

    public class PurchaserSku
    {
        public string StoreSkuId { get; set; }
        public ProductType ProductType { get; set; }
    }

    public class Purchaser : IStoreListener
    {


        public static void Kill()
        {
            m_StoreController = null;
            m_StoreExtensionProvider = null;
            instance = null;
        }

        private static Purchaser instance;
        public static Purchaser Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Purchaser();
                }
                return instance;
            }
        }

        private static IStoreController m_StoreController;          // The Unity Purchasing system.
        private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.


        private Action<string, bool, PurchaseEventArgs, PurchaseFailureReason> currentCallback = null;

        private Action<bool, string, string> currentAnalyticsCallback = null;

        private Action<ProductCollection> onPurchaserReady;



        public void InitializePurchasing(List<PurchaserSku> SKUs, Action<ProductCollection> onPurchaserReady)
        {


            // If we have already connected to Purchasing ...
            if (IsInitialized())
            {
                // ... we are done here.
                return;
            }

            this.onPurchaserReady = onPurchaserReady;
            // Create a builder, first passing in a suite of Unity provided stores.
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            //// Add a product to sell / restore by way of its identifier, associating the general identifier
            //// with its store-specific identifiers.
            ///
            foreach (var sku in SKUs)
            {
                builder.AddProduct(sku.StoreSkuId, sku.ProductType);
            }

            // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
            // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
            UnityPurchasing.Initialize(this, builder);
        }


        private bool IsInitialized()
        {
            // Only say we are initialized if both the Purchasing references are set.
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }


        public void BuyProductID(string productId, Action<string, bool, PurchaseEventArgs, PurchaseFailureReason> callback, Action<bool, string, string> analyticsCallback = null)
        {
            if (currentCallback != null) return;
            currentCallback = callback;
            currentAnalyticsCallback = analyticsCallback;

            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing 
                // system's products collection.
                Product product = m_StoreController.products.WithID(productId);


                // If the look up found a product for this device's store and that product is ready to be sold ... 
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                    // asynchronously.
                    m_StoreController.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    // ... report the product look-up failure situation  
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            // Otherwise ...
            else
            {
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
                // retrying initiailization.
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }


        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases()
        {
            // If Purchasing has not yet been set up ...
            if (!IsInitialized())
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                Debug.Log("RestorePurchases FAIL. Not initialized.");
                return;
            }

            // If we are running on an Apple device ... 
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // ... begin restoring purchases
                Debug.Log("RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions((result) =>
                {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                    // no purchases are available to be restored.
                    Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                });
            }
            // Otherwise ...
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }


        //  
        // --- IStoreListener
        //

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            Debug.Log("OnInitialized: PASS");

            // Overall Purchasing system, configured with products for this application.
            m_StoreController = controller;


            // Store specific subsystem, for accessing device-specific store features.
            m_StoreExtensionProvider = extensions;

            this.onPurchaserReady.Invoke(controller.products);
        }


        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }


        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            if (currentCallback != null)
            {
                currentCallback.Invoke(args.purchasedProduct.definition.id, true, args, PurchaseFailureReason.Unknown);
                currentCallback = null;
            }



            return PurchaseProcessingResult.Pending;
        }

        public void ConfirmPurchase(PurchaseEventArgs args)
        {
            m_StoreController.ConfirmPendingPurchase(args.purchasedProduct);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            currentCallback.Invoke(product.definition.id, false, null, failureReason);
            currentCallback = null;
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }
    }
}
#endif