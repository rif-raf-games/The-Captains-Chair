using Articy.The_Captain_s_Chair;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    public ArticyFlow ArticyFlow;

    private NavMeshAgent NavMeshAgent;
    private Vector3 CamOffset;

    private bool MovementBlocked = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(this.name + " OnTriggerEnter() other: " + other.name);

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
    private void LateUpdate()
    {
        Camera.main.transform.position = this.transform.position + CamOffset;
    }
    public void ToggleMovementBlocked(bool val)
    {
        MovementBlocked = val;
    }
}
