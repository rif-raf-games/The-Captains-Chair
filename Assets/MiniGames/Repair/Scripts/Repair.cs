using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEditor;
using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.The_Captain_s_Chair;
using UnityEngine.SceneManagement;
using Articy.Unity;

public class Repair : MiniGame
{    
    public enum eFluidType { NONE, FUEL, COOLANT }; // two types of fluid are possible in the game
    public enum eRepairPieceType { PIPE, SPLITTER, XOVER, BLOCKER, TERMINAL }; // types of pieces
    enum eGameState { OFF, ON };
    eGameState CurGameState = eGameState.OFF;

    FuelDoor FuelDoor;

    [Header("Repair")]
    public List<RepairPiece> Terminals = new List<RepairPiece>();

    RepairPiece CurTerminalStart;
    eFluidType CurTerminalStartFluidType;
    List<RepairPiece> AllPieces = new List<RepairPiece>();

    public GameObject PathErrorSphere;
    
    public static float PieceAnchorHeightValToUse = .3f;

    public static Color[] Colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };

    public Transform BoardPieces;
    public Transform Belt;

    float BeltMoveRange;

    public Text ResultsText;
    public Material FuelMat;
    public Material CoolantMat;
    public Material NoneMat;
    public GameObject[] Lights;

    class PieceConn
    {
        public RepairPiece Cur;
        public RepairPiece From;
        public string ID;

        public PieceConn( RepairPiece cur, RepairPiece from)
        {
            Cur = cur;
            From = from;
            ID = "Cur: " + Cur.name + " From: " + From.name;
        }
    }
    string PathErrorMessage = "";
    List<PieceConn> ConnsToCheck = new List<PieceConn>();
    List<PieceConn> AllConnsChecked = new List<PieceConn>();
    List<PieceConn> DEBUG_ConnsOnThisPath = new List<PieceConn>();
    List<string> ConnsResult = new List<string>();
    public List<GameObject> BeltAnchors = new List<GameObject>();
    int NumChecks = 0;
    int PiecesAndBeltMask;
    int FuelDoorMask;

    public override void Init(MiniGameMCP mcp, string sceneName)
    {
        //Debug.Log("Repair.Init()");
        base.Init(mcp, sceneName);
        ResultsText = MiniGameMCP.ResultsText;
        ResultsText.text = "";
        if (DebugText == null && MiniGameMCP.DebugText != null)
        {
            DebugText = MiniGameMCP.DebugText;
            DebugText.text = "";
        }
        
    }
    
    private void Awake()
    {
        base.Awake();
        SetLights(-1);
        FuelDoor = GetComponentInChildren<FuelDoor>();
    }

    void SetLights(int val)
    {
        foreach (GameObject go in Lights) go.SetActive(false);
        if (val == -1 || val >= Lights.Length) return;
        Lights[val].SetActive(true);
    }

    private void Start()
    {
        BeltIndexAdjusts.Clear(); // this is a bullshit way around a bug but i'll do all the cleanup during next project pre-production i mean it        
        BeltIndexAdjusts.Add(-1);
        BeltIndexAdjusts.Add(1);
        if (IsSolo == true)
        {
            ResultsText.text = "";
            BeginPuzzleStartTime();
        }        
    }

    eGameState DialogueSaveState;
    void SetGameState( eGameState newState )
    {
        DialogueSaveState = newState;
        if (DialogueActive == false)
        {
            CurGameState = newState;
        }               
    }

    public override void ResetPostDialogueState()
    {
        base.ResetPostDialogueState();
        CurGameState = DialogueSaveState;        
    }

    public override void BeginPuzzleStartTime()
    {
       // Debug.Log("BeginPUzzle()");
        base.BeginPuzzleStartTime();
        ResultsText.text = "";
        SetGameState(eGameState.OFF); 
        AllPieces = GameObject.FindObjectsOfType<RepairPiece>().ToList<RepairPiece>();        
        foreach (RepairPiece terminal in Terminals)
        {
            AllPieces.Remove(terminal);
        }        
        Bounds beltBounds = Belt.GetComponent<BoxCollider>().bounds;
        Vector3 beltTopWP = new Vector3(beltBounds.center.x, 0f, beltBounds.max.z);        
        Vector3 beltTopVP = Camera.main.WorldToViewportPoint(beltTopWP);
        Vector3 vpDiff = new Vector3(0f, 0f, beltTopVP.y - 1f);
        Vector3 wpDiff = Camera.main.ViewportToWorldPoint(vpDiff);
        BeltMoveRange = Belt.GetComponent<BoxCollider>().size.z/2f + wpDiff.z;

        //BeltAnchors = GameObject.FindGameObjectsWithTag("Repair Piece Belt Anchor").ToList<GameObject>();
        //BeltAnchors = BeltAnchors.OrderBy(go => go.name).ToList<GameObject>();

        PiecesAndBeltMask = 1 << LayerMask.NameToLayer("Repair Piece");
        PiecesAndBeltMask |= (1 << LayerMask.NameToLayer("Repair Piece Belt"));

        FuelDoorMask = LayerMask.GetMask("Parking Rotate Platform"); // we ran out of tags so i'm re-using some from another game

        //InitialBeltIndexStops.Add(-1); 
       // InitialBeltIndexStops.Add(BeltAnchors.Count);
       //BeltIndexAdjusts.Add(-1);
      //  BeltIndexAdjusts.Add(1);

        FuelDoor.Open(TurnGameOn);
    }

    

    List<int> InitialBeltIndexStops = new List<int>();
    List<int> BeltIndexAdjusts = new List<int>();
    RepairPiece HeldPiece;
    Vector3 LastWorldTouchPos;
    Vector3 StartWorldTouchPos;
    Vector3 StartHeldPieceWorldPos;
    float TapTimer;
    bool TouchingDoor = false;
    Vector3 DoorTouchPos;
    enum eMoveType { BELT, PIECE, WAITING_FOR_TYPE, NO_MOVEMENT };
    eMoveType MoveType = eMoveType.NO_MOVEMENT;
    enum eLocationType { BELT, BOARD, };

    static float TAP_TIME = .1f;
    private void Update()
    {
        if(DebugText != null)
        {
            DebugText.text = CurGameState.ToString() + "\n";
            DebugText.text += PuzzleStartTime + "\n";
            //DebugText.text += TouchingDoor + "\n";
           // DebugText.text += TapTimer + "\n";
        }
        //ResultText.text = "";
        if (CurGameState == eGameState.OFF) return;
        float deltaZ=0f;
        Vector3 newWorldTouchPos = Vector3.zero;
        if (Input.GetMouseButtonDown(0))
        {            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);            
            RaycastHit hit;            
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, PiecesAndBeltMask))
            {   // check to see if we've clicked on a piece or the belt   
                LastWorldTouchPos = GetWorldPosFromTouchPos(); // keep track of the previous frame's touch pos for comparisons
                if (hit.collider.tag.Equals("Repair Piece"))
                {   // clicked on a piece
                    RepairPiece p = hit.collider.transform.parent.GetComponent<RepairPiece>();
                    //Debug.Log("clicked on a repair piece: " + p.gameObject.name);
                    if(p.Movable == true)
                    {                        
                        if (p.transform.parent == null) { Debug.LogError("All RepairPieces need a parent"); return; }
                        // set up the data for the new held piece
                        HeldPiece = p;
                        StartHeldPieceWorldPos = HeldPiece.transform.position; // used for knowing where to put the piece back if it's an invalid move
                        StartWorldTouchPos = LastWorldTouchPos;  //  used for the angle that determines whether we move the piece or the belt
                        TapTimer = 0f;  // reset tap timer
                        if (HeldPiece.transform.parent == BoardPieces) MoveType = eMoveType.PIECE;   // if we're on the board, then we're automatically moving the piece
                        else MoveType = eMoveType.WAITING_FOR_TYPE; // if we're on the belt, we don't know if we're moving the belt or the piece yet
                    }                                                            
                }
                else
                {   // nope, we clicked on the belt so we know we're moving that from the start
                    MoveType = eMoveType.BELT;                    
                }
            }
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, FuelDoorMask))
            {
                TouchingDoor = true;
                TapTimer = 0;
                DoorTouchPos = GetWorldPosFromTouchPos();
            }
        }        
        else if (Input.GetMouseButton(0))
        {
            newWorldTouchPos = GetWorldPosFromTouchPos();
            if (MoveType == eMoveType.WAITING_FOR_TYPE && Vector3.Distance(newWorldTouchPos, StartWorldTouchPos) > .25f )
            {   // if we clicked on a piece that was on the belt, wait until the touch has moved far enough from the start touch spot before
                // determining if we're moving the belt or the piece
                float angle = Vector3.Angle(StartWorldTouchPos - newWorldTouchPos, Vector3.right); // moangle           
                if (angle > 120f) MoveType = eMoveType.PIECE;                
                else MoveType = eMoveType.BELT;                
            }
            else if(MoveType == eMoveType.BELT)
            {   // we're moving the belt, so just pay attention to the z position difference of the last two touch positions
                deltaZ = newWorldTouchPos.z - LastWorldTouchPos.z;
                Belt.position += new Vector3(0f, 0f, deltaZ);
                if (Belt.position.z > BeltMoveRange) Belt.position = new Vector3(Belt.position.x, Belt.position.y, BeltMoveRange);
                if(Belt.position.z < -BeltMoveRange ) Belt.position = new Vector3(Belt.position.x, Belt.position.y, -BeltMoveRange);
            }
            else if(MoveType == eMoveType.PIECE)
            {   // we're in PIECE type movement, but make sure we're past the TAP_TIME for tap timing before we move
                TapTimer += Time.deltaTime;
                if(TapTimer > TAP_TIME)
                {   // we're past tapping, so move
                    HeldPiece.transform.position = new Vector3(newWorldTouchPos.x, 1f, newWorldTouchPos.z);
                }
            }
            LastWorldTouchPos = newWorldTouchPos; // update previous frame's touch pos
        }
        else if (Input.GetMouseButtonUp(0))
        {   // we've released our touch, so lets see what's happenig
            if (MoveType == eMoveType.PIECE && HeldPiece != null)
            {   // if we were in PIECE type movement, check to see if we need to rotate the piece
                if (TapTimer <= TAP_TIME)
                {   // tap/click time was fast enough so rotate
                    HeldPiece.transform.Rotate(0f, 60f, 0f);                   
                }
                else
                {   // ok we've released our touch after moving a piece around, so figure out what to do
                    SnapPieceIntoPlace(HeldPiece, StartHeldPieceWorldPos);                               
                }
            }
            else if(TouchingDoor == true)
            {                
                if(TapTimer <= TAP_TIME && CurGameState == eGameState.ON)
                {
                    Vector3 doorReleasePos = GetWorldPosFromTouchPos();
                    if(doorReleasePos.z <= DoorTouchPos.z)
                    {
                        SetGameState(eGameState.OFF); 
                        FuelDoor.Close(CheckPuzzleComplete);
                    }                    
                }
            }            
            // reset all of the movement data
            TapTimer = 0f;
            TouchingDoor = false;
            HeldPiece = null;
            MoveType = eMoveType.NO_MOVEMENT;
        }        
    }

    public void SetBoardPiecesInEditor()
    {
        Debug.Log("SetBoardPiecesInEditor()");

        List<RepairPiece> allPieces = new List<RepairPiece>();
        allPieces = GameObject.FindObjectsOfType<RepairPiece>().ToList<RepairPiece>();
        Terminals.Clear();
        foreach (RepairPiece rp in allPieces)
        {
            if (rp.Type == eRepairPieceType.TERMINAL)
            {
                Terminals.Add(rp);
            }
        }        
        foreach (RepairPiece terminal in Terminals)
        {
            allPieces.Remove(terminal);
        }

        BeltAnchors = GameObject.FindGameObjectsWithTag("Repair Piece Belt Anchor").ToList<GameObject>();
        BeltAnchors = BeltAnchors.OrderBy(go => go.name).ToList<GameObject>();
        InitialBeltIndexStops.Clear();
        BeltIndexAdjusts.Clear();
        InitialBeltIndexStops.Add(-1);
        InitialBeltIndexStops.Add(BeltAnchors.Count);
        BeltIndexAdjusts.Add(-1);
        BeltIndexAdjusts.Add(1);

        foreach (RepairPiece rp in allPieces)
        {
            bool pieceSnapped = SnapPieceIntoPlace(rp, Vector3.zero);
            if(pieceSnapped == false)
            {
                DestroyImmediate(rp.gameObject);
            }
            else
            {                
                rp.transform.eulerAngles = new Vector3(0f, GetNewRot(rp.transform.eulerAngles.y), 0f);
            }
        }
        foreach (RepairPiece terminal in Terminals)
        {
            bool newLocFound = false;   // this lets us know whether or not to put the piece back to it's original position
            int terminalAnchorMask = 1 << LayerMask.NameToLayer("Terminal Anchor");
            terminal.GetComponentInChildren<MeshCollider>().enabled = false; // turn this off temporarily while we just want to raycast for anchor points                    
            Vector3 checkLoc = new Vector3(terminal.transform.position.x, 0f, terminal.transform.position.z);
            Collider[] overlapColliders = Physics.OverlapSphere(checkLoc, 1f, terminalAnchorMask); // get all the anchor points within 1 unit of distance  
            if (overlapColliders.Count() != 0)
            {
                List<AnchorHitPoint> anchorPoints = new List<AnchorHitPoint>();
                foreach (Collider c in overlapColliders)
                {
                    float dist = Vector3.Distance(checkLoc, c.transform.position);
                    anchorPoints.Add(new AnchorHitPoint(dist, c.gameObject));
                }
                anchorPoints = anchorPoints.OrderBy(p => p.Dist).ToList<AnchorHitPoint>();
                foreach (AnchorHitPoint p in anchorPoints)
                {
                    RaycastHit hit = GetHitAtAnchorPos(p.Coll);
                    if (hit.collider == null) { Debug.LogError("We should have collided with an anchor point"); return; }
                    if (hit.collider.tag == "Repair Piece") continue; // if there's a piece on this anchor spot, then keep looking     
                    else
                    {
                        terminal.transform.position = p.Coll.transform.position;
                        terminal.transform.eulerAngles = new Vector3(0f, GetNewRot(terminal.transform.eulerAngles.y), 0f);
                        newLocFound = true;
                        break;
                    }
                }
            }
            if(newLocFound == false)
            {
                DestroyImmediate(terminal.gameObject);
            }
            terminal.GetComponentInChildren<MeshCollider>().enabled = true; // turn this back on    
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    float GetNewRot(float rot)
    {
        int remainder = (int)(rot / 60f);
        int newRot = (int)(remainder * 60);
        if ((rot - newRot) > 30f) newRot += 60;
        return newRot;        
    }
    
    bool SnapPieceIntoPlace(RepairPiece piece, Vector3 origPos)
    {
        bool newLocFound = false;   // this lets us know whether or not to put the piece back to it's original position
        int piecesAndAnchorMask = 1 << LayerMask.NameToLayer("Repair Piece Anchor");
        piecesAndAnchorMask |= (1 << LayerMask.NameToLayer("Repair Piece Belt Anchor"));        
        piece.GetComponentInChildren<MeshCollider>().enabled = false; // turn this off temporarily while we just want to raycast for anchor points                    
        Vector3 checkLoc = new Vector3(piece.transform.position.x, 0f, piece.transform.position.z);        
        //Debug.Log("pieces anchor mask: " + piecesAndAnchorMask);
        Collider[] overlapColliders = Physics.OverlapSphere(checkLoc, 1f, piecesAndAnchorMask); // get all the anchor points within 1 unit of distance                                        
        //Debug.Log("num overlapColliders: " + overlapColliders.Length);
        if (overlapColliders.Count() != 0)
        {   // we have at least one AnchorHitPoint, so get a sorted list by distance
            List<AnchorHitPoint> anchorPoints = new List<AnchorHitPoint>();
            foreach (Collider c in overlapColliders)
            {
                float dist = Vector3.Distance(checkLoc, c.transform.position);
                anchorPoints.Add(new AnchorHitPoint(dist, c.gameObject));
            }
            anchorPoints = anchorPoints.OrderBy(p => p.Dist).ToList<AnchorHitPoint>();
            // find out the type of location were looking for
            eLocationType locType;
            if (overlapColliders[0].gameObject.tag == "Repair Piece Anchor") locType = eLocationType.BOARD;
            else locType = eLocationType.BELT;
            foreach (AnchorHitPoint p in anchorPoints)
            {
                RaycastHit hit = GetHitAtAnchorPos(p.Coll);
                if (hit.collider == null) { Debug.LogError("We should have collided with an anchor point"); return false; }
                if (hit.collider.tag == "Repair Piece") continue; // if there's a piece on this anchor spot, then keep looking                         
                else if (hit.collider.tag == "Repair Piece Anchor" || hit.collider.tag == "Repair Piece Belt Anchor")
                {   // nope, the anchor point is empty so put the HeldPiece there and assign it's parent                          
                    piece.transform.position = p.Coll.transform.position;
                    piece.transform.parent = (locType == eLocationType.BOARD ? BoardPieces : Belt);                    
                    newLocFound = true;
                    break;
                }
            }
            // if we're here and we're belt type then try to move the pieces on the belt up or down to make room for this spot
            if (newLocFound == false && locType == eLocationType.BELT)
            {
                newLocFound = AssignBeltSpot(piece, anchorPoints[0]);
            }
        }
        piece.GetComponentInChildren<MeshCollider>().enabled = true; // make sure to turn this back on 
        if (newLocFound == false)
        {   // didn't find an empty spot, so put the piece back to where it started from           
            piece.transform.position = origPos;
        }        
        else
        {
            bool adjustmentMade = CheckBeltSorting();
           // Debug.Log("piece found a new position, but was there an adjustment made to the belt?: " + adjustmentMade);
        }
        return newLocFound;
    }

    bool CheckBeltSorting()
    {
        int topIndex = 0;
        int bottomIndex = BeltAnchors.Count - 1;        
        bool adjustmentMade = false;
        RaycastHit hit;
        while(adjustmentMade == false)
        {   //int indexDataIndex = (HeldPiece.transform.position.z < p.Coll.transform.position.z ? 0 : 1); // 0 = move up, 1 = move down
            //Debug.Log("topIndex: " + topIndex);
            hit = GetHitAtAnchorPos(BeltAnchors[topIndex]);            
            if (hit.collider.tag == "Repair Piece")
            {   // hit the top index, so move down
                int a = (BeltAnchors.Count / 2) + 1;
                adjustmentMade = PushPiecesToMakeSpace(BeltAnchors[topIndex], a, BeltIndexAdjusts[1]); ;
            }
            if(adjustmentMade == false )
            {
                hit = GetHitAtAnchorPos(BeltAnchors[bottomIndex]);
                if (hit.collider.tag == "Repair Piece")
                {   // hit the bottom index, so move up
                    adjustmentMade = PushPiecesToMakeSpace(BeltAnchors[bottomIndex], (BeltAnchors.Count / 2) - 1, BeltIndexAdjusts[0]);
                }
            }
            topIndex++;
            bottomIndex--;
            if (topIndex > bottomIndex) break;
        }

        return adjustmentMade;
    }
    /// <summary>
    /// If we need to make room for a spot on the belt, this will check to see if there's any available space in the direction
    /// defined by the initialIndexStop and indexAdj vals
    /// </summary>    
    bool PushPiecesToMakeSpace(GameObject beltAnchor, int initialIndexStop, int indexAdj)
    {        
        int baIndex = BeltAnchors.IndexOf(beltAnchor);
       // Debug.Log("PushPiecesToMakeSpace() baIndex: " + baIndex);
        if (baIndex == -1) { Debug.LogError("we're trying to PushPiecesToMakeSpace but the beltAnchor isn't in the list: " + beltAnchor.name); return false; }
        // start going through the list, looking for an empty spot
        bool spotFound = false;
        int foundSpotIndex = -1;
        int i = baIndex;
        while(i != initialIndexStop)
        {
            RaycastHit hit = GetHitAtAnchorPos(BeltAnchors[i]);
            if (hit.collider == null) { Debug.LogError("why is there no hit on the belt at this spot?: " + i + ", " + BeltAnchors[i].name); return false; }
            if (hit.collider.tag == "Repair Piece Belt Anchor")
            {
                spotFound = true;
                foundSpotIndex = i;
                break;
            }
            i += indexAdj;
        }
        if (spotFound == true)
        {
            //Debug.Log("found an empty spot at index: " + foundSpotIndex + " so push pieces");
            i = baIndex;
            while(i != foundSpotIndex)
            {
                RaycastHit hit = GetHitAtAnchorPos(BeltAnchors[i]);
                if (hit.collider == null || hit.collider.tag != "Repair Piece") { Debug.LogError("we have some odd behavior finding a belt spot."); return false; }
                hit.collider.transform.parent.gameObject.transform.position = BeltAnchors[i + indexAdj].transform.position;
                i += indexAdj;
            }
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// If we're trying to move a piece onto the belt but there's already a piece on the anchor point, see if you can push the pieces
    /// up or down to make room.
    /// </summary>
    bool AssignBeltSpot(RepairPiece piece, AnchorHitPoint p)
    {
        // Debug.Log("AssignBeltSpot() coll: " + p.Coll.name + ", which is index: " + BeltAnchors.IndexOf(p.Coll));        
        int indexDataIndex = (piece.transform.position.z < p.Coll.transform.position.z ? 0 : 1); // 0 = move up, 1 = move down
        bool piecesMoved = false;
        piecesMoved = PushPiecesToMakeSpace(p.Coll, InitialBeltIndexStops[indexDataIndex], BeltIndexAdjusts[indexDataIndex]);
        if(piecesMoved == false) piecesMoved = PushPiecesToMakeSpace(p.Coll, InitialBeltIndexStops[1-indexDataIndex], BeltIndexAdjusts[1-indexDataIndex]);

        if (piecesMoved)
        {
            //  Debug.Log("pieces were moved so put the piece at the spot");
            piece.transform.position = p.Coll.transform.position;
            piece.transform.parent = Belt;
        }
        else
        {
           // Debug.Log("Nope, no empty spots so the belt must be full");
        }

        return piecesMoved;        
    }

    Vector3 GetWorldPosFromTouchPos()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 m = new Vector3(mousePos.x, mousePos.y, Camera.main.transform.position.y);
        return Camera.main.ScreenToWorldPoint(m);
    }

    /// <summary>
    /// Getting either the piece or anchor at a certain spot was used so often i made this helper function to keep things dity
    /// </summary>
    /// <param name="anchorPoint"></param>
    /// <returns></returns>
    RaycastHit GetHitAtAnchorPos(GameObject anchorPoint)
    {
        RaycastHit hit;
        Vector3 colliderPos = anchorPoint.transform.position + 2f * Vector3.up;
        Physics.Raycast(colliderPos, Vector3.down, out hit, Mathf.Infinity);
        return hit;
    }
    class AnchorHitPoint
    {
        public float Dist;
        public GameObject Coll;
        public AnchorHitPoint(float dist, GameObject coll)
        {
            Dist = dist;
            Coll = coll;
        }
    }

    bool HasConnBeenChecked(PieceConn pieceConn)
    {        
        foreach (PieceConn pc in AllConnsChecked)
        {
            if ((pc.Cur == pieceConn.From && pc.From == pieceConn.Cur) || (pc.Cur == pieceConn.Cur && pc.From == pieceConn.From))
            {
                return true;
            }
        }
        return false;
    }

    GameObject EndColPiece;
    void SetupPathError(string s, RepairPiece curPiece, Collider collider, Vector3 rayDir)
    {
        string err = "<FAIL>: SetupPathError() s: " + s + ", curPiece: " + curPiece.name;        
        RepairPiece collPiece = null;
        Vector3 collPos = Vector3.zero;

        PathErrorMessage = s;        
        if (collider == null)
        {
            err += ", collider was null so setting PathErrorSphere to rayDir*3";
            PathErrorSphere.transform.position = curPiece.transform.position + rayDir * 3f;            
        }
        else
        {
            err += ", collider name is: " + collider.gameObject.name;
            PathErrorSphere.transform.position = collider.transform.position + new Vector3(0f, .5f, 0f);
            collPiece = collider.transform.parent.GetComponent<RepairPiece>();            
        }
        if (collider!= null && collPiece == null)
        {
            GameObject go = new GameObject();
            collPiece = go.AddComponent<RepairPiece>();
            EndColPiece = go;
            collPiece.transform.position = collider.transform.position;
        }

        Debug.Log(err);
        PieceConn newConn = new PieceConn(collPiece, curPiece);
        DEBUG_ConnsOnThisPath.Add(newConn);
    }    

    Color GetColor(int dir)
    {
        int dirAdj = (dir + 360) % 360;
        int index = (dirAdj - 30) / 60;
        return Colors[index];        
    }
    private bool CheckPieceConn(PieceConn pieceConn)
    {             
        RepairPiece curPiece = pieceConn.Cur;        
        Vector3 curPiecePos = new Vector3(curPiece.transform.position.x, PieceAnchorHeightValToUse, curPiece.transform.position.z); //curPiece.transform.position + new Vector3(0f, Repair.MODEL_HEIGHT / 2f, 0f);        
        StaticStuff.PrintRepairPath("CheckPieceConn() pieceConn: " + pieceConn.ID + ", num OpenAngles: " + curPiece.OpenAngles.Count);
        foreach (int angle in curPiece.OpenAngles)
        {
            NumChecks++;            
            int pieceRot = Mathf.RoundToInt(curPiece.transform.localRotation.eulerAngles.y);
            int angleAdj = ( curPiece.Type == eRepairPieceType.XOVER || curPiece.Type == eRepairPieceType.SPLITTER ? angle : angle + pieceRot);            
            Color color = GetColor(angleAdj);
            Quaternion q = Quaternion.AngleAxis(angleAdj, Vector3.up);
            Vector3 rayDir = q * Vector3.right;// moangle
           // Debug.DrawRay(curPiecePos, rayDir * 4, color, 100000f);
            
            RaycastHit hit;
            Physics.Raycast(curPiecePos, rayDir, out hit, Mathf.Infinity);
            StaticStuff.PrintRepairPath("-----------------------------curPiece: " + curPiece.name + " checking dir: " + angleAdj);                  
            if (hit.collider == null)
            {                                                
                SetupPathError(curPiece.name + " hit nothing at dir: " + angleAdj + " so we must have shot off the board.", curPiece, null, rayDir);
                return false;
            }
            else
            {
                StaticStuff.PrintRepairPath(curPiece.name + " hit " + hit.collider.name + ", with tag: " + hit.collider.tag + " at angleAdj: " + angleAdj);
            }
            // We've hit something with our curPiece ray, so see what it is
            if (hit.collider.tag == "Repair Piece Anchor")
            {   // we hit an anchor, so check to see if there's a piece on it               
                Vector3 collidePos = hit.collider.transform.position + 2f * hit.collider.transform.up;
                Physics.Raycast(collidePos, Vector3.down, out hit, Mathf.Infinity);
                if (hit.collider == null)
                {                    
                    Debug.LogError("checking if piece is on location hit nothing, which should never happen so see what's up");                    
                    return false;
                }
                
                if (hit.collider.tag == "Repair Piece Anchor")
                {                    
                    SetupPathError("There is NO piece on the spot " + hit.collider.name, curPiece, hit.collider, rayDir);
                    return false;
                }
                else if (hit.collider.tag == "Repair Piece")
                {
                    RepairPiece adjacentPiece = hit.collider.transform.parent.GetComponent<RepairPiece>();                    
                    if (adjacentPiece == pieceConn.From)
                    {
                        StaticStuff.PrintRepairPath("we connected with a piece from " + curPiece.name + "'s ray but it's the piece we came from " + pieceConn.From.name + " so continue checking");                                                
                    }
                    else if (adjacentPiece.Type == eRepairPieceType.TERMINAL)
                    {
                        StaticStuff.PrintRepairPath("we've reached a Terminal: " + adjacentPiece.name + " so let's see if we're done");
                        if(adjacentPiece == CurTerminalStart)
                        {                                                        
                            SetupPathError("We've returned to the start Terminal so fail: " + adjacentPiece.name, curPiece, hit.collider, rayDir);
                            return false;
                        }
                        else if((curPiece.Type != eRepairPieceType.XOVER && curPiece.Type != eRepairPieceType.SPLITTER) && (adjacentPiece.FluidType != curPiece.FluidType) )
                        {                                                        
                            SetupPathError("We've reached a Terminal but it's of the wrong type so bail adj: " + adjacentPiece.name + ", fluidType: " + adjacentPiece.FluidType + ", cur fluid: " + curPiece.FluidType + ", adj fluid: " + adjacentPiece.FluidType, curPiece, hit.collider, rayDir);
                            return false;
                        }
                        else
                        {
                            StaticStuff.PrintRepairPath("We've reached a terminal of the same type so set the terminal's ReachedOnPath flag to true");
                            PieceConn newConn = new PieceConn(hit.collider.transform.parent.GetComponent<RepairPiece>(), curPiece);                            
                            adjacentPiece.ReachedOnPath = true;
                            if(HasConnBeenChecked(newConn) == false)
                            {
                                ConnsToCheck.Add(newConn);
                                AllConnsChecked.Add(newConn);
                                DEBUG_ConnsOnThisPath.Add(newConn);
                            }                           
                        }
                    }                    
                    else if( (curPiece.Type != eRepairPieceType.XOVER && curPiece.Type != eRepairPieceType.SPLITTER) && ( adjacentPiece.FluidType != eFluidType.NONE && adjacentPiece.FluidType != curPiece.FluidType ) )
                    {                                                
                        SetupPathError("we just crossed paths with a piece that already has a different fluid type attached so FAIL. curPiece type: " + curPiece.FluidType.ToString() + ", adjacent type: " + adjacentPiece.FluidType.ToString(), curPiece, hit.collider, rayDir);
                        Debug.Log("fluid type fail. Cur: " + curPiece.name + ", " + curPiece.FluidType + ", adj: " + adjacentPiece.name + ", " + adjacentPiece.FluidType);
                        return false;
                    }
                    else
                    {                        
                        if(adjacentPiece.Type == eRepairPieceType.XOVER)
                        {                                                   
                            StaticStuff.PrintRepairPath("There is a piece called: " + hit.collider.transform.parent.name + " on the spot that " + curPiece.name + "'s ray collided with but it " +
                                "is an XOVER type, so assign the XOVER's only relevant OpenAngle: " + angleAdj + " to check and do NOT assign a fluid type.  Create a new " +
                                "conn Us: " + hit.collider.transform.parent.name + ", From: " + curPiece.name);                            
                            adjacentPiece.OpenAngles.Clear();
                            adjacentPiece.OpenAngles.Add(angleAdj);
                        }
                        else if(adjacentPiece.Type == eRepairPieceType.SPLITTER)
                        {           // moangleupdate                                                                                                                                         
                            Debug.Log("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ HERE WE GO^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");                            
                            
                            // First get the direction vec between the curPiece and the adjacentPiece
                            Vector3 dirVecBetweenPieces = adjacentPiece.transform.position - curPiece.transform.position;                                                                                    
                            // Get the rotation of the adjacent piece
                            int adjPieceRot = Mathf.RoundToInt(adjacentPiece.transform.localRotation.eulerAngles.y);
                            Debug.Log("adjPieceRot: " + adjPieceRot);
                            // Vector3.forward is what we use for Vector3.Rotate when it's bouncing off an angle of 0, so turn Vector3.forward into an angle
                            float angleForward = Quaternion.FromToRotation(Vector3.right, Vector3.forward).eulerAngles.y; // moangle
                            Debug.Log("angleForward: " + angleForward);
                            // Add the adjacent piece rotation to the Vector3.forward's calculated angle
                            float angleOfNormalToUse = angleForward + adjPieceRot;
                            // now get the direction for that new angle
                            Quaternion _q = Quaternion.AngleAxis(angleOfNormalToUse, Vector3.up);
                            Vector3 normalToUseDirVec = _q * Vector3.right;
                            // now reflect on that angle
                            Vector3 reflectDirVec = Vector3.Reflect(dirVecBetweenPieces, normalToUseDirVec);
                            // Now get the angle that that dir vec represents
                            float reflectAngle = Quaternion.FromToRotation(Vector3.right, reflectDirVec).eulerAngles.y;
                            // round it off
                            int reflectAngleInt = Mathf.RoundToInt(reflectAngle);
                            StaticStuff.PrintRepairPath("There is a piece called: " + hit.collider.transform.parent.name + " on the spot that " + curPiece.name + "'s ray collided with but it " +
                                "is an SPLITTER type, so assign the SPLITTER's only relevant reflectAngleInt: " + reflectAngleInt + " to check and do NOT assign a fluid type.  Create a new " +
                                "conn Us: " + hit.collider.transform.parent.name + ", From: " + curPiece.name);
                            adjacentPiece.OpenAngles.Clear();
                            adjacentPiece.OpenAngles.Add(reflectAngleInt);
                        }
                        else if (adjacentPiece.Type == eRepairPieceType.BLOCKER)
                        {
                            SetupPathError("There is a broken piece on this spot " + hit.collider.name, curPiece, hit.collider, rayDir);
                            return false;
                        }
                        else
                        {                            
                            StaticStuff.PrintRepairPath("There is a piece called: " + hit.collider.transform.parent.name + " on the spot that " + curPiece.name + "'s ray collided with that does NOT have the same fluid type " +
                            "as our current path so assign the fluid type and create a new conn Us: " + hit.collider.transform.parent.name + ", From: " + curPiece.name);                            
                            adjacentPiece.FluidType = CurTerminalStartFluidType;
                            if (adjacentPiece.FluidType == eFluidType.FUEL) adjacentPiece.GetComponentInChildren<MeshRenderer>().material = FuelMat;
                            else adjacentPiece.GetComponentInChildren<MeshRenderer>().material = CoolantMat;

                        }

                        RepairPiece rp = hit.collider.transform.parent.GetComponent<RepairPiece>();
                        PieceConn newConn = new PieceConn(rp, curPiece);
                        if (HasConnBeenChecked(newConn) == false)
                        {
                            ConnsToCheck.Add(newConn);
                            AllConnsChecked.Add(newConn);
                            DEBUG_ConnsOnThisPath.Add(newConn);
                        }
                    }
                }
                else
                {                    
                    Debug.LogError("there's something other than a piece or piece anchor on the spot " + curPiece.name + "'s ray collided with so wtf: " + hit.collider.name);                    
                    return false;
                }
            }
            else if (hit.collider.tag == "Repair Piece")
            {                                
                SetupPathError("we ran into a Repair Piece's wall: " + hit.collider.transform.parent.name, curPiece, hit.collider, rayDir);
                return false;
            }
            else
            {                
                Debug.LogError("we hit something that's not a Repair Piece Anchor or a Repair Piece: " + hit.collider.name);                
                return false;
            }
        }        
        return true;
    }
    
    void ResetPuzzleState( bool startOver )
    {   
        NumChecks = 0;
        CurTerminalStart = null;
        CurTerminalStartFluidType = eFluidType.NONE;
        ConnsToCheck.Clear();
        AllConnsChecked.Clear();
        DEBUG_ConnsOnThisPath.Clear();
        ConnsResult.Clear();
       

        if (startOver == true)
        {
            foreach (RepairPiece terminal in Terminals) terminal.ReachedOnPath = false;
            foreach (RepairPiece piece in AllPieces)
            {
                if (piece == null) Debug.LogError(piece.name + ": null piece");
                if (piece.GetComponentInChildren<MeshRenderer>() == null) Debug.LogError(piece.name + ": null mr");
                if (piece.GetComponentInChildren<MeshRenderer>().material == null) Debug.LogError(piece.name + ": null material");
                if(piece.Type != eRepairPieceType.BLOCKER)
                {
                    piece.GetComponentInChildren<MeshRenderer>().material = NoneMat;
                    piece.FluidType = eFluidType.NONE;
                }                
            }
        }
    }

    bool CheckPaths()
    {
        foreach (RepairPiece terminal in Terminals) terminal.ReachedOnPath = false;
        bool puzzleSolved = false;
        bool brokenPathFound = false;
        string msg = "";
        ResultsText.text = "";
        foreach (RepairPiece terminal in Terminals)
        {
            if (terminal.ReachedOnPath == true) continue;
            ResetPuzzleState(false);
            CurTerminalStart = terminal;
            CurTerminalStartFluidType = CurTerminalStart.FluidType;  
            ConnsToCheck.Add(new PieceConn(CurTerminalStart, CurTerminalStart));
            CurTerminalStart.ReachedOnPath = true;

            int i = 0; // this is just for debugging so that we don't get into an infinite loop
            while (ConnsToCheck.Count != 0 && i < 100)
            {
                PieceConn curPieceConn = ConnsToCheck[0];
                ConnsToCheck.Remove(curPieceConn);
                StaticStuff.PrintRepairPath("************************************ going to check conn Us: " + curPieceConn.Cur.name + " , From: " + curPieceConn.From.name);
                if (CheckPieceConn(curPieceConn) == false)
                {
                    ConnsToCheck.Clear();
                    puzzleSolved = false;
                    brokenPathFound = true;
                    Debug.Log("***************************************************bailed due to broken puzzle");
                    msg = PathErrorMessage;
                    break;
                }
            }
            if (brokenPathFound == true) break;
        }

        if (brokenPathFound == false)
        {
            RepairPiece pieceNotReached = null;
            foreach (RepairPiece piece in Terminals)
            {
                if (piece.ReachedOnPath == false)
                {
                    pieceNotReached = piece;
                    break;
                }
            }
            if (pieceNotReached == null)
            {
                puzzleSolved = true;
                msg = "CONGRATS WE HAVE REACHED EVERY TERMINAL ON THE PUZZLE SO WE WIN!!!!!!";
            }
            else
            {
                puzzleSolved = false;
                msg = "WE HAVE AT LEAST 1 TERMINAL " + pieceNotReached.name + " THAT HAS NOT BEEN REACHED SO WE FAIL!!!!!!";
                PathErrorSphere.transform.position = pieceNotReached.transform.position + new Vector3(0f, .5f, 0f);
            }
        }
        string result = "";
        if (puzzleSolved == false)
        {
            result = "epic FAIL because: " + msg + ", took " + NumChecks + " to do it";
        }
        else
        {
            result = "epic WIN because: " + msg + ", took " + NumChecks + " to do it";
        }
        StartCoroutine(ShowResults(result, puzzleSolved));
        return puzzleSolved;
    }
    
    public void CheckPuzzleComplete()
    {
        CheckPaths();
    }
    public void TurnGameOn()
    {
        SetGameState(eGameState.ON); 
        SetLights(1);
    }
    
    public override void TMP_WinGame()
    {
        StartCoroutine(ShowResults("You are using a debug cheat to win.\nMo is making you lazy.", true));
    }

    IEnumerator ShowResults(string result, bool success)
    {        
        ResultsText.text = result;
        if (MiniGameMCP != null) MiniGameMCP.SavePuzzlesProgress(success);
        if (success == true) EndPuzzleTime(true);
        SetGameState(eGameState.OFF);
        FuelDoor.Open();
        if (success == true) SetLights(0);
        else SetLights(2);
        yield return new WaitForSeconds(3);
        if (success == true)
        {
            if (MiniGameMCP != null)
            {
                MiniGameMCP.PuzzleFinished();
            }
            else
            {
                ResultsText.gameObject.SetActive(true);
                ResultsText.text = "You beat the level but are not part of an MCP so restart.";
            }
        }
        else
        {
            SetGameState(eGameState.ON); 
            SetLights(1);
            ResetPuzzleState(true);
            ResultsText.text = "";
            PathErrorSphere.transform.position = new Vector3(-9999f, 0f, -9999f);
            if (EndColPiece != null)
            {
                Destroy(EndColPiece);
                EndColPiece = null;
            }
        }
    }
    GameObject p1, p2, p3;
    void ShowPathIndex()
    {
        PieceConn pc = DEBUG_ConnsOnThisPath[CurConnIndex];
        foreach (PieceConn p in DEBUG_ConnsOnThisPath)
        {
            if (p == pc) continue;
            if (p.Cur == pc.Cur && p.From == pc.From) Debug.LogWarning("WARNING: we've already got this conn on our path Cur: " + p.Cur + ", From: " + p.From);
        }

        string curName = "";
        if (pc.Cur == null)
        {
            Cur.SetActive(false);
            curName = "NONE";
        }
        else
        {
            Cur.SetActive(true);
            Cur.transform.position = pc.Cur.transform.position + new Vector3(0f, .6f, 0f);
            curName = pc.Cur.gameObject.name;
        }
        From.transform.position = pc.From.transform.position + new Vector3(0f, .6f, 0f);
        Debug.Log("Cur: " + curName + ", From: " + pc.From.gameObject.name);
    }
    int CurConnIndex = 0;
    GameObject Cur, From;
    IEnumerator ShowPath()
    {
        Debug.Log("num PieceConns: " + DEBUG_ConnsOnThisPath.Count);
        foreach (PieceConn pc in DEBUG_ConnsOnThisPath)
        {
            string curName = "";
            if (pc.Cur == null)
            {
                Cur.SetActive(false);
                curName = "NONE";
            }
            else
            {
                Cur.SetActive(true);
                Cur.transform.position = pc.Cur.transform.position + new Vector3(0f, .6f, 0f);
                curName = pc.Cur.gameObject.name;
            }
            From.transform.position = pc.From.transform.position + new Vector3(0f, .6f, 0f);
            Debug.Log("Cur: " + curName + ", From: " + pc.From.gameObject.name);
            yield return new WaitForSeconds(1f);
        }
    }
             
}