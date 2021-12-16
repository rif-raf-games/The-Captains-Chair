//#define USE_RR_ONGUI
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
#if !UNITY_ANDROID
using UnityEngine.SocialPlatforms.GameCenter;
#endif

public class GameCenterTest : MonoBehaviour
{

#if !UNITY_ANDROID
    // Start is called before the first frame update    

#if USE_RR_ONGUI
    private void OnGUI()
    {
        if(GUI.Button(new Rect(Screen.width-300, 100, 100, 100), "init gc"))
        {
            // Authenticate and register a ProcessAuthentication callback
            // This call needs to be made before we can proceed to other calls in the Social API
            Social.localUser.Authenticate(ProcessAuthentication);            
        } 
        if(GUI.Button(new Rect(Screen.width-300, 200, 100, 100), "Load Desc"))
        {
            Social.LoadAchievementDescriptions(LoadDescCallback);
        }
        if (GUI.Button(new Rect(Screen.width - 100, 300, 100, 100), "Load Ach"))
        {
            Social.LoadAchievements(LoadAchCallback);
        }
        if (GUI.Button(new Rect(Screen.width - 100, 400, 100, 100), "01 100%"))
        {
            Social.ReportProgress("t_achieve_001", 100.0f, ProgressCallback);
        }
        if (GUI.Button(new Rect(Screen.width - 100, 500, 100, 100), "02 50%"))
        {
            Social.ReportProgress("t_achieve_002", 50.0f, ProgressCallback);
        }
        if (GUI.Button(new Rect(Screen.width - 100, 600, 100, 100), "03 100%"))
        {
            Social.ReportProgress("t_achieve_003", 100.0f, ProgressCallback);
        }
        if (GUI.Button(new Rect(Screen.width - 100, 700, 100, 100), "04 100%"))
        {
            Social.ReportProgress("t_achieve_004", 100.0f, ProgressCallback);
        }
        if (GUI.Button(new Rect(Screen.width - 100, 800, 100, 100), "05 100%"))
        {
            Social.ReportProgress("t_achieve_005", 100.0f, ProgressCallback);
        }
        if (GUI.Button(new Rect(Screen.width - 100, 900, 100, 100), "Reset"))
        {
            //Social.ReportProgress("t_achieve_005", 100.0f, ProgressCallback);
            GameCenterPlatform.ResetAllAchievements(ResetAllAchievementsCallback);
        }
        if (GUI.Button(new Rect(Screen.width - 300, 900, 100, 100), "Ach UI"))
        {
            Social.ShowAchievementsUI();
        }                
    }
#endif

    void ResetAllAchievementsCallback(bool val)
    {
        Debug.Log("ResetAllAchievementsCallback() val: " + val);
    }

    // This function gets called when Authenticate completes
    // Note that if the operation is successful, Social.localUser will contain data from the server. 
    void ProcessAuthentication(bool success)
    {
        if (success)
        {
            Debug.Log("Authenticated, checking achievements");

            // Request loaded achievements, and register a callback for processing them
            Social.LoadAchievements(ProcessLoadedAchievements);
        }
        else
            Debug.Log("Failed to authenticate");
    }

    // This function gets called when the LoadAchievement call completes
    void ProcessLoadedAchievements(IAchievement[] achievements)
    {
        if (achievements.Length == 0)
            Debug.Log("Error: no achievements found");
        else
            Debug.Log("Got " + achievements.Length + " achievements");

        // You can also call into the functions like this
        Social.ReportProgress("Achievement01", 100.0, result => {
            if (result)
                Debug.Log("Successfully reported achievement progress");
            else
                Debug.Log("Failed to report achievement");
        });
    }

    void LoadDescCallback(IAchievementDescription[] descriptions)
    {
        Debug.Log("Num Desc: " + descriptions.Length);
        foreach (IAchievementDescription desc in descriptions)
        {
            string s = "ID: " + desc.id + ", Title: " + desc.title + ", unachDesc: " + desc.unachievedDescription;
            Debug.Log(s);
        }
    }

    void ProgressCallback(bool val)
    {
        Debug.Log("ProgressCallback() val: " + val);
    }

    void LoadAchCallback(IAchievement[] achievements)
    {
        Debug.Log("Num Ach: " + achievements.Length);
        foreach(IAchievement ach in achievements)
        {
            string s = "ID: " + ach.id + ", % comp: " + ach.percentCompleted;
            Debug.Log(s);
        }
    }            
#endif
}
