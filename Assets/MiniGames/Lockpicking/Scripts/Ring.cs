using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    public float CurRotateSpeed = 0f;
    public float DefaultRotateSpeed;
    public LockpickRingPath[] Paths;    
    
    public void InitPaths(LockpickRingPath[] paths)
    {
        Paths = paths;
    }
    public void SetDefaultRotateSpeed(float speed)
    {
        CurRotateSpeed = speed;
        DefaultRotateSpeed = speed;
    }
        
    public void ResetRotateSpeed()
    {
        CurRotateSpeed = DefaultRotateSpeed;
    }
    public void SetTouchRotateSpeed(float speed)
    {
        CurRotateSpeed = speed;
    }
    public void Rotate()
    {
        transform.Rotate(new Vector3(0f, CurRotateSpeed * Time.deltaTime, 0f));               
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
