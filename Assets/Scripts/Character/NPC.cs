using Articy.Unity;
using Articy.The_Captain_s_Chair.Features;
using Articy.The_Captain_s_Chair;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : CharacterEntity
{    
    Character_Action_List_Template CurBehavior;
    List<Character_Action_List_Template> Behaviors = new List<Character_Action_List_Template>();
    TextMesh BarkText;
    
    // Start is called before the first frame update
    public override void Start()
    {
        //Debug.Log(this.name + " NPC.Start()");
        base.Start();
        
        BarkText = GetComponentInChildren<TextMesh>();
        if (BarkText == null) { Debug.LogError("There's no TextMesh on this NPC: " + this.gameObject.name); return; }
        ToggleBarkText(false);
       
        AI_Template t = ArticyAIReference.GetObject() as AI_Template;
        if (t == null) { Debug.LogWarning("This NPC: " + this.name + " does not have an AI_Template on it's ArticyAIReference"); return; }
        List<ArticyObject> children = t.Children;
        foreach(ArticyObject child in children)
        {
            Behaviors.Add(child as Character_Action_List_Template);
        }
        //Debug.Log(this.name + " has " + Behaviors.Count + " Behaviors.");
        RestartBehavior();
    }    

    public bool CheckForAIChange()
    {
        Character_Action_List_Template validBehavior = GetValidBehavior();
        if(validBehavior != CurBehavior)
        {
            Debug.Log(this.name + ": CurBehavior " + CurBehavior.DisplayName + " needs to change to this behavior: " + validBehavior.DisplayName);
            GetComponent<BehaviorFlowPlayer>().StopBehavior();
            RestartBehavior();
        }
        else
        {
            Debug.Log(this.name + ": CurBehavior " + CurBehavior.DisplayName + " doesn't need to change but we should check inside it");
            bool shouldChange = GetComponent<BehaviorFlowPlayer>().CheckIfAIShouldChange();
            if (shouldChange == true)
            {
                Debug.Log(this.name + " should change");
                GetComponent<BehaviorFlowPlayer>().StopBehavior();
                RestartBehavior();
            }
            else Debug.Log(this.name + " should NOT change");
        }
        return false;
    }

    public void RestartBehavior()
    {
        //Debug.Log(this.name + ": RestartBehavior()");
        CurBehavior = GetValidBehavior();
        StartCurrentBehavior();
    }

    Character_Action_List_Template GetValidBehavior()
    {
        Character_Action_List_Template validBehavior = null;

        foreach (Character_Action_List_Template cat in Behaviors)
        {
            List<InputPin> inPins = cat.InputPins;
            foreach (InputPin pin in inPins)
            {
                ArticyScriptCondition conditionScript = pin.Text;
                bool b = conditionScript.CallScript();
                //Debug.Log(pin.Text.RawScript + ": " + b);
                if(b == true)
                {
                    if (validBehavior != null) Debug.LogWarning("You have more than one valid Behavior on this NPC's AI: " + this.name);
                    validBehavior = cat;
                    //validList = cat.Template.Character_Action_List_Feature;
                }
            }            
        }

        return validBehavior;
    }

    void StartCurrentBehavior()
    {
        if(CurBehavior == null) { Debug.LogError("trying to start a null behavior on this npc: " + this.name); return; }        
        GetComponent<BehaviorFlowPlayer>().StartBehaviorFlow(CurBehavior, this.gameObject);               
    }

    public void EndCAL(int numNodes, bool idleOrFollow)
    {
        if (this.name.Contains("Follow Test")) Debug.Log(this.name + ": NPC EndCAL() numNodes: " + numNodes + " idleOrFollow: " + idleOrFollow);
        if (CurBehavior == null) { Debug.LogError("trying to get output pins on a null behavior on this npc: " + this.name); return; }
        List<OutputPin> outputPins = CurBehavior.OutputPins;
        foreach(OutputPin pin in outputPins)
        {
            ArticyScriptInstruction instructionScrip = pin.Text;
            instructionScrip.CallScript();
            //Debug.Log(pin.Text.RawScript);
        }
        if(numNodes == 1 && idleOrFollow)
        {
            if (this.name.Contains("Follow Test"))  Debug.Log(this.name + ": we've got a 1 node Idle or Follow so just bail and don't get into the infinite loop");
        }
        else
        {
            RestartBehavior();
        }        
    }

    public void SetBarkText(string text)
    {
        //Debug.LogWarning("Setting text: " + text + " on object: " + this.name);
        BarkText.text = text;
    }
    public void ToggleBarkText(bool val)
    {
        BarkText.GetComponent<MeshRenderer>().enabled = val;
        if(val == true)
        {
            BarkText.transform.LookAt(Camera.main.transform);
            BarkText.transform.Rotate(0f, 180, 0f);
        }
    }

  /*  public GameObject DestObject;
    private void OnGUI()
    {
        if(this.name.Contains("Mall"))
        {
            if(GUI.Button(new Rect(0,Screen.height/2-25, 50, 50), "OMall"))
            {
                Debug.Log("go to the new spot Omally");                
                SetNavMeshDest(DestObject.transform.position);
            }
            if (GUI.Button(new Rect(0, Screen.height / 2 + 25, 50, 50), "stop"))
            {
                Debug.Log("FUCKING STOP");
                SetNavMeshDest(transform.position);
                SetIsStopped(true);
                Rigidbody rb = GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;

            }
        }
        if(this.name.Contains("Stu"))
        {
            if (GUI.Button(new Rect(Screen.width-50, Screen.height - 50, 50, 50), "Stu"))
            {
                Debug.Log("go to the new spot Stu");
                SetStoppingDist(0f);
                SetNavMeshDest(DestObject.transform.position);
            }
        }
    }*/

   // public GameObject DestSphere;
    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        /*if (DebugText == null) return;
        DebugText.text = this.name + "\n";
        if (CurBehavior == null) DebugText.text += "null CurBehavior\n";
        else DebugText.text += "CurBehavior: " + CurBehavior.DisplayName + "\n";        
        if (NavMeshAgent.navMeshOwner == null) DebugText.text += "no navMeshOwner\n";
        else DebugText.text += NavMeshAgent.navMeshOwner.name + "\n";
        DebugText.text += "autoBraking: " + NavMeshAgent.autoBraking + "\n";
        DebugText.text += "autoRepath: " + NavMeshAgent.autoRepath + "\n";
        DebugText.text += "destination: " + NavMeshAgent.destination + "\n";
        if (DestSphere != null) DestSphere.transform.position = NavMeshAgent.destination;
        DebugText.text += "hasPath: " + NavMeshAgent.hasPath + "\n";
        DebugText.text += "isActiveAndEnabled: " + NavMeshAgent.isActiveAndEnabled + "\n";
        DebugText.text += "isOnNavMesh: " + NavMeshAgent.isOnNavMesh + "\n";
        DebugText.text += "isPathStale: " + NavMeshAgent.isPathStale + "\n";
        DebugText.text += "isStopped: " + NavMeshAgent.isStopped + "\n";
        DebugText.text += "nextPosition: " + NavMeshAgent.nextPosition + "\n";
        DebugText.text += "pathEndPosition: " + NavMeshAgent.pathEndPosition + "\n";
        DebugText.text += "pathPending: " + NavMeshAgent.pathPending + "\n";
        DebugText.text += "pathStatus: " + NavMeshAgent.pathStatus.ToString() + "\n";
        DebugText.text += "remainingDistance: " + NavMeshAgent.remainingDistance + "\n";
        DebugText.text += "steeringTarget: " + NavMeshAgent.steeringTarget + "\n";
        DebugText.text += "stoppingDistance: " + NavMeshAgent.stoppingDistance + "\n";
        DebugText.text += "updateRotation: " + NavMeshAgent.updateRotation + "\n";*/
    }
    public override void LateUpdate()
    {
        base.LateUpdate();
    }
}
