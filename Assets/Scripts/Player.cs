using Articy.The_Captain_s_Chair;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Player : MonoBehaviour
{    
    public ArticyFlow ArticyFlow;

    //string LastNavMeshClicked = "";    
    private NavMeshAgent NavMeshAgent;
    public Vector3 CamOffset;

    public NavMeshSurface FloorNavMeshSurface;
    public NavMeshSurface[] ElevatorNavMeshSurfaces;
    bool TurnOnNavMeshes = false;

    Elevator SelectedElevator = null;

    private bool MovementBlocked = false;

    public Text DebugText;
    public GameObject DebugDestPos;
    //public bool CameraFollow = true;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(this.name + " OnTriggerEnter() other: " + other.name + ", layer: " + other.gameObject.layer);       
        if (other.gameObject.layer == LayerMask.NameToLayer("Location"))
        {
            ArticyReference colliderArtRef = other.gameObject.GetComponent<ArticyReference>();
            if (colliderArtRef != null)
            {
                Debug.Log("we collided with something that has an ArticyRef.  Now lets see what it is.");
                Dialogue dialogue = colliderArtRef.reference.GetObject() as Dialogue;
                if (dialogue != null)
                {
                    Debug.Log("we have a dialogue, so set the FlowPlayer to start on it");
                    ArticyFlow.StartConvo(dialogue);
                    NavMeshAgent.SetDestination(this.transform.position);
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
                    Debug.Log("Reached elevator StartPos");
                    //if(SelectedElevator.HasRideBegun() == false)
                    if(MovementBlocked == false)
                    {
                        Debug.Log("move player onto elevator");
                        FloorNavMeshSurface.enabled = false;
                        TurnOnNavMeshes = true;
                        Vector3 dest = other.gameObject.transform.parent.GetChild(1).position;                        
                        NavMeshAgent.SetDestination(dest);
                        DebugDestPos.transform.position = dest;
                        ToggleMovementBlocked(true);
                        
                    }
                    else
                    {
                        SelectedElevator.GetComponent<NavMeshSurface>().enabled = false;
                        TurnOnNavMeshes = true;
                        SelectedElevator = null;                        
                        ToggleMovementBlocked(false);
                    }                    
                }
                else
                {                    
                    Debug.Log("Reached elevator EndPos");                      
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
                Debug.Log("wrong elevator, keep going");
            }
        }        
    }
    
    public void ElevatorUpdate(Elevator caller)
    {
        if(caller == SelectedElevator)
        {
            NavMeshAgent.SetDestination(this.transform.position);
        }        
    }
    public void ElevatorDoneMoving(Elevator caller)
    {        
        if(caller == SelectedElevator)
        {
            NavMeshAgent.SetDestination(SelectedElevator.transform.GetChild(0).transform.position);
        }        
    }    
    
    private void LateUpdate()
    {
        Camera.main.transform.position = this.transform.position + CamOffset;
        if(TurnOnNavMeshes == true )
        {
            FloorNavMeshSurface.enabled = true;
            foreach (NavMeshSurface n in ElevatorNavMeshSurfaces) n.enabled = true;
        }        
    }
    // Start is called before the first frame update
    void Start()
    {
        NavMeshAgent = this.GetComponent<NavMeshAgent>();
        CamOffset = Camera.main.transform.position - this.transform.position;
        ToggleMovementBlocked(false);
    }

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
                NavMeshAgent.SetDestination(dest);
                DebugDestPos.transform.position = dest;
            }
        }

        DebugStuff();
    }
    
    public void ToggleMovementBlocked(bool val)
    {
        MovementBlocked = val;
    }

    void DebugStuff()
    {
        if (DebugText != null)
        {
            DebugText.text = NavMeshAgent.navMeshOwner.name + "\n";
            if (SelectedElevator == null) DebugText.text += "no SelectedElevator\n";
            else DebugText.text += "SelectedElevator: " + SelectedElevator.name + "\n";
            DebugText.text += "ClickedElevator: " + (SelectedElevator != null) + "\n";
            DebugText.text += "isPathStale: " + NavMeshAgent.isPathStale + "\n";
            DebugText.text += "pathStatus: " + NavMeshAgent.pathStatus.ToString() + "\n";
            DebugText.text += "isStopped: " + NavMeshAgent.isStopped + "\n";
            DebugText.text += "remainingDistance: " + NavMeshAgent.remainingDistance + "\n";
            DebugText.text += "MovementBlocked: " + MovementBlocked;
        }
    }
}
