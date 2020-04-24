using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MiniGame : MonoBehaviour
{
    protected MiniGameMCP MCP;
    protected bool IsSolo;
    protected bool DialogueActive;
    public string SceneName;

    [Header("Debug")]
    public Text DebugText;
    public virtual void Awake()
    {
        Debug.Log("MiniGame.Awake()");
        DialogueActive = false;
        MiniGameMCP mcp = FindObjectOfType<MiniGameMCP>();
        if(mcp == null)
        {
            //Debug.Log("we're running solo");
            IsSolo = true;
            SceneName = SceneManager.GetActiveScene().name;
        }
        else
        {
            //Debug.Log("we're part of a MCP group");
            IsSolo = false;
        }

        if(IsSolo == true)
        {
            if (this.name.Contains("LockPick"))
            {
                Debug.LogError("change to portrait on this mini game");
                StaticStuff.SetOrientation(StaticStuff.eOrientation.PORTRAIT, this.name);
            }
            else
            {
                Debug.LogError("change to landscape on this mini game");
                StaticStuff.SetOrientation(StaticStuff.eOrientation.LANDSCAPE, this.name);
            }
            if( StaticStuff.USE_DEBUG_MENU == true)
            {
                DebugMenu dm = FindObjectOfType<DebugMenu>();
                if (dm == null)
                {
                    //Debug.Log("-----------------------------------------------------------------------------------------------load debug menu " + this.name);
                    UnityEngine.Object debugObject = Resources.Load("DebugMenu");
                    Instantiate(debugObject);
                }
            }            
        }        
    }
    public virtual void Init(MiniGameMCP mcp, string sceneName)
    {
        //Debug.Log("MiniGame.Init()");
        this.MCP = mcp;
        SceneName = sceneName;
    }

    
    public virtual void ResetPostDialogueState()
    {

    }
    public void DialogueEnded()
    {
        SetDialogueActive(false);
        ResetPuzzleTimer();
        ResetPostDialogueState();
    }
    public void SetDialogueActive( bool val )
    {
        DialogueActive = val;
    }

    protected float PuzzleStartTime = 0f;
    void ResetPuzzleTimer()
    {
        PuzzleStartTime = Time.time;
    }
    public virtual void BeginPuzzleStartTime()
    {
        if (DialogueActive == false) ResetPuzzleTimer();
    }    
    public void EndPuzzleTime(bool didFinish)
    {
        float gameTime = Time.time - PuzzleStartTime;
        TimeSpan time = TimeSpan.FromSeconds(gameTime);
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("Total Time To Solve", time);
        string tag = (didFinish == true ? "_solved" : "_quit");
        StaticStuff.TrackEvent("debug_" + SceneName + tag, parameters);
    }
}