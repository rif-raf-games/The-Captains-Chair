using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.GlobalVariables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MCP : MonoBehaviour
{
    [Header("UI")]
    public Camera UICamera;
    public RifRafMenuUI MenuUI;
    public RifRafInGamePopUp InGamePopUp;
    public ConvoUI ConvoUI;
    public RifRafExchangeJobBoard ExchangeJobBoard;
    public GameObject LoadingScreen;
    public GameObject LoadingAlien;
    public RawImage Curtain;
    public FixedJoystick Joystick;

    [Header("Sound")]
    public SoundFX SoundFX;
    public BackgroundMusic BGMusic;
    
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
        ConvoUI.Init(this);
        ConvoUI.gameObject.SetActive(false);
        ExchangeJobBoard.Init(this);
        ExchangeJobBoard.gameObject.SetActive(false);
        LoadingScreen.SetActive(false);
        LoadingAlien.SetActive(false);
        SetFadeAlpha(0f);
        Joystick.gameObject.transform.parent.gameObject.SetActive(false);

        SoundFXPlayer.Init(SoundFX, GetAudioVolume());
        BackgroundMusicPlayer.Init(BGMusic, GetAudioVolume());

        DontDestroyOnLoad(this.gameObject);
    }

    public void SetupSceneSound(List<SoundFX.FXInfo> soundFXUsedInScene)
    {
        SoundFX.SetupFXList(soundFXUsedInScene);
    }

    public void TMP_ShutOffUI()
    {
        MenuUI.gameObject.SetActive(false);
        InGamePopUp.gameObject.SetActive(false);
        ConvoUI.gameObject.SetActive(false);
        LoadingScreen.SetActive(false);
        LoadingAlien.SetActive(false);
        SetFadeAlpha(0f);
        Joystick.gameObject.transform.parent.gameObject.SetActive(false);
    }
    public ConvoUI TMP_GetConvoUI()
    {
        return this.ConvoUI;
    }
    public FixedJoystick TMP_GetJoystick()
    {
        return this.Joystick;
    }
    public void TMP_ToggleBurger(bool isActive)
    {
        MenuUI.gameObject.SetActive(false);
        if (isActive == false) InGamePopUp.gameObject.SetActive(false);
        else InGamePopUp.TMP_TurnOnBurger();
    }
    public void TMP_ShutOffExchangeBoard()
    {
        ExchangeJobBoard.ShutOffPopups();
        ExchangeJobBoard.gameObject.SetActive(false);
    }

    void SetFadeAlpha(float alpha)
    {
        Curtain.color = new Color(0f, 0f, 0f, alpha);
    }

    #region SCENE_TRANSITIONS
    public void LoadNextScene(string sceneName)
    {               
        StartCoroutine(LoadNextSceneDelay(sceneName));        
    }

    IEnumerator LoadNextSceneDelay(string sceneName)
    {
       // Debug.Log("LoadNextSceneDelay() sceneName: " + sceneName + ", Time.timeScale: " + Time.timeScale);

        //Debug.Log("1) Turn on loading screen, make curtain alpha 0, set alien to false");
        // 1) Turn on loading screen, make curtain alpha 0, set alien to false
        LoadingScreen.SetActive(true);
        SetFadeAlpha(0f);
        LoadingAlien.SetActive(false);

       // Debug.Log("2) fade out current scene");
        // 2) fade out current scene
        float timer = 0f;
        while(timer < 1f)
        {            
            SetFadeAlpha(timer);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        SetFadeAlpha(1f);

       // Debug.Log("3) Shut off any menu ui's and popups.  Shut on the alien and move the camera forward");
        // 3) Shut off any menu ui's and popups.  Shut on the alien and move the camera forward
        ToggleMenuUI(false);
        ToggleInGamePopUp(false);
        LoadingAlien.SetActive(true);
        UICamera.depth = 100;

       // Debug.Log("4) Fade in to loading screen");
        // 4) Fade in to loading screen
        timer = 1f;
        while (timer > 0f)
        {
            SetFadeAlpha(timer);
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        SetFadeAlpha(0f);

        // 5) Begin unloading the old scene if necessary
      //  Debug.Log("5) Begin unloading the old scene if necessary");
        if (SceneManager.sceneCount > 1)
        {
            int sceneIndex = (SceneManager.GetSceneAt(1).name.Contains("Front") ? 0 : 1);
            AsyncOperation asyncUnLoad = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(sceneIndex));
            while (asyncUnLoad.isDone == false)
            {                
                yield return null;
            }
        }

        yield return new WaitForSeconds(.1f);        

       // Debug.Log("6) Begin loading the next scene");
        // 6) Begin loading the next scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while(asyncLoad.isDone == false)
        {
            yield return null;
        }

        float fadeTime = 1f;
       // Debug.Log("7) Fade out loading screen");
        // 7) Fade out loading screen      
        // TMP shut off convo ui in the scene        
        timer = 0f;
        while (timer < 1f)
        {
            SetFadeAlpha(timer);
            timer += Time.deltaTime/ fadeTime;
            yield return new WaitForEndOfFrame();
        }
        SetFadeAlpha(1f);

        ConvoUI.TMP_SetArticyFlow();

       // Debug.Log("8) Move camera back and fade in to the in-game scene");
        // 8) Move camera back and fade in to the in-game scene
        UICamera.depth = -2;        
        timer = 1f;
        while (timer > 0f)
        {
            SetFadeAlpha(timer);
            timer -= Time.deltaTime/ fadeTime;
            yield return new WaitForEndOfFrame();
        }
        SetFadeAlpha(0f);
        LoadingScreen.SetActive(false);

        if(FindObjectOfType<TheCaptainsChair>() != null)
        {
            FindObjectOfType<TheCaptainsChair>().CheckStartDialogue(DialogueToStartOnThisScene);
        }
        Joystick.gameObject.transform.parent.gameObject.SetActive(true);        
        InGamePopUp.TogglePopUpPanel(true);
        InGamePopUp.TMP_TurnOnBurger();
    }

    public void ShowResultsText(string result)
    {
        InGamePopUp.ShowResultsText(result);
    }
    public void HideResultsText()
    {
        InGamePopUp.HideResultsText();
    }

    Dialogue DialogueToStartOnThisScene = null;
    public void SetDialogueToStartSceneOn(Dialogue dialogueToStartOn)
    {
        DialogueToStartOnThisScene = dialogueToStartOn;
    }
        
    #endregion    

    public void TurnOnMainMenu()
    {
        Debug.LogWarning("TurnOnMainMenu()");
        ToggleMenuUI(true);
        ToggleInGamePopUp(false);
        MenuUI.ToggleMenu(RifRafMenuUI.eMenuType.MAIN, true);
    }

    public void TurnOnInGamePopUp()
    {
        ToggleMenuUI(false);
        ToggleInGamePopUp(true);
        InGamePopUp.TogglePopUpPanel(true);
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
        SoundFX.SetVolume(vol);
        BGMusic.SetVolume(vol);
    }

    #endregion    

}
