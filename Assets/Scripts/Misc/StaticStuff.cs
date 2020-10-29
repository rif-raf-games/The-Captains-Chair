

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
    static string DEBUG_SCENE_TO_LOAD = "E1.Plaza";
    public enum eOrientation { LANDSCAPE, PORTRAIT };

    
    public const int NUM_PROFILES = 4;
    public const string PROFILE_NAME_ROOT = "tcc_savegame_00";
    public const string CURRENT_PROFILE_NAME = "tcc_savegameid";

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
    
    static public void ShowDataPath()
    {
        Debug.Log(Application.persistentDataPath);
    }

    static public void SaveCurrentProfile(string s)
    {
        string saveName = GetProfileName(Current_Profile_Num);
      //  Debug.LogError("mosave - SaveCurrentProfile(): saveName: " + saveName + ", s: " + s + ", stack track: " + Environment.StackTrace);

        SaveDataDic saveData = new SaveDataDic();

        ArticyGlobalVariables.Default.TheCaptain.SaveTime = DateTime.Now.ToString();
        saveData.saveData = ArticyDatabase.DefaultGlobalVariables.Variables;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (ProfileExists(saveName) == true)
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
        //Debug.LogError("mosave - CreateNewProfile() avatar: " + avatar + ", profile: " + profile + ", saveName: " + saveName);

        if (ProfileExists(saveName) == true) { Debug.LogError("Cannot create new profile over existing profile data: " + profile); return; }

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
        //Debug.LogError("mosave - LoadCurrentProfile()");
        string saveName = GetProfileName(Current_Profile_Num);
        if (ProfileExists(saveName) == false) { Debug.LogError("Trying to load current profile: " + saveName + " but it doesn't exist."); return; }

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
            //Debug.Log("Loading default start scene");
            GameObject.FindObjectOfType<MCP>().LoadNextScene("E1.Intro", null, null, posToSave, savedPos); 
        }
        else
        {
           // Debug.Log("loading returnScene: " + returnScene);
            GameObject.FindObjectOfType<MCP>().LoadNextScene(returnScene, null, null, posToSave, savedPos); 
        }
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
    
    static public string GetProfileName(int profileNum)
    {
        string s = Application.persistentDataPath + "/" + PROFILE_NAME_ROOT + profileNum.ToString() + ".dat";        
        //Debug.LogError("mosave - GetProfileName(): " + s);
        return s;            
    }
    

    static public bool ProfileExists(int profileNum)
    {
        return File.Exists(GetProfileName(profileNum));
    }
    static public bool ProfileExists(string saveName)
    {
        return File.Exists(saveName);
    }
            
    static public void DeleteProfileNum(int profileNum)
    {        
        string fileName = GetProfileName(profileNum);
        //Debug.LogError("mosave - DeleteProfileNum(): " + fileName);
        if (ProfileExists(fileName) == false) { Debug.LogError("Trying to delete a file that doesn't exist: " + fileName); return; }

        File.Delete(fileName);
    }

    static public void CopySaveDataDebug()
    {
        //Debug.Log("mosave - CopySaveData()");
        /* if (File.Exists(GetSaveFileName()) == true)
         {
             File.Copy(GetSaveFileName(), GetSaveFileCopyName(), true);
         }
         else Debug.LogWarning("Save file doesn't exist");*/
    }
    public static void LoadSaveDataDebug()
    {
        //Debug.Log("mosave - LoadSaveData()");
        /*  if (File.Exists(GetSaveFileName()) == true)
          {
              File.Copy(GetSaveFileCopyName(), GetSaveFileName(), true);
              CheckSceneLoadSave(); // Load debug
          }
          else Debug.LogWarning("Save file copy doesn't exist");*/
    }
    public static void DeleteDaveDataDebug()
    {
        //Debug.Log("mosave - DeleteDaveDataDebug()");
        /*  string dirName = Application.persistentDataPath;
          if (Directory.Exists(dirName) == true)
          {
              string[] files = Directory.GetFiles(dirName);
              foreach (string file in files)
              {
                  Debug.Log("Delete this file: " + file);
                  File.Delete(file);
              }
              //Directory.Delete(dirName);
          }*/
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
      //  if(player.name.Contains("O'Mally")) Debug.Log("-----------------BF: " + s);
    }
    static public void PrintUI(string s)
    {
      //  Debug.Log(s);
    }

    static public void PrintTriggerEnter(string s)
    {
        //Debug.Log(s);
    }

    static public void PrintRepairPath(string s)
    {
        // Debug.LogWarning("===============================PrintRepairPath(): " + s);
    }

    static public void PrintRifRafUI(string s)
    { 
       //Debug.LogWarning("===============================PrintRifRafUI(): " + s);
    }

    

    static public void PrintCAL(string s)
    {

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
       // Debug.Log("##################################################################RifRafLookHere---------Analytic: " + e + " has result: " + ar.ToString());
        if (ar != AnalyticsResult.Ok) Debug.LogError("ERROR: we have a screwed up analytics event tracking: " + ar.ToString());
    }

}
