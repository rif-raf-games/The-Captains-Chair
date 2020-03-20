using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugMenu : MonoBehaviour
{
    enum eDebugState { OFF, ROOT, TYPE, OPTIONS };
    eDebugState CurDebugState;

    public Texture Black;
    
    List<string> MainScenes = new List<string>();
    List<string> ParkingScenes = new List<string>();
    List<string> RepairScenes = new List<string>();
    List<string> LockpickScenes = new List<string>();
    List<string> CurScenesList;
    // Start is called before the first frame update
    void Start()
    {        
        CurDebugState = eDebugState.OFF;
        int numScenes = SceneManager.sceneCountInBuildSettings;
        Debug.Log("num scenes in Build Settings: " + numScenes);
        for (int i = 0; i < numScenes; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string[] sceneInfo = scenePath.Split('/');
            string sceneName = sceneInfo[sceneInfo.Length - 1];
            sceneName = sceneName.Remove(sceneName.Length - 6);
            if (sceneName.Contains("Scene")) MainScenes.Add(sceneName);
            else if (sceneName.Contains("Parking")) ParkingScenes.Add(sceneName);
            else if (sceneName.Contains("Repair")) RepairScenes.Add(sceneName);
            else if (sceneName.Contains("LockPick")) LockpickScenes.Add(sceneName);
           // Debug.Log("scene index " + i + " has name " + sceneName);
        }
    }

    private void OnGUI()
    {
        GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button);
        ButtonStyle.fontSize = 50;
        int height;
        switch (CurDebugState)
        {
            case eDebugState.OFF:
                if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 100), "Debug Menu"))
                {
                    CurDebugState = eDebugState.ROOT;
                }
                break;
            case eDebugState.ROOT:
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Black);
                if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 100), "Off", ButtonStyle))
                {                    
                    CurDebugState = eDebugState.OFF;
                }
                height = Screen.height / 5;
                if (GUI.Button(new Rect(0, 0, Screen.width - 200, height), "Main", ButtonStyle))
                {
                    CurScenesList = MainScenes;
                    CurDebugState = eDebugState.TYPE;
                }
                if (GUI.Button(new Rect(0, height, Screen.width - 200, height), "Parking", ButtonStyle))
                {
                    CurScenesList = ParkingScenes;
                    CurDebugState = eDebugState.TYPE;
                }
                if (GUI.Button(new Rect(0, height*2, Screen.width - 200, height), "Repair", ButtonStyle))
                {
                    CurScenesList = RepairScenes;
                    CurDebugState = eDebugState.TYPE;
                }
                if (GUI.Button(new Rect(0, height * 3, Screen.width - 200, height), "LockPick", ButtonStyle))
                {
                    CurScenesList = LockpickScenes;
                    CurDebugState = eDebugState.TYPE;
                }
                if (GUI.Button(new Rect(0, height * 4, Screen.width - 200, height), "Options", ButtonStyle))
                {
                    CurDebugState = eDebugState.OPTIONS;
                }
                break;
            case eDebugState.TYPE:
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Black);
                if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 100), "Root", ButtonStyle))
                {
                    CurDebugState = eDebugState.ROOT;
                }
                height = Screen.height / CurScenesList.Count;
                for(int i=0; i<CurScenesList.Count; i++)
                {
                    if (GUI.Button(new Rect(0, height*i, Screen.width-200, height), CurScenesList[i], ButtonStyle))
                    {
                        SceneManager.LoadScene(CurScenesList[i]);
                    }
                }
                break;
            case eDebugState.OPTIONS:
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Black);
                if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 100), "Root", ButtonStyle))
                {
                    CurDebugState = eDebugState.ROOT;
                }
                CCPlayer Player = GameObject.FindObjectOfType<CCPlayer>();
                if(Player != null)
                {
                    if(GUI.Button(new Rect(0, Screen.height/2, Screen.width-200, 200), Player.GetControlType().ToString(), ButtonStyle))
                    {
                        CCPlayer.eControlType newType = (Player.GetControlType() == CCPlayer.eControlType.POINT_CLICK ? CCPlayer.eControlType.STICK : CCPlayer.eControlType.POINT_CLICK);
                        Player.ToggleControlType(newType);
                    }
                }
                break;
        }
        
    }
}
