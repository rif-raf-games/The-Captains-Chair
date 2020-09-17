using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairPiece : MonoBehaviour
{
    public Repair.eFluidType FluidType;    
    public Repair.eRepairPieceType Type;

    public List<int> OpenAngles = new List<int>();    

    public bool Movable = true;
    public bool ReachedOnPath = false; // this MIGHT want to be moved to a sub-class for terminals

    MiniGame.MiniGameTransformSave StartingTrans;

    // Start is called before the first frame update
    void Start()
    {
        StartingTrans = new MiniGame.MiniGameTransformSave(this.transform);        
    }

    public void ResetItem()
    {
        StartingTrans.ResetTransform(this.transform);
    }    
}

