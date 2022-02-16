

using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

static public class StaticStuff 
{
    /********************** DESIGNER TWEAK AREA *********************/
     static string STARTING_SCENE_TO_LOAD = "E1_Loop_Intro";
    //static string STARTING_SCENE_TO_LOAD = "E1_Hangar_Intro";
    /****************************************************************/

    public enum eOrientation { LANDSCAPE, PORTRAIT };    
    public const int NUM_PROFILES = 4;
    public const string PROFILE_NAME_ROOT = "tcc_savegame_00";
    public const string CURRENT_PROFILE_NAME = "tcc_savegameid";

    static public string SETTINGS_FILE_NAME = "TCCSettings";
    static public int SoundFXVolume = 100;
    static public int MusicVolume = 100;
    static public bool HasUnlockedFullGame = false;

    /* static public void SetOrientation(eOrientation orientation, string screenName)
     {
         //Debug.Log("=============================================== SetOrientation(): " + orientation.ToString() + " from: " + screenName);
         if(orientation == eOrientation.LANDSCAPE)
         {   // landscape
             Screen.autorotateToPortrait = false;
             Screen.autorotateToPortraitUpsideDown = false;
             Screen.autorotateToLandscapeLeft = true;
             Screen.autorotateToLandscapeRight = true;
             Screen.orientation = ScreenOrientation.LandscapeLeft;
             if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
             {
                 Screen.orientation = ScreenOrientation.LandscapeLeft;
             }
         }
         else
         {   // portrait            
             Screen.autorotateToPortrait = true;
             Screen.autorotateToPortraitUpsideDown = true;         
             Screen.autorotateToLandscapeLeft = false;
             Screen.autorotateToLandscapeRight = false;
             Screen.orientation = ScreenOrientation.Portrait;
             if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
             {
                 Screen.orientation = ScreenOrientation.Portrait;
             }
         }        
     }*/

    public static void CreateMCPScene()
    {
        SceneManager.LoadScene("Front End Launcher", LoadSceneMode.Additive);        
    }
    // C:/Users/<YourNameHere>/AppData/LocalLow/DefaultCompany/CC-MiniGames
    #region SAVE_DATA 
    [System.Serializable]
    public class SaveDataDic
    {
        public Dictionary<string, object> saveData;        

        public SaveDataDic()
        {
            saveData = new Dictionary<string, object>();            
        }
    }
    [System.Serializable]
    public class Settings
    {
        public int soundFXVolume;
        public int musicFXVolume;
        public bool hasUnlockedFullGame;
        public Settings(int sound, int music, bool hasUnlockedFullGame)
        {
            soundFXVolume = sound;
            musicFXVolume = music;
            this.hasUnlockedFullGame = hasUnlockedFullGame;
        }
    }

    static public void SaveCurrentSettings(string s)
    {
       // Debug.Log("SaveCurrentSettings(): " + s);
        string saveName = GetSettingsName();

      //  Debug.Log("CurSettings: SoundFX: " + SoundFXVolume + ", Music: " + MusicVolume + ", hasUnlocked: " + HasUnlockedFullGame);
        Settings settings = new Settings(SoundFXVolume, MusicVolume, HasUnlockedFullGame);

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (SaveFileExists(saveName) == true)
        {
            file = File.Open(saveName, FileMode.Open);
        }
        else
        {
            file = File.Create(saveName);
        }
        bf.Serialize(file, settings);
        file.Close();
    }

    static public void LoadSettings()
    {                
        string saveName = GetSettingsName();
        if (SaveFileExists(saveName) == false)
        {
            CreateNewSettings();
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(saveName, FileMode.Open);
        Settings settings = (Settings)bf.Deserialize(file);
        SoundFXVolume = settings.soundFXVolume;
        MusicVolume = settings.musicFXVolume;
        HasUnlockedFullGame = settings.hasUnlockedFullGame;
       // Debug.Log("LoadSettings() has unlock: " + HasUnlockedFullGame);
        file.Close();
    }

    static public void CreateNewSettings()
    {
       // Debug.Log("CreateNewSettings()");
        string saveName = GetSettingsName();

        if (SaveFileExists(saveName) == true) { Debug.LogError("Cannot create new settings over existing settings data."); return; }

        SoundFXVolume = 100;
        MusicVolume = 100;
        HasUnlockedFullGame = false;
        SaveCurrentSettings("CreateNewSettings()");
    }

    static public void ShowDataPath()
    {
        Debug.Log("Data path: " + Application.persistentDataPath);
    }
    static public string GetProfileName(int profileNum)
    {
        string s = Application.persistentDataPath + "/" + PROFILE_NAME_ROOT + profileNum.ToString() + ".dat";
       // Debug.LogError("mosave - GetProfileName(): " + s);
        return s;
    }
    static public string GetSettingsName()
    {
        string s = Application.persistentDataPath + "/" + SETTINGS_FILE_NAME + ".dat";
      //  Debug.Log(s);
        return s;
    }
    
    static public void SaveCurrentProfile(string s)
    {
        string saveName = GetProfileName(Current_Profile_Num);
        // Debug.Log("SaveCurrentProfile(): saveName: " + saveName + ", s: " + s + ", stack track: " + Environment.StackTrace);
        
        SaveDataDic saveData = new SaveDataDic();

        ArticyGlobalVariables.Default.TheCaptain.SaveTime = DateTime.Now.ToString();
        saveData.saveData = ArticyDatabase.DefaultGlobalVariables.Variables;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (SaveFileExists(saveName) == true)
        {
            file = File.Open(saveName, FileMode.Open);
        }
        else
        {
            file = File.Create(saveName);
        }
        bf.Serialize(file, saveData);
        file.Close();
    }

   

    public static int Current_Profile_Num = 1;    
    static public void CreateNewProfile(int avatar, int profile) // called from from avatar select
    {
        string saveName = GetProfileName(profile);
       // Debug.Log("CreateNewProfile() avatar: " + avatar + ", profile: " + profile + ", saveName: " + saveName);

        if (SaveFileExists(saveName) == true) { Debug.LogError("Cannot create new profile over existing profile data: " + profile); return; }

        SetCurrentProfile(profile);
        ArticyGlobalVariables.Default.ResetVariables();
        ArticyGlobalVariables.Default.TheCaptain.Avatar = avatar;
        
        SaveCurrentProfile("StaticStuff.CreateSaveData()");
    }

    static public void SetCurrentProfile(int profile) // called from main menu Continue and CreateNewProfile
    {
        Current_Profile_Num = profile;
    }

    
    static public void LoadCurrentProfile() // called from LoadProfileStartScene()
    {
      //  Debug.Log("LoadCurrentProfile()");
        string saveName = GetProfileName(Current_Profile_Num);
        if (SaveFileExists(saveName) == false) { Debug.LogError("Trying to load current profile: " + saveName + " but it doesn't exist."); return; }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(saveName, FileMode.Open);
        SaveDataDic saveData = (SaveDataDic)bf.Deserialize(file);
        ArticyDatabase.DefaultGlobalVariables.Variables = saveData.saveData;
        
        file.Close();
    }
    static public void LoadProfileStartScene() // called from avatar select AND main menu continue
    {
        // NOTE - the profile is now loaded before this is called becauase I need to load up the avatar
        string returnScene = ArticyGlobalVariables.Default.Save_Info.Return_Scene;
        string posToSave = ArticyGlobalVariables.Default.Save_Info.Positions_To_Save;
        string savedPos = ArticyGlobalVariables.Default.Save_Info.Saved_Positions;
                        
        if (returnScene.Equals("null") || returnScene.Equals(""))
        {
            //Debug.Log("Loading start scene: " + STARTING_SCENE_TO_LOAD);
            GameObject.FindObjectOfType<MCP>().LoadNextScene(STARTING_SCENE_TO_LOAD, null, null, posToSave, savedPos);           
        }
        else
        {
            //Debug.Log("loading returnScene: " + returnScene);
            GameObject.FindObjectOfType<MCP>().LoadNextScene(returnScene, null, null, posToSave, savedPos);                        
        }
    }

    static public void GoToCreditsScreen()
    {
        GameObject.FindObjectOfType<MCP>().LoadNextScene("Episode End");
    }

    
    public static ProfileInfo[] GetProfileInfo()
    {
        ProfileInfo[] profileInfos = new ProfileInfo[NUM_PROFILES];
        for (int i = 0; i < profileInfos.Length; i++ ) profileInfos[i] = new ProfileInfo();
        foreach(ProfileInfo pi in profileInfos ) {pi.Init(-1, "i am error"); }

        string dirName = Application.persistentDataPath;
        if (Directory.Exists(dirName) == true)
        {
            string[] fileNames = Directory.GetFiles(dirName);
            foreach (string fileName in fileNames)
            {
                if (fileName.Contains("savegame") == false) continue;
                int profileNum = int.Parse(fileName[fileName.Length - 5].ToString());
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(fileName, FileMode.Open);
                SaveDataDic saveData = (SaveDataDic)bf.Deserialize(file);
                int avatar = (int)saveData.saveData["TheCaptain.Avatar"];
                string time = (string)saveData.saveData["TheCaptain.SaveTime"];
                profileInfos[profileNum - 1].Init(avatar, time);
                file.Close();
            }
        }
        return profileInfos;
    }
    
    public static bool[] GetValidProfiles()
    {
        bool[] saveFiles = new bool[NUM_PROFILES] { false, false, false, false };
        string dirName = Application.persistentDataPath;
        if (Directory.Exists(dirName) == true)
        {
            string[] fileNames = Directory.GetFiles(dirName);
            foreach (string fileName in fileNames)
            {
                if (fileName.Contains("savegame") == false) continue;
                int saveNum = int.Parse(fileName[fileName.Length - 5].ToString());
                //Debug.Log("we have a saveNum: " + saveNum);
                saveFiles[saveNum - 1] = true;
            }            
        }
        return saveFiles;
    }                

    

    static public bool ProfileExists(int profileNum)
    {
        return File.Exists(GetProfileName(profileNum));
    }
    static public bool SaveFileExists(string saveName)
    {
        return File.Exists(saveName);
    }    
            
    static public void DeleteProfileNum(int profileNum)
    {        
        string fileName = GetProfileName(profileNum);
        //Debug.LogError("mosave - DeleteProfileNum(): " + fileName);
        if (SaveFileExists(fileName) == false) { Debug.LogError("Trying to delete a file that doesn't exist: " + fileName); return; }

        File.Delete(fileName);
    }
    
    static public string GetSaveFileCopyName(int profileNum)
    {
        string s = Application.persistentDataPath + "/" + PROFILE_NAME_ROOT + profileNum.ToString() + "Copy.dat";
        //Debug.Log("mosave - GetSaveFileCopyName(): " + s);
        return s;
    }

    public class ProfileInfo
    {
        public int avatar;
        public string time;

        public void Init(int avatar, string time)
        {
            this.avatar = avatar;
            this.time = time;
        }
    }

    #endregion


    static ArticyFlow ArticyFlowToPrint;
    static public void SetCaptainsChair(ArticyFlow articyFlowToPrint)
    {
        ArticyFlowToPrint = articyFlowToPrint;
    }
    
    static public void PrintFlowBranchesUpdate(string s, ArticyFlow articyFlowCaller)
    {
        //Debug.Log(articyFlowCaller.name + ": " + s);
    }
    static public void PrintBehaviorFlow(string s, BehaviorFlowPlayer player)
    {
       // Debug.LogWarning("===============================PrintBehaviorFlow(): " + s);
    }
    static public void PrintUI(string s)
    {
      //  Debug.Log(s);
    }

    static public void PrintTriggerEnter(string s)
    {
       // Debug.LogWarning("===============================PrintTriggerEnter(): " + s);
    }

    static public void PrintRepairPath(string s)
    {
        // Debug.LogWarning("===============================PrintRepairPath(): " + s);
    }

    static public void PrintRifRafUI(string s)
    { 
      // Debug.LogWarning("===============================PrintRifRafUI(): " + s);
    }

    

    static public void PrintCAL(string s)
    {

    }

    static public void ShareOnSocial(GameObject goToShutOff)
    {
        goToShutOff.SetActive(false);

    }
    

    static public void SetOpaque(Material material)
    {
        material.SetOverrideTag("RenderType", "");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }

    static public void SetFade(Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }
    static public void SetTransparent(Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }
    public static void TrackEvent(string e, Dictionary<string, object> parameters = null)
    {       
        if(e == "") { Debug.LogWarning("Blank analytics event"); return; }
        if (parameters == null) parameters = new Dictionary<string, object>();
                
        string platform = "Unknown";
#if UNITY_EDITOR
        platform = "Editor";
#elif UNITY_IOS
        platform = "IOS";
#elif UNITY_ANDROID
        platform = "Android";
#endif
        parameters.Add("Platform", platform);
        //parameters.Add("stack", Environment.StackTrace);
        AnalyticsResult ar = Analytics.CustomEvent(e, parameters);
       // Debug.Log("---------Analytic: " + e + " has result: " + ar.ToString());
        if (ar != AnalyticsResult.Ok) Debug.LogError("ERROR: we have a screwed up analytics event tracking: " + ar.ToString());
    }

}
