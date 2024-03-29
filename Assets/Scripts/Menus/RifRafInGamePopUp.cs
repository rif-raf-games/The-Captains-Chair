﻿using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.Features;
using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RifRafInGamePopUp : MonoBehaviour
{
    public MCP MCP;
    [Header("Main Panel Stuff")]
    public GameObject MainPopupPanel;    
    public GameObject AcceptButton;
    public GameObject ResetPuzzleButton;
    public GameObject ShareButton;
    public GameObject AcceptJobText, SuspendJobText;
    public MissionHint MissionHint;
    public GameObject QuitConfirmPopup;    
    public RifRafExchangeJobBoard ExchangeBoard;        
    public VolumeControl MusicVolume;
    public VolumeControl SoundFXVolume;
    public Text Cash;    

    [Header("IAP Panel Stuff")]
    public GameObject IAPPanel;
    public GameObject IAPQuitConfirmPopup;
    public GameObject IAPBuyPopup;
    public GameObject RestoreButton;
    public GenericPopup GenericPopup;    

    [Header("MainPopUpButtons")]
    public Button[] MainPopUpButtons;

    private void Awake()
    {
        //StaticStuff.PrintRifRafUI("RifRafInGamePopUp.Awake()");        
        MainPopupPanel.SetActive(false);       
        this.MissionHint.Init();
        MissionHint.gameObject.SetActive(false);        
        QuitConfirmPopup.gameObject.SetActive(false);
        IAPPanel.gameObject.SetActive(false);
        ButtonPrefab.gameObject.SetActive(false);
        GenericPopup.gameObject.SetActive(false);

# if UNITY_IOS
       // Debug.Log("Turn on Restore button because you're on iOS.");
        RestoreButton.SetActive(true);
#elif UNITY_ANDROID
       // Debug.Log("Turn off Restore button because you're on Android");
        RestoreButton.SetActive(false);
#endif

        this.gameObject.SetActive(false);        
    }
    public void SetMCP(MCP mcp)
    {
        //StaticStuff.PrintRifRafUI("RifRafInGamePopUp.Init()");
        this.MCP = mcp;
    }
    public void OnClickBurger()
    {
        Debug.Log("OnClickBurger()");        
        if (PopupActiveCheck() == false) return;

        if(IAPPanel.activeSelf == true)
        {
            // we don't want the burger button to do anything with the IAP stuff open
            return;
        }        
        if (MainPopupPanel.activeSelf == true )
        {
            this.MCP.StartFreeRoam();            
            StaticStuff.SaveCurrentSettings("OnClickBurger()");
        }
        else
        {
            this.MCP.StartPopupPanel();
            Cash.text = ArticyGlobalVariables.Default.Captains_Chair.Crew_Money.ToString();

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX            
            ShareButton.SetActive(false);
#else
            ShareButton.SetActive(true);
#endif
        }
    }    

    void ToggleMainPopUpButtons(bool isActive)
    {
        //Debug.Log("ToggleMainPopUpButtons(): " + isActive + "...IGNORE NOW --Menu--");
        //foreach (Button b in MainPopUpButtons) b.interactable = isActive;
    }

    public void ToggleMainPopupPanel(bool isActive)
    {
       // Debug.Log("ToggleMainPopupPanel(): " + isActive);
        MainPopupPanel.SetActive(isActive);       
        ToggleMainPopUpButtons(true); 
    }

    [Header("Menu Content")]
    public ArticyRef MissionFlowRef;
    public ArticyRef CodexRef;
    public ArticyRef ShipsLogRef;
    public ScrollRect ContentScrollView;
    public GameObject ExchangeContent;
    public GameObject TasksContent;
    public GameObject CodexContent;
    public GameObject ShipLogContent;
    public MenuButton ButtonPrefab;
    public Text FullJobNameText;
    public Text JobLocationText;
    public Text POCText;
    public Text JobDescriptionText;    
    MenuButton CurJobButton;
    enum eInGameMenus { EXCHANGE, TASKS, CODEX, SHIPS_LOG};
    eInGameMenus CurMenu;

    #region IAP
    public void ToggleIAPUI(bool isActive)
    {                        
        IAPPanel.SetActive(isActive);
        IAPQuitConfirmPopup.SetActive(false);
        IAPBuyPopup.SetActive(false);
        GenericPopup.gameObject.SetActive(false);
    }

    public void IAPPurchaseSuccessful()
    {
        
    }
    
    public void IAPGenericPopupSetup(string title, string message, UnityEngine.Events.UnityAction button01Action, UnityEngine.Events.UnityAction button02Action)
    {        
        GenericPopup.gameObject.SetActive(true);
        GenericPopup.TitleText.text = title;
        GenericPopup.MainText.text = message;

        GenericPopup.Button01.gameObject.SetActive(true);
        GenericPopup.Button01.GetComponentInChildren<Text>().text = "Retry";
        GenericPopup.Button01.onClick.RemoveAllListeners();
        GenericPopup.Button01.onClick.AddListener(button01Action);

        GenericPopup.Button02.gameObject.SetActive(true);
        GenericPopup.Button02.GetComponentInChildren<Text>().text = "Close";
        GenericPopup.Button02.onClick.RemoveAllListeners();
        GenericPopup.Button02.onClick.AddListener(button02Action);
    }


    #endregion

    /************************ IAP END **************************************/

    public void SetVolumeSliders()
    {
        MusicVolume.Slider.value = this.MCP.GetMusicVolume();
        MusicVolume.Toggle.isOn = (MusicVolume.Slider.value > 0f);
        SoundFXVolume.Slider.value = this.MCP.GetSoundFXVolume();
        SoundFXVolume.Toggle.isOn = (SoundFXVolume.Slider.value > 0f);
    }

    public bool FUUnity = false;
    float FUUnityTimer = 0.0f;
    public void TurnOnSetUpMainPopupPanel( bool initContents )
    {
        //  Debug.Log("RifRafInGamePopUp.TurnOnPopupMenu(): initContents: " + initContents);
        //  Debug.Log("------------------------------TurnOnPopupMenu() num menu buttons: " + GetNumMenuButtons() + ", numTimes: " + numTimes++);
        FUUnity = true;
        FUUnityTimer = 0.0f;
        ToggleMainPopupPanel(true);
        if (initContents == false) return;
        int debugVar = 0;

        SetVolumeSliders();
                
        List<FlowFragment> containersToCheck = new List<FlowFragment>();        
        List<Job_Card> jobs = new List<Job_Card>();
        List<Articy.The_Captain_s_Chair.Codex> codexes = new List<Articy.The_Captain_s_Chair.Codex>();
        List<Ships_Log> shipsLogs = new List<Ships_Log>();        
        
        containersToCheck.Add(MissionFlowRef.GetObject() as FlowFragment);
        containersToCheck.Add(CodexRef.GetObject() as FlowFragment);
        containersToCheck.Add(ShipsLogRef.GetObject() as FlowFragment);
        
        while(containersToCheck.Count > 0)
        {
            //Debug.Log("num containersToCheck: " + containersToCheck.Count);
            FlowFragment containerToCheck = containersToCheck[0];            
            containersToCheck.RemoveAt(0);
            if (containerToCheck == null) Debug.LogError("WTF 1");
            if (containerToCheck.Children == null) Debug.LogError("WTF 2");
            foreach(ArticyObject child in containerToCheck.Children)
            {
                if (child == null) Debug.LogError("WTF 3");
                Job_Card job = child as Job_Card;
                Articy.The_Captain_s_Chair.Codex codex = child as Articy.The_Captain_s_Chair.Codex;
                Ships_Log shipsLog = child as Ships_Log;
                if (job != null) 
                {     
                   // Debug.Log("We've found a JOB called " + job.DisplayName);
                    jobs.Add(job);
                }               
                else if(codex != null)
                {
                   // Debug.Log("We've got a CODEX called: " + codex.DisplayName);
                    codexes.Add(codex);
                }
                else if(shipsLog != null)
                {
                   // Debug.Log("We got a SHIPS LOG called: " + shipsLog.DisplayName);
                    shipsLogs.Add(shipsLog);
                }
                else
                {
                    FlowFragment childFrag = child as FlowFragment;
                    if(childFrag.InputPins[0].Text.CallScript() == true)
                    //if(true)
                    {
                        //Debug.Log("add container: " + childFrag.DisplayName + " to the containers to check");
                        containersToCheck.Add(childFrag);
                    }
                    else
                    {
                       /// Debug.Log("Do not add container: " + childFrag.DisplayName + " to containers to check;");
                    }
                }
            }
            if(debugVar++ > 100)
            {
                Debug.LogError("something in the loop is messed up");
                break;
            }
        }

       /// Debug.Log("------------------------------before creating jobs: " + GetNumMenuButtons());

       // Debug.Log("num jobs: " + jobs.Count);
        foreach (Job_Card job in jobs)
        {
            MenuButton button = CreateButton();

            button.JobNameText.text = job.Template.Exchange_Mission.Job_Name;
            button.JobNumText.text = job.Template.Exchange_Mission.Job_ID;

            button.JobLocation = job.Template.Exchange_Mission.Job_Location;
            button.PointOfContact = job.Template.Exchange_Mission.Point_Of_Contact;
            button.JobDescription = job.Template.Exchange_Mission.Job_Description;            
            if (job.Template.Exchange_Mission.Job_Type == Job_Type.Exchange) button.transform.SetParent(ExchangeContent.transform);
            else button.transform.SetParent(TasksContent.transform);
            button.transform.localScale = new Vector3(.625f, .625f, .625f);

            button.ExchangeMission = job.Template.Exchange_Mission;
            button.LoadingScreen = job.Template.LoadingScreen;
            button.PuzzlesToPlay = job.Template.Mini_Game_Puzzles_To_Play;
            button.DialogueList = job.Template.Dialogue_List;
            button.SuccessResult = job.Template.Success_Mini_Game_Result;
            button.QuitResult = job.Template.Quit_Mini_Game_Result;
            button.SuccessSaveFragment = job.Template.Success_Save_Fragment;
            button.PaymentFragment = job.Template.payment;

            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(() => OnClickMenuButton(button));
        }

       // Debug.Log("------------------------------after creating jobs: " + GetNumMenuButtons());
       // Debug.Log("------------------------------before creating codexes: " + GetNumMenuButtons());

       // Debug.Log("num codexes: " + codexes.Count);
        foreach(Articy.The_Captain_s_Chair.Codex codex in codexes)
        {
            MenuButton button = CreateButton();
            button.JobNameText.text = codex.Template.Codex.Entry_Name;
            button.JobNumText.text = "";

            button.JobLocation = "";
            button.PointOfContact = "";
            button.JobDescription = codex.Template.Codex.Entry_Info;            
            button.transform.SetParent(CodexContent.transform);
            button.transform.localScale = new Vector3(.625f, .625f, .625f);            

            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(() => OnClickMenuButton(button));
        }

        foreach(Ships_Log shipsLog in shipsLogs)
        {
            MenuButton button = CreateButton();
            button.JobNameText.text = shipsLog.Template.Codex.Entry_Name;
            button.JobNumText.text = "";

            button.JobLocation = "";
            button.PointOfContact = "";
            button.JobDescription = shipsLog.Template.Codex.Entry_Info;
            button.transform.SetParent(ShipLogContent.transform);
            button.transform.localScale = new Vector3(.625f, .625f, .625f);

            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(() => OnClickMenuButton(button));
        }
       // Debug.Log("---------------------------after creating codexes: " + GetNumMenuButtons());

       // Debug.Log("--------------------------- 1: " + GetNumMenuButtons());
        ToggleContent(false);
        ShipLogContent.SetActive(true);
        ContentScrollView.content = ShipLogContent.GetComponent<RectTransform>();
        InitMenuButtonInfo(null, eInGameMenus.SHIPS_LOG);
      //  Debug.Log("---------------------------2: " + GetNumMenuButtons());

        if (FindObjectOfType<TheCaptainsChair>() != null)
        {   // main game
            AcceptJobText.SetActive(true);
            SuspendJobText.SetActive(false);
        }
        else
        {   // mini game            
            AcceptJobText.SetActive(false);
            SuspendJobText.SetActive(true);
            //Debug.Log("Are we here: " + SuspendJobText.transform.parent.name);
        }
    }


    MenuButton CreateButton()
    {        
        MenuButton menuButton = Instantiate<MenuButton>(ButtonPrefab);
        menuButton.gameObject.SetActive(true);                                
        return menuButton;
    }

    private void Update()
    {
        if (FindObjectOfType<TheCaptainsChair>() == null)
        {   // mini game
            AcceptButton.SetActive(true);
            MiniGameMCP mgMCP = FindObjectOfType<MiniGameMCP>();
            if(mgMCP != null && mgMCP.IsPuzzleTutorial == true)
            {
                AcceptButton.SetActive(false);
            }
        }
        else
        {   // main game
            if (CurJobButton == null || CurMenu == eInGameMenus.CODEX || CurMenu == eInGameMenus.SHIPS_LOG)
            {
                AcceptButton.SetActive(false);
            }
            else
            {
                AcceptButton.SetActive(true);
            }
        }

        if (FUUnity == true)
        {
            FUUnityTimer += Time.deltaTime;
            if (FUUnityTimer > .5f) FUUnity = false;
        }
    }

    public MenuButton GetCurJobButton()
    {
        return CurJobButton;
    }

    void InitMenuButtonInfo(MenuButton button, eInGameMenus menu)
    {
        string s = "InitMenuButtonInfo(): ";
        if (button == null) 
            s += "null button, "; 
        else 
            s += "button: " + button.name + ", ";
        s += menu.ToString();
       // Debug.Log(s);

        CurMenu = menu;
        CurJobButton = button;        

        FullJobNameText.text = (button == null ? "" : button.JobNameText.text);
        JobLocationText.text = (button == null ? "" : button.JobLocation);
        POCText.text = (button == null ? "" : button.PointOfContact);
        JobDescriptionText.text = (button == null ? "" : button.JobDescription);
        if(FindObjectOfType<MiniGameMCP>() == null)
        {
            // no mini game so shut off the reset button
            ResetPuzzleButton.SetActive(false);            
        }
        else
        {
            // we're on a mini game so show reset button
            ResetPuzzleButton.SetActive(true);
        }
    }

    void ClearContent()
    {
//        Debug.Log("**************** ClearContent()");
        ToggleContent(true);
        foreach (Transform child in ExchangeContent.transform) Destroy(child.gameObject);
        foreach (Transform child in TasksContent.transform) Destroy(child.gameObject);
        foreach (Transform child in CodexContent.transform)
        {
            if(child.name != "Job Menu Button IN_SCENE") Destroy(child.gameObject);
        }
        foreach (Transform child in ShipLogContent.transform) Destroy(child.gameObject);
    }
    void ToggleContent(bool isActive)
    {
        ExchangeContent.SetActive(isActive);
        TasksContent.SetActive(isActive);
        CodexContent.SetActive(isActive);
        ShipLogContent.SetActive(isActive);
    }
    
    public void OnClickMenuTab(Button button)
    {
        GameObject currentContent = ExchangeContent;
        //ToggleContent(false);
        if (button.name.Contains("Exchange") && CurMenu != eInGameMenus.EXCHANGE)
        {
            ToggleContent(false);
            ExchangeContent.SetActive(true);
            currentContent = ExchangeContent;
            InitMenuButtonInfo(null, eInGameMenus.EXCHANGE);
        }
        else if(button.name.Contains("Task") && CurMenu != eInGameMenus.TASKS)
        {
            ToggleContent(false);
            TasksContent.SetActive(true);
            currentContent = TasksContent;
            InitMenuButtonInfo(null, eInGameMenus.TASKS);
        }
        else if (button.name.Contains("Codex") && CurMenu != eInGameMenus.CODEX)
        {
            ToggleContent(false);
            CodexContent.SetActive(true);
            currentContent = CodexContent;
            InitMenuButtonInfo(null, eInGameMenus.CODEX);
        }
        else if (button.name.Contains("Log") && CurMenu != eInGameMenus.SHIPS_LOG)
        {            
            ToggleContent(false);
            ShipLogContent.SetActive(true);
            currentContent = ShipLogContent;
            InitMenuButtonInfo(null, eInGameMenus.SHIPS_LOG);
        }

        ContentScrollView.content = currentContent.GetComponent<RectTransform>();
    }
    void OnClickMenuButton(MenuButton button)
    {
        //Debug.Log("OnClickMenuButton(): " + button.name);
        InitMenuButtonInfo(button, CurMenu);
    }

    public void ShutOffExchangeBoard()
    {
      //  Debug.Log("ShutOffExchangeBoard()");
        ExchangeBoard.ShutOffQuitAcceptPopups();        
        ToggleExchangeBoard(false);
        ClearContent();
    }

    public bool MenusActiveCheck()
    {
       // Debug.LogWarning("monewui MenusActiveCheck() CHECK THIS .PopupActiveCheck(): " + PopupActiveCheck() + ", MainPopupPanel.activeSelf: " + MainPopupPanel.activeSelf);        
        return PopupActiveCheck() && MainPopupPanel.activeSelf == false;
    }
    public bool PopupActiveCheck()
    {
        return MissionHint.gameObject.activeSelf == false /*&& this.gameObject.activeSelf == false*/ && QuitConfirmPopup.gameObject.activeSelf == false; 
    }

#region MAIN_POPUP
    
    
    public void ToggleExchangeBoard(bool isActive)
    {
        //Debug.LogError("monewui FIX THIS ToggleExchangeBoard(): " + isActive);
        if (isActive == true) ExchangeBoard.FillBoard();        
        ToggleMainPopUpButtons(!isActive);
        if (isActive == false) ClearContent();
    }

    public void OnClickExchangeBoard()
    {
       // StaticStuff.PrintRifRafUI("OnClickExchangeBoard()");      
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

    public void BailMiniGame()
    {
        Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");
        ArticyGlobalVariables.Default.Mini_Games.Coming_From_Main_Game = false;
        ArticyGlobalVariables.Default.Mini_Games.Returning_From_Mini_Game = true;
        ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success = false;
        string sceneName = jumpSave.Template.Quit_Mini_Game_Result.SceneName;
        FindObjectOfType<MCP>().LoadNextScene(sceneName, null, jumpSave);
        ShutOffExchangeBoard();
        this.MCP.ShutOffAllUI();
    }

    public void ResetGameClicked()
    {
       // StaticStuff.PrintRifRafUI("ResetGameClick");
        ToggleMissionHint(false);
        this.MCP.StartFreeRoam();
    }

    public void ToggleMissionHint(bool isActive)
    {        
        //Debug.Log("ToggleMissionHint(): " + isActive);        
        //MissionHint.ToggleResetMiniGameButton(false);
        if (isActive == true)
        {
            // Debug.LogWarning("Get the hint ready");
            MissionHint.SetupHint();
           /* if(FindObjectOfType<MiniGame>() != null) // monewui took out Feb 3
            {
                MissionHint.ToggleResetMiniGameButton(true);
            }*/
        }
        MissionHint.gameObject.SetActive(isActive);
        ToggleMainPopUpButtons(!isActive);
    }

    public void OnClickMissionHint()
    {        
        Debug.Log("OnClickMissionHint()");

        if (PopupActiveCheck() == false) return;
        ToggleMissionHint(true);
    }

    public void ShowResultsText(string result)
    {
         // Debug.Log("ShowResultsText()");
        MissionHint.gameObject.SetActive(true);
        MissionHint.HintText.text = result;
        //MissionHint.ToggleResetMiniGameButton(false);
        ResetPuzzleButton.SetActive(false);
    }
    public void HideResultsText()
    {
        //Debug.Log("HideResultsText()");
        MissionHint.gameObject.SetActive(false);
    }

    public void OnClickResumeGame()
    {
        //StaticStuff.PrintRifRafUI("OnClickResumeGame() PUT THIS BACK");
       // Debug.Log("OnClickResumeGame()");
        if (PopupActiveCheck() == false) return;

        StaticStuff.SaveCurrentSettings("OnClickResumeGame()");
        ClearContent();
        this.MCP.StartFreeRoam();
    }

    public void OnClickShareOnSocial()
    {
        this.MCP.ShareOnSocial(this.gameObject);
    }    

    public void OnClickQuitToMainMenu()
    {
        //StaticStuff.PrintRifRafUI("OnClickQuitToMainMenu()");
       // Debug.Log("OnClickQuitToMainMenu()");
        if (PopupActiveCheck() == false) return;
        
        //QuitConfirmPopup.gameObject.SetActive(true);
        ToggleQuitConfirmPopUp(true);
        //ClearContent();
    }

    void ToggleQuitConfirmPopUp(bool isActive)
    {
        QuitConfirmPopup.gameObject.SetActive(isActive);
        ToggleMainPopUpButtons(!isActive);
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
        ClearContent();
        this.MCP.LoadNextScene("Front End Launcher");
    }

    public void OnSliderAudioVolume(Slider slider)
    {
        //  Debug.Log("OnSliderAudioVolume(): " + slider.gameObject.name);
        if (FUUnity == true) return;
        // the line above is because for some reason when you start the ui the slider callbacks and toggle callbacks get called
        // multiple times without anyone touching anything.  This is just a timer to wait out that crap
        else
        {
            if (slider == MusicVolume.Slider)
            {
                // Debug.Log("RifRafMenuUI().OnSliderAudioVolume() Music: " + slider.value);
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
    }

    public void OnToggleAudioVolume(Toggle toggle)
    {
        //Debug.Log("OnToggleAudioVolume(): " + toggle.gameObject.name);
        if (FUUnity == true) return;
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
      //  StaticStuff.PrintRifRafUI("OnClickMissionHintBack()");
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
