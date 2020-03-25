

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



static public class StaticStuff 
{    
    public const bool USE_DEBUG_MENU = true; // HERE IS WHERE YOU TOGGLE THE DEBUG MENU

    public enum eOrientation { LANDSCAPE, PORTRAIT };

    static public void SetOrientation(eOrientation orientation, string screenName)
    {
        //Debug.Log("=============================================== SetOrientation(): " + orientation.ToString() + " from: " + screenName);
        if(orientation == eOrientation.LANDSCAPE)
        {   // landscape
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            /*if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
            }*/
        }
        else
        {   // portrait            
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;         
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;
            /*if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
            {
                Screen.orientation = ScreenOrientation.Portrait;
            }*/
        }        
    }        


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
       // if (ArticyFlowToPrint != null && (ArticyFlowToPrint == articyFlowCaller))
       if(true)
        {
             
            // Debug.Log("caller: " + articyFlowCaller.name + ": " + s);
        }

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
}
