using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class Repair : MonoBehaviour
{    
    public enum eFluidType { NONE, FUEL, COOLANT };
    public enum eRepairPieceType { PIPE, SPLITTER, XOVER, BLOCKER, TERMINAL };

    public RepairPiece[] Terminals;
    RepairPiece CurTerminalStart;
    eFluidType CurTerminalStartFluidType;
    List<RepairPiece> AllPieces = new List<RepairPiece>();

    public GameObject PathErrorSphere;
    
    public static float PieceAnchorHeightValToUse = .3f;

    public static Color[] Colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };

    public Transform BoardPieces;
    public Transform Belt;

    float BeltMoveRange;
   
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
    List<GameObject> BeltAnchors = new List<GameObject>();
    int NumChecks = 0;

    private void Start()
    {
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

        BeltAnchors = GameObject.FindGameObjectsWithTag("Repair Piece Belt Anchor").ToList<GameObject>();
        BeltAnchors = BeltAnchors.OrderBy(go => go.name).ToList<GameObject>();
        //for (int i = 0; i < BeltSpaces.Count; i++) Debug.Log(BeltSpaces[i].name);
    }

    RepairPiece HeldPiece;
    Vector3 LastMovePos;
    Vector3 HeldPieceStartPos;
    float TapTimer;
    enum eMoveType { BELT, PIECE, WAITING_FOR_TYPE, NO_MOVEMENT };
    eMoveType MoveType = eMoveType.NO_MOVEMENT;
    enum eLocationType { BELT, BOARD, };

    static float TAP_TIME = .1f;
    private void Update()
    {
        ResultText.text = "";
        float deltaY=0f;
        Vector3 newWorldPoint = Vector3.zero;
        if (Input.GetMouseButtonDown(0))
        {
            int mask = 1 << LayerMask.NameToLayer("Repair Piece");
            mask |= (1 << LayerMask.NameToLayer("Repair Piece Belt"));
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);            
            RaycastHit hit;            
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                //Debug.Log("clicked: " + hit.collider.name);
                if(hit.collider.tag.Equals("Repair Piece"))
                {
                    RepairPiece p = hit.collider.transform.parent.GetComponent<RepairPiece>();
                    if(p.Movable == true)
                    {
                        //Debug.Log("repair piece we clicked on is: " + p.name);
                        if (p.transform.parent == null) { Debug.LogError("All RepairPieces need a parent"); return; }
                        HeldPiece = p;
                        LastMovePos = HeldPiece.transform.position;
                        HeldPieceStartPos = LastMovePos;
                        TapTimer = 0f;
                        if(HeldPiece.transform.parent == BoardPieces) MoveType = eMoveType.PIECE;
                        else MoveType = eMoveType.WAITING_FOR_TYPE;                     
                    }                                                            
                }
                else
                {
                    MoveType = eMoveType.BELT;
                    LastMovePos = GetWorldPointFromMouse();
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            newWorldPoint = GetWorldPointFromMouse();
            if (MoveType == eMoveType.WAITING_FOR_TYPE && Vector3.Distance(newWorldPoint, HeldPieceStartPos) > .25f )
            {
                float angle = Vector3.Angle(HeldPieceStartPos - newWorldPoint, Vector3.right);
                Debug.Log("angle: " + angle);
                if (angle > 120f)
                {
                    Debug.Log("We're switching to piece move mode");
                    MoveType = eMoveType.PIECE;
                }
                else
                {
                    Debug.Log("We're moving the belt");
                    MoveType = eMoveType.BELT;
                }
            }
            else if(MoveType == eMoveType.BELT)
            {
                deltaY = newWorldPoint.z - LastMovePos.z;
                Belt.position += new Vector3(0f, 0f, deltaY);
                if (Belt.position.z > BeltMoveRange) Belt.position = new Vector3(Belt.position.x, Belt.position.y, BeltMoveRange);
                if(Belt.position.z < -BeltMoveRange ) Belt.position = new Vector3(Belt.position.x, Belt.position.y, -BeltMoveRange);
            }
            else if(MoveType == eMoveType.PIECE)
            {
                TapTimer += Time.deltaTime;
                if(TapTimer > TAP_TIME)
                {
                    HeldPiece.transform.position = new Vector3(newWorldPoint.x, 1f, newWorldPoint.z);
                }
            }
            LastMovePos = newWorldPoint;
        }
        else if (Input.GetMouseButtonUp(0))
        {            
            if (MoveType == eMoveType.PIECE && HeldPiece != null)
            {
                if (TapTimer <= TAP_TIME)
                {
                    HeldPiece.transform.Rotate(0f, 60f, 0f);
                    if (HeldPiece.transform.rotation.eulerAngles.y > 180)
                    {
                        Debug.Log("rotate");
                        HeldPiece.transform.eulerAngles = new Vector3(0f, HeldPiece.transform.rotation.eulerAngles.y - 360f, 0f);
                    }
                }
                else
                {
                    bool newLocFound = false;
                    HeldPiece.GetComponentInChildren<MeshCollider>().enabled = false;
                    int mask = 1 << LayerMask.NameToLayer("Repair Piece Anchor");
                    mask |= (1 << LayerMask.NameToLayer("Repair Piece Belt Anchor"));
                    Vector3 checkLoc = new Vector3(HeldPiece.transform.position.x, 0f, HeldPiece.transform.position.z);
                    Collider[] overlapColliders = Physics.OverlapSphere(checkLoc, 1f, mask);
                    //Debug.Log("num possible points: " + overlapColliders.Count());
                    
                    if (overlapColliders.Count() != 0)
                    {
                        List<AnchorHitPoint> anchorPoints = new List<AnchorHitPoint>();
                        foreach (Collider c in overlapColliders)
                        {
                           // Debug.Log("possible point: " + c.name + ", pos: " + c.transform.position);
                            float dist = Vector3.Distance(checkLoc, c.transform.position);
                            anchorPoints.Add(new AnchorHitPoint(dist, c.gameObject));
                        }
                        anchorPoints = anchorPoints.OrderBy(p => p.Dist).ToList<AnchorHitPoint>();
                        // find out the type of location were looking for
                        eLocationType locType;
                        if (overlapColliders[0].gameObject.tag == "Repair Piece Anchor") locType = eLocationType.BOARD;
                        else locType = eLocationType.BELT;
                        Debug.Log("******************locType: " + locType);
                        foreach (AnchorHitPoint p in anchorPoints)
                        {
                            //Debug.Log("dist: " + p.Dist + ", pos: " + p.Coll.transform.position);
                            RaycastHit hit;
                            Vector3 colliderPos = p.Coll.transform.position + 2f * Vector3.up;
                            Physics.Raycast(colliderPos, Vector3.down, out hit, Mathf.Infinity);
                             Debug.Log("hit collider: " + hit.collider.name + ", tag: " + hit.collider.tag);
                            if (hit.collider == null) { Debug.LogError("We should have collided with an anchor point"); return; }                            
                            if (hit.collider.tag == "Repair Piece") continue;                            
                            else if (hit.collider.tag == "Repair Piece Anchor" || hit.collider.tag == "Repair Piece Belt Anchor")
                            {                                
                                HeldPiece.transform.position = p.Coll.transform.position;
                                HeldPiece.transform.parent = ( locType == eLocationType.BOARD ? BoardPieces : Belt);
                                newLocFound = true;
                                break;
                            }                            
                        }
                        // if we're here and we're belt type then try to find an empty piece
                        if (newLocFound == false && locType == eLocationType.BELT) newLocFound = AssignBeltSpot(anchorPoints[0]);
                    }
                    HeldPiece.GetComponentInChildren<MeshCollider>().enabled = true;
                    if (newLocFound == false)
                    {
                        HeldPiece.transform.position = HeldPieceStartPos;                        
                    }                    
                }
            }
            TapTimer = 0f;
            HeldPiece = null;
            MoveType = eMoveType.NO_MOVEMENT;
        }

        if ( ResultText != null)
        {                        
            ResultText.text += "MoveType: " + MoveType.ToString() + "\n";
            ResultText.text += "deltaY: " + deltaY.ToString("F3") + "\n";
            ResultText.text += "newWorldPoint: " + newWorldPoint.ToString("F3") + "\n";
            if (HeldPiece != null)
            {
                ResultText.text += "HeldPiece parent: " + HeldPiece.transform.parent + "\n";
                ResultText.text += "HeldPiece.name: " + HeldPiece.name + "\n";
                ResultText.text += "HelpPieceStartPos: " + HeldPieceStartPos.ToString("F3") + "\n";
            }

        }
    }

    bool PushPiecesUp(GameObject beltAnchor)
    {
        int baIndex = BeltAnchors.IndexOf(beltAnchor);
        if(baIndex == -1) { Debug.LogError("we're trying to FindSpotAbove but the beltAnchor isn't in the list: " + beltAnchor.name); return false; }
        Debug.Log("Find spot above starting with index: " + baIndex);
        // start going through the list, looking for an empty spot
        bool spotFound = false;
        int foundSpotIndex=-1;
        for(int i=baIndex; i>=0; i--)
        {
            GameObject ba = BeltAnchors[i];
            RaycastHit hit;
            Vector3 colliderPos = ba.transform.position + 2f * Vector3.up;
            Physics.Raycast(colliderPos, Vector3.down, out hit, Mathf.Infinity);
            if(hit.collider == null) { Debug.LogError("why is there no hit on the belt at this spot?: " + i + ", " + ba.name); return false; }
            if(hit.collider.tag == "Repair Piece Belt Anchor")
            {
                spotFound = true;
                foundSpotIndex = i;
                break;
            }
        }

        if(spotFound == true)
        {
            Debug.Log("found an empty spot above at index: " + foundSpotIndex + " so push pieces up");
            for(int i=baIndex; i> foundSpotIndex; i--)
            {
                GameObject ba = BeltAnchors[i];
                RaycastHit hit;
                Vector3 colliderPos = ba.transform.position + 2f * Vector3.up;
                Physics.Raycast(colliderPos, Vector3.down, out hit, Mathf.Infinity);
                if(hit.collider == null || hit.collider.tag != "Repair Piece") { Debug.LogError("we have some odd behavor finding a belt spot."); return false; }
                hit.collider.transform.parent.gameObject.transform.position = BeltAnchors[i - 1].transform.position;
            }
            return true;
        }
        else
        {
            Debug.Log("didn't find any spot above");
        }

        return false;
    }
    bool PushPiecesDown(GameObject beltAnchor)
    {
        int baIndex = BeltAnchors.IndexOf(beltAnchor);
        if (baIndex == -1) { Debug.LogError("we're trying to FindSpotBelow but the beltAnchor isn't in the list: " + beltAnchor.name); return false; }
        Debug.Log("Find spot below starting with index: " + baIndex);
        // start going through the list, looking for an empty spot
        bool spotFound = false;
        int foundSpotIndex = -1;
        for (int i = baIndex; i < BeltAnchors.Count; i++)
        {
            GameObject ba = BeltAnchors[i];
            RaycastHit hit;
            Vector3 colliderPos = ba.transform.position + 2f * Vector3.up;
            Physics.Raycast(colliderPos, Vector3.down, out hit, Mathf.Infinity);
            if (hit.collider == null) { Debug.LogError("why is there no hit on the belt at this spot?: " + i + ", " + ba.name); return false; }
            if (hit.collider.tag == "Repair Piece Belt Anchor")
            {
                spotFound = true;
                foundSpotIndex = i;
                break;
            }
        }

        if (spotFound == true)
        {
            Debug.Log("found an empty spot below at index: " + foundSpotIndex + " so push pieces down");
            for (int i = baIndex; i < foundSpotIndex; i++)
            {
                GameObject ba = BeltAnchors[i];
                RaycastHit hit;
                Vector3 colliderPos = ba.transform.position + 2f * Vector3.up;
                Physics.Raycast(colliderPos, Vector3.down, out hit, Mathf.Infinity);
                if (hit.collider == null || hit.collider.tag != "Repair Piece") { Debug.LogError("we have some odd behavor finding a belt spot."); return false; }
                hit.collider.transform.parent.gameObject.transform.position = BeltAnchors[i + 1].transform.position;
            }
            return true;
        }
        else
        {
            Debug.Log("didn't find any spot below");
        }

        return false;
    }

    bool AssignBeltSpot(AnchorHitPoint p)
    {
        Debug.Log("AssignBeltSpot() coll: " + p.Coll.name + ", which is index: " + BeltAnchors.IndexOf(p.Coll));
        bool piecesMoved = false;
        if (HeldPiece.transform.position.z < p.Coll.transform.position.z)
        {
            Debug.Log("push pieces up");
            piecesMoved = PushPiecesUp(p.Coll);
            Debug.Log("did push the pieces up and open up the AnchorHitPoint's passed in spot?: " + piecesMoved);
            if (piecesMoved == false)
            {
                Debug.Log("Couldn't push the pieces up, so try down");
                piecesMoved = PushPiecesDown(p.Coll);
            }
        }        
        else 
        {
            Debug.Log("push pieces down");
            piecesMoved = PushPiecesDown(p.Coll);
            Debug.Log("did push the pieces down and open up the AnchorHitPoint's passed in spot?: " + piecesMoved);
            if(piecesMoved == false)
            {
                Debug.Log("Couldn't push the pieces down, so try up");
                piecesMoved = PushPiecesUp(p.Coll);
            }
        }        

        if (piecesMoved)
        {
            Debug.Log("pieces were moved so put the piece at the spot");
            HeldPiece.transform.position = p.Coll.transform.position;
            HeldPiece.transform.parent = Belt;
        }   
        else
        {
            Debug.Log("Nope, no empty spots so the belt must be full");
        }
        return piecesMoved;
    }
                
    Vector3 GetWorldPointFromMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 m = new Vector3(mousePos.x, mousePos.y, Camera.main.transform.position.y);
        return Camera.main.ScreenToWorldPoint(m);
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
    void SetupPathError(string s, RepairPiece curPiece, Collider collider, Vector3 rayDir)
    {
        RepairPiece collPiece = null;
        Vector3 collPos = Vector3.zero;

        PathErrorMessage = s;        
        if (collider == null)
        {
            PathErrorSphere.transform.position = curPiece.transform.position + rayDir * 3f;            
        }
        else
        {
            PathErrorSphere.transform.position = collider.transform.position + new Vector3(0f, .5f, 0f);
            collPiece = collider.transform.parent.GetComponent<RepairPiece>();            
        }
        if (collider!= null && collPiece == null)
        {
            GameObject go = new GameObject();
            collPiece = go.AddComponent<RepairPiece>();
            collPiece.transform.position = collider.transform.position;
        }

        PieceConn newConn = new PieceConn(collPiece, curPiece);
        DEBUG_ConnsOnThisPath.Add(newConn);
    }

    public Material Fuel;
    public Material Coolant;
    int AngleToDir(int rawAngle, int yRot, eRepairPieceType type)
    {
        if (type == eRepairPieceType.XOVER) return rawAngle;
        bool show = false;
        if(show) Debug.Log("-----rawAngle: " + rawAngle);
        if (show) Debug.Log("yRot: " + yRot);
        int adjY = 360 - yRot;
        if (show) Debug.Log("adjY: " + adjY);
        int dir = rawAngle + adjY;
        if (show) Debug.Log("dir 1: " + dir);
        if (dir > 360) dir = dir - 360;
        if (show) Debug.Log("dir 2: " + dir);
        if (dir < 0) dir += 360;
        if (show) Debug.Log("dir 3: " + dir);
        return dir;
    }
    private bool CheckPieceConn(PieceConn pieceConn)
    {             
        RepairPiece curPiece = pieceConn.Cur;        
        Vector3 curPiecePos = new Vector3(curPiece.transform.position.x, PieceAnchorHeightValToUse, curPiece.transform.position.z); //curPiece.transform.position + new Vector3(0f, Repair.MODEL_HEIGHT / 2f, 0f);        
        foreach (int angle in curPiece.OpenAngles)
        {
            NumChecks++;            
            int dir = AngleToDir(angle, Mathf.RoundToInt(curPiece.transform.localRotation.eulerAngles.y), curPiece.Type);
            Color color = Repair.Colors[(dir - 30) / 60];

            Quaternion q = Quaternion.AngleAxis(dir, -Vector3.up);
            Vector3 rayDir = q * Vector3.right;
           // Debug.DrawRay(curPiecePos, rayDir * 4, color, 5f);

            RaycastHit hit;
            Physics.Raycast(curPiecePos, rayDir, out hit, Mathf.Infinity);
            StaticStuff.PrintRepairPath("-----------------------------curPiece: " + curPiece.name + " checking dir: " + dir);                  
            if (hit.collider == null)
            {                                                
                SetupPathError(curPiece.name + " hit nothing at dir: " + dir + " so we must have shot off the board.", curPiece, null, rayDir);
                return false;
            }
            else
            {
               // Debug.Log(curPiece.name + " hit " + hit.collider.name + ", with tag: " + hit.collider.tag + " at dir: " + dir);
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
                else
                {
                    //Debug.Log("checking for hit: " + hit.collider.name);
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
                            // 0 rot: 210 -> 210
                            // -60                             
                            StaticStuff.PrintRepairPath("There is a piece called: " + hit.collider.transform.parent.name + " on the spot that " + curPiece.name + "'s ray collided with but it " +
                                "is an XOVER type, so assign the XOVER's only relevant OpenAngle: " + dir + " to check and do NOT assign a fluid type.  Create a new " +
                                "conn Us: " + hit.collider.transform.parent.name + ", From: " + curPiece.name);                            
                            adjacentPiece.OpenAngles.Clear();
                            adjacentPiece.OpenAngles.Add(dir);
                        }
                        else if(adjacentPiece.Type == eRepairPieceType.SPLITTER)
                        {                                                        
                            int dirAdj = (dir > 180 ? dir - 180 : dir);                            
                            int openAngle = (dirAdj < 150 ? dir - 60 : dir + 60);
                            if (openAngle > 360) openAngle -= 360;
                            int newDir = AngleToDir(openAngle, Mathf.RoundToInt(adjacentPiece.transform.localRotation.eulerAngles.y), eRepairPieceType.SPLITTER);
                            StaticStuff.PrintRepairPath("There is a piece called: " + hit.collider.transform.parent.name + " on the spot that " + curPiece.name + "'s ray collided with but it " +
                                "is an SPLITTER type, so assign the SPLITTER's only relevant OpenAngle: " + openAngle + " to check and do NOT assign a fluid type.  Create a new " +
                                "conn Us: " + hit.collider.transform.parent.name + ", From: " + curPiece.name);                            
                            adjacentPiece.OpenAngles.Clear();
                            adjacentPiece.OpenAngles.Add(newDir);
                        }
                        else
                        {
                            StaticStuff.PrintRepairPath("There is a piece called: " + hit.collider.transform.parent.name + " on the spot that " + curPiece.name + "'s ray collided with that does NOT have the same fluid type " +
                            "as our current path so assign the fluid type and create a new conn Us: " + hit.collider.transform.parent.name + ", From: " + curPiece.name);                            
                            adjacentPiece.FluidType = CurTerminalStartFluidType;
                            if (adjacentPiece.FluidType == eFluidType.FUEL) adjacentPiece.GetComponentInChildren<MeshRenderer>().material = Fuel;
                            else adjacentPiece.GetComponentInChildren<MeshRenderer>().material = Coolant;

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
        //Debug.Log("No fails on conn check Us: " + pieceConn.Us.name + " from: " + pieceConn.From.name + " so keep checking if necessary");
        //ConnsChecked.Add(pieceConn);
        return true;
    }

    public Text ResultText;
    private void OnGUI()
    {
        return;
        if(GUI.Button(new Rect(0,100,100,100), "path test"))
        {
            foreach (RepairPiece terminal in Terminals) terminal.ReachedOnPath = false;
            bool puzzleSolved = false;
            bool brokenPathFound = false;
            string msg = "";
            ResultText.text = "";
            foreach ( RepairPiece terminal in Terminals)
            {
                if (terminal.ReachedOnPath == true) continue;
                //foreach (RepairPiece rp in AllPieces) rp.FluidType = eFluidType.NONE;
                NumChecks = 0;                                
                CurTerminalStart = terminal;
                CurTerminalStartFluidType = CurTerminalStart.FluidType;
                ConnsToCheck.Clear();
                AllConnsChecked.Clear();
                DEBUG_ConnsOnThisPath.Clear();
                ConnsResult.Clear();
                ConnsToCheck.Add(new PieceConn(CurTerminalStart, CurTerminalStart));
                CurTerminalStart.ReachedOnPath = true;
                Debug.Log("going to check a path from terminal: " + CurTerminalStart.name);
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

            if(brokenPathFound == false)
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
            
            if (puzzleSolved == false)
            {
                string s = "epic FAIL because: " + msg + ", took " + NumChecks + " to do it";
                Debug.Log("***************************************************************" + s);
                ResultText.text = s;
            }
            else
            {
                string s = "epic WIN because: " + msg + ", took " + NumChecks + " to do it";
                Debug.Log("***************************************************************" + s);
                ResultText.text = s;
            }
        }

        if (GUI.Button(new Rect(0, 200, 100, 100), "p1, p2"))
        {
            float dist = Vector3.Distance(p1.transform.position, p2.transform.position);
            Debug.Log("p1: " + p1.transform.position);
            Debug.Log("p2: " + p2.transform.position);
            Debug.Log("dist: " + dist);
        }
        if (GUI.Button(new Rect(0, 300, 100, 100), "p1, p3"))
        {
            float dist = Vector3.Distance(p1.transform.position, p3.transform.position);
            Debug.Log("p1: " + p1.transform.position);
            Debug.Log("p3: " + p3.transform.position);
            Debug.Log("dist: " + dist);
            
        }
        /*if(GUI.Button(new Rect(0, 100, 100, 100), "prev Conn"))
        {
            if (CurConnIndex != 0) CurConnIndex--;
            ShowPathIndex();
        }
        if (GUI.Button(new Rect(100, 100, 100, 100), "next Conn"))
        {
            if (CurConnIndex < DEBUG_ConnsOnThisPath.Count - 1) CurConnIndex++;
            ShowPathIndex();
        }
        if (GUI.Button(new Rect(0, 200, 100, 100), "show path"))
        {            
            StartCoroutine(ShowPath());            
        }*/
    }
    public GameObject p1, p2, p3;
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
    public GameObject Cur, From;
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