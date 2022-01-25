using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class RifRafMenuUI : MonoBehaviour
{    
    public enum eMenuType { SPLASH, MAIN, AVATAR_SELECT, NUM_MENUS };
    [Header("Menus")]
    public GameObject[] Menus;
    public eMenuType CurActiveMenu;

    public enum ePopUpType { PROFILES, NEW_GAME, CONTINUE_GAME, DELETE_GAME, DELETE_CONFIRM, AUDIO, NUM_POPUPS };
    [Header("PopUps")]
    public GameObject[] PopUps;
    public ePopUpType CurActivePopup = ePopUpType.NUM_POPUPS;

    public enum eMainMenuButtons { MAIN_GAME, CONTINUE, DELETE, AUDIO_SETTINGS, TELL_YOUR_FRIENDS, NUM_MENU_MENU_BUTTONS };
    [Header("Main Menu Buttons")]    
    public Button[] MainMenuButtons;
    public Text[] MainMenuButtonsText;
    public bool[] MainMenuEnabledStates;
    [Header("Save Games")]
    public Button[] ProfilesButtons;
    public Text[] ProfilesText;
    public Image[] ProfilesImages;
    [Header("Audio")]
    public RifRafInGamePopUp.VolumeControl MusicVolume;
    public RifRafInGamePopUp.VolumeControl SoundFXVolume;
    [Header("Inactive Button Images")]
    public Sprite MainGameBlue;
    public Sprite MainGameGrey;
    public Sprite OtherBlue;
    public Sprite OtherGrey;
    [Header("Misc")]
    public Button BackButton;
    public Sprite DefaultProfileIcon;
    public AudioClip CaptainSelectConfirm;
    
    public enum eSaveGameFunction { NEW, CONTINUE, DELETE, NUM_SAVE_GAME_FUNCTIONS };
    public eSaveGameFunction CurActiveSaveGameFunction;    
    MCP MCP;
    public Image MenuBG;    
    int CurNumActiveProfiles = 0;
    bool[] ProfileFileStatus;
    StaticStuff.ProfileInfo[] ProfilesInfo;
    int CurProfileSlot = 0;
    bool MainMenuInitted = false;

    public Text DebugText;    

    private void Awake()
    {
       // Debug.Log("RifRafMenuUI.Awake()");
        foreach (GameObject go in Menus) go.SetActive(false);
        CurActiveMenu = eMenuType.NUM_MENUS;
        foreach (GameObject go in PopUps) go.SetActive(false);
        CurActivePopup = ePopUpType.NUM_POPUPS;
        CurActiveSaveGameFunction = eSaveGameFunction.NUM_SAVE_GAME_FUNCTIONS;        
    }

    void SetDisabledSprites(bool isPopupActive)
    {
        //Debug.Log("SetDisabledSprites(): " + isPopupActive + " --Menu--");
                 
        int buttonIndex = 0;
        foreach (Button b in MainMenuButtons)
        {            
            Sprite blueSprite = (b == MainMenuButtons[(int)eMainMenuButtons.MAIN_GAME] ? MainGameBlue : OtherBlue);
            Sprite greySprite = (b == MainMenuButtons[(int)eMainMenuButtons.MAIN_GAME] ? MainGameGrey : OtherGrey);

            SpriteState spriteState = new SpriteState();
            spriteState = b.spriteState;

            if(isPopupActive == true)
            {   // going from no popup to popup, so save sprite states and set all the main menu buttons to non-active
                MainMenuEnabledStates[buttonIndex] = b.interactable;
                if (b.interactable == true) spriteState.disabledSprite = blueSprite;
                else spriteState.disabledSprite = greySprite;
                b.interactable = false;
            }
            else
            {   // going from popup to no popup, so reset the sprites to their original state
                b.interactable = MainMenuEnabledStates[buttonIndex];
            }

            b.spriteState = spriteState;
            buttonIndex++;
        }
    }    

    public void ToggleMenu(eMenuType menuID, bool isActive)
    {
        //Debug.Log("ToggleMenu() menuID: " + menuID.ToString() + ", isActive: " + isActive + " --Menu--");
        //StaticStuff.PrintRifRafUI("ToggleMenu() menuID: " + menuID.ToString() + ", isActive: " + isActive);
        if (menuID > eMenuType.NUM_MENUS) { Debug.LogError("Invalid menu: " + menuID); return; }
        foreach (GameObject go in Menus) go.SetActive(false);
        if(menuID == eMenuType.NUM_MENUS || isActive == false)
        {
            CurActiveMenu = eMenuType.NUM_MENUS;
        }
        else
        {
            Menus[(int)menuID].SetActive(true);
            CurActiveMenu = menuID;
        }        
    }
    public void TogglePopUp(ePopUpType popUpID, bool isActive)
    {
        //Debug.Log("TogglePopUp() popUpID: " + popUpID.ToString() + ", isActive: " + isActive + " --Menu--");
        StaticStuff.PrintRifRafUI("TogglePopUp() popUpID: " + popUpID.ToString() + ", isActive: " + isActive);
        if (popUpID > ePopUpType.NUM_POPUPS) { Debug.LogError("Invalid popUp: " + popUpID); return; }
        
        if(MainMenuInitted == true)
        {   // don't mess with this if the main menu hasn't been initted yet
            if (CurActivePopup == ePopUpType.NUM_POPUPS && isActive == true)
            {   // going from no popup to popup, so save sprite states and set all the main menu buttons to non-active
                //Debug.Log("handle states here --Menu--");
                //SetDisabledSprites(true);
            }
            else if (CurActivePopup != ePopUpType.NUM_POPUPS && isActive == false)
            {   // going from popup to no popup, so reset the sprites to their original state
               // Debug.Log("handle states here as well --Menu--");
                //SetDisabledSprites(false);
            }
        }
             
        foreach (GameObject go in PopUps) go.SetActive(false);
        if(popUpID == ePopUpType.NUM_POPUPS || isActive == false)
        {
            CurActivePopup = ePopUpType.NUM_POPUPS;
        }
        else
        {
            PopUps[(int)popUpID].SetActive(true);
            CurActivePopup = popUpID;
        }        
    }
   
    #region MAIN_MENU        
    public void InitMainMenu()
    {
       // Debug.Log("InitMainMenu() --Menu--");
        RefreshProfileInfo();
       
        MainMenuButtons[(int)eMainMenuButtons.MAIN_GAME].onClick.RemoveAllListeners();
        MainMenuButtons[(int)eMainMenuButtons.CONTINUE].onClick.RemoveAllListeners();
        
        foreach (Button b in MainMenuButtons)
        {
            if (b == MainMenuButtons[(int)eMainMenuButtons.AUDIO_SETTINGS]) continue;            
            b.interactable = false;                 
        }

        MainMenuButtons[(int)eMainMenuButtons.TELL_YOUR_FRIENDS].interactable = true;
        
        switch (CurNumActiveProfiles)
        {
            case 0:
                //Debug.Log("No saves");
                MainMenuButtons[(int)eMainMenuButtons.MAIN_GAME].interactable = true;
                
                MainMenuButtons[(int)eMainMenuButtons.MAIN_GAME].onClick.AddListener(OnClickNewGame);
                MainMenuButtonsText[(int)eMainMenuButtons.MAIN_GAME].text = "NEW GAME";
                
                MainMenuButtons[(int)eMainMenuButtons.CONTINUE].onClick.AddListener(OnClickContinueGame);
                MainMenuButtonsText[(int)eMainMenuButtons.CONTINUE].text = "CONTINUE GAME";
                break;
            case StaticStuff.NUM_PROFILES:
               // Debug.Log("4 saves");
                MainMenuButtons[(int)eMainMenuButtons.MAIN_GAME].interactable = true;
                MainMenuButtons[(int)eMainMenuButtons.DELETE].interactable = true;

                MainMenuButtons[(int)eMainMenuButtons.MAIN_GAME].onClick.AddListener(OnClickContinueGame);
                MainMenuButtonsText[(int)eMainMenuButtons.MAIN_GAME].text = "CONTINUE GAME";

                MainMenuButtons[(int)eMainMenuButtons.CONTINUE].onClick.AddListener(OnClickNewGame);
                MainMenuButtonsText[(int)eMainMenuButtons.CONTINUE].text = "NEW GAME";
                break;            
            default:
               // Debug.Log("< 4 saves");
                foreach (Button b in MainMenuButtons) b.interactable = true;

                MainMenuButtons[(int)eMainMenuButtons.MAIN_GAME].onClick.AddListener(OnClickContinueGame);
                MainMenuButtonsText[(int)eMainMenuButtons.MAIN_GAME].text = "CONTINUE GAME";

                MainMenuButtons[(int)eMainMenuButtons.CONTINUE].onClick.AddListener(OnClickNewGame);
                MainMenuButtonsText[(int)eMainMenuButtons.CONTINUE].text = "NEW GAME";
                break;
        }

        int buttonIndex = 0;
        foreach (Button b in MainMenuButtons)
        {
            MainMenuEnabledStates[buttonIndex] = b.interactable;
            buttonIndex++;
        }
        MainMenuInitted = true;
    }

    void RefreshProfileInfo()
    {
        ProfileFileStatus = StaticStuff.GetValidProfiles();
        ProfilesInfo = StaticStuff.GetProfileInfo();
        CurNumActiveProfiles = 0;
        for (int i = 0; i < ProfileFileStatus.Length; i++)
        {
            if (ProfileFileStatus[i] == true) CurNumActiveProfiles++;
        }
    }

    void InitProfilesPopup()
    {
        for (int i = 0; i < ProfileFileStatus.Length; i++)
        {
            if (ProfileFileStatus[i] == true)
            {                
                ProfilesText[i].text = ProfilesInfo[i].time;
                string avatarAssetName = "Captain_0" + ProfilesInfo[i].avatar.ToString() + "_Avatar";
                ArticyObject imageAO = ArticyDatabase.GetObject(avatarAssetName);
                if (imageAO == null) Debug.LogError("This error is REAL this time I mean it, there's a problem with the captain avatar image assets: " + avatarAssetName);
                Sprite s = ((Asset)imageAO).LoadAssetAsSprite();
                if (s == null) Debug.LogError("This might be redundant but still real: " + avatarAssetName);
                ProfilesImages[i].sprite = s;
                if (CurActiveSaveGameFunction == eSaveGameFunction.NEW) ProfilesButtons[i].interactable = false;
                else ProfilesButtons[i].interactable = true;                
            }
            else
            {
                ProfilesButtons[i].interactable = false;
                ProfilesText[i].text = "Unused " + (i+1).ToString();
                ProfilesImages[i].sprite = DefaultProfileIcon;
                if (CurActiveSaveGameFunction == eSaveGameFunction.NEW) ProfilesButtons[i].interactable = true;
                else ProfilesButtons[i].interactable = false;
            }
        }
    }

    public void OnClickShowCredits()
    {
        StaticStuff.GoToCreditsScreen();
    }
    
    public void OnClickCloseProfiles()
    {        
        TogglePopUp(ePopUpType.PROFILES, false);
    }
    public void OnClickCloseCreateGame()
    {
        TogglePopUp(ePopUpType.NEW_GAME, false);
    }
    public void OnClickNewGame()
    {
        StaticStuff.PrintRifRafUI("OnClickNewGame");
        if (MenusActiveCheck() == false) return;
        if (CurNumActiveProfiles == StaticStuff.NUM_PROFILES) return;

        TogglePopUp(ePopUpType.PROFILES, true);
        CurActiveSaveGameFunction = eSaveGameFunction.NEW;
        InitProfilesPopup();        
    }
    public void OnClickContinueGame()
    {        
        StaticStuff.PrintRifRafUI("OnClickContinueGame()");
        if (MenusActiveCheck() == false) return;
        if (CurNumActiveProfiles == 0) return;

        TogglePopUp(ePopUpType.PROFILES, true);
        CurActiveSaveGameFunction = eSaveGameFunction.CONTINUE;

        InitProfilesPopup();
    }    
    public void OnClickDeleteSaveGame()
    {
        StaticStuff.PrintRifRafUI("OnClickNewGame");
        if (MenusActiveCheck() == false) return;
        if (CurNumActiveProfiles == 0) return;

        TogglePopUp(ePopUpType.PROFILES, true);
        CurActiveSaveGameFunction = eSaveGameFunction.DELETE;

        InitProfilesPopup();
    }
    
    public void OnClickMainMenuBack()
    {
        StaticStuff.PrintRifRafUI("OnClickMainMenuBack()");
        if (MenusActiveCheck() == false) return;

        this.MCP.TurnOnInGamePopUp();
    }
    #endregion

    public void OnClickBackToMainMenu()
    {
        StaticStuff.PrintRifRafUI("OnClickGenericMenuBack");
        if (MenusActiveCheck() == false) return;

        ToggleMenu(eMenuType.MAIN, true);
    }

    #region SAVE_GAME_POPUP
    public void OnClickSaveGameSlot(int slotNum)
    {
        StaticStuff.PrintRifRafUI("OnClickSaveGameSlot() slotNum: " + slotNum + ", CurActiveSaveGameFunction: " + CurActiveSaveGameFunction);
        CurProfileSlot = slotNum;
        switch (CurActiveSaveGameFunction)
        {
            case eSaveGameFunction.NEW:
                if (ProfileFileStatus[slotNum - 1] == true) return;

                TogglePopUp(ePopUpType.NEW_GAME, true);                
                break;
            case eSaveGameFunction.CONTINUE:
                if (StaticStuff.ProfileExists(CurProfileSlot) == false) return;

                TogglePopUp(ePopUpType.CONTINUE_GAME, true);                
                break;
            case eSaveGameFunction.DELETE:
                if (StaticStuff.ProfileExists(CurProfileSlot) == false) return;

                TogglePopUp(ePopUpType.DELETE_GAME, true);
                break;
            default:
                Debug.LogError("Invalid CurActiveSaveGameFunction: " + CurActiveSaveGameFunction.ToString());
                break;
        }
        CurActiveSaveGameFunction = eSaveGameFunction.NUM_SAVE_GAME_FUNCTIONS;
    }
    #endregion
    
    public Camera UICamera;
    int CaptainIndex = 0;
    Vector3 LastCameraPos;
    #region CHARACTER_SELECT
    
    private void Update()
    {
        if (CurActiveMenu == eMenuType.AVATAR_SELECT)
        {
            if (Input.GetMouseButtonDown(0))
            {                
                LastCameraPos = Input.mousePosition;                
            }
            else if (Input.GetMouseButton(0))
            {
                float deltaX = Input.mousePosition.x - LastCameraPos.x;
                CaptainModelsContainer.transform.Rotate(new Vector3(0f, -deltaX / 10f, 0f));
                LastCameraPos = Input.mousePosition;                
            }       
            else if(Input.GetMouseButtonUp(0))
            {
                Vector3 rot = CaptainModelsContainer.gameObject.transform.eulerAngles;
                CaptainIndex = (int)Mathf.Round(rot.y / 45f);
                if (CaptainIndex == 8) CaptainIndex = 0;
                float newRot = CaptainIndex * 45f;
                CaptainModelsContainer.gameObject.transform.eulerAngles = new Vector3(rot.x, newRot, rot.z);
               // CapRayCaster.SetSelectedCaptain(CaptainContainer.transform.GetChild(CaptainIndex).gameObject);
            }
        }

       
    }
    #endregion

    GameObject CaptainContainerPrefab = null;
    GameObject CaptainModelsContainer = null;
    GameObject CaptainSelect = null;
    CaptainSelectRayCaster CapRayCaster = null;
    #region NEW_GAME_POPUP
    public void OnClickNewGameYes()
    {
        StaticStuff.PrintRifRafUI("OnClickNewGameYes");
        if(CaptainSelect == null)
        {
            CaptainContainerPrefab = Resources.Load<GameObject>("Prefabs/Captain Selector Disc Prefab");
            CaptainSelect = Instantiate<GameObject>(CaptainContainerPrefab);
            CaptainSelect.transform.parent = Menus[(int)eMenuType.AVATAR_SELECT].transform;
            CapRayCaster = CaptainSelect.GetComponentInChildren<CaptainSelectRayCaster>();
            CaptainModelsContainer = CaptainSelect.transform.GetChild(2).gameObject;
            Button confirm = CaptainSelect.transform.GetChild(5).GetChild(1).gameObject.GetComponent<Button>();
            confirm.onClick.RemoveAllListeners();
            confirm.onClick.AddListener(OnClickCaptainSelectConfirm);
            ///Resources.UnloadAsset(captainContainerPrefab);            
        }        
        
        TogglePopUp(ePopUpType.NEW_GAME, false);
        ToggleMenu(eMenuType.AVATAR_SELECT, true);
        CapRayCaster.gameObject.SetActive(true);
        CaptainIndex = 0;      
        MenuBG.enabled = false;
        MainMenuInitted = false;
    }
    public void OnClickCaptainSelectConfirm()
    {
        string captainName = "none";
        int avatar = -1;
        captainName = CapRayCaster.GetSelectedCaptainName();
        avatar = int.Parse(captainName[9].ToString());
        CaptainSelect.transform.parent = null;
        Destroy(CaptainSelect);
        CaptainContainerPrefab = null;
        CaptainModelsContainer = null;
        CaptainSelect = null;
        CapRayCaster = null;
        ToggleMenu(eMenuType.AVATAR_SELECT, false);
        CurActiveMenu = eMenuType.MAIN;
        Resources.UnloadUnusedAssets();
        // Debug.Log("OnClickCaptainSelectConfirm() captainName: " + captainName + ", avatar: " + avatar);        
        this.MCP.LoadCaptainAvatar(avatar);
        StaticStuff.CreateNewProfile(avatar, CurProfileSlot);

        this.MCP.ShutOffAllUI();
        
        SoundFXPlayer.PlayVO(CaptainSelectConfirm);
        StaticStuff.LoadProfileStartScene();
    }

    public void VideoCallback()
    {
        StaticStuff.LoadProfileStartScene();
       
    }
    public void OnClickNewGameNo()
    {
        StaticStuff.PrintRifRafUI("OnClickNewGameNo");
        TogglePopUp(0, false);
    }
    #endregion

    #region CONTINUE_GAME_POPUP
    public void OnClickContinueGameYes()
    {
        StaticStuff.PrintRifRafUI("OnClickContinueGameYes");
        StaticStuff.SetCurrentProfile(CurProfileSlot);
        StaticStuff.LoadCurrentProfile();
        this.MCP.LoadCaptainAvatar(ArticyGlobalVariables.Default.TheCaptain.Avatar);
        StaticStuff.LoadProfileStartScene();
        TogglePopUp(0, false);
        MainMenuInitted = false;
    }

    
    public void OnClickContinueGameNo()
    {
        StaticStuff.PrintRifRafUI("OnClickContinueGameNo");
        TogglePopUp(0, false);
    }
    #endregion

    #region DELETE_GAME_POPUP
    public void OnClickDeleteGameYes()
    {
        StaticStuff.PrintRifRafUI("OnClickDeleteGameYes");
        TogglePopUp(ePopUpType.DELETE_CONFIRM, true);
    }
    public void OnClickDeleteGameNo()
    {
        StaticStuff.PrintRifRafUI("OnClickDeleteGameNo");
        TogglePopUp(0, false);
    }
    #endregion

    #region DELETE_CONFIRM_POPUP
    public void OnClickDeleteConfirmYes()
    {
        StaticStuff.PrintRifRafUI("OnClickDeleteConfirmYes");
        
        StaticStuff.DeleteProfileNum(CurProfileSlot);
        RefreshProfileInfo();
        InitMainMenu();
        TogglePopUp(0, false);
    }
    public void OnClickDeleteConfirmNo()
    {
        StaticStuff.PrintRifRafUI("OnClickDeleteConfirmNo");
        
        TogglePopUp(0, false);
    }
    #endregion

    public void OnClickShareWithFriends()
    {
        Debug.Log("OnClickShareWithFriends()");

        //StartCoroutine(TakeScreenshotAndShare());
        MCP.ShareOnSocial(null);
    }    

    public void OnClickAudioSettings()
    {
        StaticStuff.PrintRifRafUI("OnClickGenericMenuBack");
        if (MenusActiveCheck() == false) return;

        TogglePopUp(ePopUpType.AUDIO, true);

        MusicVolume.Slider.value = this.MCP.GetMusicVolume();
        MusicVolume.Toggle.isOn = (MusicVolume.Slider.value > 0f);
        SoundFXVolume.Slider.value = this.MCP.GetSoundFXVolume();
        SoundFXVolume.Toggle.isOn = (SoundFXVolume.Slider.value > 0f);
    }

    public void OnClickCloseAudioSettings()
    {
        StaticStuff.PrintRifRafUI("OnClickCloseAudioSettings");
        StaticStuff.SaveCurrentSettings("RifRafMenuUI.OnClickCloseAudioSettings()");
        TogglePopUp(ePopUpType.AUDIO, false);
    }

    public void OnSliderAudioVolume(Slider slider)
    {
        StaticStuff.PrintRifRafUI("OnSliderAudioVolume()");        
        if (slider == MusicVolume.Slider)
        {
          //  Debug.Log("RifRafMenuUI().OnSliderAudioVolume() Music: " + slider.value);
            this.MCP.SetMusicVolume((int)slider.value);
            MusicVolume.Toggle.isOn = (MusicVolume.Slider.value > 0f);
        }
        else
        {
          //  Debug.Log("RifRafMenuUI().OnSliderAudioVolume() SFX: " + slider.value);
            this.MCP.SetSoundFXVolume((int)slider.value);
            SoundFXVolume.Toggle.isOn = (SoundFXVolume.Slider.value > 0f);
        }
    }

    public void OnToggleAudioVolume(Toggle toggle)
    {
        if (toggle == MusicVolume.Toggle)
        {
            if (toggle.isOn == true) this.MCP.SetMusicVolume(100);
            else this.MCP.SetMusicVolume(0);
            MusicVolume.Slider.value = this.MCP.GetMusicVolume();
        }
        else
        {
            if (toggle.isOn == true) this.MCP.SetSoundFXVolume(100);
            else this.MCP.SetSoundFXVolume(0);
            SoundFXVolume.Slider.value = this.MCP.GetSoundFXVolume();
        }
    }

    bool MenusActiveCheck()
    {
        return CurActivePopup == ePopUpType.NUM_POPUPS;
    }

    #region SPLASH
    public GameObject IAPManager;
    public void OnClickTapToBegin()
    {
        StaticStuff.PrintRifRafUI("OnClickTapToBegin()");
        if (MenusActiveCheck() == false) return;

        this.MCP.StartMainMenu();                  
    }

    #endregion

    public void SetMCP(MCP mcp)
    {
        this.MCP = mcp;
    }
}

/*
 if(DebugText != null)
        {
            ProfileFileStatus = StaticStuff.GetValidProfiles();
            CurNumActiveProfiles = 0;           
            for (int i = 0; i < ProfileFileStatus.Length; i++)
            {
                if (ProfileFileStatus[i] == true) CurNumActiveProfiles++;              
            }

            string s = "CurActiveMenu: " + CurActiveMenu.ToString() + "\n";
            s += "CurActivePopup: " + CurActivePopup.ToString() + "\n";
            s += "CurActiveSaveGameFunction: " + CurActiveSaveGameFunction.ToString() + "\n\n";
            
            s += "CurNumActiveProfiles: " + CurNumActiveProfiles.ToString() + "\n";
            if(ProfileFileStatus != null) foreach (bool b in ProfileFileStatus) s += b + ", "; s += "\n";            
            if(ProfilesInfo != null) foreach(StaticStuff.ProfileInfo pi in ProfilesInfo) s += pi.avatar + ", "; s += "\n";
            s += "CurProfileSlot: " + CurProfileSlot.ToString() + "\n";
            s += "Current_Profile_Num: " + StaticStuff.Current_Profile_Num.ToString() + "\n";
            DebugText.text = s;
       }

    /* if(DebugText != null)
        {            
            string s = "CurActiveMenu: " + CurActiveMenu.ToString() + "\n";
            s += CapRayCaster.GetSelectedCaptainName() + "\n";
            Vector3 rot = CaptainContainer.gameObject.transform.eulerAngles;
            s += rot.y.ToString("F2") + "\n";
            float num = rot.y / 45f;
            s += num.ToString("F2") + "\n";
            s += Mathf.Round(num).ToString("F2") + "\n";
            int index = (int)Mathf.Round(rot.y / 45f);
            if (index == 8) index = 0;
            s += index.ToString() + "\n";
            //rot = CaptainContainer.gameObject.transform.localEulerAngles;
            //s += rot.y.ToString("F2") + "\n";
            DebugText.text = s;
       }*/
     
