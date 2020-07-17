

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

    static public void SetOrientation(eOrientation orientation, string screenName)
    {
        //Debug.Log("=============================================== SetOrientation(): " + orientation.ToString() + " from: " + screenName);
        if(orientation == eOrientation.LANDSCAPE)
        {   // landscape
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            /*if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
            }*/
        }
        else
        {   // portrait            
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;         
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;
            /*if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
            {
                Screen.orientation = ScreenOrientation.Portrait;
            }*/
        }        
    }

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

    static public void CheckSceneLoadSave() // Called from RifRafMenuUI
    {
        // Debug.LogError("()()()()(() Take out next line"); StaticStuff.DeleteSaveData();        
        //Debug.LogError("CheckSceneLoadSave()");
        StaticStuff.LoadSaveData();
       // Debug.LogError("()()()()(() Take out next line"); ArticyGlobalVariables.Default.Episode_01.First_Exchange = true;

        string returnScene = ArticyGlobalVariables.Default.Save_Info.Return_Scene;
        string posToSave = ArticyGlobalVariables.Default.Save_Info.Positions_To_Save;
        string savedPos = ArticyGlobalVariables.Default.Save_Info.Saved_Positions;
        
        //GameObject.FindObjectOfType<MCP>().LoadNextScene(DEBUG_SCENE_TO_LOAD); 
        //  return;
        if (returnScene.Equals("null") || returnScene.Equals(""))
        {
            Debug.Log("Loading default start scene");
            GameObject.FindObjectOfType<MCP>().LoadNextScene("E1.Plaza", posToSave, savedPos);
        }
        else
        {
            Debug.Log("loading returnScene: " + returnScene);
            GameObject.FindObjectOfType<MCP>().LoadNextScene(returnScene, posToSave, savedPos);
        }
    }

    static public void CreateNewSaveData()
    {
        FileStream file;
        DeleteSaveData();        
        SaveSaveData("StaticStuff.CreateSaveData()");
    }
    static public void SaveSaveData(string s)
    {       
       // Debug.Log("SaveSaveData(): " + s + ", stack track: " + Environment.StackTrace);
        SaveDataDic saveData = new SaveDataDic();
        saveData.saveData = ArticyDatabase.DefaultGlobalVariables.Variables;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (SaveDataExists() == true)
        {
            file = File.Open(Application.persistentDataPath + "/globalVars.dat", FileMode.Open);
        }
        else
        {
            file = File.Create(Application.persistentDataPath + "/globalVars.dat");
        }
        bf.Serialize(file, saveData);
        file.Close();
    }

    static public void LoadSaveData()
    {        
        if (SaveDataExists() == true)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/globalVars.dat", FileMode.Open);
            SaveDataDic saveData = (SaveDataDic)bf.Deserialize(file);
            ArticyDatabase.DefaultGlobalVariables.Variables = saveData.saveData;
            file.Close();
        }
        else
        {
            Debug.LogError("Trying to LoadSaveData() but no save data exists");
        }
    }

    static public void DeleteSaveData()
    {        
        if( SaveDataExists() == true )
        {
            File.Delete(Application.persistentDataPath + "/globalVars.dat");
        }
        ArticyDatabase.DefaultGlobalVariables.ResetVariables();
    }

    static public bool SaveDataExists()
    {        
        return File.Exists(Application.persistentDataPath + "/globalVars.dat");
    }
    #endregion


    static ArticyFlow ArticyFlowToPrint;
    static public void SetCaptainsChair(ArticyFlow articyFlowToPrint)
    {
        ArticyFlowToPrint = articyFlowToPrint;
    }
    // Start is called before the first frame update
    /*static public void PrintFlowPaused( string s, ArticyFlow articyFlowCaller)
    {
        if (ArticyFlowToPrint != null && (ArticyFlowToPrint == articyFlowCaller))
        {
            Debug.Log("caller: " + articyFlowCaller.name + ": " + s);
        }            
    }*/
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
        //Debug.Log(s);
    }

    static public void PrintRifRafUI(string s)
    {
      //  Debug.LogWarning("===============================PrintRifRafUI(): " + s);
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
