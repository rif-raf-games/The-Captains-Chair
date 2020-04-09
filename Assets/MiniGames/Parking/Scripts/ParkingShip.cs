using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingShip : MonoBehaviour
{
    public enum eParkingShipType { REGULAR, TARGET };
    public eParkingShipType ShipType;

    Parking Parking;
    public enum eParkingShipState { LOWERED, RAISING, RAISED, LOWERING };
    eParkingShipState CurState;

    public enum eMoveDir { HORIZONTAL, VERTICAL, NONE }; // available directions for pieces.  Only 1x1 can have NONE since they can change direction
    eMoveDir MoveDir;   // current direction for this block

    Vector3 LerpPosStart, LerpPosEnd;    
    float LerpStartTime, LerpDurationTime;

    private void Start()
    {
        Parking = transform.parent.GetComponent<Parking>();//FindObjectOfType<Parking>();
        CurState = eParkingShipState.LOWERED;
        MoveDir = eMoveDir.NONE;
    }
    
    public eParkingShipState GetState()
    {
        return CurState;
    }
    public eMoveDir GetMoveDir()
    {
        return MoveDir;
    }
    public void SetMoveDir(eMoveDir dir)
    {
        MoveDir = dir;
    }

    public Transform DebugCollisionPoint;
    public void Move(Vector3 moveDelta)
    {
        BoxCollider box = GetComponent<BoxCollider>();
        Vector3 startPosition = transform.position;

        Vector3 collisionPoint = CheckMovementCollision(moveDelta);
        if (collisionPoint.Equals(Vector3.positiveInfinity) == false)
        {
            if (DebugCollisionPoint != null) DebugCollisionPoint.position = collisionPoint;
            //Debug.Log("collided with something at: " + collisionPoint.ToString("F2") + " box size: " + box.size.ToString("F2"));
            if(MoveDir == eMoveDir.HORIZONTAL)
            {
                float offsetX = (IsHorizontal() ? box.size.z / 2 : box.size.x / 2);
                float newX;
                if(moveDelta.x > 0 )
                {   // moving right
                    newX = collisionPoint.x - offsetX;
                }
                else
                {   // moving left
                    newX = collisionPoint.x + offsetX;
                }
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            }
        }
        else
        {
            transform.position += moveDelta;
        }
    }
    public Vector3 CheckMovementCollision(Vector3 moveOffset)
    {
        transform.position += moveOffset;   // first move the transform to where it would be next after movement
        Vector3 size = GetComponent<BoxCollider>().size;
        Vector3 sizeOffset = Vector3.zero;
        if(IsHorizontal() == false)
        {
            if (MoveDir == eMoveDir.HORIZONTAL) sizeOffset.z = .05f;
            else sizeOffset.x = .05f;
        }
        else
        {
            if (MoveDir == eMoveDir.HORIZONTAL) sizeOffset.x = .05f;
            else sizeOffset.z = .05f;
        }
        //Debug.Log("CheckMovementCollision() size: " + size.ToString("F2"));
        Vector3 closestPoint = Vector3.positiveInfinity;
        Collider[] colliders = Physics.OverlapBox(transform.position, (size / 2)-sizeOffset, transform.rotation);
        if(colliders != null && colliders.Length > 0)
        {                    
            foreach(Collider c in colliders)
            {
                if (c.gameObject == this.gameObject) continue;
                if(c.isTrigger == false)
                {

                    closestPoint = c.ClosestPoint(transform.position);
                   // Debug.Log("collision: " + c.name + ", closestPoint: " + closestPoint.ToString("F2") + ", center: " + transform.position.ToString("F2"));                        
                    break;
                }                
            }
        }
        transform.position -= moveOffset;
        return closestPoint;
    }
    
    public bool IsHorizontal()
    {
        float rotY = transform.eulerAngles.y;
        return (Mathf.Approximately(rotY, 90f) || Mathf.Approximately(rotY, 270f));
    }
    public void BeginHold(float lerpPosTime)
    {
        SoundFXPlayer.Play("Raise");
        CurState = eParkingShipState.RAISING;
        LerpPosStart = transform.position;
        LerpPosEnd = new Vector3(transform.position.x, .5f, transform.position.z);
        LerpStartTime = Time.time;
        LerpDurationTime = lerpPosTime;
    }
    public void BeginLower(float lerpPosTime)
    {
        SoundFXPlayer.Play("Lower");
        transform.position = Parking.GetClosestCenteredPoint(this.gameObject);
        MoveDir = eMoveDir.NONE;
        CurState = eParkingShipState.LOWERING;
        LerpPosStart = transform.position;
        LerpPosEnd = new Vector3(transform.position.x, 0f, transform.position.z);
        LerpStartTime = Time.time;
        LerpDurationTime = lerpPosTime;        
    }

    public void LateUpdate()
    {
        if(CurState == eParkingShipState.RAISING || CurState == eParkingShipState.LOWERING)
        {
            float lerpTime = Time.time - LerpStartTime;
            float lerpPercentage = lerpTime / LerpDurationTime;
            transform.position = Vector3.Lerp(LerpPosStart, LerpPosEnd, lerpPercentage);
            if(lerpPercentage >= 1f)
            {
                transform.position = LerpPosEnd;
                CurState = (CurState == eParkingShipState.RAISING ? eParkingShipState.RAISED : eParkingShipState.LOWERED);
                if (CurState == eParkingShipState.LOWERED)
                {
                    SoundFXPlayer.Play("Drop");
                    Parking.CheckGameFinish();
                }
            }
        }
    }
}
