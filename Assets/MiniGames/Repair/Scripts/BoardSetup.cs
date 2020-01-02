using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class BoardSetup : MonoBehaviour
{    
    public void SetBoard()
    {
        Repair repair = GameObject.FindObjectOfType<Repair>();
        //Debug.Log("ok, set the board");
        repair.SetBoardPiecesInEditor();
    }
}
