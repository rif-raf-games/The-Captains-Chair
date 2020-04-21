using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FuelDoor : MonoBehaviour
{
    enum eDoorState { OPEN, MOVING, CLOSED };
    eDoorState CurDoorState;
    static float CLOSE_Z = 0f;
    static float OPEN_Z = 14f;

    Repair RepairGame;

    private void Awake()
    {
        RepairGame = transform.parent.parent.GetComponent<Repair>();
        CurDoorState = eDoorState.CLOSED;
        transform.position = new Vector3(transform.position.x, transform.position.y, CLOSE_Z);
    }   
    
    public void Open(Action returnCall = null)
    {
       // Debug.Log("open");
        if(CurDoorState != eDoorState.CLOSED) { Debug.LogError("The door should be CLOSED if you're going to open it, but it's in state: " + CurDoorState); return; }
        StartCoroutine(MoveDoor(eDoorState.OPEN, CLOSE_Z, OPEN_Z, returnCall));            
    }
    public void Close(Action returnCall = null)
    {
        if (CurDoorState != eDoorState.OPEN) { Debug.LogError("The door should be OPEN if you're going to close it, but it's in state: " + CurDoorState); return; }
        StartCoroutine(MoveDoor(eDoorState.CLOSED, OPEN_Z, CLOSE_Z, returnCall));
    }

    IEnumerator MoveDoor(eDoorState newState, float lerpStart, float lerpEnd, Action returnCall)
    {       
        CurDoorState = eDoorState.MOVING;
        transform.position = new Vector3(transform.position.x, transform.position.y, lerpStart);
        float timer = 0f;
        while(timer < 1f)
        {
            float z = Mathf.Lerp(lerpStart, lerpEnd, timer);
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, lerpEnd);
        CurDoorState = newState;
        if (returnCall != null) returnCall.Invoke();        
    }
}
