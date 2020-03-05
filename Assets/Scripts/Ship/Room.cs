using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
   // public enum eCollisionType { NONE, SHIP_COLLIDER, RAYCAST };
    //eCollisionType CurCollisionType;
    MeshRenderer[] ChildMeshRenderers;
    List<Material> ChildMaterials = new List<Material>();

    float RoomFadeTime;


    public void DEBUG_SetShader(string shaderName, Shader shader)
    {
        foreach(Material material in ChildMaterials)
        {
            material.shader = UnityEngine.Shader.Find(shaderName);
            //material.shader = shader;
        }
    }

    public void DEBUG_SetOpaque()
    {
        foreach (Material material in ChildMaterials)
        {
            SetOpaque(material);
        }
    }
    public void DEBUG_SetFade()
    {
        foreach (Material material in ChildMaterials)
        {
            SetFade(material);
        }
    }
    public void DEBUG_SetTransparent()
    {
        foreach (Material material in ChildMaterials)
        {
            SetTransparent(material);
        }
    }

    private void Awake()
    {
        TheCaptainsChair cChair = FindObjectOfType<TheCaptainsChair>();
        RoomFadeTime = cChair.RoomFadeTime;
     //   RoomFadeOpacity = cChair.RoomFadeOpacity;
     //   FloorFadeOpacity = cChair.FloorFadeOpacity;
    }

    // Start is called before the first frame update
    void Start()
    {
        //CurCollisionType = eCollisionType.NONE;
        ChildMeshRenderers = GetComponentsInChildren<MeshRenderer>();        
       // Debug.Log("this room: " + this.name + " has " + ChildMeshRenderers.Length + " MeshRenderers");

        foreach (MeshRenderer mr in ChildMeshRenderers)
        {
            List<Material> mrMaterials = new List<Material>();
            mr.GetMaterials(mrMaterials);            
            foreach (Material material in mrMaterials)            
            {               
                ChildMaterials.Add(material);
                material.shader = UnityEngine.Shader.Find("RifRafStandard");
            }
        }       
    }    

    float LerpStart, LerpEnd;
    float LerpStartTime, LerpDurationTime;
    void SetupLerp(float start, float end)
    {
        LerpStart = start;
        LerpEnd = end;
        LerpStartTime = Time.time;
        LerpDurationTime = RoomFadeTime;
    }
    
    enum eRenderMode { IDLE, TRANSITION };
    eRenderMode CurMode = eRenderMode.IDLE;
    public Text DebugText;
    private void LateUpdate()
    {
        if(CurMode == eRenderMode.TRANSITION)
        {    
            float lerpTime = Time.time - LerpStartTime;
            float lerpPercentage = lerpTime / LerpDurationTime;
            float alpha;
            if (lerpPercentage >= 1f)
            {
                alpha = LerpEnd;
                CurMode = eRenderMode.IDLE;
            }
            else alpha = Mathf.Lerp(LerpStart, LerpEnd, lerpPercentage);            
            foreach(Material material in ChildMaterials)
            {                
                material.color = new Color(material.color.r, material.color.g, material.color.b, alpha);
                if (CurMode == eRenderMode.IDLE)
                {
                    if (LerpEnd > .99f) SetOpaque(material);
                }
            }            
        }
      
        if(DebugText != null)
        {
            //DebugText.text = CurCollisionType.ToString() + "\n";
            DebugText.text = NumToggles + "\n";
            DebugText.text += ToggleTime.ToString("F2") + "\n";
            DebugText.text += ToggleValue.ToString("F2") + "\n";
            DebugText.text += Result;
        }
    }

    
    float ToggleTime, ToggleValue;
    int NumToggles = 0;
    //bool InterruptToggle = false;
    string Result;    
    public void ToggleAlpha(float alpha, bool skipLerp = false/*, eCollisionType collisionType = eCollisionType.SHIP_COLLIDER*/)
    {
        NumToggles++;
        if(name.Contains("Quarters_01") && NumToggles == 6)
        {
            Debug.Log("ok see what's calling this");
        }
        ToggleTime = Time.time;
        ToggleValue = alpha;        
       // Debug.Log("---------------------------------------ToggleAlpha from: " + ChildMaterials[0].color.a + " to: " + alpha + " skipLerp: " + skipLerp);
        if (ChildMaterials[0].color.a != alpha)
        {                       
            if(skipLerp == false ) CurMode = eRenderMode.TRANSITION;
            SetupLerp(ChildMaterials[0].color.a, alpha);
            
            foreach (Material material in ChildMaterials)
            {
                if (skipLerp == true)
                {
                    Result = "lerp DID NOT happen";
                    material.color = new Color(material.color.r, material.color.g, material.color.b, alpha);
                    if (alpha > .999f) SetOpaque(material);                    
                    else SetFade(material);                    
                } 
                else
                {
                    Result = "lerp happened";
                    SetFade(material);
                }
            }            
        } 
        else
        {
            Result = "skipped Toggle.  alpha: " + alpha.ToString("F2");
        }
        if(DebugText != null)
        {
            Debug.Log("************************************************");
            Debug.Log(NumToggles);
            Debug.Log(ToggleTime.ToString("F2"));
            Debug.Log(ToggleTime.ToString("F2"));
            Debug.Log(Result);            
        }
    }    

    void SetOpaque(Material material)
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
    void SetFade(Material material)
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
    void SetTransparent(Material material)
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
