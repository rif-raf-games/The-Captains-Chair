using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wanderer : NPC
{
    WanderManager WanderManager;
    public enum eWanderStates { IDLE, MOVING };
    eWanderStates WanderState;

    WanderPoint CurWanderPoint;

    float IdleTime;
    float IdleTimer;
    float MaxIdleTime = 5f;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();        
        WanderManager = transform.parent.GetComponent<WanderManager>();

        int start = Random.Range(0, 2);
        if (start == 0) StartIdleState();
        else StartWalkState();        
    }

    void StartIdleState()
    {
        WanderState = eWanderStates.IDLE;
        IdleTime = Random.Range(2f, MaxIdleTime);
        IdleTimer = 0f;
    }

    void StartWalkState()
    {
        WanderState = eWanderStates.MOVING;
        CurWanderPoint = WanderManager.GetWanderPoint(this.gameObject);
        SetNavMeshDest(CurWanderPoint.transform.position);
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        switch(WanderState)
        {
            case eWanderStates.IDLE:
                IdleTimer += Time.deltaTime;
                if(IdleTimer >= IdleTime)
                {
                    StartWalkState();
                }
                break;
            case eWanderStates.MOVING:
                if(NavMeshDone())
                {
                    StartIdleState();
                    WanderManager.ReleaseWanderPoint(CurWanderPoint);
                    CurWanderPoint = null;
                }
                break;
        }
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
    }
}
