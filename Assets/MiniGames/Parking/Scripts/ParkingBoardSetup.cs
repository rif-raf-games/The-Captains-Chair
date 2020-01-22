using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingBoardSetup : MonoBehaviour
{
    public void SetBoard()
    {
        Parking parking = GameObject.FindObjectOfType<Parking>();
        //Debug.Log("ok, set the board");
        parking.SetBoardPiecesInEditor();
    }
}
