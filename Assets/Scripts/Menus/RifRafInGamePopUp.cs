using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RifRafInGamePopUp : MonoBehaviour
{
    public GameObject PopUpPanel;
    public GameObject ExchangeBoardButton;
    public GameObject ExchangeBoardText, SuspendJobText;
    public MissionHint MissionHint;
    public GameObject QuitConfirmPopup;
    public RifRafExchangeJobBoard ExchangeBoard;
    public VolumeControl MusicVolume;
    public VolumeControl SoundFXVolume;
    public MCP MCP;

    private void Awake()
    {
        StaticStuff.PrintRifRafUI("RifRafInGamePopUp.Awake()");
        PopUpPanel.SetActive(false);
        this.MissionHint.Init();
        MissionHint.gameObject.SetActive(false);
        ExchangeBoard.gameObject.SetActive(false);
        QuitConfirmPopup.gameObject.SetActive(false);
    }
    public void Init(MCP mcp)
    {
        StaticStuff.PrintRifRafUI("RifRafInGamePopUp.Init()");
        this.MCP = mcp;
    }
    public void OnClickBurger()
    {
        StaticStuff.PrintRifRafUI("OnClickBurger()");
        TogglePopUpPanel(!PopUpPanel.activeSelf);
    }

    public void TogglePopUpPanel(bool isActive)
    {
        StaticStuff.PrintRifRafUI("RifRafInGamePopUp.TogglePopUpPanel() isActive: " + isActive);
        PopUpPanel.SetActive(isActive);
        if (isActive == true)
        {
            MusicVolume.Slider.value = this.MCP.GetMusicVolume();
            MusicVolume.Toggle.isOn = (MusicVolume.Slider.value > 0f);
            SoundFXVolume.Slider.value = this.MCP.GetSoundFXVolume();
            SoundFXVolume.Toggle.isOn = (SoundFXVolume.Slider.value > 0f);

            if (ArticyGlobalVariables.Default.Episode_01.First_Exchange == false)
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
        else
        {
            StaticStuff.SaveCurrentProfile("Closing In Game PopUp");
        }
        if(FindObjectOfType<MiniGameMCP>() != null)
        {
            FindObjectOfType<MiniGameMCP>().SetDialogueActive(isActive);
            this.MCP.ToggleJoystick(false);
        }
        else
        {
            this.MCP.ToggleJoystick(!isActive);
        }        
    }
    public void ToggleMissionHint(bool isActive)
    {
        if (isActive == true)
        {
           // Debug.LogWarning("Get the hint ready");
            MissionHint.SetupHint();
        }
        MissionHint.gameObject.SetActive(isActive);
    }

    public void ShowResultsText(string result)
    {
        MissionHint.gameObject.SetActive(true);
        MissionHint.HintText.text = result;
    }
    public void HideResultsText()
    {
        MissionHint.gameObject.SetActive(false);
    }

    

    bool PopupActiveCheck()
    {
        return MissionHint.gameObject.activeSelf == false && ExchangeBoard.gameObject.activeSelf == false && QuitConfirmPopup.gameObject.activeSelf == false;
    }

#region MAIN_POPUP
    public void OnClickResumeGame()
    {
        StaticStuff.PrintRifRafUI("OnClickResumeGame()");
        if (PopupActiveCheck() == false) return;

        TogglePopUpPanel(false);
    }
    

    public void ToggleExchangeBoard(bool isActive)
    {
        if (isActive == true) ExchangeBoard.FillBoard();
        ExchangeBoard.gameObject.SetActive(isActive);
    }

    public void OnClickExchangeBoard()
    {
        StaticStuff.PrintRifRafUI("OnClickExchangeBoard()");
        Debug.Log("OnClickExchangeBoard()");
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
    public void OnClickMissionHint()
    {
        StaticStuff.PrintRifRafUI("OnClickMissionHint()");
        if (PopupActiveCheck() == false) return;

        ToggleMissionHint(true);
    }

    public void OnClickQuitToMainMenu()
    {
        StaticStuff.PrintRifRafUI("OnClickQuitToMainMenu()");
        if (PopupActiveCheck() == false) return;

        //this.MCP.LoadNextScene("Front End Launcher");
        QuitConfirmPopup.gameObject.SetActive(true);
    }
    public void OnClickQuitToMainCancel()
    {
        QuitConfirmPopup.gameObject.SetActive(false);
    }
    public void OnClickQuitToMainConfirm()
    {
        QuitConfirmPopup.gameObject.SetActive(false);
        this.MCP.LoadNextScene("Front End Launcher");
    }
    
    public void OnSliderAudioVolume(Slider slider)
    {
        StaticStuff.PrintRifRafUI("OnSliderAudioVolume()");

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
        if(toggle == MusicVolume.Toggle)
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

#region EXCHANGE_BOARD
#endregion

#region MISSION_HINT
    public void OnClickMissionHintBack()
    {
        StaticStuff.PrintRifRafUI("OnClickMissionHintBack()");

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
