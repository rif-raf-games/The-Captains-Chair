using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Vector3 StartPos = Vector3.zero;
    public Vector3 EndPos = Vector3.zero;

    bool IsOpening = false;
    float LerpStartTime;
    public void Open()
    {
        if(transform.position == StartPos)
        {
            IsOpening = true;
            LerpStartTime = Time.time;
        }        
    }

    private void FixedUpdate()
    {
        if(IsOpening == true)
        {
            float lerpTime = Time.time - LerpStartTime;
            float lerpPercentage = lerpTime / 1f;
            transform.position = Vector3.Lerp(StartPos, EndPos, lerpPercentage);
            if(lerpPercentage >= 1f)
            {
                IsOpening = false;
                transform.position = EndPos;
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
