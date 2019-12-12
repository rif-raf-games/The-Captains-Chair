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
    float LargestRingDiameter;

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

        LargestRingDiameter = Rings[Rings.Count-1].GetComponent<MeshCollider>().bounds.extents.x;
        Debug.Log("LargestRingDiameter: " + LargestRingDiameter);
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
    float worldMoveDist, moveDistMouse;
    float unModDragAngle;
    float Speed;
    float angleDiffAbs, angleDiffAdj;
    float mouseDragAngle, worldDragAngle, centerToWorldAngle, lastCenterToWorldAngle;
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
            mouseDragAngle = 0f;
            worldMoveDist = 0f;
            moveDistMouse = 0f;

            LayerMask mask = LayerMask.GetMask("Lockpick Touch Control");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                CurTouchedRing = hit.collider.gameObject;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            CurTouchedRing = null;
        }
        else if (Input.GetMouseButton(0) && CurTouchedRing != null)
        {
            mousePos = Input.mousePosition;
            Vector3 m = new Vector3(mousePos.x, mousePos.y, 10f);
            world = Camera.main.ScreenToWorldPoint(m);
           
            Vector3 worldDir = world - lastWorld;
            worldDragAngle = GetAngle(worldDir, (world.z < lastWorld.z));

            centerToWorldAngle = GetAngle(world, world.z < 0f);

            worldMoveDist = Vector3.Distance(world, lastWorld) * 10f;

            worldDragVec = world - lastWorld;
            centerToWorldVec = world;
            angleDiff = Vector3.SignedAngle(worldDragVec, centerToWorldVec, Vector3.up);
            angleDiffAbs = Mathf.Abs(angleDiff);
            angleDiffAdj = (angleDiffAbs > 90 ? 180f - angleDiffAbs : angleDiffAbs);

            MeshCollider mc = CurTouchedRing.GetComponent<MeshCollider>();
            //Debug.Log(mc.bounds.extents.ToString("F3"));

            if(worldMoveDist > 0f)
            {
                Speed = angleDiffAdj * worldMoveDist;
                if (centerToWorldAngle > lastCenterToWorldAngle) Speed = -Speed;
                float adj = 1f / (CurTouchedRing.GetComponent<MeshCollider>().bounds.extents.x / LargestRingDiameter);
                //Debug.Log("adj: " + adj);
                Ring ring = CurTouchedRing.transform.GetChild(0).transform.GetChild(0).GetComponent<Ring>();
                ring.Rotate(Speed*adj);
            }

            //TouchPoint.transform.position = world;
           
            lastWorld = world;
            lastCenterToWorldAngle = centerToWorldAngle;
        }
        
        SetDebugString();
    }
    Vector3 worldDragVec;
    Vector3 centerToWorldVec;
    float angleDiff;
    void SetDebugString()
    {
        if (CurTouchedRing == null) DebugString = "no ring\n";
        else DebugString = CurTouchedRing.name + "\n";
       // DebugString += "mouseDragAngle: " + mouseDragAngle.ToString("F3") + "\n";
       /* DebugString += "worldDragAngle: " + worldDragAngle.ToString("F3") + "\n";
        DebugString += "centerToWorldAngle: " + centerToWorldAngle.ToString("F3") + "\n";
        DebugString += "worldMoveDist: " + worldMoveDist.ToString("F3") + "\n";
        DebugString += "\n";
        DebugString += "worldDragVec: " + worldDragVec.ToString("F3") + "\n";
        DebugString += "centerToWorldVec: " + centerToWorldVec.ToString("F3") + "\n";
        DebugString += "angleDiff: " + angleDiff.ToString("F3") + "\n";
        DebugString += "angleDiffAbs: " + angleDiffAbs.ToString("F3") + "\n";
        DebugString += "angleDiffAdj: " + angleDiffAdj.ToString("F3") + "\n";
        DebugString += "\n";
        DebugString += "worldDragVec normal: " + worldDragVec.normalized.ToString("F3") + "\n";
        DebugString += "centerToWorldVec normal: " + centerToWorldVec.normalized.ToString("F3") + "\n";
       */ /*DebugString += "world: " + world.ToString("F3") + "\n";
        DebugString += "lastWorld: " + lastWorld.ToString("F3") + "\n\n";

        DebugString += "mousePos: " + mousePos.ToString("F3") + "\n";
        DebugString += "LastMousePos: " + LastMousePos.ToString("F3") + "\n";*/
        /*DebugString += "Speed: " + Speed.ToString("F3") + "\n";
        DebugString += "dragAngle: " + dragAngle.ToString("F3") + "\n";
        DebugString += "unModDragAngle: " + unModDragAngle.ToString("F3") + "\n";
        DebugString += "centerDir: " + centerDir.ToString("F3") + "\n";
        DebugString += "moveDir: " + moveDir.ToString("F3") + "\n";
        DebugString += "diffDir: " + diffDir.ToString("F3") + "\n";
        DebugString += "moveDistWorld: " + moveDistWorld.ToString("F3") + "\n";
        DebugString += "moveDistMouse: " + moveDistMouse.ToString("F3") + "\n\n\n";

        DebugString += "mousePos: " + mousePos.ToString("F3") + "\n";
        DebugString += "LastMousePos: " + LastMousePos.ToString("F3") + "\n";        
        DebugString += "world: " + world.ToString("F3") + "\n";*/

    }

    float GetAngle(Vector3 dir, bool adjust)
    {
        float rot = Vector3.Angle(dir, Vector3.right);
        if (adjust) rot = 360f - rot;
        if (rot >= 360f) rot = rot - 360f;
        return rot;
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
