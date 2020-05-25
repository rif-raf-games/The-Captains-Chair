using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.GlobalVariables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MCP : MonoBehaviour
{
    public Camera UICamera;
    public RifRafMenuUI MenuUI;
    public RifRafInGamePopUp InGamePopUp;
    public GameObject LoadingScreen;
    public GameObject LoadingAlien;
    public RawImage Curtain;

    SoundFX soundFX;
    BackgroundMusic bgMusic;

    
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
        LoadingScreen.SetActive(false);
        LoadingAlien.SetActive(false);
        SetFadeAlpha(0f);
        DontDestroyOnLoad(this.gameObject);
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

       // Debug.Log("1) Turn on loading screen, make curtain alpha 0, set alien to false");
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

      //  Debug.Log("3) Shut off any menu ui's and popups.  Shut on the alien and move the camera forward");
        // 3) Shut off any menu ui's and popups.  Shut on the alien and move the camera forward
        ToggleMenuUI(false);
        ToggleInGamePopUp(false);
        LoadingAlien.SetActive(true);
        UICamera.depth = 100;

      //  Debug.Log("4) Fade in to loading screen");
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
            AsyncOperation asyncUnLoad = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
            while (asyncUnLoad.isDone == false)
            {                
                yield return null;
            }
        }

      //  Debug.Log("6) Begin loading the next scene");
        // 6) Begin loading the next scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while(asyncLoad.isDone == false)
        {
            yield return null;
        }

        float fadeTime = 1f;
     //   Debug.Log("7) Fade out loading screen");
        // 7) Fade out loading screen      
        // TMP shut off convo ui in the scene
        FindObjectOfType<ConvoUI>().gameObject.SetActive(false);
        timer = 0f;
        while (timer < 1f)
        {
            SetFadeAlpha(timer);
            timer += Time.deltaTime/ fadeTime;
            yield return new WaitForEndOfFrame();
        }
        SetFadeAlpha(1f);

      //  Debug.Log("8) Move camera back and fade in to the in-game scene");
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
    }

    public Dialogue DialogueToStartOnThisScene = null;
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
        InGamePopUp.TogglePopUpPanel(false);
    }

    public void SetupSceneSound()
    {      
        /*ToggleMenuUI(false);
        ToggleInGamePopUp(true);
        InGamePopUp.TogglePopUpPanel(false);
        InGamePopUp.ToggleMissionHint(false);
        LoadingScreen.SetActive(false);*/

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

    /*bool feh = false;
private void OnGUI()
{
    if(GUI.Button(new Rect(Screen.width-100, Screen.height/2, 100, 100), "feh`"))
    {
        if (feh) LoadNextScene("Ep1.S5");
        else LoadNextScene("Ep1.S6");
        feh = !feh;
    }
}*/

}
