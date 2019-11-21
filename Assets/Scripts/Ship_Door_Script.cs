using UnityEngine;

public class Ship_Door_Script : MonoBehaviour {

    public GameObject Trigger;
    public GameObject DoorTop;
    public GameObject DoorBottom;

    Animator DoorTopAnimation;
    Animator DoorBottomAnimation;

    void Start() {
        DoorTopAnimation = DoorTop.GetComponent<Animator > ();
        DoorBottomAnimation = DoorBottom.GetComponent<Animator > ();
    }

    void OnTriggerEnter(Collider coll) {
        if (coll .gameObject .tag == "Player") {
            SlideDoors(true);
        }
    }
    void OnTriggerExit(Collider coll) {
        if (coll .gameObject .tag == "Player") {
            SlideDoors(false);
        }
    }

    void SlideDoors(bool state)
    {
        DoorTopAnimation.SetBool ("slide", state);
        DoorBottomAnimation.SetBool ("slide", state);

        // true = open, false = close        
        if (state == true) SoundFXPlayer.Play("Door_Open");
        else SoundFXPlayer.Play("Door_Close");        
    }    
}