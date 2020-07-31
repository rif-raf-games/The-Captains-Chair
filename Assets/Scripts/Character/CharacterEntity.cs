using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Articy.Unity;
using UnityEngine.UI;

public class CharacterEntity : MonoBehaviour
{    
    [Header("CharacterEntity")]        
    // Animation stuff    
    public ArticyRef ArticyEntityReference;
    public ArticyRef ArticyAIReference;
    protected Animator Animator;
    public int CurFloor = 0;

    GameObject EntityToFollow;
    bool ShouldFollowEntity = false;    
          
    Dictionary<string, float> AnimClipInfo = new Dictionary<string, float>();
    Vector3 LastPos = Vector3.zero;
    Vector3 DeltaPos = Vector3.zero;
    Vector3 ForwardDir = Vector3.zero;
    Vector3 LastForwardDir = Vector3.zero;
    Vector3 MoveDir = Vector3.zero;
    float WalkDir = 0f;
    float TurnSpeed = 0f;
    float Speed = 0f;
    float LastSpeed = 0f;

    protected NavMeshAgent NavMeshAgent;

    [Header("Debug")]
    public Text DebugText;
    public void ToggleNavMeshAgent(bool val)
    {
       // Debug.Log("ToggleNavMeshAgent(): " + val);
        this.NavMeshAgent.enabled = val;
        if (val == true) NavMeshAgent.SetDestination(transform.position);
    }
    public void SetEntityToFollow(GameObject followEntity)
    {
       // Debug.Log("SetEntityToFollow: " + followEntity.name);
        EntityToFollow = followEntity;
        ShouldFollowEntity = true;
    }

    public bool IsFollowingCaptain()
    {
        if(ShouldFollowEntity == true && EntityToFollow == FindObjectOfType<CCPlayer>().gameObject)
        {
            return true;
        }
        return false;
    }    

    public void TMP_ResetFollow()
    {
        if(ShouldFollowEntity == true && EntityToFollow == null)
        {
            EntityToFollow = FindObjectOfType<CCPlayer>().gameObject;
        }
    }

    // Start is called before the first frame update
    public virtual void Start()
    {        
        // Debug.Log("Chracter Entity Start");                       
        Animator = GetComponent<Animator>();
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
        NavMeshAgent.SetDestination(transform.position);

        // get the floor if necessary
        CalcCurrentFloor();
    }

    
    public IEnumerator CheckPostTeleportTransparency()
    {
        Debug.LogError(this.name + ": CheckPostTeleportTransparency()");
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        CalcCurrentFloor();
        int captainFloor = FindObjectOfType<CCPlayer>().GetCurFloor();
        Debug.LogError(this.name + ": new Current Floor: " + CurFloor + ", captainFloor: " + captainFloor);
        float alpha;
        if (captainFloor == CurFloor) alpha = 1f;
        else alpha = 0f;
        Renderer[] childRs = this.GetComponentsInChildren<Renderer>();
        foreach (Renderer mr in childRs)
        {
            List<Material> mrMaterials = new List<Material>();
            mr.GetMaterials(mrMaterials);
            foreach (Material material in mrMaterials)
            {
                //  Debug.Log("material in this NPC: " + material.name);
                //material.shader = UnityEngine.Shader.Find("RifRafStandard");                
                material.color = new Color(material.color.r, material.color.g, material.color.b, alpha);
            }
        }
    }
    public void CalcCurrentFloor()
    {
       // if (this.name.Contains("Stu")) Debug.LogError(this.name +" CalcCurrentFloor()");
        if (GetComponent<CapsuleCollider>() != null)
        {
            Vector3 center = GetComponent<CapsuleCollider>().bounds.center;
            Vector3 rayPos = center + new Vector3(0f, 0f, 1000f);
            int layerMask = LayerMask.GetMask("Ship Level");
            RaycastHit hit;
            Physics.Raycast(rayPos, center - rayPos, out hit, Mathf.Infinity, layerMask);
            if (hit.collider != null)
            {
              //  if (this.name.Contains("Stu")) Debug.LogError(this.name + ": hit a floor: " + hit.collider.name);
                SetFloor(hit.collider.GetComponent<ShipLevel>().Level);
            }
        }
        else Debug.LogError(this.name + ": No CapsuleCollider on this NPC: " + this.name);
    }

    public int GetCurFloor()
    {
        return CurFloor;
    }
    /*private void OnGUI()
    {
        if(this.name.Contains("Captain"))
        {
            if (GUI.Button(new Rect(Screen.width - 100, Screen.height / 2 - 150, 100, 100), "set"))
            {
                this.transform.rotation = DebugPlayerRot;
            }
        }
        
    }*/
    public Quaternion DebugPlayerRot = Quaternion.identity;
    public void SetFloor(int newFloor, string floorName = "")
    {
        /*if(this.name.Contains("Stu")) Debug.LogError(this.name + ": setting " + this.name + "'s floor to: " + newFloor);
        if(this.name.Contains("Captain"))
        {
            Debug.LogError("**** transform: " + this.transform.position.ToString("F3") + ", rot: " + this.transform.rotation.ToString("F2"));
            Debug.LogError("*********************************************CAPTAIN setting " + this.name + "'s floor to: " + newFloor + " after coliding with: " + floorName + ": " + Time.time);
            DebugPlayerRot = this.transform.rotation;
            int x = 5;
            x++;
        }*/
        CurFloor = newFloor;
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
    {   // this should only be called when the CE is under NavMesh control        
        if (Animator != null)
        {   // This section handles the walking/turning            
            DeltaPos = transform.position - LastPos;
            MoveDir = transform.position - LastPos;
            ForwardDir = transform.forward;

            // determines whether or not we go to the WALK bit
            if (DeltaPos.y >= .01f) Speed = 0f;
            else Speed = DeltaPos.magnitude * 10f;
            Animator.SetFloat("Vertical", Speed);
            if(DebugText != null)
            {
                DebugText.text = this.name + "-DeltaPos.magnitude: " + DeltaPos.magnitude.ToString("F2") + ", MoveDir: " + MoveDir.ToString("F2") + ", FowardDir: " + ForwardDir.ToString("F2") + ", Speed: " + Speed.ToString("F2");
            }

            // determines how much of the WalkForward and WalkBackward we play
            /*WalkDir = Vector3.Angle(ForwardDir, MoveDir) / 180f; // 0 = forward, 1 = backward
            WalkDir *= 2; // 2*m_WalkDir: 0 = forward, 2 = backward
            WalkDir = 1 - WalkDir; // 1-(above) 1=forward, -1=backward
            Animator.SetFloat("Walk Dir", WalkDir);

            TurnSpeed = Vector3.SignedAngle(LastForwardDir, ForwardDir, Vector3.up);
            TurnSpeed /= 2f;
            Animator.SetFloat("Turn Speed", TurnSpeed);     */              

            LastPos = transform.position;            
            LastForwardDir = ForwardDir;
            LastSpeed = Speed;
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

    public void StopAnim()
    {        
        if(Animator == null ) { Debug.LogError("No Animator to StopAnim: " + this.name); return; }
        Animator.SetTrigger("BackToIdleTrigger");
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
        if (NavMeshAgent.enabled == true && !NavMeshAgent.pathPending)
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
