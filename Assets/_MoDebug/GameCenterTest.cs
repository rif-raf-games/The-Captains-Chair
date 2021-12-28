#define USE_RR_ONGUI
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

    private void Awake()
    {
        Screen.SetResolution(1280, 960, true);
    }

#if USE_RR_ONGUI
    private void OnGUI()
    {
        if(GUI.Button(new Rect(Screen.width-300, 100, 100, 100), "Init GC"))
        {
            // Authenticate and register a ProcessAuthentication callback
            // This call needs to be made before we can proceed to other calls in the Social API
            Social.localUser.Authenticate(ProcessAuthentication);            
        } 
        if(GUI.Button(new Rect(Screen.width-300, 200, 100, 100), "Load Desc"))
        {
            Social.LoadAchievementDescriptions(ProcessAchievementDescriptions);
        }
        if (GUI.Button(new Rect(Screen.width - 100, 300, 100, 100), "Load Ach"))
        {
            Social.LoadAchievements(ProcessLoadedAchievements);
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
            Debug.Log("ProcessAuthentication() Authenticated, checking achievements");

            // Request loaded achievements, and register a callback for processing them
            Social.LoadAchievementDescriptions(ProcessAchievementDescriptions);
            Social.LoadAchievements(ProcessLoadedAchievements);
        }
        else
            Debug.Log("ProcessAuthentication() Failed to authenticate");
    }

    // This function gets called when the LoadAchievement call completes
    void ProcessLoadedAchievements(IAchievement[] achievements)
    {
        if (achievements.Length == 0)
            Debug.Log("ProcessLoadedAchievements() No achievements found, which we should mean the user hasn't achieved any.");
        else
        {
            Debug.Log("ProcessLoadedAchievements() Got " + achievements.Length + " achievements");
            foreach(IAchievement achievement in achievements)
            {
                Debug.Log("id: " + achievement.id + ", completed: " + achievement.completed + ", %: " + achievement.percentCompleted);
            }
        }          
    }

    void ProcessAchievementDescriptions(IAchievementDescription[] descriptions)
    {
        Debug.Log("LoadDescCallback() Num Desc: " + descriptions.Length);
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
#endif
}
