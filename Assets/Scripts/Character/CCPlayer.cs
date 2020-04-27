using Articy.The_Captain_s_Chair;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CCPlayer : CharacterEntity
{
    public enum eControlType { POINT_CLICK, STICK };
    eControlType CurControlType;

    ArticyFlow CaptainArticyFlow;           
    
    Elevator[] Elevators;
    Elevator SelectedElevator = null;

    private bool MovementBlocked = false;
    private bool DealingWithElevator = false;
    private bool WaitingForFollowersOnElevator = false;
    TheCaptainsChair CaptainsChair;
    Rigidbody RigidBody;

    [Header("CCPlayer")]
    public float MoveSpeed = 650f;
    public float RotSpeed = 3f;
    public FixedJoystick Joystick;
    CharacterEntity Loop;    

    [Header("Player Debug")]
    public bool DEBUG_BlockMovement = false;
    public GameObject DebugDestPos;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();        

        CaptainArticyFlow = GetComponent<ArticyFlow>();
        CaptainsChair = FindObjectOfType<TheCaptainsChair>();
        RigidBody = GetComponent<Rigidbody>();

        Elevators = FindObjectsOfType<Elevator>();
        ToggleMovementBlocked(false);
        GameObject go = GameObject.Find("Loop");
        if(go != null)
        {
            Loop = go.GetComponent<CharacterEntity>();
        }
        ToggleControlType(eControlType.STICK);
    }

    public void ToggleControlType(eControlType newType)
    {
        CurControlType = newType;
        SetupForControlType(CurControlType);
    }

    public eControlType GetControlType() { return CurControlType; }
    void SetupForControlType(eControlType controlType)
    {
        switch (controlType)
        {
            case eControlType.POINT_CLICK:
                ToggleNavMeshAgent(true);
                RigidBody.isKinematic = true;
                Animator.SetFloat("Vertical", 0f);
                Animator.SetFloat("Horizontal", 0f);
                break;
            case eControlType.STICK:
                ToggleNavMeshAgent(false);
                RigidBody.isKinematic = false;
                Animator.SetFloat("Speed", 0f);
                Animator.SetFloat("Walk Dir", 0f);
                Animator.SetFloat("Turn Speed", 0f);
                break;
        }
    }
    
    
    public void SetPlayerControlStartDialogue()
    {
        //Debug.Log("CCPlayer.StartDialogue()");
        SetupForControlType(eControlType.POINT_CLICK);
    }
    public void SetPlayerControlEndDialogue()
    {
       // Debug.Log("CCPlayer.EndDialogue()");
        if (CurControlType == eControlType.STICK)
        {
            SetupForControlType(eControlType.STICK);
        }
    }
    
    
    public override void LateUpdate()
    {
        if(CurControlType == eControlType.POINT_CLICK || DealingWithElevator == true ||
            CaptainArticyFlow.CurArticyState == ArticyFlow.eArticyState.DIALOGUE)
        {
            base.LateUpdate();
        }        
    }

    /*public CapsuleCollider testCap;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Debug.Log(testCap.center);
        Vector3 center = testCap.transform.TransformPoint(testCap.center);
        Gizmos.DrawWireSphere(center, .5f);
        Vector3 start = center + testCap.height / 2 * testCap.transform.forward;
        Gizmos.DrawWireSphere(start, .5f);
        Vector3 end = center - testCap.height / 2 * testCap.transform.forward;
        Gizmos.DrawWireSphere(end, .5f);
    }*/
    // Update is called once per frame
    public override void Update()
    {
        base.Update();                
        if (MovementBlocked == false && DEBUG_BlockMovement == false)
        {
            Ray ray;
            RaycastHit hit;
            int mask;
            
            // first check to see if we have clicked on an interactive triggers
            if (Input.GetMouseButtonDown(0))
            {
                mask = LayerMask.GetMask("ITrigger");
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                {
                   // Debug.Log("clicked a trigger");
                    // we clicked on an ITrigger, so figure out which collider we need to check, then check if the Player is inside
                    mask = LayerMask.GetMask("Player");
                    GameObject container = hit.collider.transform.GetChild(0).gameObject;
                    Collider[] colliders = null;                
                    if(container.GetComponent<BoxCollider>() != null)
                    {                        
                        BoxCollider box = container.GetComponent<BoxCollider>();
                        colliders = Physics.OverlapBox(box.bounds.center, box.size / 2, container.transform.rotation, mask);
                    }
                    else if(container.GetComponent<SphereCollider>() != null)
                    {
                        SphereCollider sc = container.GetComponent<SphereCollider>();
                        colliders = Physics.OverlapSphere(sc.bounds.center, sc.radius, mask);
                    }
                    else if(container.GetComponent<CapsuleCollider>() != null)
                    {
                        CapsuleCollider cc = container.GetComponent<CapsuleCollider>();
                        Vector3 center = cc.transform.TransformPoint(cc.center);
                        Vector3 start = center + cc.height / 2 * cc.transform.forward;
                        Vector3 end = center - cc.height / 2 * cc.transform.forward;
                        colliders = Physics.OverlapCapsule(start, end, cc.radius, mask);
                    }
                    if(colliders == null) { Debug.LogError("You have the wrong kind of collider on the child of this ITrigger: " + hit.collider.name); return; }
                  //  Debug.Log("num colliders within the ITrigger captain area: " + colliders.Length);
                    if(colliders.Length == 1)
                    {   // one collider means the Player is within, so get the fragment going
                        ArticyReference triggerArtRef = hit.collider.GetComponent<ArticyReference>();
                        Dialogue dialogue = triggerArtRef.reference.GetObject() as Dialogue;                        
                        Stage_Directions_Container sdc = triggerArtRef.reference.GetObject() as Stage_Directions_Container;
                        if (dialogue != null)
                        {                            
                            CaptainArticyFlow.CheckIfDialogueShouldStart(dialogue, container.gameObject);
                        }
                        else if(sdc != null)
                        {
                            CaptainArticyFlow.SendToStageDirections(sdc);
                        }
                    }
                    else if(colliders.Length > 1)
                    {
                        Debug.LogError("Why do we have more than one collider on this ITrigger thing: " + hit.collider.name);
                    }
                }
            }
                        
            if (CurControlType == eControlType.STICK)
            {   // thumbstick control
                Rigidbody rbody = GetComponent<Rigidbody>();
                float moveX, moveZ;
                float inputH, inputV;
                
                if(Joystick != null)
                {
                    inputH = Joystick.Horizontal;
                    inputV = Joystick.Vertical;
                }
                else
                {
                    inputH = Input.GetAxis("Horizontal");
                    inputV = Input.GetAxis("Vertical");                    
                }
                inputH = -inputH;
                inputV = -inputV;

                float val = new Vector3(Mathf.Abs(inputH), Mathf.Abs(inputV)).magnitude;
                Animator.SetFloat("Vertical", val);
                Animator.SetFloat("Horizontal", inputH);

                moveX = inputH * MoveSpeed * Time.deltaTime;
                moveZ = inputV * MoveSpeed * Time.deltaTime;
                rbody.velocity = new Vector3(moveX, 0, moveZ);

                // note - some old stuff is copied below

                Vector3 direction = rbody.velocity;
                Vector3 newDir = Vector3.RotateTowards(transform.forward, direction, RotSpeed * Time.deltaTime, 0.0f);
                Quaternion newRot = Quaternion.LookRotation(newDir);
                transform.rotation = newRot;

                
            }
            else
              {   // point 'n click
                  if ( Input.GetMouseButtonDown(0) )
                  {
                      mask = 1 << LayerMask.NameToLayer("Floor");
                      mask |= (1 << LayerMask.NameToLayer("Elevator"));
                      ray = Camera.main.ScreenPointToRay(Input.mousePosition);                      
                      if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                      {
                          Vector3 dest = Vector3.zero;
                          string layerClicked = LayerMask.LayerToName(hit.collider.gameObject.layer);
                          if (layerClicked.Equals("Floor"))
                          {
                              // Debug.Log("layerClicked: " + layerClicked + " hit: " + hit.collider.gameObject.name);
                              dest = hit.point;
                              SelectedElevator = null;
                          }
                          else if (layerClicked.Equals("Elevator"))
                          {
                              //s Debug.Log("layerClicked: " + layerClicked + " hit: " + hit.collider.gameObject.name);
                              dest = hit.point;
                              if (hit.collider.gameObject.name.Contains("Lift")) SelectedElevator = hit.collider.GetComponent<Elevator>();
                              else SelectedElevator = hit.collider.GetComponentInParent<Elevator>();
                              if (SelectedElevator == null) Debug.LogError("Clicked on an Elevator with no Elevator component.");
                          }
                          SetNavMeshDest(dest);
                          if (DebugDestPos != null) DebugDestPos.transform.position = dest;
                      }
                  }
              }
          }        

          if (WaitingForFollowersOnElevator == true)
          {
              if(Loop.NavMeshDone())
              {
                 // Debug.Log("Loop is ready to rock");
                  StartElevatorRide();
              }
          }          
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Ignore Trigger")) { StaticStuff.PrintTriggerEnter(this.name + "Collided with an Ignore Trigger trigger, so bail"); return; }
        if (other.gameObject.layer == LayerMask.NameToLayer("Room")) { StaticStuff.PrintTriggerEnter(this.name + " This is a Room collider " + other.name + " on the Player, so bail and let the RoomCollider.cs handle it"); return; }
        StaticStuff.PrintTriggerEnter(this.name + " CCPlayer.OnTriggerEnter() other: " + other.name + ", layer: " + other.gameObject.layer);
        ArticyReference colliderArtRef = other.gameObject.GetComponent<ArticyReference>();
        if (colliderArtRef != null)
        {
            StaticStuff.PrintTriggerEnter("we collided with something that has an ArticyRef.  Now lets see what it is.");
            Dialogue dialogue = colliderArtRef.reference.GetObject() as Dialogue;
            Ambient_Trigger at = colliderArtRef.reference.GetObject() as Ambient_Trigger;
            Stage_Directions_Container sdc = colliderArtRef.reference.GetObject() as Stage_Directions_Container;
            if (dialogue != null)
            {
                StaticStuff.PrintTriggerEnter("we have a dialogue, so set the FlowPlayer to start on it and see what happens");
                CaptainArticyFlow.CheckIfDialogueShouldStart(dialogue, other.gameObject);
            }
            else if (at != null)
            {
                StaticStuff.PrintTriggerEnter("We have an Ambient_Trigger, so lets see if we're going to commit to it or not");
                AmbientTrigger ambientTrigger = other.GetComponent<AmbientTrigger>();
                if (ambientTrigger == null) { Debug.LogError("No AmbientTrigger component on this collider: " + other.name); return; }
                ambientTrigger.ProcessAmbientTrigger(at);
            }
            else if(sdc != null)
            {
                StaticStuff.PrintTriggerEnter("We have a Stage_Directions_Container, so lets see which of those we're gonna play");
                CaptainArticyFlow.SendToStageDirections(sdc);
            }
            else
            {
                Debug.LogWarning("not sure what to do with this type yet: " + colliderArtRef.reference.GetObject().GetType());
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Elevator Collider"))
        {
            if (CurControlType == eControlType.STICK || other.gameObject.GetComponentInParent<Elevator>() == SelectedElevator)
            {                
                if (other.gameObject.name.Contains("Start"))
                {
                    StaticStuff.PrintTriggerEnter("Reached elevator StartPos");                    
                    if (MovementBlocked == false)
                    {
                        StaticStuff.PrintTriggerEnter("move player onto elevator");
                        DealingWithElevator = true;
                        if (CurControlType == eControlType.STICK)
                        {
                            SelectedElevator = other.gameObject.GetComponentInParent<Elevator>();
                            SetupForControlType(eControlType.POINT_CLICK);
                        }
                        Vector3 dest = other.gameObject.transform.parent.GetChild(0).position;
                        SetNavMeshDest(dest);
                        if (DebugDestPos != null) DebugDestPos.transform.position = dest;
                        ToggleMovementBlocked(true);
                        if (IsLoopFollowing() == true)
                        {
                            WaitingForFollowersOnElevator = true;
                            Loop.SetShouldFollowEntity(false);
                            Loop.SetNavMeshDest(other.gameObject.transform.parent.GetChild(1).position);
                        }
                    }                    
                }
                else if (other.gameObject.name.Contains("End"))
                {
                    if (MovementBlocked == true)
                    {                     
                        StaticStuff.PrintTriggerEnter("Player moved off elevator, back to normal movement");
                        DealingWithElevator = false;
                        SetupForControlType(CurControlType);
                        SelectedElevator.transform.GetChild(0).GetComponent<SphereCollider>().enabled = true;
                        SelectedElevator = null;
                        ToggleMovementBlocked(false);
                        transform.parent = null;
                        if (Loop != null) Loop.transform.parent = null;
                    }                    
                }
                else
                {
                    StaticStuff.PrintTriggerEnter("Reached elevator EndPos so start movement");
                    if (WaitingForFollowersOnElevator == true)
                    {
                        //Debug.Log("We're ready to ride the elevator but we're still waiting for Loop");
                        return;
                    }
                    else
                    {
                        StartElevatorRide();
                    }
                }
            }
            else
            {
                StaticStuff.PrintTriggerEnter("wrong elevator, keep going");
            }
        }
        else
        {
            StaticStuff.PrintTriggerEnter("We've collided into something that doesn't have an Articy Ref and isn't an elevator so find out what's up. " + other.name);
        }
    }

    void StartElevatorRide()
    {
        ToggleNavMeshAgent(false);
        transform.parent = SelectedElevator.transform;
        if (WaitingForFollowersOnElevator == true)
        {
            //WaitingForFollowersOnElevator = false;
            Loop.ToggleNavMeshAgent(false);
            Loop.transform.parent = SelectedElevator.transform;
        }
        SelectedElevator.transform.GetChild(0).GetComponent<SphereCollider>().enabled = false;
        int newFloor = SelectedElevator.BeginMovement();
        foreach (Elevator e in Elevators)
        {
            if (e != SelectedElevator && e.ShouldMoveAsWell(newFloor) == true)
            {
                e.BeginMovement();
            }
        }
    }

    public void ElevatorDoneMoving(Elevator caller)
    {
        if (caller == SelectedElevator)
        {
            // Debug.Log("Elevator done moving, so move player back to entrance");
            ToggleNavMeshAgent(true);
            SetNavMeshDest(SelectedElevator.transform.GetChild(3).transform.position);
            if (WaitingForFollowersOnElevator == true)
            {
                WaitingForFollowersOnElevator = false;
                Loop.ToggleNavMeshAgent(true);
                Loop.SetShouldFollowEntity(true);
            }
        }
    }

    public bool IsInFreeRoam()
    {
        return (CaptainArticyFlow.IsInFreeRoam());
    }
    bool IsLoopFollowing()
    {
        return (Loop != null && Loop.IsFollowingCaptain() == true);
    }

    public void ToggleMovementBlocked(bool val)
    {
        // Debug.Log("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ToggleMovementBlocked() val: " + val);        
        MovementBlocked = val;
    }
    public bool IsMovementBlocked()
    {
        return MovementBlocked;
    }

    public bool IsSelectedElevator(Elevator caller)
    {
        return caller == SelectedElevator;
    }
    public void ElevatorUpdate(Elevator caller)
    {
        if (caller == SelectedElevator)
        {
            StopNavMeshMovement();
        }
    }
    void DebugStuff()
    {               
       // Debug.DrawRay(transform.position + offset, m_ForwardDir, Color.blue);        
        //Debug.DrawRay(transform.position + offset, m_MoveDir.normalized, Color.yellow);                
        if (DebugText != null)
        {
                        
            DebugText.text = "";
            DebugText.text += "MovementBlocked: " + MovementBlocked.ToString() + "\n";
            DebugText.text += "Loop following? " + IsLoopFollowing() + "\n";
            DebugText.text += "WaitingForFollowersOnElevator: " + WaitingForFollowersOnElevator + "\n";

            /*DebugText.text += "m_DeltaPos: " + m_DeltaPos.ToString("F3") + "\n";
            DebugText.text += "m_Speed: " + m_Speed + "\n";
            DebugText.text += "m_ForwardDir: " + m_ForwardDir.ToString("F3") + "\n";
            DebugText.text += "m_ForwardVsMoveDirDiff: " + m_WalkDir + "\n";
            DebugText.text += "90f - m_ForwardVsMoveDirDiff: " + (90f - m_WalkDir) + "\n";            
            //DebugText.text += "m_LastRot.y: " + m_LastRot.y + ", curRot.y: " + transform.rotation.eulerAngles.y + ", so \n";
            DebugText.text += "m_TurnSpeed: " + m_TurnSpeed.ToString("F3") + "\n";
            if (m_TurnSpeed < 0f) DebugText.text += "turning left" + "\n";
            else if (m_TurnSpeed > 0f) DebugText.text += "turning right" + "\n";
            else DebugText.text += "no turn" + "\n";
            DebugText.text += "num clips: " + m_Anim.GetCurrentAnimatorClipInfoCount(0) + "\n";*/

            /*DebugText.text += "remainingDistance: " + NavMeshAgent.remainingDistance + "\n";
            DebugText.text += "stoppingDistance: " + NavMeshAgent.stoppingDistance + "\n";
            DebugText.text += "hasPath: " + NavMeshAgent.hasPath + "\n";
            DebugText.text += "velocity: " + NavMeshAgent.velocity.sqrMagnitude + "\n";
            DebugText.text = "MovementBlocked: " + MovementBlocked.ToString();*/

            /*DebugText.text += NavMeshAgent.navMeshOwner.name + "\n";            
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
            DebugText.text += "updateRotation: " + NavMeshAgent.updateRotation + "\n";*/
            // if (SelectedElevator == null) DebugText.text += "no SelectedElevator\n";
            //else DebugText.text += "SelectedElevator: " + SelectedElevator.name + "\n";
            //DebugText.text += "ClickedElevator: " + (SelectedElevator != null) + "\n";
            //DebugText.text += "MovementBlocked: " + MovementBlocked;
            //DebugText.text = "";
        }
    }
}
/*Vector3 forwardDir = transform.forward;
                Vector3 moveDir = rbody.velocity;
                float dirDiff = Vector3.Angle(forwardDir, moveDir);
                float rot = dirDiff * RotSpeed * Time.deltaTime;
                transform.Rotate(0f, rot, 0f);*/

/*  moveX = inputH * Time.deltaTime;
  float rot = moveX * TurnSpeedNew;
  transform.Rotate(0, rot, 0);

  Rigidbody rbody = GetComponent<Rigidbody>();
  moveZ = inputV * Time.deltaTime;
  rbody.velocity = moveZ * transform.forward * MoveSpeed;*/

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
