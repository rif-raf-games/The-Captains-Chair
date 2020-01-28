using UnityEngine;

public class Ship_Door_Script : MonoBehaviour {

    public GameObject Trigger;
    public GameObject Door_01;
    public GameObject Door_02;
    public GameObject Door_03;

    Animator Door_01_Animation;
    Animator Door_02_Animation;
    Animator Door_03_Animation;

    void Start() {
        Door_01_Animation = Door_01.GetComponent<Animator > ();
        Door_02_Animation = Door_02.GetComponent<Animator > ();
        Door_03_Animation = Door_03.GetComponent<Animator > ();
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
        Door_01_Animation.SetBool ("slide", state);
        Door_02_Animation.SetBool ("slide", state);
        Door_03_Animation.SetBool ("slide", state);

        // true = open, false = close        
        if (state == true) SoundFXPlayer.Play("Door_Open");
        else SoundFXPlayer.Play("Door_Close");        
    }    
}