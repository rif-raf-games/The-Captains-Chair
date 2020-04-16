using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame : MonoBehaviour
{
    protected MiniGameMCP MCP;
    protected bool IsSolo;

    public virtual void Awake()
    {
        //Debug.Log("MiniGame.Awake()");
        MiniGameMCP mcp = FindObjectOfType<MiniGameMCP>();
        if(mcp == null)
        {
            //Debug.Log("we're running solo");
            IsSolo = true;
        }
        else
        {
            //Debug.Log("we're part of a MCP group");
            IsSolo = false;
        }

        if(IsSolo == true)
        {
            if (this.name.Contains("LockPick")) StaticStuff.SetOrientation(StaticStuff.eOrientation.PORTRAIT, this.name);
            else StaticStuff.SetOrientation(StaticStuff.eOrientation.LANDSCAPE, this.name);
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
    public virtual void Init(MiniGameMCP mcp)
    {
        //Debug.Log("MiniGame.Init()");
        this.MCP = mcp;
    }

    float PuzzleStartTime = 0f;
    public virtual void BeginPuzzle()
    {
        Debug.Log("Reset puzzle time");
        PuzzleStartTime = Time.time;
    }    
    public void EndPuzzle(bool success, string name)
    {
        if(success == true)
        {
            float gameTime = Time.time - PuzzleStartTime;
            TimeSpan time = TimeSpan.FromSeconds(gameTime);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Total Time To Solve", time);
            StaticStuff.TrackEvent("debug_" + name + "_solved", parameters);
        }
    }
}
