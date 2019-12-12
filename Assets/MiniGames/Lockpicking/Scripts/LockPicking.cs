using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockPicking : MonoBehaviour
{
    public Diode Diode;
    public GameObject CenterBlock;
    public List<Ring> Rings;
    public List<PathNode> StartNodes;
    public List<PathNode> DeathNodes;
    public GameObject CurTouchedRing = null;

    public void InitFromProcessing( Diode diode, GameObject centerBlock, List<Ring> rings, List<PathNode> startNodes, List<PathNode> deathNodes )    
    {
        this.Diode = diode;
        CenterBlock = centerBlock;
        Rings = rings;
        StartNodes = startNodes;
        DeathNodes = deathNodes;

        float speed = 0f;
        foreach(Ring r in Rings)
        {
            r.SetRotateSpeed(speed);
            speed = -speed;
        }
    }
    public PathNode DebugStartNode;
    // Start is called before the first frame update
    void Start()
    {
        CenterBlock.transform.position = new Vector3(CenterBlock.transform.position.x, 0f, CenterBlock.transform.position.z);
        int startIndex = Random.Range(0, StartNodes.Count);
        if (DebugStartNode != null)
            Diode.SetStartNode(DebugStartNode);
        else
            Diode.SetStartNode(StartNodes[0]);

    }   
    
    public GameObject TouchPoint, MidPoint, LastPoint;
    // float startAngle, startAngleAdjust;
    Vector3 world, lastWorld;
    float dragAngle = 0f;
    Vector3 LastMousePos = Vector3.zero;
    string DebugString = "";
    Vector3 mousePos = Vector3.zero;
    Vector3 midWorld;
    float centerDir, moveDir, diffDir;
    float moveDistWorld, moveDistMouse;
    float unModDragAngle;
    float Speed;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // initial press
        {
            mousePos = Input.mousePosition;
            LastMousePos = mousePos;
            midWorld = Vector3.zero;
            Vector3 m = new Vector3(mousePos.x, mousePos.y, 10f);
            world = Camera.main.ScreenToWorldPoint(m);
            lastWorld = world;
            dragAngle = 0f;
            moveDistWorld = 0f;
            moveDistMouse = 0f;

            LayerMask mask = LayerMask.GetMask("Lockpick Touch Control");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {                
                CurTouchedRing = hit.collider.gameObject;                                                                                              
            }            
        }        
        else if(Input.GetMouseButton(0))
        {
            mousePos = Input.mousePosition;            
            Vector3 m = new Vector3(mousePos.x, mousePos.y, 10f);
            world = Camera.main.ScreenToWorldPoint(m);            

            TouchPoint.transform.position = world;


            Vector3 mouseDir = mousePos - LastMousePos;
            unModDragAngle = Vector3.Angle(mouseDir, Vector3.right);
            dragAngle = GetAngle(mouseDir, (mousePos.y < LastMousePos.y));           

            midWorld = (lastWorld + world) / 2f;

            centerDir = GetAngle(world, false);
            moveDir = GetAngle(lastWorld - world, false);
            diffDir = Mathf.Abs(moveDir - centerDir);
            if (diffDir > 90f) diffDir = 180f - diffDir;

            MidPoint.transform.position = midWorld;
            LastPoint.transform.position = lastWorld;

            moveDistWorld = Vector3.Distance(world, lastWorld);
            Debug.Log(mousePos + ", " + LastMousePos);
            moveDistMouse = Vector3.Distance(mousePos, LastMousePos);
            Debug.Log(moveDistMouse);
                   
            if(moveDistWorld > 0f)
            {
                Speed = diffDir * moveDistWorld;
               // if (unModDragAngle > 90f) speed = -speed;
                Rings[3].Rotate(Speed*10f);
            }

            LastMousePos = mousePos;
            lastWorld = world;            
        }
        if(Input.GetMouseButtonUp(0))
        {
            //Debug.Log("up");
            CurTouchedRing = null;
        }
        SetDebugString();
    }

    float GetAngle(Vector3 dir, bool adjust)
    {
        float rot = Vector3.Angle(dir, Vector3.right);
        if (adjust) rot = 360f - rot;
        if (rot >= 360f) rot = rot - 360f;
        return rot;
    }
    
    void SetDebugString()
    {
        
        if (CurTouchedRing == null) DebugString = "no ring\n";
        else DebugString = CurTouchedRing.name + "\n";
        DebugString += "Speed: " + Speed.ToString("F3") + "\n";
        DebugString += "dragAngle: " + dragAngle.ToString("F3") + "\n";
        DebugString += "unModDragAngle: " + unModDragAngle.ToString("F3") + "\n";
        DebugString += "centerDir: " + centerDir.ToString("F3") + "\n";
        DebugString += "moveDir: " + moveDir.ToString("F3") + "\n";
        DebugString += "diffDir: " + diffDir.ToString("F3") + "\n";
        DebugString += "moveDistWorld: " + moveDistWorld.ToString("F3") + "\n";
        DebugString += "moveDistMouse: " + moveDistMouse.ToString("F3") + "\n\n\n";

        DebugString += "mousePos: " + mousePos.ToString("F3") + "\n";
        DebugString += "LastMousePos: " + LastMousePos.ToString("F3") + "\n";        
        DebugString += "world: " + world.ToString("F3") + "\n";
        
    }

    public string DebugGetTouchedRingName()
    {
        // if (CurTouchedRing == null) return "no touched ring";
        // else return CurTouchedRing.name;
        return DebugString;
    }
    public void RotateRings()
    {
        foreach (Ring r in Rings)
        {
            r.Rotate();
        }
    }

    private void Awake()
    {
        Physics.autoSimulation = true;
    }
}
