

using Articy.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Analytics;

static public class StaticStuff 
{    
    public const bool USE_DEBUG_MENU = false; // HERE IS WHERE YOU TOGGLE THE DEBUG MENU

    public enum eOrientation { LANDSCAPE, PORTRAIT };

    static public void SetOrientation(eOrientation orientation, string screenName)
    {
        //Debug.Log("=============================================== SetOrientation(): " + orientation.ToString() + " from: " + screenName);
        if(orientation == eOrientation.LANDSCAPE)
        {   // landscape
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            /*if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
            }*/
        }
        else
        {   // portrait            
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;         
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;
            /*if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
            {
                Screen.orientation = ScreenOrientation.Portrait;
            }*/
        }        
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

    static public void SaveSaveData()
    {
        SaveDataDic saveData = new SaveDataDic();
        saveData.saveData = ArticyDatabase.DefaultGlobalVariables.Variables;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (File.Exists(Application.persistentDataPath + "/globalVars.dat"))
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
        if (File.Exists(Application.persistentDataPath + "/globalVars.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/globalVars.dat", FileMode.Open);
            SaveDataDic saveData = (SaveDataDic)bf.Deserialize(file);
            ArticyDatabase.DefaultGlobalVariables.Variables = saveData.saveData;
            file.Close();
        }
    }

    static public void DeleteSaveData()
    {
        if (File.Exists(Application.persistentDataPath + "/globalVars.dat"))
        {
            File.Delete(Application.persistentDataPath + "/globalVars.dat");
        }
        ArticyDatabase.DefaultGlobalVariables.ResetVariables();
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
        //Debug.Log("caller: " + articyFlowCaller.name + ": " + s);

    }
    static public void PrintBehaviorFlow(string s, BehaviorFlowPlayer player)
    {
       // if(player.name.Contains("Captain")) Debug.Log(s);
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
        Debug.Log("##################################################################RifRafLookHere---------Analytic: " + e + " has result: " + ar.ToString());
        if (ar != AnalyticsResult.Ok) Debug.LogError("ERROR: we have a screwed up analytics event tracking: " + ar.ToString());
    }

}
