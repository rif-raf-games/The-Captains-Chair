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
    float LerpStartTime;
    int NewFloor;    

    CCPlayer Player;
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
    public int Teleport()
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
    }
    public int BeginMovement()
    {
        Debug.Log("Elevator begin movement");
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
        if(IsMoving == true)
        {            
            float lerpTime = Time.time - LerpStartTime;
            float lerpPercentage = lerpTime / 2f;
            transform.localPosition = Vector3.Lerp(StartPos, EndPos, lerpPercentage);
            Player.ElevatorUpdate(this);
            if (lerpPercentage >= 1f)
            {                
                transform.localPosition = EndPos;             
                CurrentFloor = NewFloor;
                IsMoving = false;
                Player.ElevatorDoneMoving(this);
            }
        }
    }        
}
