using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CargoPuzzle : MonoBehaviour
{    
    //******************* DEBUG
    public Text debugText;
    //*************************
    enum ePuzzleState { PLAYING, OFF };
    ePuzzleState PuzzleState;
    enum eTouchState { NONE, CLICK, RELEASE, HOLD };    // available mouse/finger touch states
    eTouchState TouchState; // current touch state

    CargoMCP CargoMCP = null;

    Block[] Blocks; // all blocks in the scene
    Block CurBlock; // currently selected block    
    Block Player;   // player block 
    Block Exit;     // exit block
    
    public int BlockMoveDelay = 5;  // how many frames between finger/mouse movement and block reaction.  Added to have a little bit of natural delay movement that would be caused by a crane
    public float HoldTime = 1f; // how long until a CLICK becomes a HOLD
    public float TapTime = 1f;  // how long you have to press the button before it becomes a tap
    public float RaiseLowerTime = 1f;   // how long the block takes to raise/lower from/to the ground
    public float RotateTime = .5f;  // how long the block takes to rotate
    public float SnapSpeed = 1f;    // how fast the block moves when it's snapping into place

    float InputTimer;   // timer used to keep track of input state
    int TapCount;   // number of taps we've accumulated.  Right now only use 1 or 2

    Vector3 StartTouchPos = Vector3.zero;   // these hold the various touch positions for the finger/mouse
    Vector3 CurTouchPos = Vector3.zero;
    Vector3 LastTouchPos = Vector3.zero;    

    List<Vector3> MoveDeltas = new List<Vector3>(); // keeps track of the movement history for the move/action delay        
    bool IgnoreBlockInput = false;
    #region INPUT_AND_MOVEMENT
    void SetCurBlock(Block block)
    {
        if (block == null)
        {
            //Debug.Log("Setting CurBlock to null");
        }
        else
        {
            // Debug.Log("Setting CurBlock to: " + block.name);
            SetIgnoreBlockInput(false);
        }
        CurBlock = block;
    }
    public void SetIgnoreBlockInput(bool val)
    {
        //Debug.Log("SetIgnoreBlockInput(): " + val);
        IgnoreBlockInput = val;
    }
    void DebugPrint()
    {
        if (debugText != null)
        {
            debugText.text = "";
            debugText.text += "TouchState: " + TouchState.ToString() + "\n";
            if (CurBlock != null)
            {
                debugText.text += "CurBlock state: " + CurBlock.GetBlockState().ToString() + "\n";
            }
            else
            {
                debugText.text += "no CurBlock" + "\n";
            }
            debugText.text += "IgnoreBlockInput: " + IgnoreBlockInput + "\n";
        }
    }
    void Update()
    {        
        if (PuzzleState != ePuzzleState.PLAYING) return;    // ignore input if we're not in a PLAYING state
        if (IgnoreBlockInput == true)
        {
            DebugPrint();
            return;
        }
        if (TouchState == eTouchState.CLICK || TouchState == eTouchState.RELEASE)
        {   // update the timer if we're on a CLICK or RELEASE to see if we need to change to HOLD or NONE
            InputTimer += Time.deltaTime;
        }
        if(Input.GetMouseButtonDown(0)) // initial press
        {
            LayerMask mask = LayerMask.GetMask("Block");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {                
                Block newBlock = hit.collider.gameObject.GetComponent<Block>();
                if (newBlock == null) { Debug.LogError("ERROR: clicked on a non-Block object."); return; }// just bail if we haven't clicked on a block                
                if (CurBlock == null) SetCurBlock(newBlock); /*CurBlock = newBlock;*/ // if we have a newBlock but no current block, assign CurBlock                                               
                if( newBlock == CurBlock)   // ignore if we're clicking on a block other than the current one
                {                    
                    if(TouchState == eTouchState.NONE)
                    {   // initial press, so enter the "CLICK" state
                        SetInputState(eTouchState.CLICK, 0f, 0);                        
                    }
                    else if(TouchState == eTouchState.RELEASE)
                    {   // going for another Tap since we have re-clicked before the TapTime has been passed
                        SetInputState(eTouchState.CLICK, 0f, TapCount);                       
                    }
                }
                else Debug.Log("clicked on a different block than the currently selected one");
            }
        }     
        else if(Input.GetMouseButton(0)) // holding down
        {
            float TouchDistance;
            float TouchAngle;
            // you're only ever in the CLICK state if you've clicked on a block, so make sure to check
            if (TouchState == eTouchState.CLICK)
            {
                if(InputTimer >= HoldTime )
                {   // held down button long enough for a HOLD, so see if we should go into that state
                    if(TapCount == 0 )
                    {   // first click, no taps so go into HOLD (don't go to HOLD if multiple taps)
                        SetInputState(eTouchState.HOLD, 0f, 0); 
                        if(CurBlock == null ) { Debug.LogError("ERROR: no CurBlock."); return; }
                        //if (CurBlock.GetBlockState() == Block.eState.RAISED) CurBlock.BeginMoving(); // start moving the black if it's raised
                        if (CurBlock.GetBlockState() == Block.eState.LOWERED) CurBlock.BeginHold(RaiseLowerTime); // movement is now started when the piece is lowered.  It raises, then goes into move state
                        /*StartTouchPos = Input.mousePosition;    // set initial touch positions
                        CurTouchPos = StartTouchPos;
                        LastTouchPos = StartTouchPos;*/
                    }
                    else
                    {   // not our first click, so just go into a NONE state
                        SetInputState(eTouchState.NONE, 0f, 0);                       
                    }
                }                
            }            
            // check to see if we're moving a block
            if( TouchState == eTouchState.HOLD && CurBlock.GetBlockState() == Block.eState.MOVING)
            {
                Block.eDir curMoveDir = Block.eDir.NONE;
                CurTouchPos = Input.mousePosition;             
                // get the angle of the finger movement since last frame
                Vector3 targetDir = CurTouchPos - LastTouchPos;
                TouchAngle = Vector3.Angle(targetDir, Vector3.right);
                if (StartTouchPos.y < CurTouchPos.y) TouchAngle = 360f - TouchAngle;                                
                if (TouchAngle > 330 || TouchAngle < 30) curMoveDir = Block.eDir.HORIZONTAL;
                if (TouchAngle > 150 && TouchAngle < 210) curMoveDir = Block.eDir.HORIZONTAL;
                if (TouchAngle > 60 && TouchAngle < 120) curMoveDir = Block.eDir.VERTICAL;
                if (TouchAngle > 240 && TouchAngle < 300) curMoveDir = Block.eDir.VERTICAL;
                // get the distance of the movement
                TouchDistance = 0f;
                if (curMoveDir == Block.eDir.HORIZONTAL) TouchDistance = CurTouchPos.x - LastTouchPos.x;
                else if (curMoveDir == Block.eDir.VERTICAL) TouchDistance = CurTouchPos.y - LastTouchPos.y;
                // check to see if we need to set the direction of a 1x1 block.  Only 1x1 blocks can ever have a direction of NONE since it can change
                if (CurBlock.GetBlockDir() == Block.eDir.NONE && TouchDistance != 0f)
                {                   
                    CurBlock.SetBlockDir(curMoveDir);
                }
                else if(TouchDistance == 0f )
                {   // if we've not moved at all, then don't have any move direction.  This will essentially cancel this frame's movement
                    curMoveDir = Block.eDir.NONE;
                }
                // We don't actually move the block on the current movement, we only add the current movement to the list.  
                // If the list is maxed out, THEN we move the block based on the oldest movement recorded
                if (CurBlock.GetBlockDir() == curMoveDir && curMoveDir != Block.eDir.NONE)
                {   // we're ready to add another block movement 
                    bool validMove = true;
                    Vector3 moveDelta = Vector3.zero;
                    // get the world position of the last and current touch position.  MONOTE - could optimize
                    Vector3 lastWorld = Camera.main.ScreenToWorldPoint(new Vector3(LastTouchPos.x, LastTouchPos.y, Camera.main.transform.position.y));
                    Vector3 curWorld = Camera.main.ScreenToWorldPoint(new Vector3(CurTouchPos.x, CurTouchPos.y, Camera.main.transform.position.y));
                    if (CurBlock.GetBlockDir() == Block.eDir.HORIZONTAL)
                    {   // if we're HORIZONTAL, only take into account the x delta
                        moveDelta.x = curWorld.x - lastWorld.x;
                        if (Mathf.Abs(moveDelta.x) > .95f) validMove = false;   // if moved too far, not a valid move                        
                    }
                    else if (CurBlock.GetBlockDir() == Block.eDir.VERTICAL)
                    {
                        // if we're VERTICAL, only take into account the z delta
                        moveDelta.z = curWorld.z - lastWorld.z;
                        if (Mathf.Abs(moveDelta.z) > .95f) validMove = false; // if moved too far, not a valid move                                     
                    }
                    if (validMove == false) Debug.LogWarning("WARNING: move too fast: " + moveDelta.ToString("F3")); // bail if we're trying to move too far
                    else
                    {   // not moving too far, so get the movement prepared    
                        // only move the block if we've maxed out our MoveDelta list
                        if (MoveDeltas.Count == BlockMoveDelay)
                        {
                            MoveCurBlock();                            
                        }
                        MoveDeltas.Add(moveDelta); // add the latest movement stuff to the list               
                    }                    
                }
                else
                {   // not currently adding a new movement (IE we stopped dragging and are just holding our finger still) but we still want to move the block based on the movement list
                    if(MoveDeltas.Count != 0 )
                    {   // move the block, but do NOT add a new movement to the list
                        MoveCurBlock();                       
                    }
                }
                LastTouchPos = CurTouchPos;                
            }
        }
        else if(Input.GetMouseButtonUp(0)) // release
        {           
            if(TouchState == eTouchState.HOLD)
            {   // releasing a HOLD just resets everything since you can't Tap/DoubleTap etc from a HOLD
                SetInputState(eTouchState.NONE, 0f, 0);                        
            }
            else if(TouchState == eTouchState.CLICK)
            {   // You were in the middle of a CLICK, so change to RELEASE and increase the Tap count
                SetInputState(eTouchState.RELEASE, 0f, TapCount + 1);               
            }
            if(CurBlock != null && CurBlock.GetBlockState() == Block.eState.MOVING )
            {   // if our move list is empty, then stop the current block's movement
                if( MoveDeltas.Count == 0 ) CurBlock.StopMoving(SnapSpeed);
                else
                {   // move list isn't empty so move the block
                    MoveCurBlock();                    
                }
            }
        }
        else // no input activity or state changes, just a raised finger/mouse       
        {
            // again, if the move list isn't empty then move the block from the oldest movement delta on the list
            if (MoveDeltas.Count != 0)
            {
                MoveCurBlock();                
            }
            else if(CurBlock != null && CurBlock.GetBlockState() == Block.eState.MOVING)
            {   // move list is empty, so stop the current block
                CurBlock.StopMoving(SnapSpeed);
            }
            if (TouchState == eTouchState.RELEASE) // we've released the mouse/finger, but not sure if we're in the middle of another tap or not
            {   
                if (InputTimer > TapTime)
                {   // you've waiting long enough after releasing the mouse button so that it can now fire off it's taps
                    if (CurBlock == null) Debug.LogError("ERROR: have a null CurBlock when trying to send taps. TouchState: " + TouchState.ToString());
                    else
                    {
                        CurBlock.SendTaps(TapCount, RaiseLowerTime, RotateTime);
                    }
                    SetInputState(eTouchState.NONE, 0f, 0); // reset the input state
                }
            }        
        }
        DebugPrint();
    }

    
    /// <summary>
    /// Takes the oldest movement from the movement list and applies it to the current selected block
    /// </summary>
    void MoveCurBlock()
    {
        if( CurBlock == null ) { Debug.LogError("ERROR: trying to MoveCurBlock() on a null CurBlock."); return; }
        if(MoveDeltas.Count == 0 ) { Debug.LogError("ERROR: trying to move a block with no movements in the block movement list."); return; }

        Vector3 moveAmount = MoveDeltas[0]; // get the oldest movement 
        MoveDeltas.RemoveAt(0);  // remove the oldest movement from the list                     
        CurBlock.Move(moveAmount);  // move the block
    }
    
    /// <summary>
    /// Helper function to set/reset all of the current input state values
    /// </summary>    
    void SetInputState(eTouchState state, float timer, int taps)
    {        
        TouchState = state;
        InputTimer = timer;
        TapCount = taps;
    }
    #endregion

    #region EXIT
    /// <summary>
    /// Called from the Block.cs when a block has finished lowering back to the grid
    /// </summary>
    public void ReleaseCurBlock()
    {
        // if we've just lowered the Player block, then check for the exit
        string s;
        if (CurBlock == null) s = "null CurBlock";
        else s = CurBlock.name;
       // Debug.Log("ReleaseCurBlock() obj name: " + this.name + ", CurBlock: " + s);
        if (CurBlock.CompareTag("Player"))
        {
            CheckExit();
        }
        SetCurBlock(null);       
    }
    public void CurBlockCheckMovementBegin()
    {
        bool beginMovement = false;
        // we're done raising but since we want to ignore input while it's raising make sure we're still selecting the block
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            LayerMask mask = LayerMask.GetMask("Block");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                Block newBlock = hit.collider.gameObject.GetComponent<Block>();
                if (newBlock == CurBlock)
                {
                    beginMovement = true;
                }
            }
        }
        if(beginMovement == true )
        {
            //Debug.Log("going to begin movement, TouchState: " + TouchState.ToString());
            SetIgnoreBlockInput(false);
            StartTouchPos = Input.mousePosition;
            CurTouchPos = StartTouchPos;
            LastTouchPos = StartTouchPos;
        }
        else
        {
           // Debug.Log("don't begin movment, move down");
            TouchState = eTouchState.NONE;
            CurBlock.StopMoving(SnapSpeed);
        }
        
    }
    /// <summary>
    /// Checks to see if the Player block is on the exit or not
    /// </summary>
    void CheckExit()
    {
        // get the distance in 2D (ignore Y so that the exit block can be where if it wants vertically)
        Vector3 playerCenter = Player.GetComponent<BoxCollider>().bounds.center;
        Vector2 playerCenter2D = new Vector2(playerCenter.x, playerCenter.z);
        Vector3 exitCenter = Exit.GetComponent<BoxCollider>().bounds.center;
        Vector2 exitCenter2D = new Vector2(exitCenter.x, exitCenter.z);
        float dist = Vector2.Distance(playerCenter2D, exitCenter2D);        ;
        // if the distance is close enough (should be 0 but I want a buffer just in case) then we're on the exit
        if (Player.GetBlockDir() == Exit.GetBlockDir() && dist < .1f)
        {            
            StartCoroutine(ExitPuzzle());
        }
    }

    /// <summary>
    /// Temp function for going back to the Hub
    /// </summary>
    /// <returns></returns>
    IEnumerator ExitPuzzle()
    {
        PuzzleState = ePuzzleState.OFF;
        if (CargoMCP != null )
        {
            yield return new WaitForSeconds(1f);            
            CargoMCP.PuzzleFinished();
        }
        else
        {
            float time = 0f;
            while (time < 1.5f)
            {
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            SceneManager.LoadScene(0);
        }
        
    }
    #endregion    

    void Start()
    {
        // Debug.Log("***************************** CargoPuzzle.Start()");
        // Init all the block data  
        if (FindObjectOfType<CargoMCP>() == null) PuzzleState = ePuzzleState.PLAYING;        
        else PuzzleState = ePuzzleState.OFF;
        IgnoreBlockInput = false;
        CurBlock = null;            
        Blocks = this.GetComponentsInChildren<Block>();
        //Debug.Log("num blocks: " + Blocks.Length);
        for (int i = 0; i < Blocks.Length; i++)
        {
            //Debug.Log("initting block: " + this.name);
            Blocks[i].Init(this);
            if (Blocks[i].tag.Equals("Player")) Player = Blocks[i].GetComponent<Block>();
            if (Blocks[i].tag.Equals("Exit")) Exit = Blocks[i].GetComponent<Block>();
        }                
        SetInputState(eTouchState.NONE, InputTimer, TapCount);       
    }
     

    public void Init( CargoMCP cargoMCP )
    {
        //Debug.Log("***************************** CargoPuzzle.Init()");
        this.CargoMCP = cargoMCP;
    }

    public void BeginPlaying()
    {
        PuzzleState = ePuzzleState.PLAYING;
    }

    /*private void OnGUI()
    {
        if(GUI.Button(new Rect(0, 0, 50, 50), "Hub"))
        {
            SceneManager.LoadScene(0);
        }
    }*/
}
