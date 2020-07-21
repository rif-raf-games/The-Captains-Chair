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
    //public GameObject LoadingAlien;
    public RawImage Curtain, SpinWheel, LoadingImage;
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
        //LoadingAlien.SetActive(false);
        //SetCurtainAlpha(0f);        
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
        //LoadingAlien.SetActive(false);
        //SetCurtainAlpha(0f);
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

   /* void SetCurtainAlpha(float alpha)
    {
        Curtain.color = new Color(0f, 0f, 0f, alpha);
    }*/

    #region SCENE_TRANSITIONS
    public void LoadNextScene(string sceneName, string posToSave="", string savedPos="")
    {               
        StartCoroutine(LoadNextSceneDelay(sceneName, posToSave, savedPos));        
    }

    float FADE_TIME = 1f;
    IEnumerator LoadNextSceneDelay(string sceneName, string posToSave="", string savedPos="")
    {
      //  Debug.LogWarning("LoadNextSceneDelay() sceneName: " + sceneName + ", Time.timeScale: " + Time.timeScale);

      //  Debug.LogWarning("1) Turn on loading screen, make curtain alpha 0, set alien to false");
        // 1) Turn on loading screen, make curtain alpha 0, set alien to false
        LoadingScreen.SetActive(true);
        SpinWheel.gameObject.SetActive(true);
        SpinWheel.color = new Color(1f, 1f, 1f, 0f);
        LoadingImage.gameObject.SetActive(true);
        LoadingImage.color = new Color(1f, 1f, 1f, 0f);
        Curtain.color = new Color(0f, 0f, 0f, 0f);        

     //   Debug.LogWarning("2) fade the curtain to opaque to cover up current scene");
        yield return new WaitForSeconds(.1f);
        // 2) fade the curtain to opaque to cover up current scene
        float timer = 0f; 
        while(timer < FADE_TIME)
        {
            Curtain.color = new Color(0f, 0f, 0f, timer/FADE_TIME);
            SpinWheel.color = new Color(1f, 1f, 1f, timer/FADE_TIME);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Curtain.color = new Color(0f, 0f, 0f, 1f);
        SpinWheel.color = new Color(1f, 1f, 1f, 1f);

       // Debug.LogWarning("3) Shut off any menu ui's and popups.  Shut on the alien and move the camera forward");
        // 3) Shut off any menu ui's and popups.  Shut on the alien and move the camera forward
        ToggleMenuUI(false);
        ToggleInGamePopUp(false);
       // LoadingAlien.SetActive(true);
        UICamera.depth = 100;       

     //   Debug.LogWarning("4) Fade in to loading screen image"); // so far everything is ok
        // 4) Fade in to loading screen        
        timer = 0f;
        while (timer < FADE_TIME)
        {
            LoadingImage.color = new Color(1f, 1f, 1f, timer);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        LoadingImage.color = new Color(1f, 1f, 1f, 1f);

        // 5) Begin unloading the old scene if necessary
       // Debug.LogWarning("5) Begin unloading the old scene if necessary");
        string curSceneName = "";
        if (SceneManager.sceneCount > 1)
        {
            int sceneIndex = (SceneManager.GetSceneAt(1).name.Contains("Front") ? 0 : 1);
            curSceneName = SceneManager.GetSceneAt(sceneIndex).name;
            AsyncOperation asyncUnLoad = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(sceneIndex));
            while (asyncUnLoad.isDone == false)
            {                
                yield return null;
            }
        }

      //  Debug.LogWarning("5a) Scene should be done being unloaded but wait another .1 seconds");
        yield return new WaitForSeconds(.1f);
     //   Debug.LogWarning("5b) after the .1 seconds");
     //   Debug.LogWarning("6) Begin loading the next scene");

        // fade to black, fade in spinny        
        // fade in image

        // fade out image
        // fade out black, fade out spinny

        // 6) Begin loading the next scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while(asyncLoad.isDone == false)
        {
            yield return null;
        }

      //  Debug.LogWarning("6a) dont loading the next scene.  Do another .1f delay");
        yield return new WaitForSeconds(.1f);
        //  Debug.LogWarning("6b) after .1 second delay");

        // Debug.LogError("I TOOK OUT SOME CODE SO MAKE SURE TO PUT IT BACK");
        
        if(posToSave != "" )
        {
        // Debug.LogError("!!!!!!!!!!! mosavepos02 - set up all the positions based on what was loaded");
        // Debug.LogError("Positions_To_Save: " + posToSave);
        // Debug.LogError("Saved_Positions: " + savedPos);
            string[] entityNames = posToSave.Split(',');
            string[] posVals = savedPos.Split(',');
            int index = 0;
            foreach(string entityName in entityNames)
            {
            // Debug.LogError("try to find: " + entityName);
                GameObject go = GameObject.Find(entityName);
                if(go == null) { Debug.LogError("no entity called: " + entityName + " is in the game."); continue; }
                Vector3 pos = new Vector3(float.Parse(posVals[index * 3]), float.Parse(posVals[index * 3 + 1]), float.Parse(posVals[index * 3 + 2]));
            //  Debug.LogError("setting " + entityName + " to this position: " + pos.ToString("F5"));
                go.transform.position = pos;
                index++;
            }
        }
        // Debug.LogError("cur scene: " + curSceneName + ", next scene: " + sceneName);
        if(curSceneName.Contains("E1.Exchange") && sceneName.Contains("E1.Plaza"))
        {
         //   Debug.LogWarning("You're about to set up the correct values in this special case");
            GameObject go = GameObject.Find("Captain");
            if (go == null) Debug.LogError("No object named Captain in this scene");
            else go.transform.position = new Vector3(-48f, 0f, 30f);
            go = GameObject.Find("Grunfeld");
            if (go == null) Debug.LogError("No object named Grunfeld in this scene");
            go.transform.position = new Vector3(-44f, 0f, -24f);
            go = GameObject.Find("Carver");
            if (go == null) Debug.LogError("No object named Carver in this scene");
            go.transform.position = new Vector3(-44f, 0f, -40f);
        }

        float fadeTime = 1f;
    //    Debug.LogWarning("7) Fade out loading screen image");
        // 7) Fade out loading screen image             
        timer = FADE_TIME;
        while (timer > 0f)
        {
            LoadingImage.color = new Color(1f, 1f, 1f, timer);
            timer -= Time.deltaTime/ fadeTime;
            yield return new WaitForEndOfFrame();
        }
        LoadingImage.color = new Color(1f, 1f, 1f, 0f);

        ConvoUI.TMP_SetArticyFlow();

     //    Debug.LogWarning("8) Move camera back and fade in to the in-game scene");
        // 8) Move camera back and fade out the spinny and black curtain
        UICamera.depth = -2;        
        timer = FADE_TIME;
        while (timer > 0f)
        {
            Curtain.color = new Color(0f, 0f, 0f, timer);
            SpinWheel.color = new Color(1f, 1f, 1f, timer);
            timer -= Time.deltaTime/ fadeTime;
            yield return new WaitForEndOfFrame();
        }
     //   Debug.LogWarning("8a) done with fade");
        Curtain.color = new Color(0f, 0f, 0f, 0f);
        SpinWheel.color = new Color(1f, 1f, 1f, 0f);
     //   Debug.LogWarning("9a) turn LoadingScreen off");
        LoadingScreen.SetActive(false);

        if(FindObjectOfType<TheCaptainsChair>() != null)
        {
       //     Debug.LogWarning("10a) Check start dialogue");
            FindObjectOfType<TheCaptainsChair>().CheckStartDialogue(DialogueToStartOnThisScene);
        }

        if (FindObjectOfType<MiniGameMCP>() == null)
        {
      //      Debug.Log("10b) turn joystick on");
            Joystick.gameObject.transform.parent.gameObject.SetActive(true);
        }
        else
        {
     //       Debug.Log("10b) turn joystick off");
            Joystick.gameObject.transform.parent.gameObject.SetActive(false);
        }

     //   Debug.LogWarning("11a) TogglePopUpPanel(true)");
        InGamePopUp.TogglePopUpPanel(true);
     //   Debug.LogWarning("11b) TMP_TurnOnBurger");
        InGamePopUp.TMP_TurnOnBurger();
    }

    public void ToggleJoystick(bool val)
    {
        // monote - this gets called a lot during the articy flow stuff, so get rid of that
      //  Debug.Log("ToggleJoystick() val: " + val);
        this.Joystick.ResetInput();
        Joystick.gameObject.transform.parent.gameObject.SetActive(val);
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
