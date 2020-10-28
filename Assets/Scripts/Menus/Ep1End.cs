using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ep1End : MonoBehaviour
{
    public GameObject[] Menus;
    int CurMenu = 0;
    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i<Menus.Length; i++)
        {
            Menus[i].SetActive(false);
        }
        CurMenu = 0;
        Menus[0].SetActive(true);
    }

    void OnGUI()
    {
        if(GUI.Button(new Rect(Screen.width-100, Screen.height-100, 100, 100),"Next"))
        {
            Menus[CurMenu].SetActive(false);
            CurMenu = (CurMenu + 1) % Menus.Length;
            Menus[CurMenu].SetActive(true);
        }
    }
}
