﻿//#define USE_RR_ONGUI
#define RECEIPT_VALIDATION

using System;
using System.Collections;
using System.Collections.Generic;
using Articy.The_Captain_s_Chair;
using UnityEngine;
using UnityEngine.Purchasing;

#if RECEIPT_VALIDATION
using UnityEngine.Purchasing.Security;
#endif


public class IAPManager : MonoBehaviour, IStoreListener
{ 
    private IStoreController m_Controller;

    private IAppleExtensions m_AppleExtensions;
    private bool m_IsGooglePlayStoreSelected;
    private IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;
    private ITransactionHistoryExtensions m_TransactionHistoryExtensions;

    [Header("Timers")]
    public bool m_InitInProgress = false;
    public bool m_PurchaseInProgress = false;
    public bool m_RestoreInProgress = false;    
    public float m_ActionTimer = 0f;

    string PurchaseFailMessage = "";
    public static string CUR_IAP_ID = "com.tales_tcc1_modebug001";

    Product IAPProduct = null;
    string InitFailureReason = "";

    System.Action IAPInitSuccessCallback = null;
    System.Action IAPInitFailCallback = null;

    public string TimeoutCause = "";
    public enum eIAPPopup { NONE, MAIN, QUIT_CONFIRM, INIT_ERROR, RESTORE_FAILED, PURCHASE_SUCCESS, PURCASE_FAILED, ACTION_TIMED_OUT };
    public eIAPPopup CurIAPPopup = eIAPPopup.NONE;

    public MCP _MCP;
    public RifRafInGamePopUp _RifRafInGamePopup;

    static string UNLOCK_VERSION_CHECK = "1.";

    void Start()
    {
#if UNITY_ANDROID
       // Screen.SetResolution(1123, 540, true);
#else
        //Screen.SetResolution(1280, 960, true);
#endif
        InitIAP(null, null);
    }

    ConfigurationBuilder Builder;
    #region IAP_INIT
    void InitIAP(System.Action successCallback, System.Action failCallback)
    {
        string result = ActionBeingTaken();
        if (result != "") { Debug.LogWarning("Can't InitIAP() because: " + result); return; }

        Debug.Log("InitIAP() --IAP--");
        StandardPurchasingModule module = StandardPurchasingModule.Instance();
        Builder = ConfigurationBuilder.Instance(module);

        m_IsGooglePlayStoreSelected =
            Application.platform == RuntimePlatform.Android && module.appStore == AppStore.GooglePlay;      

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
                Builder.AddProduct(product.id, product.type, ids);                
            }
            else
            {                
                Builder.AddProduct(product.id, product.type);
            }
        }

#if RECEIPT_VALIDATION
        string appIdentifier;
        appIdentifier = Application.identifier;
        Debug.Log("appIdentifier: " + appIdentifier + " --IAP App Receipt--");
        try
        {
            validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), appIdentifier);
        }
        catch (NotImplementedException exception)
        {
            Debug.Log("Cross Platform Validator Not Implemented: " + exception + " --IAP App Receipt--");
        }
        if (validator == null) Debug.Log("null validator --IAP App Receipt--");
        else Debug.Log("we have a validator --IAP App Receipt--");
#endif

        IAPInitSuccessCallback = successCallback;
        IAPInitFailCallback = failCallback;
        m_InitInProgress = true;
        m_ActionTimer = 0f;

#if UNITY_IOS || UNITY_IPHONE
        CheckAppReceipt("InitIAP()");
#endif

        UnityPurchasing.Initialize(this, Builder);
    }

    void UpdateAndSaveIAP(string callingFunction)
    {
        Debug.Log("UpdateAndSaveIAP() called from: " + callingFunction);
        StaticStuff.HasUnlockedFullGame = true;
        StaticStuff.SaveCurrentSettings("IAPManager.CheckIfNeedToUnlockDueToVerion1_0() called from: " + callingFunction);
    }

#if UNITY_IOS || UNITY_IPHONE
    public void CheckAppReceipt(string callingFunction)
    {
        if(Builder == null) { Debug.LogError("No Builder in IAPManager. --IAP App Receipt--"); return; }

        IAppleConfiguration appleConfig = Builder.Configure<IAppleConfiguration>();        
        if (appleConfig.appReceipt.Contains("fake receipt")) { Debug.Log(callingFunction + " we're in the editor so just bail --IAP App Receipt--"); return; }
        
        Debug.Log("CheckAppReceipt() called from: " + callingFunction + ", appReceipt: " + appleConfig.appReceipt + ", canMakePayments: " + appleConfig.canMakePayments + " --IAP App Receipt--");        

        if (appleConfig.appReceipt != "")
        {
            // we have an app receipt so check the version
            var receiptData = System.Convert.FromBase64String(appleConfig.appReceipt);
            AppleReceipt appleReceipt = new AppleValidator(AppleTangle.Data()).Validate(receiptData);
            Debug.Log("Apple Receipt's originalApplicationVersion: " + appleReceipt.originalApplicationVersion + " --IAP App Receipt--");
            if (appleReceipt.originalApplicationVersion.Contains(UNLOCK_VERSION_CHECK))
            {
                Debug.Log("Has original app version " + UNLOCK_VERSION_CHECK + " so make check if they need unlock --IAP App Receipt--");
                CheckIfNeedToUnlockDueToVerion1_(callingFunction);                
            }
        }
        else
        {
            Debug.LogError("Blank app receipt...should not be here...REPORT TO FBI IMMEDIATELY!!!! --IAP App Receipt--");            
            /*if(m_AppleExtensions != null && callingFunction.Contains("InitIAP") == false && callingFunction.Contains("TheCaptain") == false)
            {
                Debug.Log("we have apple extensions but no appReceipt (and we're not in a spot where we care about a login) so try and refresh it --IAP App Receipt--");
                m_AppleExtensions.RefreshAppReceipt(OnRefreshAppReceiptSuccess, OnRefreshAppReceiptFailure);
            }
            else if(callingFunction.Contains("InitIAP") == true)
            {
                Debug.Log("wanted to refresh but we're on InitIAP so don't do that because it will bring up login when we don't want it --IAP App Receipt--");
            }
            else if (callingFunction.Contains("TheCaptain") == true)
            {
                Debug.Log("wanted to refresh but we're on Loading up the Hangar scene so don't do that because it will bring up login when we don't want it --IAP App Receipt--");
            }*/
        }        
    }
#endif

    void CheckIfNeedToUnlockDueToVerion1_(string callingFunction)
    {
        if(StaticStuff.HasUnlockedFullGame == true)
        {
           Debug.Log("CheckIfNeedToUnlockDueToVerion1_() called from: " + callingFunction + ", user already has unlock so nothing to do --IAP App Receipt--");
           return;
        }

        // if we're here then the user bought 1.0 but they do not have the unlock so do it now
        Debug.Log("CheckIfNeedToUnlockDueToVerion1_0() user does not have game unlocked but bought " + UNLOCK_VERSION_CHECK + " so update and save. --IAP App Receipt--");
        UpdateAndSaveIAP("CheckIfNeedTo?UnlockDuetoVersion1_() via " + callingFunction);        
    }


   /* void OnRefreshAppReceiptSuccess(string receipt)
    {
        // This does not mean anything was modified,
        // merely that the refresh process succeeded.
        // For information on parsing receipts, see:
        // https://developer.apple.com/library/archive/releasenotes/General/ValidateAppStoreReceipt/Chapters/ValidateLocally.html#//apple_ref/doc/uid/TP40010573-CH1-SW2
        // as well as:
        // https://docs.unity3d.com/Manual/UnityIAPValidatingReceipts.html
        Debug.Log("OnRefreshAppReceiptSuccess() receipt: " + receipt + " --IAP App Receipt--");
        CheckAppReceipt("OnRefreshAppReceiptSuccess()");
    }

    void OnRefreshAppReceiptFailure()
    {
        Debug.Log("OnRefreshAppReceiptFailure() --IAP App Receipt--");        
    }*/


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
        Debug.Log("OnInitialized() --IAP--");
        m_InitInProgress = false;
        m_Controller = controller;
        m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
        m_GooglePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
        m_TransactionHistoryExtensions = extensions.GetExtension<ITransactionHistoryExtensions>();

        // On Apple platforms we need to handle deferred purchases caused by Apple's Ask to Buy feature.
        // On non-Apple platforms this will have no effect; OnDeferred will never be called.
        m_AppleExtensions.RegisterPurchaseDeferredListener(OnDeferred);        

        List<string> hasReceipt = new List<string>();
        Debug.Log("Available items: --IAP--");
        foreach (Product item in controller.products.all)
        {
            if (item.availableToPurchase)
            {
                //Debug.Log(string.Join(" - ",
                string s = string.Join(" - ",
                    new[]
                    {
                        item.definition.id,
                        item.metadata.localizedTitle,
                        item.definition.type.ToString(),
                        item.hasReceipt.ToString(),
                        item.receipt
                    });
                s += " --IAP--";
                Debug.Log(s);
                if (item.hasReceipt == true)
                {
                    hasReceipt.Add("id: " + item.definition.id + " has receipt: " + item.receipt + " --IAP--");
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
            if (product.definition.id == CUR_IAP_ID)
            {
                IAPProduct = product;
                Debug.Log("gameunlock: product is CUR_SAVE FILE. hasReceipt: " + product.hasReceipt + ", HasUnlockedFullGame: " + StaticStuff.HasUnlockedFullGame + " --IAP--");

                if (product.hasReceipt == true && StaticStuff.HasUnlockedFullGame == true)
                {
                    Debug.Log("gameunlock: receipt and settings are both true --IAP--");
                    // Skip IAP popup go to intro dialogue
                }
                else if (product.hasReceipt == true && StaticStuff.HasUnlockedFullGame == false)
                {
                    Debug.Log("gameunlock: SCS receipt is true but settings is false so update settings --IAP--");
                    StaticStuff.HasUnlockedFullGame = true;
                    StaticStuff.SaveCurrentSettings("IAPManager.OnInitialized()");
                    // Skip IAP popup go to intro dialogue
                }
                else if (product.hasReceipt == false && StaticStuff.HasUnlockedFullGame == false)
                {
                    Debug.Log("gameunlock: receipt and settings are false so no purchase --IAP--");
                    // Show IAP popup
                }
                else if (product.hasReceipt == false && StaticStuff.HasUnlockedFullGame == true)
                {   
                   // Debug.LogError("gameunlock: receipt is false but settings is true...this is odd --IAP--");
                }
            }
        }

        //Debug.LogWarning("Going to call the Init Fail callback upon a restore just to test");
        //if (IAPInitFailCallback != null) IAPInitFailCallback.Invoke();
        if (IAPInitSuccessCallback != null) IAPInitSuccessCallback.Invoke();
    }      
        
    /// <summary>
    /// Purchasing failed to initialise for a non recoverable reason.
    /// </summary>
    /// <param name="error"> The failure reason. </param>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("--OnInitializeFailed()--");
        m_InitInProgress = false;
        switch (error)
        {
            case InitializationFailureReason.AppNotKnown:
                InitFailureReason = "App Not Known";
                //Debug.LogError("Is your App correctly uploaded on the relevant publisher console?");
                break;
            case InitializationFailureReason.PurchasingUnavailable:
                // Ask the user if billing is disabled in device settings.
                InitFailureReason = "Purchasing Unavailable. Billing might be disabled.";
                //Debug.LogError("Billing disabled!");
                break;
            case InitializationFailureReason.NoProductsAvailable:
                // Developer configuration error; check product metadata.
                InitFailureReason = "No Products Available.";
                //Debug.LogError("No products available for purchase!");
                break;
        }
        Debug.LogError("OnInitializeFailed() reason: " + InitFailureReason + "--IAP--");

        if (IAPInitFailCallback != null) IAPInitFailCallback.Invoke();
    }

    void RRInitFailed()
    {
        Debug.Log("RRInitFailed() --IAP--");
        CurIAPPopup = eIAPPopup.INIT_ERROR;
        _RifRafInGamePopup.IAPGenericPopupSetup("Init Failed.", "", OnClickInitFailRetry, OnClickInitFailClose);
    }
    
#endregion // IAP_INIT

#region PURCHASE
    /// <summary>
    /// Triggered when the user presses the <c>Buy</c> button on a product user interface component.
    /// </summary>
    /// <param name="productID">The product identifier to buy</param>
    public void OnClickBuyIAP(string productID) // moiap
    {
        productID = CUR_IAP_ID;
        Debug.Log("OnClickBuyIAP(): " + productID + " --IAP--");
        string result = ActionBeingTaken();
        if (result != "") { Debug.LogWarning("Can't Purchase because: " + result + " --IAP--"); return; }

        if (m_Controller == null)
        {
            Debug.LogError("Purchasing is not initialized --IAP--");
            InitIAP(RestorePurchases, RRInitFailed);
            return;
        }

        if (m_Controller.products.WithID(productID) == null)
        {
            Debug.LogError("No product has id " + productID + " --IAP--");
            return;
        }

        m_PurchaseInProgress = true;
        m_ActionTimer = 0f;
        m_Controller.InitiatePurchase(m_Controller.products.WithID(productID));
    }

    /// <summary>
    /// A purchase succeeded.
    /// </summary>
    /// <param name="e"> The <c>PurchaseEventArgs</c> for the purchase event. </param>
    /// <returns> The result of the successful purchase </returns>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        m_PurchaseInProgress = false;
        Debug.Log("PurchaseProcessingResult() ID: " + e.purchasedProduct.definition.id + " --IAP--");
        Debug.Log("Receipt: " + e.purchasedProduct.receipt + " --IAP--");

        if (e.purchasedProduct.definition.id == CUR_IAP_ID)
        {
            Debug.Log("gameunlock: SCS just unlocked game so update settings --IAP--");
            Debug.Log("receipt: " + e.purchasedProduct.receipt + " --IAP--");
            RestoredValidIAP = true;
            StaticStuff.HasUnlockedFullGame = true;
            StaticStuff.SaveCurrentSettings("IAPManager.PurchaseProcessingResult()");
            if (CurIAPPopup != eIAPPopup.NONE)
            {
                RRPurchaseSuccess();
            }
        }

        return PurchaseProcessingResult.Complete;
    }

#if RECEIPT_VALIDATION
    private CrossPlatformValidator validator;
#endif

    /// <summary>
    /// A purchase failed with specified reason.
    /// </summary>
    /// <param name="item">The product that was attempted to be purchased. </param>
    /// <param name="r">The failure reason.</param>
    public void OnPurchaseFailed(Product item, PurchaseFailureReason r)
    {
        m_PurchaseInProgress = false;
        Debug.Log("--OnPurchaseFailed(): " + item.definition.id + "--IAP--");
        Debug.Log(r);

        // Detailed debugging information
        Debug.Log("Store specific error code: " + m_TransactionHistoryExtensions.GetLastStoreSpecificPurchaseErrorCode() + " --IAP--");
        if (m_TransactionHistoryExtensions.GetLastPurchaseFailureDescription() != null)
        {
            string message = m_TransactionHistoryExtensions.GetLastPurchaseFailureDescription().message;
            PurchaseFailMessage = message;
            RRPurchaseFailed();
        }
    }

    void RRPurchaseSuccess()
    {
        Debug.Log("RRPurchaseSuccess() --IAP--");
        CurIAPPopup = eIAPPopup.NONE;
        _MCP.StopIAPPanel();
        CCPlayer p = FindObjectOfType<CCPlayer>();
        IAPDialogues iadd = FindObjectOfType<IAPDialogues>();
        if (iadd == null) { Debug.LogError("ERROR: no IAPDialogues in this scene"); return; }
        Dialogue dialogue = iadd.IntroDialogue.GetObject() as Dialogue;
        p.GetComponent<ArticyFlow>().CheckIfDialogueShouldStart(iadd.IntroDialogue.GetObject() as Dialogue, p.gameObject);
    }
    void RRPurchaseFailed()
    {
        Debug.Log("RRPurchaseFailed() PurchaseFailMessage: " + PurchaseFailMessage + " --IAP--");
        CurIAPPopup = eIAPPopup.PURCASE_FAILED;
        if(PurchaseFailMessage.Contains("APPLE") == true)
        {
            PurchaseFailMessage = "The operation couldn’t be completed or was interrupted";
        }
        _RifRafInGamePopup.IAPGenericPopupSetup("Transaction Failed", PurchaseFailMessage, OnClickPurchaseFailRetry, OnClickPurchaseFailClose);
    }    
    
#endregion


#region IAP_RESTORE    
    public void RestorePurchases()
    {
        Debug.Log("RestorePurchases() --IAP--");

        string result = ActionBeingTaken();
        if (result != "") { Debug.LogWarning("Can't Restore because: " + result); return; }

        RestoredValidIAP = false;
        Debug.Log("1 --IAP--");
        m_RestoreInProgress = true;
        m_ActionTimer = 0f;

        if (m_IsGooglePlayStoreSelected)
        {
            Debug.Log("RestoreButtonClick() Google --IAP--");
            m_GooglePlayStoreExtensions.RestoreTransactions(OnTransactionsRestored);
        }
        else
        {
            Debug.Log("RestoreButtonClick() Apple --IAP--");
            m_AppleExtensions.RestoreTransactions(OnTransactionsRestored);
        }        
    }
    private void OnTransactionsRestored(bool success)
    {
        m_RestoreInProgress = false;
        Debug.Log("--OnTransactionsRestored() success: " + success + " --IAP--");
        if (success == true)
        {
            string s = "Restore was a success, so check to see if the valid IAP was part of the ProcessPurchase stuff: ";
            if(RestoredValidIAP == true)
            {
                s += "We got the valid IAP during the restore process so show a success --IAP--";
            }
            else
            {
                s += "We did not get the valid IAP so show RRestoreFailed with message from notes --IAP--";
                RRRestoreFailed("No purchases to be restored.");
            }
            Debug.Log(s);
        }
        else
        {
            Debug.Log("Restore failed, show fail popup --IAP--");
            RRRestoreFailed("Transactions unable to be restored.");
        }
    }

    public bool RestoredValidIAP = false;
    public void OnClickRestorePurchases() // moiap
    {
        Debug.Log("OnClickRestorePurchases() --IAP--");
        if (m_Controller == null)
        {
            Debug.LogWarning("Trying to restore purchases but IAP isn't active so re-init --IAP--");
            InitIAP(OnClickRestorePurchases, RRInitFailed);
        }
        else if (m_Controller != null && IAPProduct == null)
        {
            Debug.LogError("We have a Controller but not Product. Should not be here. Check catalog and control panel --IAP--");
        }
        else
        {
            Debug.Log("we have a valid controller and a valid product so try the restore --IAP--");
            RestorePurchases();
        }
    }

    void RRRestoreFailed(string message)
    {
        Debug.Log("RRRestoreFailed() message: " + message + " --IAP--");
        CurIAPPopup = eIAPPopup.RESTORE_FAILED;
        _RifRafInGamePopup.IAPGenericPopupSetup("Restore Failed", message, OnClickRestoreFailRetry, OnClickRestoreFailClose);
    }
               
#endregion // IAPRESTORE

#region IAP_FETCH_INFO
    public void FetchInfoButtonClicked()
    {
        Debug.Log("--FetchInfoButtonClicked()--");
        HashSet<ProductDefinition> productsHash = new HashSet<ProductDefinition>();
        foreach (Product product in m_Controller.products.all)
        {
            productsHash.Add(new ProductDefinition(product.definition.id, product.definition.type));
        }

        m_Controller.FetchAdditionalProducts(productsHash, FetchInfoSuccessCallback, FetchInfoFailCallback);
    }

    public void FetchInfoSuccessCallback()
    {
        Debug.Log("--FetchInfoSuccessCallback()--");
    }
    public void FetchInfoFailCallback(InitializationFailureReason reason)
    {
        Debug.LogError("--FetchInfoFailCallback() reason: " + reason.ToString() + "--");
    }

    private void LogProductDefinitions()
    {
        Debug.Log("--LogProductDefinitions()--");
        var products = m_Controller.products.all;
        foreach (var product in products)
        {
            Debug.Log(string.Format("id: {0}\nstore-specific id: {1}\ntype: {2}\nenabled: {3}\n", product.definition.id, product.definition.storeSpecificId, product.definition.type.ToString(), product.definition.enabled ? "enabled" : "disabled"));
        }
    }
#endregion // IAP_FETCH_INFO

    private void Update()
    {
        string result = ActionBeingTaken();
        if (result != "" && CurIAPPopup != eIAPPopup.ACTION_TIMED_OUT)
        {
            m_ActionTimer += Time.deltaTime;
            if (m_ActionTimer > 15f)
            {
                if (FindObjectOfType<IAPDialogues>() == null)
                {
                    Debug.LogWarning("IAP timeout due to: " + result + ", but we're on the front end so just ignore");
                    ResetActionTimerItems();
                }
                else
                {
                    Debug.Log("Action timeout and we're on hangar scene so show popup about it: " + result + " --IAP--");
                    TimeoutCause = result;
                    RRActionTimedOut();
                }
            }
        }
    }

    void RRActionTimedOut()
    {
        Debug.Log("RRActionTimedOut() TimeoutCause: " + TimeoutCause + " --IAP--");
        CurIAPPopup = eIAPPopup.ACTION_TIMED_OUT;
        
        //if (TimeoutCause.Contains("Purchase"))
        if(m_PurchaseInProgress == true)
        {
            _RifRafInGamePopup.IAPGenericPopupSetup("Action Timed Out.", TimeoutCause, OnClickPurchaseFailRetry, OnClickPurchaseFailClose);
        }
        //else if (TimeoutCause.Contains("Restore"))
        else if(m_RestoreInProgress == true)
        {
            _RifRafInGamePopup.IAPGenericPopupSetup("Action Timed Out.", TimeoutCause, OnClickRestoreFailRetry, OnClickRestoreFailClose);
        }
        //else if (TimeoutCause.Contains("Init"))
        else if(m_InitInProgress == true)
        {            
            _RifRafInGamePopup.IAPGenericPopupSetup("Action Timed Out.", TimeoutCause, OnClickInitFailRetry, OnClickInitFailClose);
        }        
    }

    public void OnClickPurchaseFailRetry()
    {
        Debug.Log("OnClickPurchaseFailRetry() --IAP--");
        ResetActionTimerItems();
        _RifRafInGamePopup.GenericPopup.gameObject.SetActive(false);
        OnClickBuyIAP(CUR_IAP_ID);
    }
    public void OnClickPurchaseFailClose()
    {
        Debug.Log("OnClickPurchaseFailClose() --IAP--");
        ResetActionTimerItems();
        CurIAPPopup = eIAPPopup.MAIN;
        _RifRafInGamePopup.GenericPopup.gameObject.SetActive(false);
    }

    public void OnClickRestoreFailRetry()
    {
        Debug.Log("OnClickRestoreFailRetry() --IAP--");
        ResetActionTimerItems();
        _RifRafInGamePopup.GenericPopup.gameObject.SetActive(false);
        RestorePurchases();
    }
    public void OnClickRestoreFailClose()
    {
        Debug.Log("OnClickPurchaseFailClose() --IAP--");
        ResetActionTimerItems();
        CurIAPPopup = eIAPPopup.MAIN;
        _RifRafInGamePopup.GenericPopup.gameObject.SetActive(false);
    }

    public void OnClickInitFailRetry()
    {
        Debug.Log("OnClickInitFailRetry() --IAP--");
        ResetActionTimerItems();
        _RifRafInGamePopup.GenericPopup.gameObject.SetActive(false);
        CurIAPPopup = eIAPPopup.MAIN;
        InitIAP(OnClickRestorePurchases, RRInitFailed);
    }
    public void OnClickInitFailClose()
    {
        Debug.Log("OnClickInitFailClose() --IAP--");
        ResetActionTimerItems();
        _RifRafInGamePopup.GenericPopup.gameObject.SetActive(false);
        CurIAPPopup = eIAPPopup.MAIN;
    }

    public void OnClickGoToMainMenu()
    {
        Debug.Log("OnClickGoToMainMenu() --IAP--");        
        if(_RifRafInGamePopup.IAPQuitConfirmPopup.activeSelf == true ) { Debug.Log("checking UI Logic DELETE THIS --IAP--"); return; }
        CurIAPPopup = eIAPPopup.QUIT_CONFIRM;
        _RifRafInGamePopup.IAPQuitConfirmPopup.gameObject.SetActive(true);
    }

    public void OnClickGoToMainCancel()
    {
        CurIAPPopup = eIAPPopup.MAIN;
        _RifRafInGamePopup.IAPQuitConfirmPopup.gameObject.SetActive(false);
    }
    public void OnClickGoToMainConfirm()
    {
        CurIAPPopup = eIAPPopup.NONE;
        _RifRafInGamePopup.IAPQuitConfirmPopup.gameObject.SetActive(false);
        _MCP.LoadNextScene("Front End Launcher");        
    }

    public void RRBeginIAPProcess()
    {
        Debug.Log("IAPManager.RRBeginIAPProcess() (called from VideoPlayer) --IAP--");
        CurIAPPopup = eIAPPopup.MAIN;
        _MCP.StartIAPPanel();
        _MCP.StartIAPPanel();

    }
                      
    string ActionBeingTaken()
    {
        string result = "";
        if (m_InitInProgress == true) result += "Device appears offline, please check your internet connection";// --Init in Progress--";
        if (m_RestoreInProgress == true) result += "Device appears offline, please check your internet connection";//"--Restore in Progress";
        if (m_PurchaseInProgress == true) result += "Device appears offline, please check your internet connection";
        return result;
    }

#if false
    private void OnGUI()
    {
        int x = 0;
        int y = Screen.height - 300;
        int w = 300; int h = 300;
        GUI.DrawTexture(new Rect(x, y, w, h), Black);

        if (GUI.Button(new Rect(x, y + 100, 90, 90), "Check\nApp\nReceipt"))
        {
            CheckAppReceipt("OnGUI()");
        }
        if (GUI.Button(new Rect(x + 100, y + 100, 90, 90), "Refresh\nApp\nReceipt"))
        {
            m_AppleExtensions.RefreshAppReceipt(OnRefreshAppReceiptSuccess, OnRefreshAppReceiptFailure);
        }
    }
#endif

    public GUIStyle guiStyle = new GUIStyle();
    public Texture Black;
#if USE_RR_ONGUI
    private void OnGUI()
    {
        int x = 0;
        int y = Screen.height - 300;
        int w = 300; int h = 300;
        //GUI.DrawTexture(new Rect(x, y, w, h), Black);

        GUI.DrawTexture(new Rect(x, y, w, h), Black);
        //GUI.Label(new Rect(x + 10, y + 10, w - 20, h - 20), "Buy IAP?");
        string s = "Init: " + m_InitInProgress + "\n";
        s += "Restore: " + m_RestoreInProgress + "\n";
        s += "Purchase: " + m_PurchaseInProgress + "\n";
        s += "Timer: " + m_ActionTimer.ToString("F2");
        GUI.Label(new Rect(x + 10, y + 10, w - 20, h - 20), s);
        if (GUI.Button(new Rect(x+300, y + 100, 90, 90), "Try Again"))
        {
            RRBeginIAPProcess();
        }
        return;
#if false
        switch (CurIAPPopup)
        {
            case eIAPPopup.MAIN:
                GUI.DrawTexture(new Rect(x, y, w, h), Black);
                GUI.Label(new Rect(x + 10, y + 10, w - 20, h - 20), "Buy IAP?");
                /*if (GUI.Button(new Rect(x, y + 100, 90, 90), "Try Again"))
                {
                    RRBeginIAPProcess();
                }*/
                    /*if (GUI.Button(new Rect(x, y + 100, 90, 90), "Restore"))
                    {
                        m_Controller = null;
                        Debug.Log("Restore purchases");
                        if (m_Controller == null)
                        {
                            Debug.LogWarning("Trying to restore purchases but IAP isn't active so re-init IAP");
                            InitIAP(RestorePurchases, RRInitFailed);
                        }
                        else if (m_Controller != null && IAPProduct == null)
                        {
                            Debug.LogError("We have a Controller but not Product. Should not be here. Check catalog and control panel");
                        }
                        else
                        {
                            Debug.Log("we have a valid controller and a valid product so try the restore");
                            RestorePurchases();
                        }

                    }*/
                    /*if (GUI.Button(new Rect(x + 100, y + 100, 90, 90), "Buy"))
                    {
                        Debug.Log("Buy IAP");
                        PurchaseButtonClick(CUR_IAP_ID);

                    }*/
                    /* if (GUI.Button(new Rect(x + 200, y + 100, 90, 90), "Leave"))
                     {
                         FindObjectOfType<MCP>().LoadNextScene("Front End Launcher");
                         CurIAPPopup = eIAPPopup.NONE;
                     }*/
                    break;
            case eIAPPopup.PURCHASE_SUCCESS:
                GUI.DrawTexture(new Rect(x, y, w, h), Black);
                GUI.Label(new Rect(x + 10, y + 10, w - 20, h - 20), "Purchase success!");
                /*if (GUI.Button(new Rect(x, y + 100, 90, 90), "Continue"))
                {
                    Debug.Log("successful purchase so move onto the intro");
                    CurIAPPopup = eIAPPopup.NONE;
                    CCPlayer p = FindObjectOfType<CCPlayer>();
                    IAPDialogues iadd = FindObjectOfType<IAPDialogues>();
                    if (iadd == null) { Debug.LogError("ERROR: no IAPDialogues in this scene"); return; }
                    Dialogue dialogue = iadd.IntroDialogue.GetObject() as Dialogue;
                    p.GetComponent<ArticyFlow>().CheckIfDialogueShouldStart(iadd.IntroDialogue.GetObject() as Dialogue, p.gameObject);
                }*/
                break;
            case eIAPPopup.PURCASE_FAILED:
                GUI.DrawTexture(new Rect(x, y, w, h), Black);
                GUI.Label(new Rect(x + 10, y + 10, w - 20, h - 20), "Purchase fail\n!" + PurchaseFailMessage);
                /*if (GUI.Button(new Rect(x, y + 100, 90, 90), "Retry Purchase"))
                {
                    Debug.Log("Retry purchase");
                    OnClickBuyIAP(CUR_IAP_ID);
                }
                if (GUI.Button(new Rect(x + 100, y + 100, 90, 90), "Leave"))
                {
                    Debug.Log("Purchase error but going back to front end");
                    FindObjectOfType<MCP>().LoadNextScene("Front End Launcher");
                    CurIAPPopup = eIAPPopup.NONE;
                }*/
                break;
            case eIAPPopup.INIT_ERROR:
                GUI.DrawTexture(new Rect(x, y, w, h), Black);
                GUI.Label(new Rect(x + 10, y + 10, w - 20, h - 20), "Init failed:\n" + InitFailureReason);
                if (GUI.Button(new Rect(x, y + 100, 90, 90), "Retry Init"))
                {
                    Debug.Log("Retry init");
                    InitIAP(IAPInitSuccessCallback, IAPInitFailCallback);
                }
                if (GUI.Button(new Rect(x + 100, y + 100, 90, 90), "Leave"))
                {
                    Debug.Log("INIT_ERROR but going back to front end");
                    FindObjectOfType<MCP>().LoadNextScene("Front End Launcher");
                    CurIAPPopup = eIAPPopup.NONE;
                }
                break;
            case eIAPPopup.RESTORE_FAILED:
                GUI.DrawTexture(new Rect(x, y, w, h), Black);
                GUI.Label(new Rect(x + 10, y + 10, w - 20, h - 20), "Restore failed. Try again?");
                /*if (GUI.Button(new Rect(x, y + 100, 90, 90), "Retry Restore"))
                {
                    Debug.Log("Retry Restore");
                    RestorePurchases();
                }
                if (GUI.Button(new Rect(x + 100, y + 100, 90, 90), "Leave"))
                {
                    Debug.Log("Restore error but going back to front end");
                    FindObjectOfType<MCP>().LoadNextScene("Front End Launcher");
                    CurIAPPopup = eIAPPopup.NONE;
                }*/
                break;
            
            case eIAPPopup.ACTION_TIMED_OUT:
                GUI.DrawTexture(new Rect(x, y, w, h), Black);
                GUI.Label(new Rect(x + 10, y + 10, w - 20, h - 20), "Timeout:\n" + TimeoutCause);
                if (GUI.Button(new Rect(x, y + 100, 90, 90), "Retry"))
                {
                    if (m_InitInProgress == true)
                    {
                        m_InitInProgress = false;
                        InitIAP(IAPInitSuccessCallback, IAPInitFailCallback);
                    }
                    else if (m_PurchaseInProgress == true)
                    {
                        m_PurchaseInProgress = false;
                        OnClickBuyIAP(CUR_IAP_ID);
                    }
                    else if (m_RestoreInProgress == true)
                    {
                        Debug.Log("3 --IAP--");
                        m_RestoreInProgress = false;
                        RestorePurchases();
                    }
                }
                break;
        }

        /* int top = 100;
         w = 100;
         h = 100;
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
         if (GUI.Button(new Rect(Screen.width - 100, top + 100, w, h), "Buy Full Game"))
         {
             PurchaseButtonClick("com.tales_tcc1_modebugFullGameBuy");
         }*/
#endif
    }
#endif

#region IAP_NOT_USED
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
        Debug.Log("--OnDeferred(): " + item.definition.id + "--");
    }        
                    
    private void OnTransactionsRefreshedSuccess(string message)
    {
        Debug.Log("--OnTransactionsRefreshedSuccess() message: " + message + "--");
    }
    private void OnTransactionsRefreshedFail()
    {
        Debug.Log("--OnTransactionsRefreshedFail()--");
    }
#endregion // IAP_NOT_USED

    
    void ResetActionTimerItems()
    {
        m_InitInProgress = false;
        m_PurchaseInProgress = false;        
        m_RestoreInProgress = false;
        m_ActionTimer = 0f;
    }

    }
