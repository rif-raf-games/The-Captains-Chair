using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RifRafInGamePopUp : MonoBehaviour
{
    public GameObject MainPopupPanel;
    public GameObject ExchangeBoardButton;
    public GameObject ExchangeBoardText, SuspendJobText;
    public MissionHint MissionHint;
    public GameObject QuitConfirmPopup;
    public RifRafExchangeJobBoard ExchangeBoard;
    public VolumeControl MusicVolume;
    public VolumeControl SoundFXVolume;
    public MCP MCP;
    [Header("MainPopUpButtons")]
    public Button[] MainPopUpButtons;

    private void Awake()
    {
        StaticStuff.PrintRifRafUI("RifRafInGamePopUp.Awake()");
      //  Debug.Log("RifRafInGamePopUp.Awake()");

        MainPopupPanel.SetActive(false);
        this.MissionHint.Init();
        MissionHint.gameObject.SetActive(false);
        ExchangeBoard.gameObject.SetActive(false);
        QuitConfirmPopup.gameObject.SetActive(false);
    }
    public void SetMCP(MCP mcp)
    {
        //StaticStuff.PrintRifRafUI("RifRafInGamePopUp.Init()");
        this.MCP = mcp;
    }
    public void OnClickBurger()
    {
        StaticStuff.PrintRifRafUI("OnClickBurger()");
      //  Debug.Log("OnClickBurger()");
        if (PopupActiveCheck() == false) return;

        if (MainPopupPanel.activeSelf == true)
        {
            this.MCP.StartFreeRoam();
            //StaticStuff.SaveCurrentProfile("Closing In Game PopUp");
            StaticStuff.SaveCurrentSettings("OnClickBurger()");
        }
        else
        {
            this.MCP.StartPopupPanel();            
        }        
    }

    void ToggleMainPopUpButtons(bool isActive)
    {
      //  Debug.Log("*********ToggleMainPopUpButtons(): " + isActive);
        foreach (Button b in MainPopUpButtons) b.interactable = isActive;
    }

    public void ToggleMainPopupPanel(bool isActive)
    {
      //  Debug.Log("ToggleMainPopupPanel(): " + isActive);
        MainPopupPanel.SetActive(isActive);
        ToggleMainPopUpButtons(true);
    }

    public void TurnOnPopupMenu()
    {        
     //   Debug.Log("RifRafInGamePopUp.TurnOnPopupMenu()");
       
        ToggleMainPopupPanel(true);
        MusicVolume.Slider.value = this.MCP.GetMusicVolume();
        MusicVolume.Toggle.isOn = (MusicVolume.Slider.value > 0f);
        SoundFXVolume.Slider.value = this.MCP.GetSoundFXVolume();
        SoundFXVolume.Toggle.isOn = (SoundFXVolume.Slider.value > 0f);

        if (ArticyGlobalVariables.Default.Episode_01.First_Exchange == false)
        //if(false)
        {
            ExchangeBoardButton.SetActive(false);
        }
        else
        {
            ExchangeBoardButton.SetActive(true);
            if (FindObjectOfType<TheCaptainsChair>() != null)
            {
                ExchangeBoardText.SetActive(true);
                SuspendJobText.SetActive(false);
            }
            else
            {
                ExchangeBoardText.SetActive(false);
                SuspendJobText.SetActive(true);
            }
        }
    }
    

    

    public void ShutOffExchangeBoard()
    {
       // Debug.Log("ShutOffExchangeBoard()");
        ExchangeBoard.ShutOffQuitAcceptPopups();
        //ExchangeBoard.gameObject.SetActive(false);
        ToggleExchangeBoard(false);
    }

    public bool MenusActiveCheck()
    {
        return PopupActiveCheck() && MainPopupPanel.activeSelf == false;
    }
    public bool PopupActiveCheck()
    {
        return MissionHint.gameObject.activeSelf == false && ExchangeBoard.gameObject.activeSelf == false && QuitConfirmPopup.gameObject.activeSelf == false;
    }

#region MAIN_POPUP
    //mo put back here
    
    public void ToggleExchangeBoard(bool isActive)
    {
       // Debug.Log("ToggleExchangeBoard(): " + isActive);
        if (isActive == true) ExchangeBoard.FillBoard();
        ExchangeBoard.gameObject.SetActive(isActive);
        ToggleMainPopUpButtons(!isActive);
    }

    public void OnClickExchangeBoard()
    {
        StaticStuff.PrintRifRafUI("OnClickExchangeBoard()");
      //  Debug.Log("OnClickExchangeBoard()");
        if (PopupActiveCheck() == false) return;

        if (FindObjectOfType<TheCaptainsChair>() != null)
        {
            ToggleExchangeBoard(true);
        }
        else
        {
            Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");
            ArticyGlobalVariables.Default.Mini_Games.Coming_From_Main_Game = false;
            ArticyGlobalVariables.Default.Mini_Games.Returning_From_Mini_Game = true;
            ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success = false;
            string sceneName = jumpSave.Template.Quit_Mini_Game_Result.SceneName;
            FindObjectOfType<MCP>().LoadNextScene(sceneName, null, jumpSave);
        }
    }

    public void ResetGameClicked()
    {
        StaticStuff.PrintRifRafUI("ResetGameClick");
        ToggleMissionHint(false);
        this.MCP.StartFreeRoam();
    }

    public void ToggleMissionHint(bool isActive)
    {
        StaticStuff.PrintRifRafUI("ToggleMissionHint(): " + isActive);        
        MissionHint.ToggleResetMiniGameButton(false);
        if (isActive == true)
        {
            // Debug.LogWarning("Get the hint ready");
            MissionHint.SetupHint();
            if(FindObjectOfType<MiniGame>() != null)
            {
                MissionHint.ToggleResetMiniGameButton(true);
            }
        }
        MissionHint.gameObject.SetActive(isActive);
        ToggleMainPopUpButtons(!isActive);
    }

    public void OnClickMissionHint()
    {
        StaticStuff.PrintRifRafUI("OnClickMissionHint()");
      //  Debug.Log("OnClickMissionHint()");
        if (PopupActiveCheck() == false) return;

        ToggleMissionHint(true);
    }

    public void ShowResultsText(string result)
    {
        //  Debug.Log("ShowResultsText()");
        MissionHint.gameObject.SetActive(true);
        MissionHint.HintText.text = result;
        MissionHint.ToggleResetMiniGameButton(false);
    }
    public void HideResultsText()
    {
        //Debug.Log("HideResultsText()");
        MissionHint.gameObject.SetActive(false);
    }

    public void OnClickResumeGame()
    {
        StaticStuff.PrintRifRafUI("OnClickResumeGame() PUT THIS BACK");
       // Debug.Log("OnClickResumeGame()");
        if (PopupActiveCheck() == false) return;

        StaticStuff.SaveCurrentSettings("OnClickResumeGame()");
        this.MCP.StartFreeRoam();
    }

    void ToggleQuitConfirmPopUp(bool isActive)
    {
        QuitConfirmPopup.gameObject.SetActive(isActive);
        ToggleMainPopUpButtons(!isActive);
    }

    public void OnClickQuitToMainMenu()
    {
        StaticStuff.PrintRifRafUI("OnClickQuitToMainMenu()");
       // Debug.Log("OnClickQuitToMainMenu()");
        if (PopupActiveCheck() == false) return;
        
        //QuitConfirmPopup.gameObject.SetActive(true);
        ToggleQuitConfirmPopUp(true);
    }
    public void OnClickQuitToMainCancel()
    {
       // Debug.Log("OnClickQuitToMainCancel()");
        //QuitConfirmPopup.gameObject.SetActive(false);
        ToggleQuitConfirmPopUp(false);
    }
    public void OnClickQuitToMainConfirm()
    {
       // Debug.Log("OnClickQuitToMainConfirm()");
        //QuitConfirmPopup.gameObject.SetActive(false);
        ToggleQuitConfirmPopUp(false);
        this.MCP.LoadNextScene("Front End Launcher");
    }
    
    public void OnSliderAudioVolume(Slider slider)
    {
        StaticStuff.PrintRifRafUI("OnSliderAudioVolume()");
      //  Debug.Log("OnSliderAudioVolume(): " + slider.gameObject.name);
        if(slider == MusicVolume.Slider)
        {
            this.MCP.SetMusicVolume((int)slider.value);
            MusicVolume.Toggle.isOn = (MusicVolume.Slider.value > 0f);
        }
        else
        {
            this.MCP.SetSoundFXVolume((int)slider.value);
            SoundFXVolume.Toggle.isOn = (SoundFXVolume.Slider.value > 0f);
        }        
    }

    public void OnToggleAudioVolume(Toggle toggle)
    {
        //Debug.Log("OnToggleAudioVolume(): " + toggle.gameObject.name);
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
#endregion

#region MISSION_HINT
    public void OnClickMissionHintBack()
    {
        StaticStuff.PrintRifRafUI("OnClickMissionHintBack()");
      //  Debug.Log("OnClickMissionHintBack()");

        ToggleMissionHint(false);
    }
#endregion

    [System.Serializable]
    public class VolumeControl
    {
        public Slider Slider;
        public Toggle Toggle;
    }
}
