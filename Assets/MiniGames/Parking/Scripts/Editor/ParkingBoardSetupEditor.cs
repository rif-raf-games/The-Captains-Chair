using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ParkingBoardSetup))]
public class ParkingBoardSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ParkingBoardSetup mySetup = (ParkingBoardSetup)target;

        if (GUILayout.Button("Set Board", GUILayout.Width(100f)))
        {
            //Debug.Log("clicked Set Board button");
            mySetup.SetBoard();
        }
    }
}
