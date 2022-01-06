//#define USE_RR_ONGUI
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
#if !UNITY_ANDROID
using UnityEngine.SocialPlatforms.GameCenter;
#endif

public class GameCenterManager : MonoBehaviour
{ 

    #if !UNITY_ANDROID
    // Start is called before the first frame update
    void Start()
    {
        // Authenticate and register a ProcessAuthentication callback
        // This call needs to be made before we can proceed to other calls in the Social API
        Social.localUser.Authenticate(ProcessAuthentication);

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
            Debug.LogError("ProcessAuthentication() Failed to authenticate"); // GCIssue
    }

    // This function gets called when the LoadAchievement call completes
    void ProcessLoadedAchievements(IAchievement[] achievements)
    {
        if (achievements.Length == 0)
        {
            Debug.Log("ProcessLoadedAchievements() No achievements found, which we should mean the user hasn't achieved any.");
        }
        else
        {
            Debug.Log("ProcessLoadedAchievements() Got " + achievements.Length + " achievements");
            foreach (IAchievement achievement in achievements)
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

    string TrophyToGive = "";
    bool ShowTrophyUI = false;
    public void GiveTrophy(string trophyID)
    {
        Debug.Log("GiveTrophy() trophyID: " + trophyID);
        TrophyToGive = trophyID;
        Social.ReportProgress(TrophyToGive, 100.0f, ProgressCallback);
    }

    void ProgressCallback(bool val)
    {
        if(val == true)
        {
            Debug.Log("ProgressCallback() val is true so we got a new achievement: " + TrophyToGive);
            ShowTrophyUI = true;
        }
        else
        {
            Debug.Log("ProgressCallback() val is false so we either failed or already had the achievement");
        }        
    }


    private void OnGUI()
    {
#if USE_RR_ONGUI
        if (GUI.Button(new Rect(0, Screen.height - 100, 100, 100), "Reset Trophies"))
        {
            GameCenterPlatform.ResetAllAchievements(ResetAllAchievementsCallback);
        }

        if (GUI.Button(new Rect(100, Screen.height - 100, 100, 100), "Ach UI"))
        {            
            Social.ShowAchievementsUI();
        }
#endif

        if (ShowTrophyUI == true)
        {
            string s = "Got Trophy " + TrophyToGive + "\nClick to close.";
            if (GUI.Button(new Rect(Screen.width/2 - 200, Screen.height/2 - 200, 200, 200), s))
            {                
                TrophyToGive = "";
                ShowTrophyUI = false;
            }
        }
    }


    void ResetAllAchievementsCallback(bool val)
    {
        Debug.Log("ResetAllAchievementsCallback() val: " + val);
    }

#endif
}
