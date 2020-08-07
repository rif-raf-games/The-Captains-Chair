using Articy.The_Captain_s_Chair.GlobalVariables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RifRafMenuUI : MonoBehaviour
{    
    public enum eMenuType { SPLASH, MAIN, COMING_SOON, ACHIEVEMENTS, TELL_FRIENDS, AVATAR_SELECT, NUM_MENUS };
    [Header("Menus: Splash, Main, Coming Soon, Achievements, Tell Friends\n")]
    public GameObject[] Menus;
    public eMenuType CurActiveMenu;

    public enum ePopUpType { PROFILES, NEW_GAME, DELETE_GAME, DELETE_CONFIRM, NUM_POPUPS };
    [Header("PopUps: Saved Games, New, Delete, Delete Confirm")]
    public GameObject[] PopUps;
    public ePopUpType CurActivePopup;

    public enum eMainMenuButtons { NEW, CONTINUE, DELETE, NUM_MENU_MENU_BUTTONS };
    [Header("Main Menu Buttons")]
    public Text[] MainMenuButtonsText;

    public enum eSaveGameFunction { NEW, CONTINUE, DELETE, NUM_SAVE_GAME_FUNCTIONS };
    public eSaveGameFunction CurActiveSaveGameFunction;

    public Text[] ProfilesText;

    public MCP MCP;

    public Image MenuBG;
    public GameObject CaptainContainer;

    public Text DebugText;
    #region DEBUG
    public void DebugButton1()
    {
        Debug.Log("DebugButton1()");
        string s = Time.time.ToString();
        Debug.Log(s);
    }

    
    #endregion

    private void Awake()
    {
        foreach (GameObject go in Menus) go.SetActive(false);
        CurActiveMenu = eMenuType.NUM_MENUS;
        foreach (GameObject go in PopUps) go.SetActive(false);
        CurActivePopup = ePopUpType.NUM_POPUPS;

        CurActiveSaveGameFunction = eSaveGameFunction.NUM_SAVE_GAME_FUNCTIONS;
    }

 
    public void Init(MCP mcp)
    {
        this.MCP = mcp;
    }

    public void ToggleMenu(eMenuType menuID, bool isActive)
    {
        StaticStuff.PrintRifRafUI("ToggleMenu() menuID: " + menuID.ToString() + ", isActive: " + isActive);
        if (menuID >= eMenuType.NUM_MENUS) { Debug.LogError("Invalid menu: " + menuID); return; }
        foreach (GameObject go in Menus) go.SetActive(false);
        Menus[(int)menuID].SetActive(isActive);
        CurActiveMenu = (isActive == true ? menuID : eMenuType.NUM_MENUS);
    }
    public void TogglePopUp(ePopUpType popUpID, bool isActive)
    {
        StaticStuff.PrintRifRafUI("TogglePopUp() popUpID: " + popUpID.ToString() + ", isActive: " + isActive);
        if (popUpID >= ePopUpType.NUM_POPUPS) { Debug.LogError("Invalid popUp: " + popUpID); return; }
        foreach (GameObject go in PopUps) go.SetActive(false);
        PopUps[(int)popUpID].SetActive(isActive);
        CurActivePopup = (isActive == true ? popUpID : ePopUpType.NUM_POPUPS);
    }

    bool MenusActiveCheck()
    {
        return CurActivePopup == ePopUpType.NUM_POPUPS;
    }

    #region SPLASH
    public void OnClickTapToBegin()
    {        
        StaticStuff.PrintRifRafUI("OnClickTapToBegin()");
        // moUI01 - clicked "Tap to begin"
        if (MenusActiveCheck() == false) return;

        ToggleMenu(eMenuType.MAIN, true);
        InitMainMenu();
    }


    #endregion

    #region MAIN_MENU

    int CurNumActiveProfiles = 0;
    bool[] ProfileFileStatus;
    StaticStuff.ProfileInfo[] ProfilesInfo;
    int CurProfileSlot = 0;
    void InitMainMenu()
    {        
        RefreshProfileInfo();       
        foreach (Text t in MainMenuButtonsText) t.color = Color.black;
        switch (CurNumActiveProfiles)
        {
            case 0:
                MainMenuButtonsText[(int)eMainMenuButtons.NEW].color = Color.white;
                break;
            case StaticStuff.NUM_PROFILES: 
                MainMenuButtonsText[(int)eMainMenuButtons.CONTINUE].color = Color.white;
                MainMenuButtonsText[(int)eMainMenuButtons.DELETE].color = Color.white;
                break;            
            default:
                foreach (Text t in MainMenuButtonsText) t.color = Color.white;
                break;
        }
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
                ProfilesText[i].color = Color.white;
                //ProfilesText[i].text = "Mo Finish This";
                ProfilesText[i].text = ProfilesInfo[i].avatar.ToString() + ": " + ProfilesInfo[i].time;
            }
            else
            {
                ProfilesText[i].color = Color.black;
                ProfilesText[i].text = "Unused " + (i+1).ToString();
            }
        }
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

    public void OnClickComingSoon()
    {
        StaticStuff.PrintRifRafUI("OnClickComingSoon");
        if (MenusActiveCheck() == false) return;

        ToggleMenu(eMenuType.COMING_SOON, true);
    }
    public void OnClickAchievements()
    {
        StaticStuff.PrintRifRafUI("OnClickAchievements");
        if (MenusActiveCheck() == false) return;

        ToggleMenu(eMenuType.ACHIEVEMENTS, true);
    }
    public void OnClickTellYourFriends()
    {
        StaticStuff.PrintRifRafUI("OnClickTellYourFriends");
        if (MenusActiveCheck() == false) return;

        ToggleMenu(eMenuType.TELL_FRIENDS, true);
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
                StaticStuff.SetCurrentProfile(slotNum);
                StaticStuff.LoadProfileStartScene();
                TogglePopUp(0, false);
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

    private void OnGUI()
    {
        if (CurActiveMenu == eMenuType.AVATAR_SELECT && SelectedCaptain != null)
        {
            string captainName = "none";
            int avatar = -1; ;
            if (GUI.Button(new Rect(0, Screen.height / 50, 100, 100), "go"))
            {
                captainName = SelectedCaptain.name;
                avatar = int.Parse(captainName[9].ToString());
                StaticStuff.CreateNewProfile(avatar, CurProfileSlot);
                StaticStuff.LoadProfileStartScene();     // Avatar select       
                ToggleMenu(eMenuType.AVATAR_SELECT, false);

                CurActiveMenu = eMenuType.MAIN;
            }
        }
    }

    public Camera UICamera;
    GameObject SelectedCaptain = null;
    Vector3 LastCameraPos;
    #region CHARACTER_SELECT
    private void Update()
    {
        if (CurActiveMenu == eMenuType.AVATAR_SELECT)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Debug.Log("button down");
                LastCameraPos = Input.mousePosition;
                Ray ray = UICamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                  //  Debug.Log("clicked on: " + hit.collider.name);
                    SelectedCaptain = hit.collider.gameObject;
                    
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if(SelectedCaptain != null)
                {
                    float deltaX = Input.mousePosition.x - LastCameraPos.x;
                    //Debug.Log("deltaX: " + deltaX);
                    CaptainContainer.transform.Rotate(new Vector3(0f, -deltaX/10f, 0f));
                    LastCameraPos = Input.mousePosition;
                }
            }            
        }

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
    }
    #endregion

    
    #region NEW_GAME_POPUP
    public void OnClickNewGameYes()
    {
        StaticStuff.PrintRifRafUI("OnClickNewGameYes");
        TogglePopUp(ePopUpType.NEW_GAME, false);
        ToggleMenu(eMenuType.AVATAR_SELECT, true);
        MenuBG.enabled = false;      
    }
    public void OnClickNewGameNo()
    {
        StaticStuff.PrintRifRafUI("OnClickNewGameNo");
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
        TogglePopUp(0, false);
    }
    public void OnClickDeleteConfirmNo()
    {
        StaticStuff.PrintRifRafUI("OnClickDeleteConfirmNo");
        
        TogglePopUp(0, false);
    }
    #endregion
}
