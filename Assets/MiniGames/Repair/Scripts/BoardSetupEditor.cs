using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BoardSetup))]
public class BoardSetupEdit : Editor
{
    public override void OnInspectorGUI()
    {        
        BoardSetup mySetup = (BoardSetup)target;

        if (GUILayout.Button("Set Board", GUILayout.Width(100f)))
        {
            //Debug.Log("clicked Set Board button");
            mySetup.SetBoard();
        }
    }
}
