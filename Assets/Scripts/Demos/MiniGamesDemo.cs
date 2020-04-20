using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGamesDemo : MonoBehaviour
{
    private void Start()
    {
        StaticStuff.TrackEvent("mini_game_demo_start");
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
