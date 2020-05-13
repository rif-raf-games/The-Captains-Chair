using Articy.The_Captain_s_Chair.GlobalVariables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCP : MonoBehaviour
{
    
    public RifRafMenuUI MenuUI;
    public RifRafInGamePopUp InGamePopUp;

    public SoundFX soundFX;
    public BackgroundMusic bgMusic;
    

    private void Awake()
    {
        if (MenuUI == null || InGamePopUp == null) { Debug.LogError("No MenuUI or InGamePopUp in MCP"); return; }

        MCP mcp = FindObjectOfType<MCP>();
        if (mcp != this)
        {
            Debug.LogError("There should only ever be one MCP in the scene.  Tell Mo."); 
            return;
        }
         
        ToggleMenuUI(true);
        MenuUI.Init(this);
        ToggleInGamePopUp(false);
        InGamePopUp.Init(this);
        MenuUI.ToggleMenu(RifRafMenuUI.eMenuType.SPLASH, true);
        DontDestroyOnLoad(this.gameObject);
    }    

    public void TurnOnMainMenu()
    {
        ToggleMenuUI(true);
        ToggleInGamePopUp(false);
        MenuUI.ToggleMenu(RifRafMenuUI.eMenuType.MAIN, true);
    }

    public void TurnOnInGamePopUp()
    {
        ToggleMenuUI(false);
        ToggleInGamePopUp(true);
        InGamePopUp.TogglePopUpPanel(false);
    }

    public void InitMenusForMainGame()
    {      
        ToggleMenuUI(false);
        ToggleInGamePopUp(true);
        InGamePopUp.TogglePopUpPanel(false);
        InGamePopUp.ToggleMissionHint(false);

        soundFX = FindObjectOfType<SoundFX>();        
        bgMusic = FindObjectOfType<BackgroundMusic>();        
    }
    public void ToggleMenuUI(bool isActive)
    {
        MenuUI.gameObject.SetActive(isActive);        
    }
    public void ToggleInGamePopUp(bool isActive)
    {
        InGamePopUp.gameObject.SetActive(isActive);
    }

    #region GAME_SETTINGS
    public int GetAudioVolume()
    {
        return ArticyGlobalVariables.Default.Game_Settings.Audio_Volume;
    }
    public void SetAudioVolume(int vol)
    {
        ArticyGlobalVariables.Default.Game_Settings.Audio_Volume = vol;
        soundFX.SetVolume(vol);
        bgMusic.SetVolume(vol);
    }

#endregion

    /*  private void OnGUI()
      {
          if(MenuUI.gameObject.activeSelf == true)
          {
              if (GUI.Button(new Rect(Screen.width - 100, Screen.height / 2, 100, 100), "Menu Off"))
              {
                  MenuUI.gameObject.SetActive(false);
              }
          }        
      }*/
}
