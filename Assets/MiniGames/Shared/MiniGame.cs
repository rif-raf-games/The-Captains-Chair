using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame : MonoBehaviour
{
    protected MiniGameMCP MCP;
    protected bool IsSolo;

    public virtual void Awake()
    {
        Debug.Log("MiniGame.Awake()");
        MiniGameMCP mcp = FindObjectOfType<MiniGameMCP>();
        if(mcp == null)
        {
            Debug.Log("we're running solo");
            IsSolo = true;
        }
        else
        {
            Debug.Log("we're part of a MCP group");
            IsSolo = false;
        }

        if(StaticStuff.USE_DEBUG_MENU == true)
        {
            DebugMenu dm = FindObjectOfType<DebugMenu>();
            if (dm == null)
            {
                Debug.Log("-----------------------------------------------------------------------------------------------load debug menu " + this.name);
                Object debugObject = Resources.Load("DebugMenu");
                Instantiate(debugObject);
            }
        }        
    }
    public virtual void Init(MiniGameMCP mcp)
    {
        Debug.Log("MiniGame.Init()");
        this.MCP = mcp;
    }
    
    public virtual void BeginPuzzle()
    {

    }    
}
