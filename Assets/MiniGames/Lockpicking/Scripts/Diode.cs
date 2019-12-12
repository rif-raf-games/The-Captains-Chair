using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Diode : MonoBehaviour
{
    enum eMoveDir { FORWARD, BACKWARD, NUM_DIRS };
    eMoveDir m_MoveDir = eMoveDir.FORWARD;

    Vector3 CamOffset;    
    SphereCollider SC;
    public Ring.LockpickRingPath CurPath;
    bool Moving = false;
    float MoveLerpPercentage;
    float SpeedAdj = .5f;
    LockPicking MCP;

    // Start is called before the first frame update
    void Start()
    {        
        CamOffset = Camera.main.transform.position - this.transform.position;
        SC = GetComponent<SphereCollider>();
        MCP = FindObjectOfType<LockPicking>();
    }

    void SetNewPath(Ring.LockpickRingPath path)
    {
        CurPath = path;
       // this.transform.parent = CurPath.Ring.transform;
       // LastPosition.transform.parent = CurPath.Ring.transform;
    }

    public void SetStartNode(PathNode startNode)
    {
        Moving = true;
        m_MoveDir = eMoveDir.FORWARD;
        SetNewPath(startNode.Path);        
        MoveLerpPercentage = .5f;
        Vector3 pos = Vector3.Lerp(CurPath.Start.transform.position, CurPath.End.transform.position, MoveLerpPercentage);
        transform.position = pos;
        transform.LookAt(CurPath.End.transform);
    }
    // Vector3 LastPosition = Vector3.zero;
    public GameObject LastPosition;
    public Text DebugText;
    bool JustChangedPaths = false;
    bool IgnoreCollisions = false;
    public bool rotate = false;
    //public bool stickToWall = false;
    public Ring ring1, ring2, ring;
    public float rotSpeed = 50f;
    void FixedUpdate()
    {
        if (Moving == true )
        {
            /// Debug.Log("b4 pos: " + transform.position.ToString("F4"));
            // ring1.transform.Rotate(new Vector3(0f, 50f * Time.deltaTime, 0f));
            // Debug.Log("aR pos: " + transform.position.ToString("F4"));
            
            CurPath.Start.transform.LookAt(CurPath.End.transform);
            CurPath.End.transform.LookAt(CurPath.Start.transform);
            Vector3 moveDir;
            if (m_MoveDir == eMoveDir.FORWARD) moveDir = CurPath.Start.transform.forward;
            else moveDir = CurPath.End.transform.forward;
            transform.position = transform.position + (moveDir * Time.deltaTime / 2f);
            Collider[] colliders;

            if (IgnoreCollisions == true)
            {                
                colliders = Physics.OverlapSphere(transform.position, SC.radius);
                /*if (colliders.Length > 0) Debug.Log("------ num colliders: " + colliders.Length);
                foreach (Collider c in colliders)
                {
                    Debug.Log(c.name);
                }*/
                if (colliders.Length == 1 && colliders[0].gameObject == this.gameObject) IgnoreCollisions = false;
            }

            if(IgnoreCollisions == false )
            {
                int layerMask = LayerMask.GetMask("Lockpick Ring");
                colliders = Physics.OverlapSphere(transform.position, SC.radius, layerMask);
                if (colliders.Length != 0)
                {
                    transform.position = LastPosition.transform.position;
                   // Debug.Log("col with: " + colliders[0].name + ", num cols: " + colliders.Length);
                    IgnoreCollisions = true;
                    ChangeDir();
                }
            }

            if (IgnoreCollisions == false)
            {
                int layerMask = LayerMask.GetMask("Lockpick Path Node");
                colliders = Physics.OverlapSphere(transform.position, SC.radius, layerMask);
                foreach(Collider c in colliders)
                {
                    if (c.gameObject == CurPath.Start.gameObject || c.gameObject == CurPath.End.gameObject) continue;
                    else
                    {                        
                        SetNewPath(c.gameObject.GetComponent<PathNode>().Path);
                        if (m_MoveDir == eMoveDir.FORWARD) transform.position = CurPath.Start.transform.position;
                        else transform.position = CurPath.End.transform.position;                       
                        IgnoreCollisions = true;
                        //this.transform.parent = CurPath.Ring.transform;
                        //LastPosition.transform.parent = CurPath.Ring.transform;
                        break;
                    }
                }
            }

            LastPosition.transform.position = transform.position;

            MCP.RotateRings();

            if (DebugText.text != null)
            {
                //DebugText.text = "IgnoreCollisions: " + IgnoreCollisions + "\n";
                //DebugText.text += "touched ring: " + MCP.DebugGetTouchedRingName() + "\n";
                DebugText.text = "dirs:\n" + MCP.DebugGetTouchedRingName();
            }
        }
            return;
#if false
            Vector3 pos;
            MoveLerpPercentage += Time.deltaTime * SpeedAdj;
            if (m_MoveDir == eMoveDir.FORWARD)
            {
                transform.LookAt(CurPath.End.transform);
                pos = Vector3.Lerp(CurPath.Start.transform.position, CurPath.End.transform.position, MoveLerpPercentage);
            }
            else
            {
                transform.LookAt(CurPath.Start.transform);
                pos = Vector3.Lerp(CurPath.End.transform.position, CurPath.Start.transform.position, MoveLerpPercentage);
            }
            transform.position = pos;

            bool checkLockpicks = true;
            int layerMask = LayerMask.GetMask("Lockpick Path Node");
            Collider[] colliders = Physics.OverlapSphere(transform.position, SC.radius, layerMask);
            if(JustChangedPaths == true)
            {
                if (colliders.Length < 2) JustChangedPaths = false;
            }
            if(JustChangedPaths == false)
            {
                foreach (Collider c in colliders)
                {
                    if (c.gameObject == CurPath.Start.gameObject || c.gameObject == CurPath.End.gameObject) continue;
                    else
                    {
                        CurPath = c.gameObject.GetComponent<PathNode>().Path;
                        transform.position = c.transform.position;
                        MoveLerpPercentage = .4f;
                        checkLockpicks = false;
                        JustChangedPaths = true;
                    }
                }
            }
            

            if(checkLockpicks == true )
            {
                layerMask = LayerMask.GetMask("Lockpick Ring");
                colliders = Physics.OverlapSphere(transform.position, SC.radius, layerMask);
                if (colliders.Length == 1)
                {
                    string s = "colliding with: " + colliders[0].name + ", ";
                    Vector3 rayDir = (m_MoveDir == eMoveDir.FORWARD ? CurPath.Start.transform.forward : CurPath.End.transform.forward);
                    //s += "moveDir: " + rayDir.ToString("F4") + ", ";
                    RaycastHit hit;
                    // Physics.Raycast(transform.position, rayDir, out hit, Mathf.Infinity, layerMask);
                    // if (hit.collider == null) { Debug.LogError("Major WTF error...the Raycast didn't hit??"); return; }
                    // Debug.Log("collider name: " + hit.collider.name + ", pos: " + hit.point.ToString("F4"));                    
                    // pos = hit.point + ((SC.radius * 1.01f) * (-rayDir.normalized));                    
                    // s += "before pos: " + transform.position.ToString("F4") + ", ";
                    //transform.position = pos;
                    transform.position = LastPosition;
                    //s += "after pos: " + transform.position.ToString("F4");
                    float nodeDist = Vector3.Distance(CurPath.Start.transform.position, CurPath.End.transform.position);
                    float ourDist;
                    if(m_MoveDir == eMoveDir.FORWARD) ourDist = Vector3.Distance(transform.position, CurPath.Start.transform.position);
                    else ourDist = Vector3.Distance(transform.position, CurPath.End.transform.position);                    
                    float percentage = 1f - (ourDist / nodeDist);
                    MoveLerpPercentage = percentage;
                    ChangeDir();
                    //Moving = false;
                   // Debug.Log(s);
                }                
            }            
        }
        LastPosition = transform.position;
        Camera.main.transform.position = this.transform.position + CamOffset;
        if(DebugText != null)
        {
            DebugText.text = "MoveLerpPercentage: " + MoveLerpPercentage.ToString("F4") + "\n";
            DebugText.text += "m_MoveDir: " + m_MoveDir + "\n";
            DebugText.text += "forward: " + transform.forward.ToString("F4") + "\n";
            DebugText.text += "start forward: " + CurPath.Start.transform.forward.ToString("F4") + "\n";
            DebugText.text += "JustChangedPaths: " + JustChangedPaths + "\n";

            /*RaycastHit hit;
            int ringLayerMask = LayerMask.GetMask("Lockpick Ring");
            Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, ringLayerMask);
            if (hit.collider != null)
            {
                DebugText.text += "hit: " + hit.collider.name + ", dist: " + Vector3.Distance(transform.position, hit.point).ToString("F4") + ", frame: " + frame++; 
            }
            else DebugText.text += "none";*/
        }
#endif
    }
   
    int frame = 0;
    Vector3 startPos = Vector3.zero;
    public PathNode debugP;
    private void OnGUI()
    {
       /* if (GUI.Button(new Rect(0, 0, 100, 100), "go to point"))
        {
            //float dist;
            int ringLayerMask = LayerMask.GetMask("Lockpick Ring");
            RaycastHit hit;
            Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, ringLayerMask);
            if (hit.collider != null)
            {
                Debug.Log("hit " + hit.collider.name + " at point: " + hit.point);
                transform.position = hit.point;                
            }
        }
        if (GUI.Button(new Rect(0, 100, 100, 100), "back off"))
        {
            transform.position = transform.position + ((SC.radius*1.01f) * -transform.forward.normalized);
            float nodeDist = Vector3.Distance(CurPath.Start.transform.position, CurPath.End.transform.position);
            float ourDist = Vector3.Distance(transform.position, CurPath.Start.transform.position);
            float percentage = 1f - (ourDist / nodeDist);
            Debug.Log("nodeDist: " + nodeDist.ToString("F4") + ", ourDist: " + ourDist.ToString("F4") + ", percentage: " + percentage.ToString("F4"));
        }
        if (GUI.Button(new Rect(0, 200, 100, 100), "check col"))
        {
            int ringLayerMask = LayerMask.GetMask("Lockpick Ring");
            Collider[] colliders = Physics.OverlapSphere(transform.position, SC.radius, ringLayerMask);
            foreach (Collider c in colliders)
            {
                Debug.Log("colliding with c: " + c.name);
            }
            if (colliders.Length == 0) Debug.Log("no collision");


        }*/
        /*if (GUI.Button(new Rect(0, 100, 100, 100), "toggle move"))
        {
            Moving = !Moving;
        }*/
        /* */


        /* if (GUI.Button(new Rect(0, 0, 100, 100), "1"))
         {
             startPos = transform.position;
             transform.position += new Vector3(0f, 1f, 0f);
         }
         if (GUI.Button(new Rect(0, 100, 100, 100), "2"))
         {
             int touchControlMask = LayerMask.GetMask("Lockpick Touch Control");
             RaycastHit[] hits = Physics.RaycastAll(transform.position, -transform.up, Mathf.Infinity, touchControlMask);
             if(hits.Length != 0)
             {
                 transform.position = hits[0].point;
             }
         }
         if (GUI.Button(new Rect(0, 200, 100, 100), "3"))
         {
             //int touchControlMask = LayerMask.GetMask("Lockpick Touch Control");
             int ringLayerMask = LayerMask.GetMask("Lockpick Ring");
             //RaycastHit hit;
             RaycastHit[] hits = Physics.SphereCastAll(transform.position, SC.radius*2f, transform.forward, Mathf.Infinity, ringLayerMask);
             if(hits.Length != 0)
             {
                 string s = "hits: ";
                 foreach(RaycastHit hit in hits)
                 {
                     s += hit.collider.name + ", ";
                 }
                 Debug.Log(s);
             }
             else
             {
                 Debug.Log("no hit");
             }
         }*/
        //reCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = 
        /*
        if (GUI.Button(new Rect(0, 100, 100, 100), "toggle move"))
        {
            Moving = !Moving;
        }
        if (GUI.Button(new Rect(0, 200, 100, 100), "goto start"))
        {
            Vector3 pos = Vector3.Lerp(CurPath.Start.transform.position, CurPath.End.transform.position, 0f);
            transform.position = pos;
        }
        if (GUI.Button(new Rect(0, 300, 100, 100), "goto end"))
        {
            Vector3 pos = Vector3.Lerp(CurPath.Start.transform.position, CurPath.End.transform.position, 1f);
            transform.position = pos;
        }*/
    }
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
       // Gizmos.DrawLine(transform.position, transform.position + 1f * transform.up);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + 1f * transform.forward);
        Gizmos.color = Color.red;
       // Gizmos.DrawLine(transform.position, transform.position + 1f * transform.right);

        /* Gizmos.color = Color.white;
         Gizmos.DrawLine(transform.position, transform.position + 1f * Vector3.up);
         Gizmos.color = Color.magenta;
         Gizmos.DrawLine(transform.position, transform.position + 1f * Vector3.forward);
         Gizmos.color = Color.cyan;
         Gizmos.DrawLine(transform.position, transform.position + 1f * Vector3.right);   */

    }
    void ChangeDir()
    {
        m_MoveDir = (m_MoveDir == eMoveDir.FORWARD ? eMoveDir.BACKWARD : eMoveDir.FORWARD);     
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Enter: " + collision.collider.name);        
    }
    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Exit: " + collision.collider.name);
    }
    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("Stay: " + collision.collider.name);
    }               

    /* private void OnGUI()
   {
       if (GUI.Button(new Rect(0, 0, 100, 100), "check col"))
       {
           int layerMask = LayerMask.GetMask("Lockpick Path Node");
           Collider[] colliders = Physics.OverlapSphere(transform.position, SC.radius, layerMask);
           foreach (Collider c in colliders)
           {
               Debug.Log("colliding with c: " + c.name);
               CurPath = c.GetComponent<PathNode>().Path;
           }
       }
   }*/
}
