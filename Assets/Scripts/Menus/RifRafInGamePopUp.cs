using Articy.The_Captain_s_Chair.GlobalVariables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RifRafInGamePopUp : MonoBehaviour
{
    public GameObject PopUpPanel;
    public GameObject ExchangeBoardButton;
    public MissionHint MissionHint;
    public RifRafExchangeJobBoard ExchangeBoard;
    public VolumeControl MusicVolume;
    public VolumeControl SoundFXVolume;
    public MCP MCP;

    private void Awake()
    {
        PopUpPanel.SetActive(false);
        this.MissionHint.Init();
        MissionHint.gameObject.SetActive(false);
    }
    public void Init(MCP mcp)
    {
        this.MCP = mcp;
    }
    public void OnClickBurger()
    {
        StaticStuff.PrintRifRafUI("OnClickBurger()");
        TogglePopUpPanel(!PopUpPanel.activeSelf);
    }

    public void TogglePopUpPanel(bool isActive)
    {
        //Debug.Log("RifRafInGamePopUp.TogglePopUpPanel() isActive: " + isActive);
        PopUpPanel.SetActive(isActive);  
        if(isActive == true)
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
            }
        }
        else
        {
            StaticStuff.SaveCurrentProfile("Closing In Game PopUp");
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
        return MissionHint.gameObject.activeSelf == false && ExchangeBoard.gameObject.activeSelf == false;
    }

#region MAIN_POPUP
    public void OnClickResumeGame()
    {
        StaticStuff.PrintRifRafUI("OnClickResumeGame()");
        if (PopupActiveCheck() == false) return;

        TogglePopUpPanel(false);
    }
    public void TMP_TurnOnBurger()
    {
        this.gameObject.SetActive(true);
        TogglePopUpPanel(false);
        ToggleMissionHint(false);
    }

    public void ToggleExchangeBoard(bool isActive)
    {
        if (isActive == true) ExchangeBoard.FillBoard();
        ExchangeBoard.gameObject.SetActive(isActive);
    }

    public void OnClickExchangeBoard()
    {
        StaticStuff.PrintRifRafUI("OnClickExchangeBoard()");
        if (PopupActiveCheck() == false) return;

        ToggleExchangeBoard(true);
    }
    public void OnClickMissionHint()
    {
        StaticStuff.PrintRifRafUI("OnClickMissionHint()");
        if (PopupActiveCheck() == false) return;

        ToggleMissionHint(true);
    }

    public void OnClickMainMenu()
    {
        StaticStuff.PrintRifRafUI("OnClickMainMenu()");
        if (PopupActiveCheck() == false) return;

        this.MCP.TurnOnMainMenu();
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
