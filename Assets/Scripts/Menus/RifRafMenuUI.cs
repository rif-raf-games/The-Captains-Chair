using Articy.The_Captain_s_Chair.GlobalVariables;
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

    public enum ePopUpType { SAVED_GAMES, NEW_GAME, DELETE_GAME, DELETE_CONFIRM, NUM_POPUPS };
    [Header("PopUps: Saved Games, New, Delete, Delete Confirm")]
    public GameObject[] PopUps;
    public ePopUpType CurActivePopup;

    public enum eSaveGameFunction { NEW, CONTINUE, DELETE, NUM_SAVE_GAME_FUNCTIONS };
    public eSaveGameFunction CurActiveSaveGameFunction;

    public MCP MCP;

    public Image MenuBG;
    public GameObject CaptainContainer;

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
        if (MenusActiveCheck() == false) return;

        ToggleMenu(eMenuType.MAIN, true);
    }
    #endregion
    
    #region MAIN_MENU
    public void OnClickContinue()
    {        
        StaticStuff.PrintRifRafUI("OnClickContinue");
        if (MenusActiveCheck() == false) return;

        TogglePopUp(ePopUpType.SAVED_GAMES, true);
        CurActiveSaveGameFunction = eSaveGameFunction.CONTINUE;
    }
    public void OnClickNewGame()
    {
        StaticStuff.PrintRifRafUI("OnClickNewGame");
        if (MenusActiveCheck() == false) return;

        TogglePopUp(ePopUpType.SAVED_GAMES, true);
        CurActiveSaveGameFunction = eSaveGameFunction.NEW;
    }
    public void OnClickDeleteSaveGame()
    {
        StaticStuff.PrintRifRafUI("OnClickNewGame");
        if (MenusActiveCheck() == false) return;

        TogglePopUp(ePopUpType.SAVED_GAMES, true);
        CurActiveSaveGameFunction = eSaveGameFunction.DELETE;
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
        StaticStuff.PrintRifRafUI("OnClickSaveGameSlot() slowNum: " + slotNum);
        switch(CurActiveSaveGameFunction)
        {
            case eSaveGameFunction.NEW:
                TogglePopUp(ePopUpType.NEW_GAME, true);                
                break;
            case eSaveGameFunction.CONTINUE:
                if (StaticStuff.SaveDataExists() == true)
                {
                    StaticStuff.CheckSceneLoadSave();
                    TogglePopUp(0, false);
                }
                else
                {
                    TogglePopUp(ePopUpType.NEW_GAME, true);
                }                
                break;
            case eSaveGameFunction.DELETE:
                if (StaticStuff.SaveDataExists() == true)
                {
                    TogglePopUp(ePopUpType.DELETE_GAME, true);
                }
                else
                {
                    Debug.LogError("Trying to delete save file that doesn't exist");
                }                
                break;
            default:
                Debug.LogError("Invalid CurActiveSaveGameFunction: " + CurActiveSaveGameFunction.ToString());
                break;
        }
        CurActiveSaveGameFunction = eSaveGameFunction.NUM_SAVE_GAME_FUNCTIONS;
    }
    #endregion

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
            else if (Input.GetMouseButtonUp(0))
            {
                /*string captainName = "none";
                int avatar = -1; ;
                if (SelectedCaptain != null)
                {
                    captainName = SelectedCaptain.name;
                    avatar = int.Parse(captainName[9].ToString());                    
                    StaticStuff.CreateNewSaveData();
                    ArticyGlobalVariables.Default.TheCaptain.Avatar = avatar;
                    StaticStuff.CheckSceneLoadSave();
                }
                Debug.Log("Button up.  Do we have a selected character: " + captainName + ", avatar: " + avatar);    */            
               // SelectedCaptain = null;
            }
        }
    }
    #endregion

    private void OnGUI()
    {
        if (CurActiveMenu == eMenuType.AVATAR_SELECT && SelectedCaptain != null)
        {
            string captainName = "none";
            int avatar = -1; ;
            if (GUI.Button(new Rect(0,Screen.height/50, 100, 100), "go"))
            {
                captainName = SelectedCaptain.name;
                avatar = int.Parse(captainName[9].ToString());
                StaticStuff.CreateNewSaveData(avatar);
                ArticyGlobalVariables.Default.TheCaptain.Avatar = avatar;
                StaticStuff.CheckSceneLoadSave();
                CurActiveMenu = eMenuType.MAIN;
            }
        }
    }
    #region NEW_GAME_POPUP
    public void OnClickNewGameYes()
    {
        StaticStuff.PrintRifRafUI("OnClickNewGameYes");
        TogglePopUp(ePopUpType.NEW_GAME, false);
        ToggleMenu(eMenuType.AVATAR_SELECT, true);
        MenuBG.enabled = false;
      //  StaticStuff.CreateNewSaveData();
       // StaticStuff.CheckSceneLoadSave();
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
        StaticStuff.CreateNewSaveData();
        TogglePopUp(0, false);
    }
    public void OnClickDeleteConfirmNo()
    {
        StaticStuff.PrintRifRafUI("OnClickDeleteConfirmNo");
        TogglePopUp(0, false);
    }
    #endregion
}
