
//#define USE_RR_ONGUI
using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UDP;
using System.Linq;
using System;

public class MCP : MonoBehaviour
{
    [Header("UI Root Objects")]    
    public RifRafMenuUI MenuUI;
    public RifRafInGamePopUp InGamePopUp;
    public ConvoUI ConvoUI;
    public RifRafExchangeJobBoard ExchangeJobBoard;
    [Header("Loading Screen")]
    public GameObject LoadingScreen;    
    public RawImage Curtain, SpinWheel, LoadingImage;    
    [Header("Misc")]
    public FixedJoystick Joystick;
    public Button CameraToggleButton;
    public Sprite CaptainAvatar;
    public VideoPlayerRR VideoPlayerRR;
    public Camera UICamera;

    [Header("IAP")]
    //public IAPManager IAPManager;
    public bool SaveNextObjectForIAP = false;
   // public IFlowObject IAPPauseObject = null;

    [Header("Sound")]
    public SoundFX SoundFX;
    public BackgroundMusic BGMusic;
    
    [Header("Aspect Ratio")]
    public bool TabletMode = false;
    public float AspectVal = 0f;      

    private void Awake()
    {
#if UNITY_ANDROID
        Debug.Log("You're on UNITY_ANDROID");
#elif UNITY_IOS
        Debug.Log("You're on UNITY_IOS");
#elif UNITY_IPHONE
    Debug.Log("You're on UNITY_IPHONE");
#elif UNITY_EDITOR_WIN
    Debug.Log("You're on UNITY_EDITOR_WIN");
#elif UNITY_STANDALONE_WIN
    Debug.Log("You're on UNITY_STANDALONE_WIN");
#elif UNITY_EDITOR_OSX
    Debug.Log("You're on UNITY_EDITOR_OSX");
#elif UNITY_STANDALONE_OSX
    Debug.Log("You're on UNITY_STANDALONE_OSX");
#else
    Debug.LogError("Unknown platform");
#endif

        if (MenuUI == null || InGamePopUp == null) 
        {
            string s = "MenuUI==null: " + (MenuUI == null) + ", InGamePopUp==null: " + (InGamePopUp == null);
            Debug.LogError(s); 
            return; 
        }
        MCP mcp = FindObjectOfType<MCP>();
        if (mcp != this)
        {
            Debug.LogError("There should only ever be one MCP in the scene."); 
            return;
        }
                 
        MenuUI.SetMCP(this);        
        InGamePopUp.SetMCP(this);
        ConvoUI.SetMCP(this);
        ExchangeJobBoard.SetMCP(this);

        // below is bullshit but it's the only way for the UI to not send
        // callback events when opening up the in game popup the first time
        InGamePopUp.gameObject.SetActive(true);
        InGamePopUp.TurnOnSetUpMainPopupPanel(false);
        InGamePopUp.gameObject.SetActive(false);
        //
        StaticStuff.LoadSettings();
        SoundFXPlayer.Init(SoundFX, GetSoundFXVolume());
        BackgroundMusicPlayer.Init(BGMusic, GetMusicVolume());                
        
        DontDestroyOnLoad(this.gameObject);        
        StartSplashScreen();

        // we only support landscape        
        float w = Screen.width;
        float h = Screen.height;
        AspectVal = w / h;
        TabletMode = (AspectVal < 1.44f);
       // Debug.Log("AspectVal: " + AspectVal + ", TabletMode: " + TabletMode);
    }

    private void Start()
    {
        AudioListener.volume = .1f;
    }

    #region IAP
   /* IEnumerator LoadIAPManager()
    {
        yield return new WaitForSeconds(1);
        this.IAPManager.gameObject.SetActive(true);
    }*/

    public void StopIAPPanel()
    {
        ShutOffAllUI();

        InGamePopUp.ToggleIAPUI(false);
        InGamePopUp.gameObject.SetActive(false);
    }
    public void StartIAPPanel()
    {        
        ShutOffAllUI();

        InGamePopUp.gameObject.SetActive(true);
        InGamePopUp.ToggleIAPUI(true);
    }
    #endregion

    public void StartPopupPanel()
    {
        // Debug.Log("StartPopupPanel()");        
        ShutOffAllUI();

        InGamePopUp.gameObject.SetActive(true);
        InGamePopUp.TurnOnSetUpMainPopupPanel(true);
    }

    public void StartDialogueConversation()
    {
       // Debug.Log("--------------------------------------------------StartDialogueConversation()");
        ShutOffAllUI();
        ConvoUI.gameObject.SetActive(true);           
    }

    public bool IsConvoUIOn()
    {
        return ConvoUI.gameObject.activeSelf;
    }

    void StartSplashScreen()
    {
        ShutOffAllUI();

        MenuUI.gameObject.SetActive(true);
        MenuUI.UICamera.enabled = true;
        MenuUI.ToggleMenu(RifRafMenuUI.eMenuType.SPLASH, true);
    }
    public void StartMainMenu()
    {
        ShutOffAllUI();

        MenuUI.gameObject.SetActive(true);
        MenuUI.UICamera.enabled = true;
        MenuUI.ToggleMenu(RifRafMenuUI.eMenuType.MAIN, true);
        MenuUI.InitMainMenu();
        BackgroundMusicPlayer.Play("Exchange_Background_Track");
    }

    public void StartFreeRoam()
    {
       // Debug.Log("StartFreeRoam()");
        ShutOffAllUI();

        InGamePopUp.gameObject.SetActive(true);
        InGamePopUp.ToggleMainPopupPanel(false);
        InGamePopUp.ToggleMissionHint(false);
        if (FindObjectOfType<MiniGameMCP>() != null)
        {
            ToggleJoystick(false);
        }
        else
        {
            ToggleJoystick(true);
        }
        InGamePopUp.gameObject.SetActive(true);
        InGamePopUp.gameObject.SetActive(true);
        InGamePopUp.gameObject.SetActive(true);
        InGamePopUp.gameObject.SetActive(true);
        InGamePopUp.gameObject.SetActive(true);
    }

    public void ShutOffAllUI()
    {
       // Debug.LogWarning("-------------ShutOffAllUI() start");
        MenuUI.UICamera.enabled = false;

        MenuUI.gameObject.SetActive(false);
        MenuUI.ToggleMenu(RifRafMenuUI.eMenuType.NUM_MENUS, false);
        MenuUI.TogglePopUp(RifRafMenuUI.ePopUpType.NUM_POPUPS, false);

        ToggleJoystick(false);

        ConvoUI.gameObject.SetActive(false);

        InGamePopUp.ToggleMainPopupPanel(false);

        InGamePopUp.gameObject.SetActive(false);

        //VideoPlayerRR.ToggleVideoPlayerChild(false);

        // Debug.LogWarning("-------------ShutOffAllUI() end");
    }

    public void ShareOnSocial(GameObject goToShutOff)
    {        
        StartCoroutine(TakeScreenshotAndShare(goToShutOff));
    }
        
    
    private IEnumerator TakeScreenshotAndShare(GameObject goToShutOff)
    {
        if(goToShutOff != null) goToShutOff.SetActive(false);

#if UNITY_IOS
        string subject = "Hey, look what I found!";
        string body = "I wanted you to see this great new game! Check out Tales from the Crossing!";
        string url = "http://bit.ly/TalesEp1";
#elif UNITY_ANDROID
        string subject = "Hey, look what I found!";
        string body = "I wanted you to see this great new game! Check out Tales from the Crossing!";
        string url = "https://bit.ly/Tales-A";
#else
        string subject = "Hey, look what I found!";
        string body = "I wanted you to see this great new game! Check out Tales from the Crossing!";
        string url = "www.unity3d.com";
#endif

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if(goToShutOff == null)
        {
            // if we're here then we're coming from the main menu so load up the image                    
            string imageName = "Ast_9E5BBE78";
            ArticyObject imageAO = ArticyDatabase.GetObject(imageName);
            if (imageAO == null) Debug.LogError("ERROR: no image to load named " + imageName);
            Sprite s = ((Asset)imageAO).LoadAssetAsSprite();
            if (s == null) Debug.LogError("ERROR: couldn't load sprite from asset named: " + imageName);

            new NativeShare().AddFile(s.texture)
                .SetSubject(subject).SetText(body).SetUrl(url)
                .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
                .Share();

            yield return new WaitForEndOfFrame();
            Destroy(s);
            s = null;           
        }
        else
        {
            Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            ss.Apply();


            string filePath = System.IO.Path.Combine(Application.temporaryCachePath, "TCC Share.png");
           // string filePath = Application.persistentDataPath + "/TCC Share.png";
            Debug.Log("filePath: " + filePath);
            File.WriteAllBytes(filePath, ss.EncodeToPNG());

            // To avoid memory leaks
            Destroy(ss);

            new NativeShare().AddFile(filePath)
                .SetSubject(subject).SetText(body).SetUrl(url)
                .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
                .Share();
        }
        

        yield return new WaitForEndOfFrame();
        if (goToShutOff != null) goToShutOff.SetActive(true);
    }

    public void StartMiniGame()
    {
        StartFreeRoam();
    }    

#if USE_RR_ONGUI
    bool ShowSceneSelect = false;
    public Vector2 ScrollPosition = Vector2.zero;
    public Rect ScrollViewPos = new Rect(10, 300, 240, 100);
    Rect ScrollViewScrollPos = new Rect(0, 0, 220, 200);
    public int ButtonHeight = 50;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            Rect toggleBox = new Rect(Screen.width - 100, Screen.height - 100, 100, 100);
            //Debug.Log("mousePos: " + mousePos.ToString("F2"));
            if (toggleBox.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
            {
                //Debug.Log("toggle scene select");
                ShowSceneSelect = !ShowSceneSelect;
            }
        }
    }
    private void OnGUI()
    {
        if (ShowSceneSelect == false) return;

        // scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 200, 400), scrollPosition, new Rect(0, 0, 220, 200));
        ScrollPosition = GUI.BeginScrollView(ScrollViewPos, ScrollPosition, ScrollViewScrollPos);

        int numScenes = SceneManager.sceneCountInBuildSettings;
        ScrollViewScrollPos.height = numScenes * ButtonHeight;
        for (int i = 0; i < numScenes; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string[] sceneInfo = scenePath.Split('/');
            string sceneName = sceneInfo[sceneInfo.Length - 1];
            sceneName = sceneName.Remove(sceneName.Length - (".unity".Length), (".unity".Length));
            if (GUI.Button(new Rect(0, i * ButtonHeight, 200, ButtonHeight), sceneName))
            {
               // Debug.Log("clicked on " + sceneName);
                LoadNextScene(sceneName);
                //FindObjectOfType<MCP>().LoadNextScene(sj.Template.LoadingScreen.SceneToLoad, sj);
                //FindObjectOfType<MCP>().LoadNextScene
            }
        }

        GUI.EndScrollView();
    }
#endif

    #region SCENE_TRANSITIONS
    public void LoadNextScene(string sceneName = "", Scene_Jump sceneJump = null, Mini_Game_Jump miniGameJump = null, string posToSave = "", string savedPos = "", MenuButton menuButton = null)
    {
        StartCoroutine(LoadNextSceneDelay(sceneName, sceneJump, miniGameJump, posToSave, savedPos));
    }

    IEnumerator LoadNextSceneDelay(string sceneName = "", Scene_Jump sceneJump = null, Mini_Game_Jump miniGameJump = null, string posToSave = "", string savedPos = "", MenuButton menuButton = null)
    {
       // Debug.LogWarning("LoadNextSceneDelay() sceneName: " + sceneName + ", Time.timeScale: " + Time.timeScale);

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

        // start the music fade
        BGMusic.StartFadeOut();

        if (sceneJump != null)
        {
            loadingImageAOs = sceneJump.Template.LoadingScreen.LoadingImages;
            delayTime = sceneJump.Template.LoadingScreen.DisplayTime;
            fadeTime = sceneJump.Template.LoadingScreen.FadeTime;
        }
        else if (miniGameJump != null)
        {
            //  Debug.LogWarning("miniGameJump: " + miniGameJump.TechnicalName);
            if (ArticyGlobalVariables.Default.Mini_Games.Coming_From_Main_Game == true)
            {
                loadingImageAOs = miniGameJump.Template.LoadingScreen.LoadingImages;
                delayTime = miniGameJump.Template.LoadingScreen.DisplayTime;
                fadeTime = miniGameJump.Template.LoadingScreen.FadeTime;
            }
            else
            {
                if (ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success == true)
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
        else if(menuButton != null)
        {            
            if (ArticyGlobalVariables.Default.Mini_Games.Coming_From_Main_Game == true)
            {
                loadingImageAOs = menuButton.LoadingScreen.LoadingImages;
                delayTime = menuButton.LoadingScreen.DisplayTime;
                fadeTime = menuButton.LoadingScreen.FadeTime;
            }
            else
            {
                if (ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success == true)
                {
                    loadingImageAOs = menuButton.SuccessResult.LoadingImages;
                    delayTime = menuButton.SuccessResult.DisplayTime;
                    fadeTime = menuButton.SuccessResult.FadeTime;
                }
                else
                {
                    loadingImageAOs = menuButton.QuitResult.LoadingImages;
                    delayTime = menuButton.QuitResult.DisplayTime;
                    fadeTime = menuButton.QuitResult.FadeTime;
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
            //      Debug.LogError("This SceneJump/MiniGameJump isn't set up properly. textures count: " + loadingTextures.Count + ", delayTime: " + delayTime + ", fadeTime: " + fadeTime);
            loadingTextures.Add(LoadingImage.texture);
            delayTime = 1f;
            fadeTime = 1f;
        }

        // 2) fade the curtain/spinwheel to opaque to cover up current scene    
        // Debug.LogWarning("----- starting the fade in of curtain/spinwheel");
        curImages = new List<RawImage>() { Curtain, SpinWheel };
        yield return StartCoroutine(FadeObjects(curImages, fadeTime, 0f));
        //  Debug.LogWarning("----- end of curtain/spinwheel fade in");
        ShutOffAllUI();   // MCP.LoadNextSceneDelay() right after starting a coroutine to fade in spinwheel    

        // 3) Fade in the first image        
        // Debug.LogWarning("-------- fade in first image");
        int curLoadingImageIndex = 0;
        LoadingImage.texture = loadingTextures[curLoadingImageIndex];
        curImages = new List<RawImage>() { LoadingImage };
        LoadingImage.gameObject.SetActive(true);
        yield return StartCoroutine(FadeObjects(curImages, fadeTime, 0f));
        //   Debug.LogWarning("------- done with fade in of first image");

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
        //  Debug.LogWarning("---- finished unloading the scene with an unload time of: " + unloadTime);

        // 5) Ok the scene is unloaded, so now load the next scene
        // So the AsyncOperation is in two parts:
        // first .9 - loading the scene
        // last .1 to get to 1.0 - scene starting
        // So since I turned off allowSceneActivation, when we're here the scene has been loaded but it has NOT 
        // started.  So if we've taken longer to load the scene than the loading screen images time go ahead and
        // start the scene. If not, wait until the loading images are done.
        //  Debug.LogWarning("------ about to start the scene load");
        if (sceneName.Contains("Front") == false)
        {
            float loadStart = Time.time;
            AsyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            AsyncLoad.allowSceneActivation = false;            
            while (AsyncLoad.isDone == false)
            {
                // Debug.Log("load: " + AsyncLoad.progress);            
                if (AsyncLoad.progress >= .9f) break;
                yield return null;
                curImageTime += Time.deltaTime;
                if (curImageTime >= delayTime)
                {
                    //  Debug.LogWarning("---- during the load, we went past the time for the current image so get the next one going");
                    curLoadingImageIndex++;
                    if (curLoadingImageIndex >= loadingTextures.Count)
                    {
                        //  Debug.LogError("---- ok we're actually done with loading screens, so just hang here and don't do anything");
                        curImageTime = Mathf.NegativeInfinity;
                    }
                    else
                    {
                        // Debug.LogError("----- ok we're ready to switch loading images so get the next one up there");
                        LoadingImage.texture = loadingTextures[curLoadingImageIndex];
                        curImageTime = curImageTime - delayTime;
                    }
                }
            }
            float loadTime = Time.time - loadStart;
            //  Debug.LogWarning("----- done with the scene load, progress is: " + AsyncLoad.progress);
            //  Debug.LogWarning("---- loadTime: " + loadTime.ToString("F2"));
            showLoadButton = true;

            // Debug.LogWarning("---- after the first .9 of the asyncLoad operation (which means the scene is loaded but hasn't done any initialization");
            // 6) Ok we're here, so the scene is loaded but it has not started or even initialized.  
            // At this point check to see if we're done with the loading images or not.  If not, then just cycle thru them. If
            // we are done, then get the curtain fade going.
            //  Debug.LogWarning("------ curLoadingImageIndex: " + curLoadingImageIndex.ToString("F2")  + ", curImageTime: " + curImageTime.ToString("F2"));
            if (curLoadingImageIndex >= loadingTextures.Count)
            {
                //     Debug.LogWarning("------ ok we're done with the scene load and we're all done with the images based on the curLoadingImageIndex so just get to the fade in");
            }
            else
            {
                //      Debug.LogWarning("------- done with scene load but we're not done with the images yet");
                while (curLoadingImageIndex < loadingTextures.Count)
                {
                    curImageTime += Time.deltaTime;
                    if (curImageTime >= delayTime)
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
        }


        // 7) ok we're here so the scene is loaded and the images have all been shown so just get the fade up going
        //   Debug.LogWarning("------- ok we're done with loading and images so fade out the image");
        curImages = new List<RawImage>() { LoadingImage };
        yield return StartCoroutine(FadeObjects(curImages, fadeTime, 1f));
        //  Debug.LogWarning("----- done fading out the image, so start the scene");
        AsyncLoad.allowSceneActivation = true;
        while (AsyncLoad.isDone == false)
        {
            // Debug.Log("starting: " + AsyncLoad.progress);
            yield return new WaitForEndOfFrame();
        }
        int num = 0;
        //  Debug.LogWarning("Ok the scene has officially started so do any scene initting");
        if (FindObjectOfType<TheCaptainsChair>() != null)
        //if(false)
        {
            GameObject captain = GameObject.Find("Captain");
            int avatar = ArticyGlobalVariables.Default.TheCaptain.Avatar;            
            CaptainAssets = Resources.Load<GameObject>("Prefabs\\Characters\\Captain Assets\\Captain_0" + avatar.ToString() + "_Assets");
            CaptainAssets = Instantiate(CaptainAssets);
            Destroy(captain.transform.GetChild(1).gameObject);
            CaptainAssets.transform.parent = captain.transform;
            CaptainAssets.transform.localPosition = Vector3.zero;
            CaptainAssets.transform.localRotation = Quaternion.identity;
            yield return new WaitForEndOfFrame();
            captain.GetComponent<Animator>().Rebind();
        }

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
                
        BGMusic.ResetVolume();

        if (sceneName.Contains("Front") == false)
        {            
            ConvoUI.SetSceneArticyFlowObject();
            if (FindObjectOfType<TheCaptainsChair>() != null)
            {
                FindObjectOfType<TheCaptainsChair>().CheckStartDialogue(DialogueToStartOnThisScene);
               /* // check to see if we're on the E1_Hangar_Intro scene to check for IAP stuff
                if (sceneName.Contains("E1_Hangar_Intro") == true)
                {
                    // IF IAP is initialized and the flag is true, then continue on with normal intro
                    Debug.Log("Check the IAP stuff");
                }
                else
                {
                    FindObjectOfType<TheCaptainsChair>().CheckStartDialogue(DialogueToStartOnThisScene);
                }         */       
            }          
        }                

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name.Contains("Front") == false && SceneManager.GetSceneAt(i).name.Contains("MG_") == false)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneAt(i));
            }
        }

        if(SceneManager.GetActiveScene().name.Contains("Game_MCP"))
        {            
            BackgroundMusicPlayer.Play("MiniGames/" + FindObjectOfType<MiniGameMCP>().BackgroundMusicName);           
        }        

        string elevatorPosString = ArticyGlobalVariables.Default.Save_Info.Majestic_Elevators;
        string[] elevatorPositions = elevatorPosString.Split(',');
        Debug.Log("Loading next scene elevatorPosString: " + elevatorPosString + ", elevatorPositions count: " + elevatorPositions.Length + "--elev--");
        List<Elevator> elevators = FindObjectsOfType<Elevator>().ToList();
        if (elevators.Count > 0 && elevatorPosString != "")
        {
            List<Elevator> sortedElevatorList = elevators.OrderBy(o => o.name).ToList<Elevator>();                       
            int index = 0;
            foreach (Elevator elevator in sortedElevatorList)
            {
                int elevatorCurFloor = Int32.Parse(elevatorPositions[index]);
                //Debug.Log("elevator: " + elevator.name + " is on floor: " + elevator.CurrentFloor + " and should be on floor: " + elevatorCurFloor + " --elev--");
                if(elevator.CurrentFloor != elevatorCurFloor)
                {
                    Debug.Log("elevator: " + elevator.name + " is on floor: " + elevator.CurrentFloor + " but needs to go to: " + elevatorCurFloor + " --elev--");
                    elevator.CurrentFloor = elevatorCurFloor;
                    float yPos = (elevatorCurFloor == elevator.TopFloor ? elevator.TopY : elevator.BottomY);
                    Vector3 pos = new Vector3(elevator.transform.localPosition.x, yPos, elevator.transform.localPosition.z);
                    elevator.transform.localPosition = pos;
                }
                index++;
            }            
        }

        curImages = new List<RawImage>() { Curtain, SpinWheel };
        yield return StartCoroutine(FadeObjects(curImages, fadeTime, 1f));
        LoadingImage.texture = defaultTexture;

        LoadingScreen.SetActive(false);        
        
        if (sceneName.Contains("Front End Launcher"))
        {
            BackgroundMusicPlayer.Play("Exchange_Background_Track");
            StartMainMenu();
        }
    }

    

    public void LoadCaptainAvatar(int avatar)
    {
        ArticyObject imageAO = ArticyDatabase.GetObject("Captain_0" + avatar.ToString() + "_Avatar");
        CaptainAvatar = ((Asset)imageAO).LoadAssetAsSprite();
    }    

    public void SetupSceneSound(List<SoundFX.FXInfo> soundFXUsedInScene)
    {
        SoundFX.SetupFXList(soundFXUsedInScene);
    }

    public void AssignCameraToggleListeners(CamFollow camFollow)
    {
        CameraToggleButton.onClick.RemoveAllListeners();
        CameraToggleButton.onClick.AddListener(camFollow.OnClickCameraToggle);
    }    
    
    public ConvoUI GetConvoUI()
    {       
        return this.ConvoUI;
    }
   
    public Text DebugText;
    IEnumerator FadeObjects(List<RawImage> images, float fadeTime, float alphaStart)
    {
        foreach (RawImage image in images) image.color = new Color(1f, 1f, 1f, alphaStart);
        float alphaEnd = 1f - alphaStart;
        float timer = 0f;
        while (timer < fadeTime)
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

    public void ShutOffUICamera()
    {
        
    }

    IEnumerator CaptainLoadStall(GameObject captain)
    {
        //Debug.Log("CaptainLoadStall");        
        yield return new WaitForEndOfFrame();
        captain.GetComponent<Animator>().Rebind(); 
    }

    public GameObject CaptainAssets;

    AsyncOperation AsyncLoad;
    bool showLoadButton = false;

    public FixedJoystick GetJoystick()
    {
        return this.Joystick;
    }
    public void ToggleJoystick(bool val)
    {        
       // Debug.Log("ToggleJoystick() val: " + val); 
        Joystick.ResetInput();
        Joystick.gameObject.transform.parent.gameObject.SetActive(val);
        // take care of camera toggle thing
        if(val == true && FindObjectOfType<CamFollow>() != null && FindObjectOfType<CamFollow>().ShouldShowCameraToggle() == true)
        {
            CameraToggleButton.gameObject.SetActive(true);
        }
        else
        {
            CameraToggleButton.gameObject.SetActive(false);
        }
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
        //Debug.LogWarning("TurnOnMainMenu()");
        ToggleMenuUI(true);
        ToggleInGamePopUp(false);
        MenuUI.ToggleMenu(RifRafMenuUI.eMenuType.MAIN, true);
    }

    public void TurnOnInGamePopUp()
    {       
        ToggleMenuUI(false);
        ToggleInGamePopUp(true);
        InGamePopUp.TurnOnSetUpMainPopupPanel(true);
    }

    
  /*  public void ToggleConvoUI(bool isActive)
    {
        ConvoUI.gameObject.SetActive(isActive);
    }*/
    public void ToggleMenuUI(bool isActive)
    {
        MenuUI.gameObject.SetActive(isActive);        
    }
    public void ToggleInGamePopUp(bool isActive)
    {
        //Debug.Log("MCP.ToggleInGamePopUp(): " + isActive);
        InGamePopUp.gameObject.SetActive(isActive);
    }

#region GAME_SETTINGS
    public int GetMusicVolume()
    {
        //Debug.LogError("mosound GetMusicVolume(): " + StaticStuff.MusicVolume);
        return StaticStuff.MusicVolume;
       // return 0;
        //return ArticyGlobalVariables.Default.Game_Settings.Music_Volume;
    }
    public void SetMusicVolume(int vol)
    {
        //Debug.LogError("mosound SetMusicVolume(): " + StaticStuff.MusicVolume);
        StaticStuff.MusicVolume = vol;        
        BGMusic.SetVolume(vol);                
    }
    public int GetSoundFXVolume()
    {
      //  Debug.LogError("mosound GetSoundFXVolume(): " + StaticStuff.SoundFXVolume);
        return StaticStuff.SoundFXVolume;
        //return 0;
        //return ArticyGlobalVariables.Default.Game_Settings.SoundFX_Volume;
    }
    public void SetSoundFXVolume(int vol)
    {
      //  Debug.LogError("mosound SetSoundFXVolume(): " + StaticStuff.SoundFXVolume);
        StaticStuff.SoundFXVolume = vol;        
        SoundFX.SetVolume(vol);
    }

#endregion
}
