using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.Unity;
using Articy.Unity.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionHint : MonoBehaviour
{
    public ArticyRef MissionHints;
    public Text HintText;
    public Button ResetMiniGameButton;

    Decision_Fragment MissionHintsFrag;
    ArticyObject HintsFlowStart;
    
    public void Init()
    {
        MissionHintsFrag = MissionHints.GetObject() as Decision_Fragment;        
        List<IInputPin> inputPins = MissionHintsFrag.GetInputPins();
        InputPin inputPin = inputPins[0] as InputPin;
        List<IOutgoingConnection> outCons = inputPin.GetOutgoingConnections();
        OutgoingConnection outCon = outCons[0] as OutgoingConnection;
        HintsFlowStart = outCon.Target;
        //Debug.LogError("type: " + HintsFlowStart.GetType() + ", articyType: " + HintsFlowStart.GetArticyType());
    }

    public void ToggleResetMiniGameButton(bool isActive)
    {
       // Debug.Log("ToggleResetMiniGameButton() isActive: " + isActive);
        ResetMiniGameButton.gameObject.SetActive(isActive);
    }
    public void SetupHint()
    {
     //   Debug.Log("SetupHint()");        
        //ArticyGlobalVariables.Default.Episode_01.Scene = 1;
        ArticyObject CurHint = HintsFlowStart;
        
        int numCheck = 0;
        
        while(numCheck < 50 )
        {
            numCheck++;
            List<ArticyObject> validArticyObjects = GetValidArticyObjects(CurHint);
            if (validArticyObjects.Count == 0)
            {
               // Debug.LogError("count is zero so we have our hint");
                break;
            }
            else
            {
                int choice = UnityEngine.Random.Range(0, validArticyObjects.Count);
                CurHint = validArticyObjects[choice];
            }
        }
        
        FlowFragment ff = CurHint as FlowFragment;
        string hint = ff.Text;
       // Debug.Log("we have our hint and it's: " + hint);
        HintText.text = hint;

       // List<ArticyObject> validArticyObjects = GetValidArticyObjects(HintsFlowStart);
       //  Debug.LogError("num valid articy objects: " + validArticyObjects.Count);

    }

    // monote - this is redundant fromthe articy flow debug so combine during pre-prod
    List<ArticyObject> GetValidArticyObjects(ArticyObject curAO)
    {
        List<ArticyObject> validBranches = new List<ArticyObject>();
        Jump jump = curAO as Jump;
        if (jump != null)
        {
            validBranches.Add(jump.Target);
        }
        else
        {
            List<IOutputPin> oPins = (curAO as IOutputPinsOwner).GetOutputPins();
            OutputPin cpoOutputPin = (oPins[0] as OutputPin);
            // Debug.LogWarning("Executing this script: " + cpoOutputPin.Text.RawScript);
            cpoOutputPin.Text.CallScript();
            // now evaluate all the targets to see which ones are valid
            validBranches = new List<ArticyObject>();
            // Debug.LogWarning("this pin has " + cpoOutputPin.Connections.Count + " connections.");
            foreach (OutgoingConnection outCon in cpoOutputPin.Connections)
            {
                ArticyObject target = outCon.Target;
                List<IInputPin> iPins = (target as IInputPinsOwner).GetInputPins();
                if (iPins.Count != 1) Debug.LogWarning("You should only have 1 input pin on dialogue nodes: " + target.TechnicalName);
                InputPin targetInputPin = iPins[0] as InputPin;

                bool val = targetInputPin.Text.CallScript();
                // Debug.LogWarning("This target's input pin conditional: " + targetInputPin.Text.RawScript + " and it's eval is: " + val);
                if (val == true) validBranches.Add(target);
            }
        }
        return validBranches;
    }
}
