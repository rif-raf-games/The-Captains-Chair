using UnityEngine;
using UnityEngine.UI;

public class Ship_HallDoor_Script : MonoBehaviour {

    public GameObject Trigger;
    public GameObject SlideDoor;

    public Animator Door_Slide_Control;


    public Text DebugText;

    void Start() {
        Door_Slide_Control = SlideDoor.GetComponent<Animator > ();
     
    }

    /// <summary>
    /// Gets a list of all objects within collision distance of the trigger collider
    /// </summary>
    /// <returns></returns>
    Collider[] GetColliders()
    {
        int characterMask = 1 << LayerMask.NameToLayer("NPC");
        characterMask |= (1 << LayerMask.NameToLayer("Player"));
        BoxCollider DoorCollider = GetComponent<BoxCollider>();
        Collider[] colliders = Physics.OverlapBox(DoorCollider.bounds.center, DoorCollider.bounds.extents, Quaternion.identity, characterMask);        
        return colliders;
    }
    void OnTriggerEnter(Collider coll) 
    {
       /// Debug.Log("********************" + this.name + " door OnTriggerEnter() BEGIN: " + coll.name);
        Collider[] colliders = GetColliders();
        string s = "**********Door.OnTriggerEnter() coll: " + coll.name + "\n";
        s += colliders.Length + "\n";
        foreach (Collider c in colliders) s += c.name + "\n";
       // Debug.Log(s);
        if (colliders.Length == 0 || colliders.Length == 1 && colliders[0] == coll)
        {   // if there's nothing within the trigger's collider box other than the object colliding with it
            // then the doors are ready to be opened
            //Debug.Log("no collisions other than self so call OpenDoor()");
            OpenDoor();
        }        
       // Debug.Log("********************" + this.name + " door OnTriggerEnter() END: " + coll.name);              
    }
    void OnTriggerExit(Collider coll) 
    {
        //Debug.Log("********************" + this.name + " door OnTriggerExit(): START" + coll.name);              
        Collider[] colliders = GetColliders();
        if (colliders.Length == 0)
        {   // If there's nothing within the trigger's collision box then go ahead and close
           // Debug.Log("no collisions so call CloseDoor()");
            CloseDoor();            
        }        
      //  Debug.Log("********************" + this.name + " door OnTriggerExit(): END" + coll.name);
    }
    
    public void OpenDoor()
    {       
        // The door animations are always on layer 0 so get it's information
        AnimatorStateInfo a0 = Door_Slide_Control.GetCurrentAnimatorStateInfo(0);
        float curTime = a0.normalizedTime;

        /*string name = "UNNOWN";
        if (a0.IsName("Door_Slide_Open")) name = "Door_Slide_Open";
        if (a0.IsName("Door_Slide_Close")) name = "Door_Slide_Close";
        if (a0.IsName("Door_Slide_Idle")) name = "Door_Slide_Idle";        
        Debug.Log("OPEN: " + name + ", " + a0.length + ", " + a0.normalizedTime.ToString("F3") + ", " + curTime.ToString("F3") + ", " + a0.speed + ", " + a0.speedMultiplier);*/

        // If the Abs(curTime) is less than .5f then it's in the middle of a Close animation.  In that case
        // we want to adjust the start time from 0 to that value so we can just reverse what's been done instead of 
        // starting from the beginning
        float startTime = 0f;
        if(Mathf.Abs(curTime) < .5f)
        {
            startTime = Mathf.Abs(curTime);
            Debug.Log("redoing Open startTime to " + startTime);
        }

        SoundFXPlayer.Play("Door_Open");
        Door_Slide_Control.SetFloat("Speed", 1.0f);
        Door_Slide_Control.Play("Door_Slide_Open", 0, startTime);

    }
    public void CloseDoor()
    {
        // The door animations are always on layer 0 so get it's information
        AnimatorStateInfo a0 = Door_Slide_Control.GetCurrentAnimatorStateInfo(0);
        float curTime = a0.normalizedTime;// % 1;

        /*string name = "UNNOWN";
        if (a0.IsName("Door_Slide_Open")) name = "Door_Slide_Open";
        if (a0.IsName("Door_Slide_Close")) name = "Door_Slide_Close";
        if (a0.IsName("Door_Slide_Idle")) name = "Door_Slide_01_Idle";        
        Debug.Log("CLOSE: " + name + ", " + a0.length + ", " + a0.normalizedTime.ToString("F3") + ", " + curTime.ToString("F3") + ", " + a0.speed + ", " + a0.speedMultiplier);*/

        // If the Abs(curTime) is less than .5f then it's in the middle of a Open animation.  In that case
        // we want to adjust the start time from .5 to .5 - (that value) so we can just reverse what's been done instead of 
        // starting from the beginning
        float startTime = .5f;
        if (Mathf.Abs(curTime) < .5f)
        {
            startTime = .5f - Mathf.Abs(curTime);
           // Debug.LogWarning("redoing close startTime to " + startTime);
        }

        SoundFXPlayer.Play("Door_Close");
        Door_Slide_Control.SetFloat("Speed", -1.0f);
        Door_Slide_Control.Play("Door_Slide_Open", 0, startTime);

    }

    private void Update()
    {
        AnimatorStateInfo a0 = Door_Slide_Control.GetCurrentAnimatorStateInfo(0);
        if(DebugText != null)
        {
            string name = "UNNOWN";
            if (a0.IsName("Door_Slide_Open")) name = "Door_Slide_Open";
            if (a0.IsName("Door_Slide_Close")) name = "Door_Slide_Close";
            if (a0.IsName("Door_Slide_Idle")) name = "Door_Slide_Idle";
            float curTime = a0.normalizedTime % 1;
            DebugText.text = name + ", " + a0.length + ", " + a0.normalizedTime.ToString("F3") + ", "  + curTime.ToString("F3") + ", " + a0.speed + ", " + a0.speedMultiplier;                                          
        }       
    }   
}