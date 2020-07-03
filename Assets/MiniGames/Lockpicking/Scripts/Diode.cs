using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Diode : MonoBehaviour
{
    enum eMoveDir { FORWARD, BACKWARD, NUM_DIRS };
    eMoveDir m_MoveDir = eMoveDir.FORWARD;

    public float StartSpeed = 1f;
    public float CurSpeed; 
    public Ring.LockpickRingPath CurPath;
    public GameObject LastPosition;
    SphereCollider SC;
    LockPicking LockPicking;
    bool IgnoreCollisions = false;
    //public Text DebugText;

    public bool Moving = false;
    public bool Evil = false;

    [Header("Debug")]
    public PathNode DebugStartNode;

    private void Awake()
    {
        LastPosition = new GameObject("Last Position_" + this.GetHashCode());            ;
        LastPosition.transform.parent = this.transform.parent;
    }
    // Start is called before the first frame update
    void Start()
    {                
        SC = GetComponent<SphereCollider>();                
    }

    public void SetLockPickingComponent(LockPicking lp)
    {
        this.LockPicking = lp;
    }

    Coroutine CamLerpRoutine;
    void SetNewPath(Ring.LockpickRingPath path)
    {
       // Debug.Log("SetNewPath: " + path.Ring.name);
        CurPath = path;
        this.transform.parent = CurPath.Ring.transform;
        LastPosition.transform.parent = CurPath.Ring.transform;

        if(Evil == false)
        {
            if (CurPath.Ring != LockPicking.Rings[LockPicking.Rings.Count - 1])
            {
                if (CamLerpRoutine != null)
                {
                    StopCoroutine(CamLerpRoutine);
                    CamLerpRoutine = null;
                }
                //StartCoroutine(LerpCamPos());
            }
        }                     
    }

    IEnumerator LerpCamPos()
    {        
        string sub = CurPath.Ring.name.Substring(CurPath.Ring.name.Length - 2, 2);
        int index = int.Parse(sub);        
        float startY = Camera.main.transform.position.y;
        float endY = LockPicking.RingCamYs[index];        
        float lerpStartTime = Time.time;
        float lerpPercentage = 0f;
        yield return new WaitForEndOfFrame();
        while(lerpPercentage < 1f)
        {
            float lerpTime = Time.time - lerpStartTime;
            lerpPercentage = lerpTime / 1f;
           // Debug.Log(startY + " , " + endY + ",  " + lerpPercentage);
            float latestY = Mathf.Lerp(startY, endY, lerpPercentage);
            if(lerpPercentage >= 1f)
            {
                latestY = endY;
            }
           // Debug.Log(latestY);
            Camera.main.transform.position = new Vector3(0f, latestY, 0f);
            yield return new WaitForEndOfFrame();
        }
    }

    public void ResetDiodeForGame(PathNode startNode)
    {
        CurSpeed = StartSpeed;
        Moving = true;        
        m_MoveDir = eMoveDir.FORWARD;
        //Debug.Log("a set m_MoveDir to Forward SetStartNode()");
        SetNewPath(startNode.Path);
        transform.position = startNode.transform.position + .05f * startNode.transform.forward;
        transform.LookAt(CurPath.End.transform);
    }   
    
    
    public void DiodeFixedUpdate()
    {       
        //Debug.Log(this.name + " FixedUpdate()");
        if (Moving == true )
        {            
            CurPath.Start.transform.LookAt(CurPath.End.transform);
            CurPath.End.transform.LookAt(CurPath.Start.transform);
            Vector3 moveDir;
            if (m_MoveDir == eMoveDir.FORWARD)
            {                
                moveDir = CurPath.Start.transform.forward;                
            }
            else
            {
                moveDir = CurPath.End.transform.forward;
            }
            CurSpeed = (Evil == false ? LockPicking.AdjustDiodeSpeed(StartSpeed) : StartSpeed);
            //CurSpeed = LockPicking.AdjustDiodeSpeed(StartSpeed);
            transform.position = transform.position + (moveDir * Time.deltaTime * (CurSpeed / 10f));
            Collider[] colliders = Physics.OverlapSphere(transform.position, SC.radius);
            if(Evil==false)
            {
                foreach(Collider c in colliders)
                {
                    Diode d = c.gameObject.GetComponent<Diode>();
                    if(d != null && d.Evil == true)
                    {
                        Debug.Log("you hit an evil diode");
                        LockPicking.HitEvilDiode(d);
                    }
                }
            }

            if (IgnoreCollisions == true)
            {                
               // colliders = Physics.OverlapSphere(transform.position, SC.radius);               
                if (colliders.Length == 1 && colliders[0].gameObject == this.gameObject) IgnoreCollisions = false;
            }

            if(IgnoreCollisions == false )
            {
                int layerMask = LayerMask.GetMask("Lockpick Ring");
                colliders = Physics.OverlapSphere(transform.position, SC.radius, layerMask);
                if (colliders.Length != 0)
                {                    
                    transform.position = LastPosition.transform.position;                   
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
                        break;
                    }
                }
            }                        
            LastPosition.transform.position = transform.position;            
        }       
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Diode: " + this.name + " OnTriggerEnter(): " + other.name);
        PathNode pathNode = other.GetComponent<PathNode>();
        if (pathNode != null) LockPicking.CheckDeathNode(this, pathNode);
        if(Evil == false)
        {
            Gate g = other.GetComponent<Gate>();
            if (g != null) LockPicking.CollectGate(g);
        }
        /*if (Evil == true)
        {
            //Debug.LogError("evil diode collided with a trigger: " + other.name);
            if(pathNode != null && LockPicking.IsDeathNode(pathNode) == true)
            {
                ResetDiodeForGame(DebugStartNode);
            }                        
        }
        else
        {
            if (pathNode != null) LockPicking.CheckDeathNode(pathNode);
            Gate g = other.GetComponent<Gate>();
            if (g != null) LockPicking.CollectGate(g);            
        }        */
    }    

    void ChangeDir()
    {
        m_MoveDir = (m_MoveDir == eMoveDir.FORWARD ? eMoveDir.BACKWARD : eMoveDir.FORWARD);
       // Debug.Log(this.name + " ChangeDir() to " + m_MoveDir);
    }

    public void OnDrawGizmos()
    {
        /*Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + 1f * transform.up);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + 1f * transform.forward);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + 1f * transform.right);*/

        /* Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + 1f * Vector3.up);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + 1f * Vector3.forward);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + 1f * Vector3.right);   */

    }
}
