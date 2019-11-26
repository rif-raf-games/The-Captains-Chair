﻿using Articy.The_Captain_s_Chair;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CCPlayer : MonoBehaviour
{    
    ArticyFlow ArticyFlow;               

    private NavMeshAgent NavMeshAgent;
    NavMeshSurface[] FloorNavMeshSurfaces;
    NavMeshSurface[] ElevatorNavMeshSurfaces;
    bool TurnOnNavMeshes = false;
    Vector3 CamOffset;
    Animator PlayerAnimator;

    Elevator SelectedElevator = null;

    private bool MovementBlocked = false;
    
    public GameObject DebugDestPos;

    // Start is called before the first frame update
    void Start()
    {
        NavMeshAgent = this.GetComponent<NavMeshAgent>();       
        ArticyFlow = FindObjectOfType<ArticyFlow>();
        m_Anim = GetComponent<Animator>();
        m_Anim.enabled = false;
        StartCoroutine(AnimStartDelay());
        m_Rbody = GetComponent<Rigidbody>();

        GameObject[] floors = GameObject.FindGameObjectsWithTag("FloorNavMesh");
        FloorNavMeshSurfaces = new NavMeshSurface[floors.Length];
        for (int i = 0; i < floors.Length; i++) FloorNavMeshSurfaces[i] = floors[i].GetComponent<NavMeshSurface>();
        GameObject[] elevators = GameObject.FindGameObjectsWithTag("ElevatorNavMesh");
        ElevatorNavMeshSurfaces = new NavMeshSurface[elevators.Length];
        for (int i = 0; i < elevators.Length; i++) ElevatorNavMeshSurfaces[i] = elevators[i].GetComponent<NavMeshSurface>();

        m_LastPos = transform.position;
        Debug.Log("start pos: " + transform.position.ToString("F4"));
        CamOffset = Camera.main.transform.position - this.transform.position;
        ToggleMovementBlocked(false);
    }
   
    IEnumerator AnimStartDelay()
    {
        yield return new WaitForEndOfFrame();
        m_Anim.enabled = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        StaticStuff.PrintTriggerEnter(this.name + " OnTriggerEnter() other: " + other.name + ", layer: " + other.gameObject.layer);       
        if (other.gameObject.layer == LayerMask.NameToLayer("Location"))
        {
            ArticyReference colliderArtRef = other.gameObject.GetComponent<ArticyReference>();
            if (colliderArtRef != null)
            {
                StaticStuff.PrintTriggerEnter("we collided with something that has an ArticyRef.  Now lets see what it is.");
                Dialogue dialogue = colliderArtRef.reference.GetObject() as Dialogue;
                if (dialogue != null)
                {
                    StaticStuff.PrintTriggerEnter("we have a dialogue, so set the FlowPlayer to start on it and see what happens");
                    ArticyFlow.StartConvo(dialogue);
                    //NavMeshAgent.SetDestination(this.transform.position);
                }
                else
                {
                    Debug.LogWarning("not sure what to do with this type yet: " + colliderArtRef.reference.GetObject().GetType());
                }
            }
            else
            {
                Debug.LogWarning("null ArticyReference on the thing we collided with.");
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Elevator Collider"))
        {            
            if(other.gameObject.GetComponentInParent<Elevator>() == SelectedElevator )
            {                
                if(other.gameObject.name.Contains("Start"))
                {
                    StaticStuff.PrintTriggerEnter("Reached elevator StartPos");
                    //if(SelectedElevator.HasRideBegun() == false)
                    if(MovementBlocked == false)
                    {
                        StaticStuff.PrintTriggerEnter("move player onto elevator");                        
                        foreach(NavMeshSurface n in FloorNavMeshSurfaces) n.enabled = false;
                        TurnOnNavMeshes = true;
                        Vector3 dest = other.gameObject.transform.parent.GetChild(1).position;
                        //NavMeshAgent.SetDestination(dest);
                        SetNavMeshDest(dest);
                        DebugDestPos.transform.position = dest;
                        ToggleMovementBlocked(true);
                        
                    }
                    else
                    {
                        StaticStuff.PrintTriggerEnter("Player moved off elevator, back to normal movement");
                        SelectedElevator.transform.GetChild(1).GetComponent<SphereCollider>().enabled = true;
                        SelectedElevator.GetComponent<NavMeshSurface>().enabled = false;
                        TurnOnNavMeshes = true;
                        SelectedElevator = null;
                        ToggleMovementBlocked(false);
                    }                    
                }
                else
                {
                    StaticStuff.PrintTriggerEnter("Reached elevator EndPos so start movement");                    
                    SelectedElevator.transform.GetChild(1).GetComponent<SphereCollider>().enabled = false;
                    int newFloor = SelectedElevator.BeginMovement();   
                    foreach(NavMeshSurface n in ElevatorNavMeshSurfaces)
                    {
                        Elevator e = n.GetComponent<Elevator>();
                        if( e != SelectedElevator && e.ShouldMoveAsWell(newFloor) == true )
                        {
                            e.BeginMovement();
                        }
                    }
                }                            
            }
            else
            {
                StaticStuff.PrintTriggerEnter("wrong elevator, keep going");
            }
        }        
    }

    void SetNavMeshDest(Vector3 dest)
    {
        NavMeshAgent.SetDestination(dest);
    }

    public void StopNavMeshMovement()
    {                
        SetNavMeshDest(this.transform.position);
    }
    
    public void ElevatorUpdate(Elevator caller)
    {
        if(caller == SelectedElevator)
        {
            StopNavMeshMovement();
        }        
    }
    public void ElevatorDoneMoving(Elevator caller)
    {        
        if(caller == SelectedElevator)
        {
            Debug.Log("Elevator done moving, so move player back to entrance");            
            SetNavMeshDest(SelectedElevator.transform.GetChild(0).transform.position);            
        }        
    }

    Vector3 m_LastPos = Vector3.zero;
    Vector3 m_DeltaPos = Vector3.zero;
    Vector3 m_ForwardDir = Vector3.zero;
    Vector3 m_MoveDir = Vector3.zero;
    float m_AngleDiff = 0f;
    private void LateUpdate()
    {        
        Camera.main.transform.position = this.transform.position + CamOffset;
        if(TurnOnNavMeshes == true )
        {
            foreach (NavMeshSurface n in FloorNavMeshSurfaces) n.enabled = true;
            foreach (NavMeshSurface n in ElevatorNavMeshSurfaces) n.enabled = true;
        }

        if(m_Anim != null)
        {
            /*if (m_DeltaPos.magnitude > 0f)
            {
                Debug.Log("why we greater?: " + m_DeltaPos.magnitude);
                Debug.Log("pos: " + transform.position.ToString("F4") + ", m_DeltaPos: " + m_DeltaPos.ToString("F4"));
            }*/
            m_DeltaPos = transform.position - m_LastPos;
            m_MoveDir = transform.position - m_LastPos;
            m_ForwardDir = transform.forward;
            m_AngleDiff = Vector3.Angle(m_ForwardDir, m_MoveDir);            
            m_Anim.SetFloat("Delta Position Magnitude", m_DeltaPos.magnitude);
            m_Anim.SetFloat("Angle Diff", m_AngleDiff);
            m_LastPos = transform.position;
        }        
    }

    public Animator m_Anim;
    public Rigidbody m_Rbody;

    // Update is called once per frame
    void Update()
    {        
        if (MovementBlocked == false && Input.GetMouseButtonDown(0))
        {            
            int maskk = 1 << LayerMask.NameToLayer("Floor");            
            maskk |= (1 << LayerMask.NameToLayer("Elevator"));            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, maskk))
            {
                Vector3 dest = Vector3.zero;
                string layerClicked = LayerMask.LayerToName(hit.collider.gameObject.layer);
                if(layerClicked.Equals("Floor"))
                {
                    dest = hit.point;
                    SelectedElevator = null;
                }
                else if(layerClicked.Equals("Elevator"))
                {
                    Elevator e = hit.collider.GetComponent<Elevator>();
                    if(e != SelectedElevator)
                    {
                        Debug.Log("clicked on NEW elevator");
                        dest = hit.collider.transform.GetChild(0).position;
                        SelectedElevator = hit.collider.GetComponent<Elevator>();
                        
                    }                                        
                }
                //Debug.Log("Set Dest b");
                //NavMeshAgent.SetDestination(dest);
                SetNavMeshDest(dest);
                DebugDestPos.transform.position = dest;
            }
        }        

        DebugStuff();
    }
    
    public void ToggleMovementBlocked(bool val)
    {
        MovementBlocked = val;
    }

    public Vector3 offset = new Vector3(0f, 0f, 0f);   
    public Text DebugText;
    void DebugStuff()
    {               
        Debug.DrawRay(transform.position + offset, m_ForwardDir, Color.blue);        
        Debug.DrawRay(transform.position + offset, m_MoveDir.normalized, Color.yellow);                
        if (DebugText != null)
        {
            DebugText.text = "";           
            DebugText.text += "m_DeltaPos: " + m_DeltaPos.ToString("F3") + ", mag: " + m_DeltaPos.magnitude + "\n";
            DebugText.text += "m_ForwardDir: " + m_ForwardDir.ToString("F3") + "\n";
            DebugText.text += "m_AngleDiff: " + m_AngleDiff + "\n";            
            return;
            DebugText.text += NavMeshAgent.navMeshOwner.name + "\n";            
            DebugText.text += "autoBraking: " + NavMeshAgent.autoBraking + "\n";
            DebugText.text += "autoRepath: " + NavMeshAgent.autoRepath + "\n";
            DebugText.text += "destination: " + NavMeshAgent.destination + "\n";
            DebugText.text += "hasPath: " + NavMeshAgent.hasPath + "\n";
            DebugText.text += "isActiveAndEnabled: " + NavMeshAgent.isActiveAndEnabled + "\n";
            DebugText.text += "isOnNavMesh: " + NavMeshAgent.isOnNavMesh + "\n";
            DebugText.text += "isPathStale: " + NavMeshAgent.isPathStale + "\n";
            DebugText.text += "isStopped: " + NavMeshAgent.isStopped + "\n";
            DebugText.text += "nextPosition: " + NavMeshAgent.nextPosition + "\n";
            DebugText.text += "pathEndPosition: " + NavMeshAgent.pathEndPosition + "\n";
            DebugText.text += "pathPending: " + NavMeshAgent.pathPending + "\n";            
            DebugText.text += "pathStatus: " + NavMeshAgent.pathStatus.ToString() + "\n";            
            DebugText.text += "remainingDistance: " + NavMeshAgent.remainingDistance + "\n";
            DebugText.text += "steeringTarget: " + NavMeshAgent.steeringTarget + "\n";
            DebugText.text += "stoppingDistance: " + NavMeshAgent.stoppingDistance + "\n";
            DebugText.text += "updateRotation: " + NavMeshAgent.updateRotation + "\n";
            // if (SelectedElevator == null) DebugText.text += "no SelectedElevator\n";
            //else DebugText.text += "SelectedElevator: " + SelectedElevator.name + "\n";
            //DebugText.text += "ClickedElevator: " + (SelectedElevator != null) + "\n";
            //DebugText.text += "MovementBlocked: " + MovementBlocked;
        }
    }
}

/*
 * bool lastPath = false;
    int frameNum = 0;
 * if (NavMeshAgent.hasPath == false && lastPath == true)
        {
            Debug.Log("stopped. remain: " + NavMeshAgent.remainingDistance + ", stopping: " + NavMeshAgent.stoppingDistance +
                ", diff: " + (NavMeshAgent.remainingDistance - NavMeshAgent.stoppingDistance) + ", frameNum: " + frameNum);
            // Debug.Log("close enough?: " + (NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance));
        }
        lastPath = NavMeshAgent.hasPath;
        // Debug.Log("1: " + NavMeshAgent.hasPath);
        if (NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance)
        {
            Debug.Log("less.  hasPath: " + NavMeshAgent.hasPath + ", frameNum: " + frameNum);
        }

        if (NavMeshAgent.hasPath == true)
        { // apparently this will never be true
            if (NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance)
            {
                Debug.Log("stop");
                //NavMeshAgent.isStopped = true;
            }
        }
        if(NavMeshAgent.remainingDistance <= 0f)
        {
            Debug.Log("remaingDistance is 0" + ", frameNum: " + frameNum);
        }
        frameNum++;*/
