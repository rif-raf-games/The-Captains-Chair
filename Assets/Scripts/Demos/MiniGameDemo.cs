using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MiniGameDemo : MonoBehaviour
{
    public string GameNameRoot = "Parking";
    public GameObject RootButtons;
    public GameObject ChooseButtons;

    List<string> MiniGameScenes = new List<string>();
    
    void Start()
    {
        int numScenes = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < numScenes; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string[] sceneInfo = scenePath.Split('/');
            string sceneName = sceneInfo[sceneInfo.Length - 1];
            sceneName = sceneName.Remove(sceneName.Length - 6);
            if (sceneName.Contains("Demo") || sceneName.Contains("MCP")) continue;
            if (sceneName.Contains(GameNameRoot)) MiniGameScenes.Add(sceneName);
            Debug.Log("scene index " + i + " has name " + sceneName);
        }
        
        RootButtons.SetActive(true);        
        for(int i=0; i<MiniGameScenes.Count; i++)
        {
            ChooseButtons.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = MiniGameScenes[i];            
        }        
        ChooseButtons.SetActive(false);

        StaticStuff.LoadSaveData();
    }
    public void OnClickStart()
    {
        SceneManager.LoadScene(GameNameRoot + "_Game_MCP");
    }
    public void OnClickChoose()
    {
        RootButtons.SetActive(false);
        ChooseButtons.SetActive(true);
    }
    public void OnClickMainMenu()
    {
        SceneManager.LoadScene("MiniGamesDemo");
    }
    public void OnClickReset()
    {
        StaticStuff.DeleteSaveData();
    }

    public void OnClickPuzzle(Button button)
    {
        string buttonInfo = button.transform.GetChild(0).GetComponent<Text>().text;
        if (buttonInfo.Contains("Back"))
        {
            RootButtons.SetActive(true);
            ChooseButtons.SetActive(false);
        }
        else
        {
            SceneManager.LoadScene(buttonInfo);
        }
    }
}
