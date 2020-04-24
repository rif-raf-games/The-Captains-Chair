using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGamesDemo : MonoBehaviour
{
    private void Awake()
    {
        StaticStuff.SetOrientation(StaticStuff.eOrientation.LANDSCAPE, this.name);
    }
    public void OnClickParking()
    {
        SceneManager.LoadScene("ParkingDemo");
    }
    public void OnClickLockPicking()
    {
        SceneManager.LoadScene("LockPickingDemo");
    }
    public void OnClickRepair()
    {
        SceneManager.LoadScene("RepairDemo");
    }
}
