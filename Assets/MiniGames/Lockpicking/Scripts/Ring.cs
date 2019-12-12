using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    public float RotateSpeed = 0f;    
    public LockpickRingPath[] Paths;
    bool UnderFingerControl = false;
    
    public void InitPaths(LockpickRingPath[] paths)
    {
        Paths = paths;
    }
    public void SetRotateSpeed(float speed)
    {
        RotateSpeed = speed;
    }

    // Update is called once per frame
    
    public void Rotate()
    {
        if(UnderFingerControl == false)        
        {
            transform.Rotate(new Vector3(0f, RotateSpeed * Time.deltaTime, 0f));
        }        
    }

    public void Rotate(float speed)
    {
        transform.Rotate(new Vector3(0f, speed * Time.deltaTime, 0f));
    }

    [System.Serializable]
    public class LockpickRingPath
    {
        public PathNode Start, End;
        public Ring Ring;
        public void Init(Ring ring)
        {
            Start.transform.LookAt(End.transform);
            End.transform.LookAt(Start.transform);
            Start.Path = this;
            End.Path = this;
            Ring = ring;
        }
    }
}
