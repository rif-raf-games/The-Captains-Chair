using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
   // public enum eCollisionType { NONE, SHIP_COLLIDER, RAYCAST };
    //eCollisionType CurCollisionType;
    MeshRenderer[] ChildMeshRenderers;
    List<Material> ChildMaterials = new List<Material>();
    public List<Material> NeverOpaqueMaterials = new List<Material>();
    float FadeTime;


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
            StaticStuff.SetOpaque(material);
        }
    }
    public void DEBUG_SetFade()
    {
        foreach (Material material in ChildMaterials)
        {
            StaticStuff.SetFade(material);
        }
    }
    public void DEBUG_SetTransparent()
    {
        foreach (Material material in ChildMaterials)
        {
            StaticStuff.SetTransparent(material);
        }
    }

    private void Awake()
    {
        TheCaptainsChair cChair = FindObjectOfType<TheCaptainsChair>();
        FadeTime = cChair.FadeTime;     
    }

    // Start is called before the first frame update
    void Start()
    {        
        ChildMeshRenderers = GetComponentsInChildren<MeshRenderer>();
        bool addToNeverOpaque = false;
        foreach (MeshRenderer mr in ChildMeshRenderers)
        {
            List<Material> mrMaterials = new List<Material>();
            mr.GetMaterials(mrMaterials);
            //Debug.Log("------ This MR " + mr.name + " has " + mrMaterials.Count + " materials");
            addToNeverOpaque = mr.tag.Equals("Never Opaque");
            foreach (Material material in mrMaterials)            
            {               
                ChildMaterials.Add(material);
                material.shader = UnityEngine.Shader.Find("RifRafStandard");
                if (addToNeverOpaque) NeverOpaqueMaterials.Add(material);
                /*string rt = material.GetTag("RenderType", false, "fuck me");
                if (rt.Contains("Opaq")) Debug.Log(rt);
                else Debug.LogWarning(rt);
                int rq = material.renderQueue;
                Debug.Log(rq);*/
            }
        }
        //Debug.Log("Room " + this.name + ", num child materials:  " + ChildMaterials.Count + " num never opaque: " + NeverOpaqueMaterials.Count);
    }    

    float LerpStart, LerpEnd;
    float LerpStartTime, LerpDurationTime;
    void SetupLerp(float start, float end)
    {
        LerpStart = start;
        LerpEnd = end;
        LerpStartTime = Time.time;
        LerpDurationTime = FadeTime;
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
            // use new alpha (and possibly new mode) to get this ready
            foreach(Material material in ChildMaterials)
            {                
                material.color = new Color(material.color.r, material.color.g, material.color.b, alpha);
                if (CurMode == eRenderMode.IDLE && LerpEnd > .99f && NeverOpaqueMaterials.Contains(material) == false)
                {
                    StaticStuff.SetOpaque(material);
                    
                }
            }       
            if(CurMode == eRenderMode.IDLE)
            {
                foreach (Material m in NPCMaterials) ChildMaterials.Remove(m);
                NPCMaterials.Clear();
                if (LerpEnd < .01f) this.gameObject.SetActive(false);
            }
        }              
    }    

    List<Material> NPCMaterials = new List<Material>();
    float ToggleTime, ToggleValue;  
    string Result;    
    public void ToggleAlpha(float alpha, bool skipLerp = false)
    {     
        ToggleTime = Time.time;
        ToggleValue = alpha;        
       // Debug.Log("---------------------------------------ToggleAlpha from: " + ChildMaterials[0].color.a + " to: " + alpha + " skipLerp: " + skipLerp);
        if (ChildMaterials[0].color.a != alpha)
        {                       
            if(skipLerp == false ) CurMode = eRenderMode.TRANSITION;
            SetupLerp(ChildMaterials[0].color.a, alpha);
            this.gameObject.SetActive(true);

            NPCMaterials.Clear();
            int layerMask = LayerMask.GetMask("NPC");
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null) box = transform.GetChild(0).GetComponent<BoxCollider>();
            Collider[] colliders = Physics.OverlapBox(box.bounds.center, box.size / 2, transform.rotation, layerMask);
            if(colliders.Length != 0)
            {
                //Debug.Log("***********************this room: " + this.name + " has " + colliders.Length + " NPC's in it that we're going to turn to alpha: " + alpha );
               // Debug.Log("before " + ChildMaterials.Count);
                foreach(Collider c in colliders)
                {
                   // Debug.Log("this collider is: " + c.name);
                    Renderer[] childRs = c.GetComponentsInChildren<Renderer>();
                    foreach (Renderer mr in childRs)
                    {
                        List<Material> mrMaterials = new List<Material>();
                        mr.GetMaterials(mrMaterials);
                        foreach (Material material in mrMaterials)
                        {
                          //  Debug.Log("material in this NPC: " + material.name);
                            material.shader = UnityEngine.Shader.Find("RifRafStandard");
                            NPCMaterials.Add(material);                            
                        }
                    }
                }
               // Debug.Log("those NPC's had " + NPCMaterials.Count + " materials in child NPC's");
                foreach (Material m in NPCMaterials) ChildMaterials.Add(m);
               // Debug.Log("after " + ChildMaterials.Count);
               // foreach (Material material in ChildMaterials) Debug.Log(material.name);
            }
            //skipLerp = true;
           // if (colliders.Length != 0) Debug.Log("about to do stuff: " + ChildMaterials.Count);
            foreach (Material material in ChildMaterials)
            {                
                if (skipLerp == true)
                {
                    Result = "lerp DID NOT happen";
                    material.color = new Color(material.color.r, material.color.g, material.color.b, alpha);
                    if (alpha > .999f && NeverOpaqueMaterials.Contains(material) == false) StaticStuff.SetOpaque(material);                    
                    else StaticStuff.SetFade(material);
                    
                } 
                else
                {
                    Result = "lerp happened";
                    StaticStuff.SetFade(material);
                }
            }    
            if(skipLerp == true)
            {
                foreach (Material m in NPCMaterials) ChildMaterials.Remove(m);
                NPCMaterials.Clear();
                if (alpha < .01f) this.gameObject.SetActive(false);
            }
        } 
        else
        {
            Result = "skipped Toggle.  alpha: " + alpha.ToString("F2");
        }
        if(DebugText != null)
        {
            Debug.Log("************************************************");
            //Debug.Log(NumToggles);
            Debug.Log(ToggleTime.ToString("F2"));
            Debug.Log(ToggleTime.ToString("F2"));
            Debug.Log(Result);            
        }
    }    

    /*void SetOpaque(Material material)
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
    }*/


}
