using Articy.The_Captain_s_Chair;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheCaptainsChair : MonoBehaviour
{
    public Door CaptainsDoor;

    public void OpenCaptainsDoor()
    {
        CaptainsDoor.Open();
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Welcome to The Captain's Chair!!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
