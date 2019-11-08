using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Articy.Unity;
using Articy.The_Captain_s_Chair;

public class MRLabPlayer : MonoBehaviour
{
    NavMeshAgent NavMeshAgent;
    private Vector3 CamOffset;
    public ArticyFlowPlayer FlowPlayer;

    private bool MovementBlocked = false;
    public void ToggleMovementBlocked(bool val)
    {
        MovementBlocked = val;
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(this.name + " OnTriggerEnter() other: " + other.name);

        ArticyReference colliderArtRef = other.gameObject.GetComponent<ArticyReference>();
        if (colliderArtRef == null)
        {
            //Debug.LogWarning("null ArticyReference on the thing we collided with.");
            Debug.LogWarning("null ArticyReference on the thing we collided with.");
        }
        else
        {
            //Debug.Log("we connected with something that has an ArticyRef.  Now lets see if it's an NPC.");
            Debug.Log("we collided with something that has an ArticyRef.  Now lets see what it is.");
            Dialogue dialogue = colliderArtRef.reference.GetObject() as Dialogue;
            if (dialogue != null)
            {
                Debug.Log("we have a dialogue, so set the FlowPlayer to start on it");                
                FlowPlayer.StartOn = dialogue;
                NavMeshAgent.SetDestination(this.transform.position);
            }
            else
            {
                Debug.LogWarning("not sure what to do with this type yet: " + colliderArtRef.reference.GetObject().GetType());
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        NavMeshAgent = this.GetComponent<NavMeshAgent>();
        CamOffset = Camera.main.transform.position - this.transform.position;
    }

    private void LateUpdate()
    {
        Camera.main.transform.position = this.transform.position + CamOffset;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("MovementBlocked: " + MovementBlocked);
        if (MovementBlocked == false && Input.GetMouseButtonDown(0))
        {
            LayerMask mask = LayerMask.GetMask("Floor");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                Vector3 dest = hit.point;
                NavMeshAgent.SetDestination(dest);
            }
        }
    }
}
