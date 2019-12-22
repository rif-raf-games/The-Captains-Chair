using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairPiece : MonoBehaviour
{
    public Repair.eFluidType FluidType;    
    public Repair.eRepairPieceType Type;

    public List<int> OpenAngles = new List<int>();
    public List<int> AdjAngles = new List<int>();

    public bool ReachedOnPath = false; // this MIGHT want to be moved to a sub-class for terminals


    // Start is called before the first frame update
    void Start()
    {
        
    }



    // Update is called once per frame
    /*void CheckDirs()
    {
        if (Type == Repair.eRepairPieceType.BLOCKER) return;
        Vector3 pos = new Vector3(transform.position.x, Repair.PieceAnchorHeightValToUse, transform.position.z);//transform.position + new Vector3(0f, Repair.MODEL_HEIGHT / 2f, 0f);
        foreach (int angle in OpenAngles)
        {
            int rawY = Mathf.RoundToInt(transform.localRotation.eulerAngles.y);
            int adjY = 360 - rawY;
            int dir = angle + adjY;
            if (dir > 360) dir = dir - 360;
            if (dir < 0) dir += 360;            
            Color color = Repair.Colors[(dir - 30) / 60];

            Quaternion q = Quaternion.AngleAxis(dir, -Vector3.up);
            Vector3 rayDir = q * Vector3.right;
          //  Debug.DrawRay(pos, rayDir * 4, color);
        }
    }*/
}

