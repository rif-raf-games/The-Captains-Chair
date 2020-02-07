using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LockpickBoardSetup))]
public class LockpickBoardSetupEditor : Editor
{    
    public override void OnInspectorGUI()
    {
        LockpickBoardSetup mySetup = (LockpickBoardSetup)target;

        if (GUILayout.Button("Set Board", GUILayout.Width(100f)))
        {
            //Debug.Log("clicked Set Board button");
            mySetup.SetBoard();
        }
    }
}
