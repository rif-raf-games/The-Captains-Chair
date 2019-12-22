using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Repair : MonoBehaviour
{    
    public enum eFluidType { NONE, FUEL, COOLANT };
    public enum eRepairPieceType { PIPE, SPLITTER, XOVER, BLOCKER, TERMINAL };

    public RepairPiece[] Terminals;
    RepairPiece CurTerminalStart;
    eFluidType CurTerminalStartFluidType;
    //RepairPiece[] AllPieces;
    List<RepairPiece> AllPieces = new List<RepairPiece>();

    public GameObject PathErrorSphere;

    //public static float MODEL_HEIGHT = 0.5542563f; // This was calculated elsewhere for use here    
    public static float PieceAnchorHeightValToUse = .3f;

    public static Color[] Colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };
    private void Start()
    {
        AllPieces = GameObject.FindObjectsOfType<RepairPiece>().ToList<RepairPiece>();
       // Debug.Log("num before: " + AllPieces.Count);
        foreach(RepairPiece terminal in Terminals)
        {
            AllPieces.Remove(terminal);
        }
        //Debug.Log("num after: " + AllPieces.Count);
        foreach (RepairPiece r in AllPieces)
        {
            foreach (int angle in r.OpenAngles)
            {
                int dir = AngleToDir(angle, Mathf.RoundToInt(r.transform.localRotation.eulerAngles.y), r.Type);
                r.AdjAngles.Add(dir);
            }
        }
    }
   
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
    int NumChecks = 0;
   
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
        bool show = false;// name.Contains("NonMovable");// || name.Contains("Pipe_1_Way") ;
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
    //PieceConn pcA, pcB;
   // public RepairPiece debugPiece;
    private void OnGUI()
    {
        if(GUI.Button(new Rect(0,0,100,100), "path test"))
        {
            foreach (RepairPiece terminal in Terminals) terminal.ReachedOnPath = false;
            bool puzzleSolved = false;
            bool brokenPathFound = false;
            string msg = "";
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
                    //Debug.Log("a");
                }
                if (brokenPathFound == true) break;
                //Debug.Log("b");                
            }
            //Debug.Log("c");

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
                Debug.Log("***************************************************** epic FAIL because: " + msg + ", took " + NumChecks + " to do it");
            }
            else
            {
                Debug.Log("******************************************************* epic WIN because: " + msg + ", took " + NumChecks + " to do it");
            }
        }
       


        if(GUI.Button(new Rect(0, 100, 100, 100), "prev Conn"))
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
            /*foreach(RepairPiece rp in AllPieces)
            {
                if (rp.Type == eRepairPieceType.BLOCKER) continue;
                if (rp.gameObject.activeSelf == false) continue;
                Debug.Log("checking angles for: " + rp.name);
                Vector3 pos = new Vector3(rp.transform.position.x, PieceAnchorHeightValToUse, rp.transform.position.z);//rp.transform.position + new Vector3(0f, Repair.MODEL_HEIGHT / 2f, 0f);
                foreach (int angle in rp.OpenAngles)
                {
                    int rawY = Mathf.RoundToInt(rp.transform.localRotation.eulerAngles.y);                    
                    int adjY = 360 - rawY;
                    int dir = angle + adjY;
                    if (dir > 360) dir = dir - 360;
                    if (dir < 0) dir += 360;
                    Debug.Log("Raw angle is: " + angle);
                    Debug.Log("rawY is: " + rawY);
                    Debug.Log("adjY is: " + adjY);
                    Debug.Log("final dir: " + dir);
                    Color color = Repair.Colors[(dir - 30) / 60];

                    Quaternion q = Quaternion.AngleAxis(dir, -Vector3.up);
                    Vector3 rayDir = q * Vector3.right;
                    Debug.DrawRay(pos, rayDir * 4, color, 5f);
                }
            }*/

        }
    }

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
    private void Update()
    {

    }    
}