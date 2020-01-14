using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterEntity : MonoBehaviour
{    
    [Header("CharacterEntity")]        
    // Animation stuff
    public Animator Animator;
    Dictionary<string, float> AnimClipInfo = new Dictionary<string, float>();
    Vector3 LastPos = Vector3.zero;
    Vector3 DeltaPos = Vector3.zero;
    Vector3 ForwardDir = Vector3.zero;
    Vector3 LastForwardDir = Vector3.zero;
    Vector3 MoveDir = Vector3.zero;
    float WalkDir = 0f;
    float TurnSpeed = 0f;
    float Speed = 0f;

    protected NavMeshAgent NavMeshAgent;

    public GameObject EntityToFollow;
    public bool ShouldFollowEntity = false;
   
    // Start is called before the first frame update
    public virtual void Start()
    {
       // Debug.Log("Chracter Entity Start");        

        if (Animator != null)
        {
            Animator.enabled = false;
            RuntimeAnimatorController rac = Animator.runtimeAnimatorController;
            AnimationClip[] clips = rac.animationClips;
            foreach (AnimationClip ac in clips)
            {
                if (AnimClipInfo.ContainsKey(ac.name) == false) AnimClipInfo.Add(ac.name, ac.length);
            }
        }
        StartCoroutine(AnimStartDelay());
        LastPos = transform.position;

        NavMeshAgent = this.GetComponent<NavMeshAgent>();

        if(EntityToFollow != null)
        {
            ShouldFollowEntity = true;
        }
    }

    

    IEnumerator AnimStartDelay()
    {
        yield return new WaitForEndOfFrame();
        if (Animator != null) Animator.enabled = true;
    }
    public virtual void Update()
    {

    }

    public virtual void LateUpdate()
    {
        if (Animator != null)
        {   // This section handles the walking/turning            
            DeltaPos = transform.position - LastPos;
            MoveDir = transform.position - LastPos;
            ForwardDir = transform.forward;

            // determines whether or not we go to the WALK bit
            Speed = DeltaPos.magnitude * 10f;
            Animator.SetFloat("Speed", Speed);

            // determines how much of the WalkForward and WalkBackward we play
            WalkDir = Vector3.Angle(ForwardDir, MoveDir) / 180f; // 0 = forward, 1 = backward
            WalkDir *= 2; // 2*m_WalkDir: 0 = forward, 2 = backward
            WalkDir = 1 - WalkDir; // 1-(above) 1=forward, -1=backward
            Animator.SetFloat("Walk Dir", WalkDir);

            TurnSpeed = Vector3.SignedAngle(LastForwardDir, ForwardDir, Vector3.up);
            TurnSpeed /= 2f;
            Animator.SetFloat("Turn Speed", TurnSpeed);

            LastPos = transform.position;            
            LastForwardDir = ForwardDir;
        }

        if(EntityToFollow != null && ShouldFollowEntity == true)
        {
            NavMeshAgent.SetDestination(EntityToFollow.transform.position);
        }
    }

    public float PlayAnim(string anim)
    {       
        if(Animator != null)
        {
            //Debug.Log("PlayAnim: " + "Base Layer." + anim);
            Animator.Play("Base Layer." + anim);
            if (AnimClipInfo.ContainsKey(anim) == false)
            {
                Debug.LogError("This anim is not in the dict");
                return -1;
            }
            else
            {
                float time = AnimClipInfo[anim];
               // Debug.Log("time of anim: " + time);
                return time;
            }            
        }
        return -1f;
    }

    public void ToggleFollowEntity(bool val)
    {
        if (EntityToFollow != null) ShouldFollowEntity = val;        
    }

    public void SetNavMeshDest(Vector3 dest)
    {
        //Debug.Log("------------------- SetNavMeshDest(): " + dest.ToString("F2") +", on object: " + this.name);
        NavMeshAgent.SetDestination(dest);
    }

    public void StopNavMeshMovement()
    {
        // Debug.Log("------------------- StopNavMeshMovement()");
        SetNavMeshDest(this.transform.position);
    }    
    public bool GetShouldFollowEntity()
    {
        return ShouldFollowEntity;
    }
    public void SetShouldFollowEntity(bool val)
    {
        ShouldFollowEntity = val;
    }
    public float GetStoppingDist()
    {
        return NavMeshAgent.stoppingDistance;
    }
    public void SetStoppingDist(float dist)
    {
        NavMeshAgent.stoppingDistance = dist;
    }


    public bool NavMeshDone()
    {
        if (!NavMeshAgent.pathPending)
        {
            if (NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance)
            {
                if (!NavMeshAgent.hasPath || NavMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Update is called once per frame
    
}
