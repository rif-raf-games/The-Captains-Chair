//#define USE_RR_ONGUI
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;


public class IAPManager : MonoBehaviour, IStoreListener
{ 
    private IStoreController m_Controller;

    private IAppleExtensions m_AppleExtensions;
    private bool m_IsGooglePlayStoreSelected;
    private IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;
    private ITransactionHistoryExtensions m_TransactionHistoryExtensions;

    void Start()
    {
#if UNITY_ANDROID
       // Screen.SetResolution(1123, 540, true);
#else
        Screen.SetResolution(1280, 960, true);
#endif
    }

    void InitIAP()
    {
        Debug.Log("InitIAP()");
        StandardPurchasingModule module = StandardPurchasingModule.Instance();
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        m_IsGooglePlayStoreSelected =
            Application.platform == RuntimePlatform.Android && module.appStore == AppStore.GooglePlay;
        Debug.Log("IAPManager.Awake() m_IsGooglePlayStoreSelected: " + m_IsGooglePlayStoreSelected);

        ProductCatalog catalog = ProductCatalog.LoadDefaultCatalog();

        foreach (var product in catalog.allValidProducts)
        {
            if (product.allStoreIDs.Count > 0)
            {
                var ids = new IDs();
                foreach (var storeID in product.allStoreIDs)
                {
                    ids.Add(storeID.id, storeID.store);
                }
                builder.AddProduct(product.id, product.type, ids);
                // Debug.Log("1) id: " + product.id + ", type: " + product.type + ", ids: " + ids);
            }
            else
            {
                // Debug.Log("2) id: " + product.id + ", type: " + product.type);
                builder.AddProduct(product.id, product.type);
            }
        }

        UnityPurchasing.Initialize(this, builder);
    }

    /// <summary>
    /// Purchasing initialized successfully.
    ///
    /// The <c>IStoreController</c> and <c>IExtensionProvider</c> are
    /// available for accessing purchasing functionality.
    /// </summary>
    /// <param name="controller"> The <c>IStoreController</c> created during initialization. </param>
    /// <param name="extensions"> The <c>IExtensionProvider</c> created during initialization. </param>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        m_Controller = controller;
        m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
        m_GooglePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
        m_TransactionHistoryExtensions = extensions.GetExtension<ITransactionHistoryExtensions>();

        // On Apple platforms we need to handle deferred purchases caused by Apple's Ask to Buy feature.
        // On non-Apple platforms this will have no effect; OnDeferred will never be called.
        m_AppleExtensions.RegisterPurchaseDeferredListener(OnDeferred);
        List<string> hasReceipt = new List<string>();

        Debug.Log("Available items:");
        foreach (Product item in controller.products.all)
        {
            if (item.availableToPurchase)
            {
                Debug.Log(string.Join(" - ",
                    new[]
                    {
                        item.definition.id,
                        item.metadata.localizedTitle,
                        item.definition.type.ToString(),
                        item.hasReceipt.ToString(),
                        item.receipt
                    }));
                if (item.hasReceipt == true)
                {
                    hasReceipt.Add("id: " + item.definition.id + " has receipt: " + item.receipt);
                }
            }
        }

        if (hasReceipt.Count > 0)
        {
            Debug.Log("These items have a receipt:");
            foreach (string s in hasReceipt)
            {
                Debug.Log(s);
            }
        }

        for (int i = 0; i < m_Controller.products.all.Length; i++)
        {
            Product product = m_Controller.products.all[i];
            if (product.definition.id == CUR_SAVE_FILE)
            {
                Debug.Log("gameunlock: " + product.hasReceipt + ", settings: " + StaticStuff.HasUnlockedFullGame);
                if (product.hasReceipt == true && StaticStuff.HasUnlockedFullGame == true)
                {
                    Debug.Log("receipt and settings are both true");
                }
                else if (product.hasReceipt == true && StaticStuff.HasUnlockedFullGame == false)
                {
                    Debug.Log("receipt is true but settings is false so update settings");
                    StaticStuff.HasUnlockedFullGame = true;
                    StaticStuff.SaveCurrentSettings("IAPManager.OnInitialized()");
                }
                else if (product.hasReceipt == false && StaticStuff.HasUnlockedFullGame == false)
                {
                    Debug.Log("receipt and settings are false so no purchase");
                }
                else if (product.hasReceipt == false && StaticStuff.HasUnlockedFullGame == true)
                {
                    Debug.LogError("receipt is false but settings is true...this is odd");
                }
            }
        }
    }

    public void FetchInfoButtonClicked()
    {        
        HashSet<ProductDefinition> productsHash = new HashSet<ProductDefinition>();
        foreach (Product product in m_Controller.products.all)
        {
            productsHash.Add(new ProductDefinition(product.definition.id, product.definition.type));
        }

        m_Controller.FetchAdditionalProducts(productsHash, FetchInfoSuccessCallback, FetchInfoFailCallback);
    }

    public void FetchInfoSuccessCallback()
    {
        Debug.Log("FetchInfoSuccessCallback()");
    }
    public void FetchInfoFailCallback(InitializationFailureReason reason)
    {
        Debug.LogError("FetchInfoFailCallback() reason: " + reason.ToString());
    }

    private void LogProductDefinitions()
    {
        var products = m_Controller.products.all;
        foreach (var product in products)
        {
            Debug.Log(string.Format("id: {0}\nstore-specific id: {1}\ntype: {2}\nenabled: {3}\n", product.definition.id, product.definition.storeSpecificId, product.definition.type.ToString(), product.definition.enabled ? "enabled" : "disabled"));
        }
    }

    /// <summary>
    /// iOS Specific.
    /// This is called as part of Apple's 'Ask to buy' functionality,
    /// when a purchase is requested by a minor and referred to a parent
    /// for approval.
    ///
    /// When the purchase is approved or rejected, the normal purchase events
    /// will fire.
    /// </summary>
    /// <param name="item">Item.</param>
    private void OnDeferred(Product item)
    {
        Debug.Log("Purchase deferred: " + item.definition.id);
    }

    /// <summary>
    /// Purchasing failed to initialise for a non recoverable reason.
    /// </summary>
    /// <param name="error"> The failure reason. </param>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed");
        switch (error)
        {
            case InitializationFailureReason.AppNotKnown:
                Debug.LogError("Is your App correctly uploaded on the relevant publisher console?");
                break;
            case InitializationFailureReason.PurchasingUnavailable:
                // Ask the user if billing is disabled in device settings.
                Debug.LogError("Billing disabled!");
                break;
            case InitializationFailureReason.NoProductsAvailable:
                // Developer configuration error; check product metadata.
                Debug.LogError("No products available for purchase!");
                break;
        }
    }

    /// <summary>
    /// A purchase succeeded.
    /// </summary>
    /// <param name="e"> The <c>PurchaseEventArgs</c> for the purchase event. </param>
    /// <returns> The result of the successful purchase </returns>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        Debug.Log("----------------Purchase OK: " + e.purchasedProduct.definition.id);
        Debug.Log("----------------Receipt: " + e.purchasedProduct.receipt);

        if (e.purchasedProduct.definition.id == CUR_SAVE_FILE)
        {
            Debug.Log("just unlocked game so update settings");
            StaticStuff.HasUnlockedFullGame = true;
            StaticStuff.SaveCurrentSettings("IAPManager.PurchaseProcessingResult()");            
        }

        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// A purchase failed with specified reason.
    /// </summary>
    /// <param name="item">The product that was attempted to be purchased. </param>
    /// <param name="r">The failure reason.</param>
    public void OnPurchaseFailed(Product item, PurchaseFailureReason r)
    {
        Debug.Log("OnPurchaseFailed(): " + item.definition.id);
        Debug.Log(r);

        // Detailed debugging information
        Debug.Log("Store specific error code: " + m_TransactionHistoryExtensions.GetLastStoreSpecificPurchaseErrorCode());
        if (m_TransactionHistoryExtensions.GetLastPurchaseFailureDescription() != null)
        {
            string message = m_TransactionHistoryExtensions.GetLastPurchaseFailureDescription().message;
            Debug.Log("Purchase failure description message: " + message);            
        }
    }

    public static string CUR_SAVE_FILE = "com.tales_tcc1.gameunlock3";
    /// <summary>
    /// Triggered when the user presses the <c>Buy</c> button on a product user interface component.
    /// </summary>
    /// <param name="productID">The product identifier to buy</param>
    public void PurchaseButtonClick(string productID)
    {        
        Debug.Log("PurchaseButtonClick(): " + productID);

        if (m_Controller == null)
        {
            Debug.LogError("Purchasing is not initialized");     
            return;
        }

        if (m_Controller.products.WithID(productID) == null)
        {
            Debug.LogError("No product has id " + productID);            
            return;
        }

        m_Controller.InitiatePurchase(m_Controller.products.WithID(productID), "developerPayload");
    }   

    public void RestoreButtonClick()
    {
        if (m_IsGooglePlayStoreSelected)
        {
            Debug.Log("RestoreButtonClick() - Google");
            m_GooglePlayStoreExtensions.RestoreTransactions(OnTransactionsRestored);
        }
        else
        {
            Debug.Log("RestoreButtonClick() - Apple");
            m_AppleExtensions.RestoreTransactions(OnTransactionsRestored);            
        }
    }
    
    private void OnTransactionsRefreshedSuccess(string message)
    {
        Debug.Log("OnTransactionsRefreshedSuccess() message: " + message);
    }
    private void OnTransactionsRefreshedFail()
    {
        Debug.Log("Transaction refresh failed");
    }

    private void OnTransactionsRestored(bool success)
    {
        Debug.Log("OnTransactionsRestored() success: " + success);
    }

    public GUIStyle guiStyle = new GUIStyle();
    bool showIAPInfo = false;
    public Texture Black;
#if USE_RR_ONGUI
    private void OnGUI()
    {        
        int top = 100;
        int w = 100;
        int h = 100;
        if (GUI.Button(new Rect(Screen.width - 300, top, w, h), "Init IAP"))
        {
            InitIAP();
        }
        if (GUI.Button(new Rect(Screen.width - 200, top, w, h), "Fetch Info"))
        {
            FetchInfoButtonClicked();
        }
        if (GUI.Button(new Rect(Screen.width - 100, top, w, h), "Restore"))
        {
            RestoreButtonClick();
        }
        if (GUI.Button(new Rect(Screen.width - 200, top+h, w, h), "Buy\nDBG001"))
        {
            PurchaseButtonClick("com.tales_tcc1_modebug001");
        }
        if (GUI.Button(new Rect(Screen.width - 200, top + (h*2), w, h), "Buy\n_gameunlock"))
        {
            PurchaseButtonClick("com.tales_tcc1_gameunlock");
        }
        if (GUI.Button(new Rect(Screen.width - 200, top + (h * 3), w, h), "Buy\n.gameunlock"))
        {
            PurchaseButtonClick("com.tales_tcc1.gameunlock");
        }
        if (GUI.Button(new Rect(Screen.width - 100, top + h, w, h), "Buy\ngameunlock3"))
        {
            PurchaseButtonClick("com.tales_tcc1.gameunlock3");
        }
        if (GUI.Button(new Rect(Screen.width - 100, top + (h * 2), w, h), "Buy\ngameunlock4"))
        {
            PurchaseButtonClick("com.tales_tcc1.gameunlock4");
        }
        if (GUI.Button(new Rect(Screen.width - 100, top + (h * 3), w, h), "Buy\ngameunlock5"))
        {
            PurchaseButtonClick("com.tales_tcc1.gameunlock5");
        }
        if (GUI.Button(new Rect(Screen.width - 100, top + (h * 4), w, h), "Buy\ngameunlock5"))
        {
            PurchaseButtonClick("com.tales_tcc1.gameunlock5");
        }        
    }
#endif


    /*
     /*if (m_Controller != null && showIAPInfo == true)
        {
            int topY = 130;
            int numButtons = m_Controller.products.all.Length;
            int buttonHeight = (Screen.height - topY) / numButtons;
            int i = 0;
            foreach (Product product in m_Controller.products.all)
            {
                string st = "ID: " + product.definition.id + ". Type: " + product.definition.type + "\n";
                st += "Enabled: " + product.definition.enabled + ". Available: " + product.availableToPurchase + ". Has Receipt: " + product.hasReceipt;
                GUI.DrawTexture(new Rect(0, topY + (i * buttonHeight), Screen.width, buttonHeight), Black);
                GUI.TextArea(new Rect(0, topY + (i * buttonHeight), Screen.width, buttonHeight), st, guiStyle);
                i++;
            }
        }
        if (GUI.Button(new Rect(Screen.width - 250, 0, 250, 125), "Toggle\nIAP Info"))
        {
            showIAPInfo = !showIAPInfo;
        }
        if (GUI.Button(new Rect(Screen.width - 600, 0, 250, 125), "Reset Save"))
        {
            StaticStuff.HasUnlockedFullGame = false;
            StaticStuff.SaveCurrentSettings("IAPManager.ResetUnlockInLocalSave()");
            Debug.LogWarning("Deleted unlock from local save.");
        }
    
    CUR_SAVE_FILE = GUI.TextArea(new Rect(Screen.width - 1375, 0, 750, 100), CUR_SAVE_FILE, guiStyle);
     */     
}
