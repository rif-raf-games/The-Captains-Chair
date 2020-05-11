using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RifRafMenuUI : MonoBehaviour
{    
    public enum eMenuType { SPLASH, MAIN, COMING_SOON, ACHIEVEMENTS, TELL_FRIENDS, NUM_MENUS };
    [Header("Menus: Splash, Main, Coming Soon, Achievements, Tell Friends\n")]
    public GameObject[] Menus;
    public eMenuType CurActiveMenu;

    public enum ePopUpType { SAVED_GAMES, NEW_GAME, DELETE_GAME, DELETE_CONFIRM, NUM_POPUPS };
    [Header("PopUps: Saved Games, New, Delete, Delete Confirm")]
    public GameObject[] PopUps;
    public ePopUpType CurActivePopup;

    public enum eSaveGameFunction { NEW, CONTINUE, DELETE, NUM_SAVE_GAME_FUNCTIONS };
    public eSaveGameFunction CurActiveSaveGameFunction;

    private void Awake()
    {
        foreach (GameObject go in Menus) go.SetActive(false);
        CurActiveMenu = eMenuType.NUM_MENUS;
        foreach (GameObject go in PopUps) go.SetActive(false);
        CurActivePopup = ePopUpType.NUM_POPUPS;

        CurActiveSaveGameFunction = eSaveGameFunction.NUM_SAVE_GAME_FUNCTIONS;
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

    #region NEW_GAME_POPUP
    public void OnClickNewGameYes()
    {
        StaticStuff.PrintRifRafUI("OnClickNewGameYes");
        TogglePopUp(0, false);
        StaticStuff.CreateNewSaveData();
        StaticStuff.CheckSceneLoadSave();
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
