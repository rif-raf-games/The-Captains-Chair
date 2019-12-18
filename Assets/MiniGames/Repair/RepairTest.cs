using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairTest : MonoBehaviour
{
    public Transform AnchorPoint;
    public Transform Piece;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(0,0,100,100), "feh"))
        {
            Piece.transform.position = AnchorPoint.transform.position;
        }
    }
}
