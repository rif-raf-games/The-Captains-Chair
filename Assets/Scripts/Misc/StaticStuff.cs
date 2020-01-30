﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class StaticStuff 
{
    static ArticyFlow ArticyFlowToPrint;
    static public void SetCaptainsChair(ArticyFlow articyFlowToPrint)
    {
        ArticyFlowToPrint = articyFlowToPrint;
    }
    // Start is called before the first frame update
    static public void PrintFlowPaused( string s, ArticyFlow articyFlowCaller)
    {
        if (ArticyFlowToPrint != null && (ArticyFlowToPrint == articyFlowCaller))
        {
            Debug.Log("caller: " + articyFlowCaller.name + ": " + s);
        }            
    }
    static public void PrintFlowBranchesUpdate(string s, ArticyFlow articyFlowCaller)
    {
        if (ArticyFlowToPrint != null && (ArticyFlowToPrint == articyFlowCaller))
        {
             
             Debug.Log("caller: " + articyFlowCaller.name + ": " + s);
        }

    }
    static public void PrintUI(string s)
    {
       // Debug.Log(s);
    }

    static public void PrintTriggerEnter(string s)
    {
       // Debug.Log(s);
    }

    static public void PrintRepairPath(string s)
    {
        //Debug.Log(s);
    }

    static public void PrintCAL(string s)
    {

    }
}
