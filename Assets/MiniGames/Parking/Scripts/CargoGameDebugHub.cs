using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CargoGameDebugHub : MonoBehaviour
{
    int ButtonSize, ScreenLeftover;
    // Start is called before the first frame update
    void Start()
    {
        ButtonSize = Mathf.FloorToInt(Screen.height / 5);
        ScreenLeftover = Screen.height - (ButtonSize * 5);
        //Debug.Log("ButtonSize: " + ButtonSize + ", ScreenLeftOver: " + ScreenLeftover);        
    }

    private void OnGUI()
    {
        int x = Screen.width / 2 - ButtonSize / 2;
        if(GUI.Button(new Rect(x, ScreenLeftover/2, ButtonSize, ButtonSize), "Play All"))
        {
            //Debug.Log("Play All");
            SceneManager.LoadScene("CargoGame");
        }
        if (GUI.Button(new Rect(x, ScreenLeftover / 2 + ButtonSize, ButtonSize, ButtonSize), "Puzzle 01"))
        {
           // Debug.Log("Puzzle 01");
            SceneManager.LoadScene("Puzzle01Orig");
        }
        if (GUI.Button(new Rect(x, ScreenLeftover / 2 + ButtonSize * 2, ButtonSize, ButtonSize), "Puzzle 02"))
        {
            // Debug.Log("Puzzle 02");
            SceneManager.LoadScene("Puzzle02Orig");
        }
        if (GUI.Button(new Rect(x, ScreenLeftover / 2 + ButtonSize * 3, ButtonSize, ButtonSize), "Puzzle 03"))
        {
            // Debug.Log("Puzzle 03");
            SceneManager.LoadScene("Puzzle03Orig");
        }
        if (GUI.Button(new Rect(x, ScreenLeftover / 2 + ButtonSize * 4, ButtonSize, ButtonSize), "Puzzle 04"))
        {
            // Debug.Log("Puzzle 04");
            SceneManager.LoadScene("Puzzle04Orig");
        }
    }
}
