using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockpickBoardSetup : MonoBehaviour
{
    public void SetBoard()
    {
        LockPicking lockPicking = GameObject.FindObjectOfType<LockPicking>();
        //Debug.Log("ok, set the board");
        lockPicking.ProcessBoardSetup();
    }
}
