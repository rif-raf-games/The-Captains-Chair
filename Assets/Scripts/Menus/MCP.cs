using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.Unity;
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
    public void LoadNextScene(string sceneName = "", Scene_Jump sceneJump = null, Mini_Game_Jump miniGameJump = null, string posToSave ="", string savedPos="")
    {               
        StartCoroutine(LoadNextSceneDelay(sceneName, sceneJump, miniGameJump, posToSave, savedPos));        
    }

    public Text DebugText;
    IEnumerator FadeObjects(List<RawImage> images, float fadeTime, float alphaStart)
    {
        foreach (RawImage image in images) image.color = new Color(1f, 1f, 1f, alphaStart);
        float alphaEnd = 1f - alphaStart;
        float timer = 0f;
        while(timer < fadeTime)
        {
            float percentage = timer / fadeTime;
            float alpha = Mathf.Lerp(alphaStart, alphaEnd, percentage);
            Color color = new Color(1f, 1f, 1f, alpha);
            foreach (RawImage image in images) image.color = color;
           // if(DebugText != null) DebugText.text = "percentage: " + percentage.ToString("F2") + ", timer: " + timer.ToString("F2") + ", color: " + color.ToString("F2"); 
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        foreach (RawImage image in images) image.color = new Color(1f, 1f, 1f, alphaEnd);
    }

    IEnumerator SceneUnloadLoad(string sceneName)
    {
        Debug.LogWarning("------ Begin scene unload: " + Time.time);
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
        Debug.LogWarning("------ End scene unload");
        Debug.LogWarning("----- begin scene load");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (asyncLoad.isDone == false)
        {
            yield return null;
        }
        Debug.LogWarning("----- end scene load");
        Pause = false;
    }
    
    float FADE_TIME = 1f;
    bool Pause;
    IEnumerator LoadNextSceneDelay(string sceneName = "", Scene_Jump sceneJump = null, Mini_Game_Jump miniGameJump = null, string posToSave="", string savedPos="")
    {
        Debug.LogWarning("LoadNextSceneDelay() sceneName: " + sceneName + ", Time.timeScale: " + Time.timeScale);
       
        List<Texture> loadingTextures = new List<Texture>();
        List<ArticyObject> loadingImageAOs = new List<ArticyObject>();
        List<RawImage> curImages;      
        float delayTime = 0f, fadeTime = 0f;
        Texture defaultTexture = LoadingImage.texture;
               
        // 1) Init things and get the data from the Scene_Jump or Mini_Game_Jump
        LoadingScreen.SetActive(true);
        SpinWheel.gameObject.SetActive(true);
        Curtain.gameObject.SetActive(true);
        LoadingImage.gameObject.SetActive(false);        
         
        if (sceneJump != null)
        {            
            loadingImageAOs = sceneJump.Template.LoadingScreen.LoadingImages;                     
            delayTime = sceneJump.Template.LoadingScreen.DisplayTime;
            fadeTime = sceneJump.Template.LoadingScreen.FadeTime;
        }
        else if(miniGameJump != null)
        {
           // Debug.LogWarning("miniGameJump: " + miniGameJump.TechnicalName);
            if(ArticyGlobalVariables.Default.Mini_Games.Coming_From_Main_Game == true)
            {               
                loadingImageAOs = miniGameJump.Template.LoadingScreen.LoadingImages;
                delayTime = miniGameJump.Template.LoadingScreen.DisplayTime;
                fadeTime = miniGameJump.Template.LoadingScreen.FadeTime;
            }            
            else
            {
                if(ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success == true)
                {             
                    loadingImageAOs = miniGameJump.Template.Success_Mini_Game_Result.LoadingImages;
                    delayTime = miniGameJump.Template.Success_Mini_Game_Result.DisplayTime;
                    fadeTime = miniGameJump.Template.Success_Mini_Game_Result.FadeTime;
                }
                else
                {                    
                    loadingImageAOs = miniGameJump.Template.Quit_Mini_Game_Result.LoadingImages;
                    delayTime = miniGameJump.Template.Quit_Mini_Game_Result.DisplayTime;
                    fadeTime = miniGameJump.Template.Quit_Mini_Game_Result.FadeTime;
                }
            }
        }

        foreach (ArticyObject imageAO in loadingImageAOs)
        {
            Sprite s = ((Asset)imageAO).LoadAssetAsSprite();
            loadingTextures.Add(s.texture);
        }


        if (loadingTextures.Count == 0 || delayTime == 0f || fadeTime == 0f)
        {
           // Debug.LogError("This SceneJump/MiniGameJump isn't set up properly. textures count: " + loadingTextures.Count + ", delayTime: " + delayTime + ", fadeTime: " + fadeTime);
            loadingTextures.Add(LoadingImage.texture);
            delayTime = 1f;
            fadeTime = 1f;
        }
        
        // 2) fade the curtain/spinwheel to opaque to cover up current scene    
       // Debug.LogWarning("----- starting the fade in of curtain/spinwheel");
        curImages = new List<RawImage>() { Curtain, SpinWheel };        
        yield return StartCoroutine(FadeObjects(curImages, fadeTime, 0f));
       // Debug.LogWarning("----- end of curtain/spinwheel fade in");
        // do some cleanup
        ToggleMenuUI(false);
        ToggleInGamePopUp(false);

        // 3) Fade in the first image        
       // Debug.LogWarning("-------- fade in first image");
        int curLoadingImageIndex = 0;
        LoadingImage.texture = loadingTextures[curLoadingImageIndex];
        curImages = new List<RawImage>() { LoadingImage };
        LoadingImage.gameObject.SetActive(true);
        yield return StartCoroutine(FadeObjects(curImages, fadeTime, 0f));
       // Debug.LogWarning("------- done with fade in of first image");

        // 4) Ok, now we're ready to do the unload of the current scene and loading the next.  For these two processes 
        // I'm going to be keeping track of time manually during the unload and load, then just do a loop for the rests.
        float totalImageTime = loadingTextures.Count * delayTime;
        float curImageTime = 0f;        
        string curSceneName = "";
        float unloadStart = Time.time;
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
        float unloadTime = Time.time - unloadStart;
        curImageTime = unloadTime;
        if (unloadTime >= delayTime) Debug.LogError("ERROR: the unload time for the scene was longer than the delay time so the delay time must be wack.  delayTime: " + delayTime + ", unloadTime: " + unloadTime);
     //   Debug.LogWarning("---- finished unloading the scene with an unload time of: " + unloadTime);

        // 5) Ok the scene is unloaded, so now load the next scene
        // So the AsyncOperation is in two parts:
        // first .9 - loading the scene
        // last .1 to get to 1.0 - scene starting
        // So since I turned off allowSceneActivation, when we're here the scene has been loaded but it has NOT 
        // started.  So if we've taken longer to load the scene than the loading screen images time go ahead and
        // start the scene. If not, wait until the loading images are done.
      //  Debug.LogWarning("------ about to start the scene load");
        float loadStart = Time.time;
        AsyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        AsyncLoad.allowSceneActivation = false;
        while (AsyncLoad.isDone == false)
        {
         //   Debug.Log("load: " + AsyncLoad.progress);            
            if (AsyncLoad.progress >= .9f) break;
            yield return null;
            curImageTime += Time.deltaTime;
            if(curImageTime >= delayTime)
            {
            //    Debug.LogWarning("---- during the load, we went past the time for the current image so get the next one going");
                curLoadingImageIndex++;
                if(curLoadingImageIndex >= loadingTextures.Count)
                {
           //         Debug.LogError("---- ok we're actually done with loading screens, so just hang here and don't do anything");
                    curImageTime = Mathf.NegativeInfinity;
                }
                else
                {
           //         Debug.LogError("----- ok we're ready to switch loading images so get the next one up there");
                    LoadingImage.texture = loadingTextures[curLoadingImageIndex];
                    curImageTime = curImageTime - delayTime;
                }
            }
        }
        float loadTime = Time.time - loadStart;
      //  Debug.LogWarning("----- done with the scene load, progress is: " + AsyncLoad.progress);
      //  Debug.LogWarning("---- loadTime: " + loadTime.ToString("F2"));
        showLoadButton = true;
        
     //   Debug.LogWarning("---- after the first .9 of the asyncLoad operation (which means the scene is loaded but hasn't done any initialization");
        // 6) Ok we're here, so the scene is loaded but it has not started or even initialized.  
        // At this point check to see if we're done with the loading images or not.  If not, then just cycle thru them. If
        // we are done, then get the curtain fade going.
     //   Debug.LogWarning("------ curLoadingImageIndex: " + curLoadingImageIndex.ToString("F2")  + ", curImageTime: " + curImageTime.ToString("F2"));
        if (curLoadingImageIndex >= loadingTextures.Count)
        {
      //      Debug.LogWarning("------ ok we're done with the scene load and we're all done with the images based on the curLoadingImageIndex so just get to the fade in");
        }
        else
        {
     //       Debug.LogWarning("------- done with scene load but we're not done with the images yet");
            while(curLoadingImageIndex < loadingTextures.Count)
            {
                curImageTime += Time.deltaTime;
                if(curImageTime >= delayTime)
                {
      //              Debug.LogWarning("----- we're post loading the scene and am going to check if we have more images");
                    curLoadingImageIndex++;
                    if (curLoadingImageIndex < loadingTextures.Count)
                    {
         //               Debug.LogError("---- not done with the images yet so get the next one up");
                        LoadingImage.texture = loadingTextures[curLoadingImageIndex];
                        curImageTime = curImageTime - delayTime;
                    }                    
                    else
                    {
         //               Debug.LogWarning("------ ok we're done with images post loading so just let the loop fall through");
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }

        // 7) ok we're here so the scene is loaded and the images have all been shown so just get the fade up going
     //   Debug.LogWarning("------- ok we're done with loading and images so fade out the image");
        curImages = new List<RawImage>() { LoadingImage };        
        yield return StartCoroutine(FadeObjects(curImages, fadeTime, 1f));
      //  Debug.LogWarning("----- done fading out the image, so start the scene");
        AsyncLoad.allowSceneActivation = true;
        while(AsyncLoad.isDone == false)
        {
           // Debug.Log("starting: " + AsyncLoad.progress);
            yield return new WaitForEndOfFrame();
        }
        int num = 0;
        //  Debug.LogWarning("Ok the scene has officially started so do any scene initting");
        if (FindObjectOfType<TheCaptainsChair>() != null)
        {
            GameObject captain = GameObject.Find("Captain");
            int avatar = ArticyGlobalVariables.Default.TheCaptain.Avatar;
         //   Debug.Log("avatar: " + avatar);
            LoadedCaptain = Resources.Load<GameObject>("Prefabs\\Characters\\Crew\\Captain_0" + avatar.ToString());
            LoadedCaptain = Instantiate(LoadedCaptain, captain.transform.position, captain.transform.rotation);
          //  Debug.Log("loaded up captain: " + LoadedCaptain.name);            
            LoadedCaptain.name = "Captain";
            Destroy(captain);
            
            while(LoadedCaptain.GetComponent<ArticyFlow>().StartCalled == false)
            {
                yield return null;
                num++;
                if (num > 100) break;
            }
            FindObjectOfType<TheCaptainsChair>().Player = LoadedCaptain.GetComponent<CCPlayer>();
        }
      //  Debug.Log(num);
            
        if (posToSave != "")
        {
            string[] entityNames = posToSave.Split(',');
            string[] posVals = savedPos.Split(',');
            int index = 0;
            foreach (string entityName in entityNames)
            {                
                GameObject go = GameObject.Find(entityName);
                if (go == null) { Debug.LogError("no entity called: " + entityName + " is in the game."); continue; }
                Vector3 pos = new Vector3(float.Parse(posVals[index * 3]), float.Parse(posVals[index * 3 + 1]), float.Parse(posVals[index * 3 + 2]));             
                go.transform.position = pos;
                index++;
            }
        }        
        if (curSceneName.Contains("E1.Exchange") && sceneName.Contains("E1.Plaza"))
        {
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

        ConvoUI.TMP_SetArticyFlow();
        if (FindObjectOfType<TheCaptainsChair>() != null)
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


        curImages = new List<RawImage>() { Curtain, SpinWheel };
        yield return StartCoroutine(FadeObjects(curImages, fadeTime, 1f));
        LoadingImage.texture = defaultTexture;
        LoadingScreen.SetActive(false);
        //  Debug.LogError("---- ok by now we're done!!!!");
    }

    public GameObject LoadedCaptain;

    AsyncOperation AsyncLoad;
    bool showLoadButton = false;
   /* private void OnGUI()
    {
        if (showLoadButton == false) return;
        if(GUI.Button(new Rect(0,500, 100, 100), "feh"))
        {
            Debug.Log("do your thing");
            AsyncLoad.allowSceneActivation = true;
        }
    }*/
    public void ToggleJoystick(bool val)
    {
        // monote - this gets called a lot during the articy flow stuff, so get rid of that
        //Debug.Log("ToggleJoystick() val: " + val);
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
