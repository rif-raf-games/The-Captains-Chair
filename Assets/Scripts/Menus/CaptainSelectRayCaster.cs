using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptainSelectRayCaster : MonoBehaviour
{
    Collider SelectedCaptain = null;
    public GameObject Portal;
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("CaptainSelectRayCaster.OnTriggerEnter() other: " + other.name);
        SelectedCaptain = other;
        Portal.SetActive(true);
    }
    private void OnTriggerExit(Collider other)
    {
       // Debug.Log("CaptainSelectRayCaster.OnTriggerExit() other: " + other.name);
        SelectedCaptain = null;
        Portal.SetActive(false);
    }
    
    public string GetSelectedCaptainName()
    {
        if(SelectedCaptain == null) { Debug.LogError("Trying to select a captain but none is chosen"); return "none"; }
        return SelectedCaptain.name;
    }
    
}
