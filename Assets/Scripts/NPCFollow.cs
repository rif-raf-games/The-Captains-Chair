using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCFollow : MonoBehaviour
{
    public CCPlayer ToFollow;
    private NavMeshAgent NavMeshAgent;

    private void Start()
    {
        NavMeshAgent = this.GetComponent<NavMeshAgent>();
    }
    void LateUpdate()
    {
        NavMeshAgent.SetDestination(ToFollow.transform.position);
    }
}
