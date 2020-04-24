using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Block : MonoBehaviour
{
    CargoPuzzle CargoPuzzle;  //  reference to the CargoPuzzle script
    BoxCollider MyBoxCollider; // reference to the collider on this block
    public enum eDir { HORIZONTAL, VERTICAL, NONE }; // available directions for pieces.  Only 1x1 can have NONE since they can change direction
    eDir Dir;   // current direction for this block
    public enum eState { LOWERED, RAISE, RAISED, MOVING, SNAPPING, ROTATING, LOWER }; // different states the block can be in
    eState CurState;    // current state for this block
    List<eState> StateChanges = new List<eState>();
    // variables to hold Lerping values since those occur over multiple frames
    Vector3 LerpPosStart, LerpPosEnd;
    float LerpRotStart, LerpRotEnd;
    float LerpStartTime, LerpDurationTime;
    // variables to hold snap values since those occur over multiple frames
    Vector3 SnapStartPos, SnapEndPos;
    float SnapSpeed;

    #region MOVEMENT
    /// <summary>
    /// Begin movement state on this block
    /// </summary>    
    /*public void BeginMoving()
    {
        CurState = eState.MOVING;
        GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
    }*/
    
       
    /// <summary>
    /// Stop this block from moving. Handles setting up the Snap values
    /// </summary>    
    public void StopMoving(float snapSpeed)
    {        
        if (CurState != eState.MOVING) { Debug.LogError("ERROR: trying to stop a block that isn't MOVING: " + CurState); return; }   
        if(StateChanges.Count != 0 ) { Debug.LogError("ERROR: StopMoving() with an unempty state change list."); return; }
        //Debug.Log("StopMoving()");
        CurState = eState.RAISED;        
        GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        // Check for snapping if necessary
        SnapStartPos = transform.position;
        if(Dir == eDir.HORIZONTAL )
        {   // calculate how far off we are from an exact block position horizontally            
            float curX = transform.position.x;
            float newX = Mathf.Floor(curX);
            float dec = curX - newX;
            if (dec != 0)
            {   // if we're not exactly on a grid spot, then get the snap going horizontally
                //CurState = eState.SNAPPING;                
                StateChanges.Add(eState.SNAPPING);
                if (dec > .5) newX = newX + 1f;
                SnapEndPos = new Vector3(newX, transform.position.y, transform.position.z);
                SnapSpeed = snapSpeed;
                if (newX < curX) SnapSpeed = -SnapSpeed;                
            }
        }
        else
        {   // calculate how far off we are from an exact block position vertically
            float curZ = transform.position.z;
            float newZ = Mathf.Floor(curZ);
            float dec = curZ - newZ;
            if (dec != 0)
            {
                //CurState = eState.SNAPPING;
                StateChanges.Add(eState.SNAPPING);
                if (dec > .5) newZ = newZ + 1f;
                SnapEndPos = new Vector3(transform.position.x, transform.position.y, newZ);
                SnapSpeed = snapSpeed;
                if (newZ < curZ) SnapSpeed = -SnapSpeed;                
            }
        }
        StateChanges.Add(eState.LOWER);
        if(StateChanges[0] == eState.SNAPPING )
        {
            SetupPosLerp(new Vector3(SnapEndPos.x, .5f, SnapEndPos.z), new Vector3(SnapEndPos.x, 0f, SnapEndPos.z), LerpDurationTime);
        }
        else
        {
            SetupPosLerp(transform.position, new Vector3(transform.position.x, 0f, transform.position.z), LerpDurationTime);
        }        
        StateChanges.Add(eState.LOWERED);
        CurState = StateChanges[0];
        if (MyBoxCollider.size.x < 1.1f) Dir = eDir.NONE; // reset direction to NONE if we're a 1x1
        CargoPuzzle.SetIgnoreBlockInput(true);
    }

    void PrintStates()
    {
        if(StateChanges.Count == 0) { Debug.LogError("ERROR: PrintStates() with no StateChanges."); return; }
        string s = "";
        for (int i = 0; i < StateChanges.Count; i++) s += StateChanges[i].ToString() + ", ";
    }
    public void SendTaps(int numTaps, float lerpPosTime, float lerpRotTime) //region INPUT
    {        
        if (numTaps == 2 && CurState == eState.LOWERED && this.tag == "Player")
        {   // if we double click the player while it's LOWERED, raise the piece, rotate it, and lower it back
            Vector3 collisionPoint = CheckRotationCollision(45);
            if (collisionPoint.Equals(Vector3.positiveInfinity) == false) { /*Debug.Log("can't rotate");*/ return; }
            // we can rotate, so get the state change queue going along with the first values
            if(StateChanges.Count != 0 ) { Debug.LogError("ERROR: StateChanges not empty when trying rotate."); return; }
            StateChanges.Add(eState.RAISE);
            StateChanges.Add(eState.ROTATING);
            StateChanges.Add(eState.LOWER);
            StateChanges.Add(eState.LOWERED);
            CurState = StateChanges[0];
            SetupPosLerp(transform.position, new Vector3(transform.position.x, .5f, transform.position.z), lerpPosTime);
            LerpDurationTime = lerpRotTime;
            GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            CargoPuzzle.SetIgnoreBlockInput(true);
        } 
        else
        {
           // Debug.Log("Release location: SendTaps() but not 2 taps on LOWERED Player");
            CargoPuzzle.ReleaseCurBlock();
        }
    }
    public void BeginHold(float lerpPosTime) // region MOVEMENT
    {
        if (StateChanges.Count != 0) { Debug.LogError("ERROR: BeginHold() without empty state changes list."); return; }
        StateChanges.Add(eState.RAISE);
        StateChanges.Add(eState.MOVING);
        CurState = StateChanges[0];
        SetupPosLerp(transform.position, new Vector3(transform.position.x, .5f, transform.position.z), lerpPosTime);
        GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
        CargoPuzzle.SetIgnoreBlockInput(true);
    }
    // monote - make sure that the lerp times are correct.
    /// <summary>
    /// This is where we handle automatic block movement/rotation
    /// </summary>
    void FixedUpdate()
    {                
        if(StateChanges.Count != 0)
        {            
            if (CurState == eState.RAISE || CurState == eState.LOWER)
            {   // if we're raising or lowering, lerp the piece up or down accordingly
                float lerpTime = Time.time - LerpStartTime;
                float lerpPercentage = lerpTime / LerpDurationTime;
                transform.position = Vector3.Lerp(LerpPosStart, LerpPosEnd, lerpPercentage);
                if (lerpPercentage >= 1f)
                {   // we've either finished or gone beyond the lerp end, so make sure the position is at the lerp end exactly
                    transform.position = LerpPosEnd;
                    StateChanges.RemoveAt(0);
                    if(StateChanges.Count == 0 )
                    {
                        Debug.Log("done with state, changes, so return");
                        return;
                    }
                    // if we're here then we have a new state
                    CurState = StateChanges[0];
                    if(CurState == eState.ROTATING)
                    {
                        LerpRotStart = transform.eulerAngles.y;
                        LerpRotEnd = LerpRotStart + 90f;
                        LerpStartTime = Time.time;                        
                    }
                    else if(CurState == eState.MOVING)
                    {
                        //Debug.Log("Finished RAISE-ing before a MOVING");
                        StateChanges.Clear();
                        CargoPuzzle.CurBlockCheckMovementBegin();
                    }
                    else if(CurState == eState.LOWERED)
                    {
                        StateChanges.Clear();                        
                        GetComponentInChildren<MeshRenderer>().material.color = Color.white;
                        //Debug.Log("Release block FixedUpdate() state to LOWERED");
                        CargoPuzzle.SetIgnoreBlockInput(false);
                        CargoPuzzle.ReleaseCurBlock();
                    }
                }
            }
            else if (CurState == eState.ROTATING)
            {   // handle the rotation
                // first rotate back to the original position
                Vector3 center = MyBoxCollider.bounds.center;
                float rot = transform.eulerAngles.y;
                transform.RotateAround(center, Vector3.up, -rot);
                // now re-rotate to the new rotation based on the rotation calculated by the lerping
                float lerpTime = Time.time - LerpStartTime;
                float lerpPercentage = lerpTime / LerpDurationTime;
                if (lerpPercentage > 1f) lerpPercentage = 1f;
                rot = Mathf.LerpAngle(LerpRotStart, LerpRotEnd, lerpPercentage);
                transform.RotateAround(center, Vector3.up, rot);
                // check to see if we're done rotating
                if (lerpPercentage >= 1f)
                {   // done rotating, so make sure we're at the exact rotation we wanted and not too far
                    CurState = eState.RAISED;
                    rot = transform.eulerAngles.y;
                    transform.RotateAround(center, Vector3.up, -rot);
                    transform.RotateAround(center, Vector3.up, LerpRotEnd);
                    // now make sure that we're also at the exact position we want to be
                    float x = Mathf.Round(transform.position.x);
                    float z = Mathf.Round(transform.position.z);
                    transform.position = new Vector3(x, .5f, z);
                    GetCenter(); // this resets the direction
                    StateChanges.RemoveAt(0);
                    if (StateChanges.Count == 0)
                    {
                        Debug.Log("done with state, changes, so return");
                        return;
                    }
                    // if we're here then we have a new state
                    CurState = StateChanges[0];
                    if (CurState == eState.LOWER)
                    {
                        SetupPosLerp(transform.position, new Vector3(transform.position.x, 0f, transform.position.z), LerpDurationTime);
                    }
                }
            }
            else if (CurState == eState.SNAPPING)
            {   // snapping is handled based on speed, not lerping over time so handle that here
                float dist = Time.deltaTime * SnapSpeed;
                bool animEnd = false;
                if (SnapStartPos.x != SnapEndPos.x)
                {
                    transform.position += new Vector3(dist, 0f, 0f);
                    if (SnapSpeed > 0 && transform.position.x >= SnapEndPos.x) animEnd = true; // snapping done if we've gone past the end value
                    else if (SnapSpeed < 0 && transform.position.x <= SnapEndPos.x) animEnd = true;
                }
                else
                {
                    transform.position += new Vector3(0f, 0f, dist);
                    if (SnapSpeed > 0 && transform.position.z >= SnapEndPos.z) animEnd = true; // snapping done if we've gone past the end value
                    else if (SnapSpeed < 0 && transform.position.z <= SnapEndPos.z) animEnd = true;
                }
                if (animEnd == true)
                {   // if the animation is ended, make sure we're at the exact correct position and change to RAISED state (can't snap on the ground)
                    transform.position = SnapEndPos;
                    //CurState = eState.RAISED;
                    StateChanges.RemoveAt(0);
                    if(StateChanges.Count == 0 ) { Debug.LogError("ERROR: shouldn't have an empty state change list after snapping"); return; }
                    CurState = StateChanges[0];
                    if(CurState != eState.LOWER ) { Debug.LogError("ERROR: post SNAPPING should be state LOWER."); return; }
                    SetupPosLerp(transform.position, new Vector3(transform.position.x, 0f, transform.position.z), LerpDurationTime);
                    /*string s = "FixedUpdate() SNAPPING state: " + ", LerpPosStart: " + LerpPosStart.ToString("F3") + ", LerpPosEnd: " + LerpPosEnd.ToString("F3");
                    s += ", LerpDurationTime: " + LerpDurationTime + ", LerpStartTime: " + LerpStartTime + ", Time.time: " + Time.time;
                    s += ", time Diff: " + (Time.time - LerpStartTime);
                    Debug.Log(s);  */            
                    GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                }
            }
        }        
    }
    /// <summary>
    /// Helper funciton to set up the global lerping values
    /// </summary>
    void SetupPosLerp(Vector3 start, Vector3 end, float time)
    {        
        LerpPosStart = start;
        LerpPosEnd = end;
        LerpStartTime = Time.time;
        LerpDurationTime = time;
    }
    #endregion
    #region COLLISIONS
    
    /// <summary>
    /// Handles the actual movment of the block based on the values calculated in the MCP
    /// </summary>   
    public void Move(Vector3 moveDelta)
    {
        // various error checks just to be safe
        if (CurState != eState.MOVING) { Debug.LogError("ERROR: trying to move a block not in a MOVING state. State: " + CurState); return; }
        if (moveDelta.magnitude == 0) { Debug.LogError("ERROR: trying to move block but moveDelta is 0."); return; }
        if (moveDelta.magnitude > .95f || moveDelta.magnitude < -.95f) { Debug.LogError("ERROR: trying to move too far: " + moveDelta.magnitude); return; }
        if (Dir == eDir.HORIZONTAL && moveDelta.z != 0f) { Debug.LogError("have z value for HORIZONTAL movement: " + moveDelta.ToString("F3")); return; }
        if (Dir == eDir.VERTICAL && moveDelta.x != 0f) { Debug.LogError("have x value for VERTICAL movement: " + moveDelta.ToString("F3")); return; }
        // we're good, so start the movement
        Vector3 startPosition = transform.position;
        Vector3 collisionPoint = CheckMovementCollision(moveDelta); // check for collisions
        if (collisionPoint.Equals(Vector3.positiveInfinity) == false)
        {   // if we've made it here then there was a collision with something   
            // get our rotation so we know which direction we're moving in either HORIZONTAL (up/down) or VERTICAL (left/right)
            float rotY = transform.eulerAngles.y;
            if (rotY < 0) rotY += 360f;
            if (rotY >= 360f) rotY = 0f;
            if (Dir == eDir.HORIZONTAL)
            {   // get the X position that we want the block to be based on where the collision point is
                float newX = 0f;
                if (moveDelta.x > 0)
                {   // moving right
                    if (rotY < 90f) newX = collisionPoint.x - MyBoxCollider.size.x;
                    else newX = collisionPoint.x;
                }
                else
                {   // moving left
                    if (rotY < 90f) newX = collisionPoint.x;
                    else newX = collisionPoint.x + MyBoxCollider.size.x;
                }
                // Debug.Log("newX: " + newX + ", rotY: " + rotY + ", moveDelta.x: " + moveDelta.x);
                transform.position = new Vector3(newX, transform.position.y, transform.position.z); // set our position to the newX point based on the collision point          
            }
            else
            {   // get the Y position that we want the block to be based on where the collision point is
                float newZ = 0f;
                if (moveDelta.z > 0)
                {   // moving up                                      
                    if (rotY < 100f && MyBoxCollider.size.x >= 1.1f) newZ = collisionPoint.z;
                    else newZ = collisionPoint.z - MyBoxCollider.size.x;
                }
                else
                {   // moving down                    
                    if (rotY < 100f && MyBoxCollider.size.x >= 1.1f) newZ = collisionPoint.z + MyBoxCollider.size.x;
                    else newZ = collisionPoint.z;
                }
                transform.position = new Vector3(transform.position.x, transform.position.y, newZ); // set our position to the newX point based on the collision point                         
            }
        }
        else
        {   // no collision, so just move the block to the new spot based on the movement provided by the CargoPuzzle            
            transform.position += moveDelta;
        }
    }
    /// <summary>
    /// Handles checking for collision based on movement
    /// </summary>
    public Vector3 CheckMovementCollision(Vector3 moveOffset)
    {
        transform.position += moveOffset;   // first move the transform to where it would be next after movement
        Vector3 center = GetCenter();   // don't use the collider center because the collider's position doesn't get updated until after the next physics update (See GetCenter() definition)
        Vector3 size = MyBoxCollider.size;
        Vector3 sizeOffset = Vector3.zero;
        // We want to shrink the collider box just a little bit because, for example, if you're moving a piece
        // past another piece one row above or below, you can get collisions because the size of the colliders on
        // both make them touch exactly but we don't want those to count since they're supposed to move freely
        // when next to each other (or the boundaries).  Adjust the size based on the kind of block
        if (MyBoxCollider.size.x < 1.1f)
        {
            if (Dir == eDir.HORIZONTAL) sizeOffset.z = .05f;
            else sizeOffset.x = .05f;
        }
        else
        {
            sizeOffset.z = .05f;
        }
        Vector3 closestPoint = GetClosestCollisionPoint(center, size, sizeOffset);
        transform.position -= moveOffset;
        return closestPoint;
    }
    /// <summary>
    /// Helper function used by the collision detection to return the closest collision point based on the 
    /// inputted values.  
    /// </summary>
    Vector3 GetClosestCollisionPoint(Vector3 center, Vector3 size, Vector3 sizeOffset)
    {
        Vector3 closestPoint = Vector3.positiveInfinity;
        Collider[] colliders = Physics.OverlapBox(center, (size / 2) - sizeOffset, transform.rotation);
        if (colliders != null)
        {
            if (colliders.Length > 0)
            {
                foreach (Collider c in colliders)
                {
                    if (c.gameObject == this.gameObject) continue; // ignore your own collider
                    if (c.isTrigger == false) // make sure it's not a trigger
                    {
                        closestPoint = c.ClosestPoint(center);
                        //Debug.Log("collision: " + c.name + ", closestPoint: " + closestPoint.ToString("0.000") + ", center: " + center.ToString("F3"));                        
                        break;
                    }
                }
            }
        }
        return closestPoint;
    }
    /// <summary>
    /// Handles checking for collisions based on rotation
    /// </summary>    
    Vector3 CheckRotationCollision(float rot)
    {
        if (rot == Mathf.Infinity) { Debug.LogError("ERROR: calling CheckRotationCollision() with Mathf.infinity"); return Vector3.positiveInfinity; }

        // rotate around the rot amount, check for collisions, then rotate back.  The lerping will handle the actual rotation
        // if there are no collisions
        Vector3 center = MyBoxCollider.bounds.center;
        Vector3 size = MyBoxCollider.size;
        Vector3 sizeOffset = new Vector3(.05f, .05f, .05f);
        transform.RotateAround(center, Vector3.up, rot);
        Vector3 closestPoint = GetClosestCollisionPoint(center, size, sizeOffset);
        transform.RotateAround(center, Vector3.up, -rot);
        return closestPoint;
    }
    #endregion

    #region INPUT
    /// <summary>
    /// The CargoPuzzle has determined based on it's input engine that it's time to send taps to this block.  
    /// This is where those taps are handled
    /// </summary>
    
    #endregion


    #region INITIALIZATION        
    public void Init( CargoPuzzle mcp )
    {
        this.CargoPuzzle = mcp;
        CurState = eState.LOWERED;
        MyBoxCollider = this.GetComponent<BoxCollider>();
        if (MyBoxCollider.size.x <= 1.1f) Dir = eDir.NONE;
        else if (transform.rotation.y == 0 || transform.rotation.y == 180) Dir = eDir.HORIZONTAL;
        else Dir = eDir.VERTICAL;
       // SetDir();               
    }
    #endregion
    #region DATA
    public eState GetBlockState()
    {
        return CurState;
    }
    public eDir GetBlockDir()
    {
        return Dir;
    }
    public void SetBlockDir(eDir dir)
    {
        this.Dir = dir;
    }
    #endregion

    #region MISC
    /// <summary>
    /// This function is used as an alternate to the collider.bounds.center because if we're testing to see if a block
    /// is going to collide if the given movement is applied we first apply the movement to the transform.  However,
    /// the collider itself won't get updated until the next physics update in FixedUpdate.  There is a way around this
    /// to set Physics.autoSimulation = false; and then call Physics.Simulate, but since collision detection is 
    /// called often when moving pieces I didn't want to do that for efficiency reasons so we do it like this instead.
    /// This will also reset the Dir of the player piece after it's done rotating
    /// </summary>
    /// <returns></returns>
    Vector3 GetCenter()
    {        
        Vector3 size = MyBoxCollider.size;
        float x, y, z;
        Vector3 pos = transform.position;
        Vector3 rot = transform.eulerAngles;
        if (rot.y < 0) rot.y += 360f;
        //Debug.Log("rot y: " + rot.y);
        if ((rot.y <= 1f && rot.y >= 0f) || (rot.y >= 359f))
        {
            if (MyBoxCollider.size.x > 1.1f) Dir = eDir.HORIZONTAL;
            x = pos.x + size.x / 2f;
            y = pos.y + size.y / 2f;
            z = pos.z + size.z / 2f;
        }
        else if (rot.y >= 89f && rot.y <= 91f)
        {
            if (MyBoxCollider.size.x > 1.1f) Dir = eDir.VERTICAL;
            x = pos.x + size.z / 2f;
            y = pos.y + size.y / 2f;
            z = pos.z - size.x / 2f;
        }
        else if (rot.y >= 179f && rot.y <= 181f)
        {
            if (MyBoxCollider.size.x > 1.1f) Dir = eDir.HORIZONTAL;
            x = pos.x - size.x / 2f;
            y = pos.y + size.y / 2f;
            z = pos.z - size.z / 2f;
        }
        else if (rot.y >= 269f && rot.y < 271f)
        {
            if (MyBoxCollider.size.x > 1.1f) Dir = eDir.VERTICAL;
            x = pos.x - size.z / 2f;
            y = pos.y + size.y / 2f;
            z = pos.z + size.x / 2f;
        }
        else
        {
            x = 0f; y = 0f; z = 0f;
        }
        Vector3 center = new Vector3(x, y, z);
        return center;
    }
    #endregion
    /*void Update()
    {
        Debug.DrawRay(transform.position, Vector3.up, Color.blue);
        Debug.DrawRay(transform.position, Vector3.down, Color.green);
        Debug.DrawRay(transform.position, Vector3.forward, Color.yellow);
        Debug.DrawRay(transform.position, Vector3.back, Color.cyan);
        Debug.DrawRay(transform.position, Vector3.left, Color.magenta);
        Debug.DrawRay(transform.position, Vector3.right, Color.white);  
    }*/
}