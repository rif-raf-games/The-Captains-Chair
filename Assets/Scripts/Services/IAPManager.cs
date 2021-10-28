
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;


public class IAPManager : MonoBehaviour, IStoreListener
{
    public RifRafInGamePopUp IGPopup;

    private IStoreController m_Controller;

    private IAppleExtensions m_AppleExtensions;
    private bool m_IsGooglePlayStoreSelected;
    private IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;
    private ITransactionHistoryExtensions m_TransactionHistoryExtensions;

    private void Awake()
    {
        InitIAP();
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
                        "/n",                        
                        item.receipt
                    }));
            }
        }
        
        for (int i=0; i<m_Controller.products.all.Length; i++)
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
                else if(product.hasReceipt == false && StaticStuff.HasUnlockedFullGame == false)
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

        if(e.purchasedProduct.definition.id == CUR_SAVE_FILE)
        {
            Debug.Log("just unlocked game so update settings");
            StaticStuff.HasUnlockedFullGame = true;
            StaticStuff.SaveCurrentSettings("IAPManager.PurchaseProcessingResult()");
            FindObjectOfType<RifRafInGamePopUp>().IAPPurchaseSuccessful();
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
            FindObjectOfType<RifRafInGamePopUp>().IAPPurchaseFailed(message);
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
            FindObjectOfType<RifRafInGamePopUp>().SetupResultsPopup("Error", "Purchasing is not initialized.");
            return;
        }

        if (m_Controller.products.WithID(productID) == null)
        {            
            Debug.LogError("No product has id " + productID);
            FindObjectOfType<RifRafInGamePopUp>().SetupResultsPopup("Error", "No product has id " + productID);
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
            //m_AppleExtensions.RefreshAppReceipt(OnTransactionsRefreshedSuccess, OnTransactionsRefreshedFail);
        }
    }
    //OnTransactionsRefreshedSuccess
    //OnTransactionsRefreshedFail
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
        FindObjectOfType<RifRafInGamePopUp>().IAPPurchasesRestored();
    }
    
    public GUIStyle guiStyle = new GUIStyle();
    bool showIAPInfo = false;
    public Texture Black;
    private void OnGUI()
    {
        if (m_Controller != null && showIAPInfo == true)
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
    }
}
