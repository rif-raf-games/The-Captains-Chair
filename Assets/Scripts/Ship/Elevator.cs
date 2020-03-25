using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public float TopY, BottomY;
    public int TopFloor, BottomFloor;
    [SerializeField]
    int CurrentFloor;

    Vector3 StartPos, EndPos;
    bool IsMoving = false;
    bool IsFading = false;
    float LerpStartTime;
    int NewFloor;    

    CCPlayer Player;
    //Renderer[] ChildRenderers;
    List<Material> ChildMaterials = new List<Material>();

    private void Awake()
    {
        TheCaptainsChair cChair = FindObjectOfType<TheCaptainsChair>();
        FadeTime = cChair.FadeTime;

        Renderer mr = GetComponent<Renderer>();
        List<Material> mrMaterials = new List<Material>();
        mr.GetMaterials(mrMaterials);
        foreach (Material material in mrMaterials)
        {
            ChildMaterials.Add(material);
            material.shader = UnityEngine.Shader.Find("RifRafStandard");
        }
        Debug.Log("num materials: " + ChildMaterials.Count);
    }
    private void Start()
    {
        Player = FindObjectOfType<CCPlayer>();
    }   
    public bool ShouldMoveAsWell(int newFloor)
    {
        if (newFloor == TopFloor && CurrentFloor == BottomFloor) return true;
        if (newFloor == BottomFloor && CurrentFloor == TopFloor) return true;
        return false;
    }

    float AlphaLerpStart, AlphaLerpEnd;
    float AlphaLerpStartTime, AlphaLerpDurationTime;
    float FadeTime;
    public void SetupAlphaLerp(float alpha, bool skipLerp)
    {
        skipLerp = true;
       // Debug.Log(this.name + ": SetupAlphaLerp(): " + alpha + " on this many materials: " + ChildMaterials.Count);
        if(skipLerp == false)
        {
            AlphaLerpStart = ChildMaterials[0].color.a;
            AlphaLerpEnd = alpha;
            AlphaLerpStartTime = Time.time;
            AlphaLerpDurationTime = FadeTime;
            IsFading = true;
            foreach (Material material in ChildMaterials) StaticStuff.SetFade(material);
        }
        else
        {
            foreach (Material material in ChildMaterials)
            {
                material.color = new Color(material.color.r, material.color.g, material.color.b, alpha);
                if (alpha > .99f) StaticStuff.SetOpaque(material);
                else StaticStuff.SetFade(material);
            }
        }
    }
    public bool CheckIfSameFloor(int level)
    {
        if(IsMoving == true)
        {
            if (Player.IsSelectedElevator(this)) return false;
            if (NewFloor == level) return true;
        }
        else if (CurrentFloor == level) return true;        
        return false;
    }
    public int BeginMovement()
    {
        //Debug.Log("Elevator begin movement");
        StartPos = this.transform.localPosition;        
        IsMoving = true;        
        LerpStartTime = Time.time;
        if (CurrentFloor == TopFloor)
        {            
            EndPos = new Vector3(this.transform.localPosition.x, BottomY, this.transform.localPosition.z);
            NewFloor = BottomFloor;
        }
        else
        {         
            EndPos = new Vector3(this.transform.localPosition.x, TopY, this.transform.localPosition.z);
            NewFloor = TopFloor;
        }
        return NewFloor;
    }

    private void FixedUpdate()
    {
        if(IsFading == true)
        {
            float lerpTime = Time.time - AlphaLerpStartTime;
            float lerpPercentage = lerpTime / 2f;
            float alpha;
            if (lerpPercentage >= 1f)
            {
                alpha = AlphaLerpEnd;
                IsFading = false;
            }
            else alpha = Mathf.Lerp(AlphaLerpStart, AlphaLerpEnd, lerpPercentage);
            foreach (Material material in ChildMaterials)
            {
                material.color = new Color(material.color.r, material.color.g, material.color.b, alpha);
                if (IsFading == false && AlphaLerpEnd > .99f)
                {
                    StaticStuff.SetOpaque(material);
                }
            }
        }
        if(IsMoving == true)
        {            
            float lerpTime = Time.time - LerpStartTime;
            float lerpPercentage = lerpTime / 2f;
            transform.localPosition = Vector3.Lerp(StartPos, EndPos, lerpPercentage);            
            if (lerpPercentage >= 1f)
            {                
                transform.localPosition = EndPos;             
                CurrentFloor = NewFloor;
                IsMoving = false;
                Player.ElevatorDoneMoving(this);
            }
        }
    }

    /*public int Teleport()
    {
        Debug.Log("Elevator Teleport");
        StartPos = this.transform.localPosition;
        //IsMoving = true;
        LerpStartTime = Time.time;
        if (CurrentFloor == TopFloor)
        {
            EndPos = new Vector3(this.transform.localPosition.x, BottomY, this.transform.localPosition.z);
            NewFloor = BottomFloor;
        }
        else
        {
            EndPos = new Vector3(this.transform.localPosition.x, TopY, this.transform.localPosition.z);
            NewFloor = TopFloor;
        }
        transform.localPosition = EndPos;
        CurrentFloor = NewFloor;
        return NewFloor;
    }*/
}
