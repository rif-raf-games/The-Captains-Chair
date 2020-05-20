using Articy.The_Captain_s_Chair.GlobalVariables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MCP : MonoBehaviour
{
    
    public RifRafMenuUI MenuUI;
    public RifRafInGamePopUp InGamePopUp;
    public GameObject LoadingScreen;
   // public RectTransform RT;
    public Text T;

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
        DontDestroyOnLoad(this.gameObject);
    }

    #region SCENE_TRANSITIONS
    public void LoadNextScene(string sceneName)
    {
       // Debug.LogWarning("MCP.LoadNextScene() sceneName: " + sceneName);
        LoadingScreen.SetActive(true);
        //SceneManager.LoadScene("Loading");
        StartCoroutine(LoadNextSceneDelay(sceneName));        
    }

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

    IEnumerator LoadNextSceneDelay(string sceneName)
    {
        float startTime = Time.time;
        yield return new WaitForEndOfFrame();        
        
        // unload the current scene if necessary
        if(SceneManager.sceneCount > 1)
        {            
            AsyncOperation asyncUnLoad = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
            while (asyncUnLoad.isDone == false)
            {                
                yield return null;
            }
        }        
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;        
        while (asyncLoad.progress < 0.90f)
        {                        
            yield return null;
        }
        
        asyncLoad.allowSceneActivation = true;        
    }

        
    #endregion    

    public void TurnOnMainMenu()
    {
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

    public void InitMenusForMainGame()
    {      
        ToggleMenuUI(false);
        ToggleInGamePopUp(true);
        InGamePopUp.TogglePopUpPanel(false);
        InGamePopUp.ToggleMissionHint(false);
        LoadingScreen.SetActive(false);

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

    
}
