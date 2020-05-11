using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCP : MonoBehaviour
{
    
    public RifRafMenuUI MenuUI;

    private void Awake()
    {
        MCP mcp = FindObjectOfType<MCP>();
        if (mcp != this)
        {
            Debug.LogError("There should only ever be one MCP in the scene.  Tell Mo."); 
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
        Debug.Log("If we're here then we're starting on the MCP, so yippie us");
        if (MenuUI == null) { Debug.LogError("No MenuUI in MCP"); return; }
        MenuUI.ToggleMenu(RifRafMenuUI.eMenuType.SPLASH, true);
    }

    public void ToggleUI(bool isActive)
    {
        MenuUI.gameObject.SetActive(isActive);
    }
    private void OnGUI()
    {
        if(MenuUI.gameObject.activeSelf == true)
        {
            if (GUI.Button(new Rect(Screen.width - 100, Screen.height / 2, 100, 100), "Menu Off"))
            {
                MenuUI.gameObject.SetActive(false);
            }
        }        
    }
}
