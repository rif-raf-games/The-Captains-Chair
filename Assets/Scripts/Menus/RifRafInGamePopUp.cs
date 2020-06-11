﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RifRafInGamePopUp : MonoBehaviour
{
    public GameObject PopUpPanel;
    public GameObject MissionHint;
    public RifRafExchangeJobBoard ExchangeBoard;    
    public MCP MCP;

    private void Awake()
    {
        PopUpPanel.SetActive(false);
        MissionHint.SetActive(false);
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
            GetComponentInChildren<Slider>().value = this.MCP.GetAudioVolume();
            SetAudioVolumeToggle();
        }
        else
        {
            StaticStuff.SaveSaveData("Closing In Game PopUp");
        }
    }
    public void ToggleMissionHint(bool isActive)
    {
        MissionHint.SetActive(isActive);
    }
    public void ToggleExchangeBoard(bool isActive)
    {
        if(isActive == true) ExchangeBoard.FillBoard();
        ExchangeBoard.gameObject.SetActive(isActive);
    }

    bool PopupActiveCheck()
    {
        return MissionHint.activeSelf == false && ExchangeBoard.gameObject.activeSelf == false;
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

    void SetAudioVolumeToggle()
    {
        if (this.MCP.GetAudioVolume() == 0)
        {
            GetComponentInChildren<Toggle>().isOn = false;
        }
        else
        {
            GetComponentInChildren<Toggle>().isOn = true;
        }
    }
    public void OnSliderAudioVolume(Slider slider)
    {
        StaticStuff.PrintRifRafUI("OnSliderAudioVolume()");
        
        //Debug.Log(slider.value);
        this.MCP.SetAudioVolume((int)slider.value);
        SetAudioVolumeToggle();
    }

    public void OnToggleAudioVolume(Toggle toggle)
    {
        if(toggle.isOn == true)
        {
            this.MCP.SetAudioVolume(100);
        }
        else
        {
            this.MCP.SetAudioVolume(0);
        }
        GetComponentInChildren<Slider>().value = this.MCP.GetAudioVolume();
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
}
