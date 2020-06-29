using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RepairScanLines : MonoBehaviour
{
    public GameObject UpDownLine;
    public float[] UpDownZRange = { 3.6f, -11f };//new Vector2(3.6f, -11f);
    public GameObject LeftRightLine;
    public float[] LeftRightXRange = {-11f, 10.3f }; //new Vector2(-11f, 10.3f);
    enum eDoorState { OFF, MOVING  };
    eDoorState CurDoorState;
    //static float CLOSE_Z = 0f;
   // static float OPEN_Z = 13.7f;

    Repair RepairGame;

    private void Awake()
    {
        RepairGame = transform.parent.parent.GetComponent<Repair>();
        CurDoorState = eDoorState.OFF;
       // transform.position = new Vector3(transform.position.x, transform.position.y, CLOSE_Z);
    }   
    
    public void ResetLines()
    {
        UpDownLine.transform.position = new Vector3(0f, 0f, UpDownZRange[0]);
        LeftRightLine.transform.position = new Vector3(LeftRightXRange[0], 0f, 0f);
    }
    public void Scan(Action returnCall = null)
    {
       // Debug.Log("open");
        if(CurDoorState != eDoorState.OFF) { Debug.LogError("The scan lines are animating so don't do anything: " + CurDoorState); return; }
        StartCoroutine(ScanLines(returnCall));
       // StartCoroutine(MoveDoor(eDoorState.OPEN, CLOSE_Z, OPEN_Z, returnCall));            
    }    

    IEnumerator ScanLine(GameObject line, Vector3 start, Vector3 end)
    {
        float timer = 0f;
        while (timer < .5f)
        {
            Vector3 pos = Vector3.Lerp(start, end, (timer * 2f));
            line.transform.position = pos;
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        line.transform.position = end;
    }

    IEnumerator ScanLines(/*eDoorState newState, float lerpStart, float lerpEnd,*/ Action returnCall)
    {       
        CurDoorState = eDoorState.MOVING;
        float timer = 0f;        
        while (timer < .5f)
        {
            UpDownLine.transform.position = Vector3.Lerp(new Vector3(0f, 0f, UpDownZRange[0]), new Vector3(0f, 0f, UpDownZRange[1]), (timer * 2f));
            LeftRightLine.transform.position = Vector3.Lerp(new Vector3(LeftRightXRange[0], 0f, 0f), new Vector3(LeftRightXRange[1], 0f, 0f), (timer * 2f));           
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        UpDownLine.transform.position = new Vector3(0f, 0f, UpDownZRange[1]);
        LeftRightLine.transform.position = new Vector3(LeftRightXRange[1], 0f, 0f);
        
        timer = 0f;
        while (timer < .5f)
        {
            UpDownLine.transform.position = Vector3.Lerp(new Vector3(0f, 0f, UpDownZRange[1]), new Vector3(0f, 0f, UpDownZRange[0]), (timer * 2f));
            LeftRightLine.transform.position = Vector3.Lerp(new Vector3(LeftRightXRange[1], 0f, 0f), new Vector3(LeftRightXRange[0], 0f, 0f), (timer * 2f));
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        UpDownLine.transform.position = new Vector3(0f, 0f, UpDownZRange[0]);
        LeftRightLine.transform.position = new Vector3(LeftRightXRange[0], 0f, 0f);
        //  yield return StartCoroutine(ScanLine(UpDownLine, new Vector3(0f, 0f, UpDownZRange[0]), new Vector3(0f, 0f, UpDownZRange[1])));
        //   yield return StartCoroutine(ScanLine(LeftRightLine, new Vector3(LeftRightXRange[0], 0f, 0f), new Vector3(LeftRightXRange[1], 0f, 0f)));
        //  yield return StartCoroutine(ScanLine(UpDownLine, new Vector3(0f, 0f, UpDownZRange[1]), new Vector3(0f, 0f, UpDownZRange[0])));
        //   yield return StartCoroutine(ScanLine(LeftRightLine, new Vector3(LeftRightXRange[1], 0f, 0f), new Vector3(LeftRightXRange[0], 0f, 0f)));
        CurDoorState = eDoorState.OFF;                
        if (returnCall != null) returnCall.Invoke();        
    }

    /*private void OnGUI()
    {
        if(GUI.Button(new Rect(0,0,100,100), "feh"))
        {
            Scan();
        }
    }*/
}
