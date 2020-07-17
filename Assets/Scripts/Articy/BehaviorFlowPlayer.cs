using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Articy.The_Captain_s_Chair.Features;
using Articy.The_Captain_s_Chair;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.AI;

public class BehaviorFlowPlayer : MonoBehaviour
{
    public Text DebugText;
    public Text DebugText2;

    //ArticyFlow CaptainArticyFlow;
    StageDirectionPlayer StageDirectionPlayer;
    CamFollow CamFollow;
    Dictionary<CharacterEntity, EntitySaveData> CurCALEntitySaveData = new Dictionary<CharacterEntity, EntitySaveData>();
    List<Character_Action_Template> CurActions = new List<Character_Action_Template>();
    List<ActionState> ActionsStates = new List<ActionState>();
    Character_Action_List_Template CurBehavior;
    GameObject CurCallingObject; 
    Coroutine CurBehaviorCoroutine;
    bool IdleOrFollow = false;
    
    private void Start()
    {
        CamFollow = FindObjectOfType<CamFollow>();
        StageDirectionPlayer = FindObjectOfType<StageDirectionPlayer>();
        //Debug.Log(this.name + ": setting EA to false in Start");
        ExecutingActions = false;
    }

    string ThisName;
    public void StartBehaviorFlow(Character_Action_List_Template behavior, GameObject callingObject)
    {
        StaticStuff.PrintBehaviorFlow(this.name + ": BehaviorFlowPlayer.StartBehaviorFlow(): " + behavior.DisplayName, this);
      //p  Debug.LogWarning(this.name + ": BehaviorFlowPlayer.StartBehaviorFlow(): " + behavior.DisplayName, this);
        if (ExecutingActions == true)
        {
            Debug.LogError("ok WTF: " + this.name + ": BehaviorFlowPlayer.StartBehaviorFlow(): " + behavior.DisplayName + ", the current behavrior is: " + ThisName);
        }

        ExecutingActions = true;
        ThisName = behavior.DisplayName;
        CurBehavior = behavior;
        CurCallingObject = callingObject;
        CurActions.Clear();
        ThisPath.Clear();
        if (this.name.Equals("Follow Test")) Debug.Log("setting IdleOrFollow to FALSE");
        IdleOrFollow = false;


        List<FlowFragment> newValidTargets = GetInitialActions(CurBehavior.InputPins[0]);   
        StaticStuff.PrintBehaviorFlow(this.name + ": we've got " + newValidTargets.Count + " new valid targets to start with", this);
        //if(this.name.Equals("Child")) Debug.Log(this.name + ": we've got " + newValidTargets.Count + " new valid targets to start with");
        PrepActions(newValidTargets);
    }    

    List<FlowFragment> GetInitialActions(InputPin firstPin)
    {
        List<FlowFragment> newValidTargets = new List<FlowFragment>();
        foreach (OutgoingConnection outCon in firstPin.Connections)
        {
            StaticStuff.PrintBehaviorFlow("this outCon has a target type: " + outCon.Target.GetType(), this);
           // if (this.name.Equals("Child")) Debug.Log("this outCon has a target type: " + outCon.Target.GetType());
            FlowFragment target = outCon.Target as FlowFragment;
            InputPin targetInputPin = target.InputPins[0];
            if (targetInputPin.Text.CallScript() == true)
            {
                //if (this.name.Equals("Child")) Debug.Log("code: " + targetInputPin.Text + " is true");
                if (newValidTargets.Contains(target) == false) newValidTargets.Add(target);
            }
        }
        return newValidTargets;
    }

    int NumExecuteChecks = 0;
    public bool CheckIfAIShouldChange()
    {
        if(CurBehavior == null)
        {
            Debug.Log(this.name + " CheckIfAIShouldChange() has a null CurBehavior, so you're either Idling or Following (with no other actions), so restart behavior");                
            return true;
        }
        List<Character_Action_Template> newFrameCats = new List<Character_Action_Template>();
        List<FlowFragment> curFrame = new List<FlowFragment>();
        curFrame = GetInitialActions(CurBehavior.InputPins[0]);
       // Debug.Log("curFrame count: " + curFrame.Count + " 0 has type: " + curFrame[0].GetType());
        bool doneChecking = false;
        int frameIndex = 0;
        while(doneChecking == false)
        {
            if(curFrame.Count == 1 && curFrame[0] as Stage_Directions != null )
            {
                Debug.Log(frameIndex + " is a Stage_Direction");               
            }
            else
            {
                newFrameCats.Clear();
                foreach (FlowFragment f in curFrame) newFrameCats.Add(f as Character_Action_Template);
                List<Character_Action_Template> curFrameCats = ThisPath[frameIndex];
                List<Character_Action_Template> firstNotSecond = new List<Character_Action_Template>();
                firstNotSecond = newFrameCats.Except(curFrameCats).ToList();
                string s = "";// "frame: " + frameIndex + ": firstNotSecond has a length of: " + firstNotSecond.Count + ": ";
                foreach (Character_Action_Template cat in firstNotSecond) s += cat.DisplayName + ", ";
                //Debug.Log(s);
                
                if(firstNotSecond.Count != 0)
                {
                    Debug.Log("at frameIndex: " + frameIndex + " we have a difference so change the AI: " + s);
                    return true;
                }
                frameIndex++;
            }
            
            if(frameIndex >= ThisPath.Count)
            {
                Debug.Log("at frameIndex: " + frameIndex + " we're done");
                doneChecking = true;
                return false;
            }

            FlowFragment[] curFrameCopy = new FlowFragment[curFrame.Count];
            curFrame.CopyTo(curFrameCopy);
            curFrame.Clear();
            foreach (FlowFragment frag in curFrameCopy)
            {
                OutputPin outputPin = frag.OutputPins[0];                
                AddValidTargetsFromPins(outputPin, curFrame);
            }

            NumExecuteChecks++;
            if(NumExecuteChecks >= 50)
            {
                Debug.Log("NumExectueChecks has gotten too big");
                return false;
            }
        }
        return false;
    }

    void CurrentActionsDone(Stage_Directions_Container sdcToSkipOver = null)
    {
        StaticStuff.PrintBehaviorFlow(this.name + ": CurrentActionsDone()", this);
        List<FlowFragment> newValidTargets = new List<FlowFragment>();

        if (sdcToSkipOver != null)
        {
            StaticStuff.PrintBehaviorFlow("we're on a Stage_Directions_Container so get the new stuff from here", this);
            OutputPin outputPin = sdcToSkipOver.OutputPins[0];
            AddValidTargetsFromPins(outputPin, newValidTargets);
        }
        else
        {
            foreach (Character_Action_Template action in ActionsToTake)
            {
                OutputPin outputPin = action.OutputPins[0];
               // if (this.name.Equals("Idle Test")) Debug.Log("gonna shut call a script");
                outputPin.Text.CallScript();
                AddValidTargetsFromPins(outputPin, newValidTargets);
            }
        }

        StaticStuff.PrintBehaviorFlow(this.name + ": we've got " + newValidTargets.Count + " new targets after the last set", this);
        if (newValidTargets.Count == 0)
        {            
            Debug.LogError(this.name + ": we should always have at least 1 target, even if it's the end of the flow, so check the flow on this npc please: " + this.name);
            Debug.LogError("ThisName: " + ThisName);
            if (CurBehavior == null) Debug.LogError("CurBehavior is null");
            else Debug.LogError("CurBehavior name: " + CurBehavior.DisplayName);

            if (this.GetComponent<BehaviorFlowPlayer>().CurBehavior == null) Debug.LogError("regetting all null");
            else Debug.LogError("reget CurBehavior name: " + this.GetComponent<BehaviorFlowPlayer>().CurBehavior.DisplayName);
            return;
        }
        PrepActions(newValidTargets);
    }

    List<Character_Action_Template> PrepActions(List<FlowFragment> flowList)
    {
        //Debug.Log(this.name + ": PrepActions()");
        ActionsToTake.Clear();
        foreach (FlowFragment ao in flowList)
        {
            if (ao as Character_Action_Template != null)
            {
                ActionsToTake.Add(ao as Character_Action_Template);
            }
            else if(ao as Stage_Directions_Container != null)
            {
                StaticStuff.PrintBehaviorFlow("We've got a Stage_Directions_Container to deal with during a BehavirFlowPlayer", this);
                Stage_Directions_Container sdc = ao as Stage_Directions_Container;
                StageDirectionPlayer.HandleStangeDirectionContainer(sdc);
                /*foreach(OutgoingConnection oc in sdc.InputPins[0].Connections)
                {
                    Stage_Directions sd = oc.Target as Stage_Directions;
                    if(sd == null) { Debug.LogError("We're expecting a Stage_Directions here: " + oc.Target.GetType()); continue; }
                    StageDirectionPlayer.HandleStageDirection(sd); // mosd - BehaviorFlowPlayer.PrepActions() FlowFragment is Stage_Directions_Container
                }*/
                CurrentActionsDone(sdc);
                return null;
            }
            else if (ao as Stage_Directions != null)
            {
                Debug.LogError("Got a stage direction during a behavior flow, please update to a Stage_Directions_Container");
                //Stage_Directions sd = ao as Stage_Directions;
                //CaptainArticyFlow.HandleStageDirections(sd);
                //CurrentActionsDone(sd);
                return null;
            }
            else if (ao as Character_Action_List_Template != null)
            {
                StaticStuff.PrintBehaviorFlow(this.name + ": we've got a Character_Action_List_Template is our fragment, which is the end of the flow so this SHOULD be the only node connected so we should be ending after this", this);
            }
            else
            {
                Debug.LogWarning(this.name + ": we don't know what to do with this yet: " + ao.GetType().ToString());
            }
        }

        if (ActionsToTake.Count == 0)
        {
            string displayName = CurBehavior.DisplayName;
            //StaticStuff.PrintBehaviorFlow(this.name + ": we've got no ActionsToTake so bail", this);
           // if (this.name.Contains("Captain")) Debug.Log(this.name + ": with behavior: " + displayName + ": we've got no ActionsToTake so bail");
            foreach (KeyValuePair<CharacterEntity, EntitySaveData> entry in CurCALEntitySaveData)
            {
                entry.Key.SetStoppingDist(entry.Value.NavMeshStoppingDist);
                entry.Key.SetShouldFollowEntity(entry.Value.ShouldFollowEntity);
            }

            CurBehavior.OutputPins[0].Text.CallScript();
            CurBehavior = null;
            CurCallingObject = null;
            CurBehaviorCoroutine = null;
            int numNodes = NumNodesInPath();
            ThisPath.Clear();
            ExecutingActions = false;
            if (GetComponent<NPC>() != null) GetComponent<NPC>().EndCAL(numNodes, IdleOrFollow);
            else if (GetComponent<ArticyFlow>() != null) GetComponent<ArticyFlow>().EndCAL(this.name, displayName);
            else if (GetComponent<AmbientTrigger>() != null) GetComponent<AmbientTrigger>().EndCAL();
            else Debug.LogError("We're ending a BehaviorFlowPlayer but we don't know what type is calling it: " + this.name);            
            return null;
        }

        Character_Action_Template[] latestActions = new Character_Action_Template[ActionsToTake.Count];
        ActionsToTake.CopyTo(latestActions);
        ThisPath.Add(latestActions.ToList());
        CurBehaviorCoroutine = StartCoroutine(ExecuteActions());
        return ActionsToTake;
    }

    int NumNodesInPath()
    {
        int numNodes = 0;
        foreach (List<Character_Action_Template> l in ThisPath)
        {
            foreach (Character_Action_Template cat in l)
            {
                numNodes++;
            }            
        }
        return numNodes;
    }
    private void Update()
    {
        if (DebugText2 != null)
        {
            string s = this.name + "\n";
                s += "ExecutingActions: " + ExecutingActions.ToString() + "\n";
            s += "ThisName: " + ThisName + "\n";
            s += (CurBehavior == null ? "Null CurBehavior\n" : "CurBehavior name: " + CurBehavior.name + ", TechnicalName: " + CurBehavior.TechnicalName + ", DisplayName: " + CurBehavior.DisplayName + "\n");
            s += (CurCallingObject == null ? "Null CurCallingObject\n" : "CurCallingObject: " + CurCallingObject.name + "\n");
            foreach(Character_Action_Template cat in CurActions)
            {
                s += "cat: " + cat.name + "\n";
            }
            s += Time.time + "\n";
            DebugText2.text = s;
           /* ExecutingActions = true;
            ThisName = behavior.DisplayName;
            CurBehavior = behavior;
            CurCallingObject = callingObject;
            CurActions.Clear();
            ThisPath.Clear();*/
            /*DebugText2.text = ThisPath.Count + "\n";
            foreach (List<Character_Action_Template> l in ThisPath)
            {
                foreach (Character_Action_Template cat in l)
                {
                    DebugText2.text += cat.DisplayName + ", ";
                }
                DebugText2.text += "\n";
            }*/
        }
    }

    void AddValidTargetsFromPins(OutputPin outputPin, List<FlowFragment> newValidTargets)
    {
        foreach (OutgoingConnection outCon in outputPin.Connections)
        {
            FlowFragment target = outCon.Target as FlowFragment;
            InputPin targetInputPin = target.InputPins[0];
            if (targetInputPin.Text.CallScript() == true)
            {
                if (newValidTargets.Contains(target) == false) newValidTargets.Add(target);
            }
        }
    }    
    
    List<List<Character_Action_Template>> ThisPath = new List<List<Character_Action_Template>>();
    List<Character_Action_Template> ActionsToTake = new List<Character_Action_Template>();
    public bool ExecutingActions = false;
    IEnumerator ExecuteActions()
    {                
        ActionsStates.Clear();
        foreach (Character_Action_Template actionTemplate in ActionsToTake)
        {   // Set up the initial state of the data             
            Character_Action_FeatureFeature curActionData = actionTemplate.Template.Character_Action_Feature;
            PrintActionInfo(actionTemplate.DisplayName, curActionData);
            string[] actionObjects = curActionData.ObjectsToAct.Split(',');
            //Debug.Log("num objects to act: " + actionObjects.Count());
            foreach (string objectName in actionObjects)
            {                
                GameObject actionObject;                
                if(objectName.Equals("Self"))
                {
                    actionObject = this.gameObject;
                }
                else
                {
                    actionObject = GameObject.Find(objectName);
                }
                if (actionObject == null) { Debug.LogError(this.name + ": No object in the scene called: " + objectName); yield break; }
                ActionState actionState = new ActionState(curActionData.Action, curActionData.ActionInfo, actionObject);
                ActionsStates.Add(actionState);

                if (SetupActionState(actionState, ActionsStates, curActionData.Delay_Range) == false) yield break;
                if (actionState.delayDone == true)
                {
                    if (StartActionState(actionState) == false) yield break;
                }
            }            
        }
        //Debug.Log(this.name + ": setting EA to true in ExecuteActions");
        //ExecutingActions = true;
        while (AllActionsDone(ActionsStates) == false)
        {                        
            foreach (ActionState actionState in ActionsStates)
            {                                
                if (actionState.isActionDone == false)
                {                    
                    if (actionState.delayDone == false)
                    {                        
                       /* if (DebugText != null)
                        {
                            DebugText.text = "ExecutingActions: " + ExecutingActions + "\n";
                            DebugText.text += CurCallingObject.name + " is delaying for: " + actionState.delayTime.ToString("F2") + ", we're at: " + actionState.timer.ToString("F2");// + ", active?: " + IsActive;
                        }  */                      
                        actionState.timer += Time.deltaTime;
                        if (actionState.timer >= actionState.delayTime)
                        {                            
                            actionState.delayDone = true;
                            if (StartActionState(actionState) == false) yield break;                            
                        }
                        break;
                    }
                    else
                    {                        
                       /* if (DebugText != null)
                        {
                            DebugText.text = "ExecutingActions: " + ExecutingActions + "\n";
                            DebugText.text += CurCallingObject.name + " is now on to the action!: " + actionState.action;// + ", active?: " + IsActive;
                        }*/
                    }                    
                    switch (actionState.action)
                    {
                        case Action.WalkToLocation:
                            actionState.isActionDone = actionState.actionObject.GetComponent<CharacterEntity>().NavMeshDone();
                            break;
                        case Action.WalkToObject:
                            actionState.isActionDone = actionState.actionObject.GetComponent<CharacterEntity>().NavMeshDone();                            
                            break;
                        case Action.Animation:
                            actionState.timer += Time.deltaTime;
                            if (actionState.timer >= actionState.actionTime)
                            {
                                actionState.isActionDone = true;
                            }
                            break;
                        case Action.TurnTowards:
                            actionState.timer += Time.deltaTime * 5f;
                            actionState.actionObject.transform.rotation = Quaternion.Lerp(actionState.LerpRotStart, actionState.LerpRotEnd, actionState.timer);
                            if (actionState.timer > 1f)
                            {
                              //  if (this.name.Contains("Captain")) Debug.LogError("TurnTowards is done");
                                actionState.actionObject.transform.rotation = actionState.LerpRotEnd;
                                actionState.isActionDone = true;
                            }
                            break;
                        case Action.Wander:
                            actionState.isActionDone = actionState.actionObject.GetComponent<CharacterEntity>().NavMeshDone();
                            if (actionState.isActionDone == true)
                            {
                                GameObject wanderManagerObject = GameObject.Find(actionState.actionInfo);
                                WanderManager wm = wanderManagerObject.GetComponent<WanderManager>();
                                wm.ReleaseWanderPoint(actionState.infoObject.GetComponent<WanderPoint>());
                            }
                            break;
                        case Action.Bark:
                            actionState.timer += Time.deltaTime;
                            if (actionState.timer > 3f)
                            {
                                actionState.actionObject.GetComponent<NPC>().ToggleBarkText(false);
                                actionState.isActionDone = true;
                            }
                            break;
                        case Action.TurnTowardsEachOther:
                            Debug.LogError("We should never get to Action.TurnTowardsEachOther here because the Action(s) for each ActionState on the characters turning towards each other were both changed to Action.TurnTowards");
                            yield break;
                        case Action.Idle:
                            if(this.name.Equals("Follow Test")) Debug.Log("shut off idle");
                            actionState.isActionDone = true;
                            actionState.delayDone = true;
                            break;
                    }
                }
            }
            
            yield return new WaitForEndOfFrame();
        }
        //Debug.Log(this.name + ": setting EA to false in ExecuteActions");
        //ExecutingActions = false;
        //Debug.Log(this.name + ": all actions are done");
        CurrentActionsDone();
    }

    bool SetupActionState(ActionState actionState, List<ActionState> actionsStates, string delayRangeString)
    {
        CharacterEntity ce;

        /*if(this.name.Contains("Captain"))
        {
            Debug.LogError("SetupActionState() actionInfo: " + actionState.actionInfo);
        }*/
        if (delayRangeString.Equals(""))
        {
            StaticStuff.PrintCAL("delay blank, so no delay");
            actionState.delayDone = true;
        }
        else
        {
            string[] delayRange = delayRangeString.Split(',');
            actionState.delayTime = Random.Range(float.Parse(delayRange[0]), float.Parse(delayRange[1]));
            actionState.delayDone = false;
            StaticStuff.PrintCAL("have a delay of " + actionState.delayTime.ToString("F2") + " seconds.");
        }
        switch (actionState.action)
        {
            case Action.CameraFollow:
                ce = actionState.actionObject.GetComponent<CharacterEntity>();
                if (ce == null) { Debug.LogError("Camera can't follow something that's not a Player/NPC: " + actionState.actionObject.name); return false; }
                string[] offset = actionState.actionInfo.Split(',');
                actionState.vectorData = new Vector3(float.Parse(offset[0]), float.Parse(offset[1]), float.Parse(offset[2]));
                if(offset.Length > 3)
                {
                    actionState.LerpRotEnd = Quaternion.Euler(float.Parse(offset[3]), float.Parse(offset[4]), float.Parse(offset[5]));
                }
                else
                {
                    actionState.LerpRotEnd = CamFollow.transform.rotation;
                }                
                break;
            case Action.WalkToObject:
                ce = actionState.actionObject.GetComponent<CharacterEntity>();
                if (ce == null) { Debug.LogError("Can't tell an object that's not a Player/NPC to walk to an object: " + actionState.actionObject.name); return false; }
                string[] walkToInfo = actionState.actionInfo.Split(',');
                actionState.infoObject = GameObject.Find(walkToInfo[0]);                
                if (actionState.infoObject == null) { Debug.LogError("The object to walk to: " + walkToInfo[0] + " is not in the scene."); return false; }
                if (CurCALEntitySaveData.ContainsKey(ce) == false)
                {
                    CurCALEntitySaveData.Add(ce, new EntitySaveData(ce.transform.position, ce.GetStoppingDist(), ce.GetShouldFollowEntity()));
                    ce.SetShouldFollowEntity(false);
                }
                if(walkToInfo.Length > 1)
                {
                    ce.SetStoppingDist(float.Parse(walkToInfo[1]));
                }
                break;
            case Action.WalkToLocation:
                ce = actionState.actionObject.GetComponent<CharacterEntity>();
                if (ce == null) { Debug.LogError("Can't tell an object that's not a Player/NPC to walk to a location: " + actionState.actionObject.name); return false; }
                // if (curActionData.ActionInfo == "Start_Position")
                if (actionState.actionInfo == "Start_Position")
                {
                    if (CurCALEntitySaveData.ContainsKey(ce) == false) { Debug.LogError("This entity object isn't in the save data list. Make sure Start_Position isn't the first ActionInfo for walking on this entity: " + actionState.actionObject.name); return false; } // moaction
                    actionState.vectorData = CurCALEntitySaveData[ce].StartPos;
                }
                else
                {
                    string[] pos = actionState.actionInfo.Split(',');
                    Vector3 loc = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
                    if (CurCALEntitySaveData.ContainsKey(ce) == false)
                    {
                        CurCALEntitySaveData.Add(ce, new EntitySaveData(ce.transform.position, ce.GetStoppingDist(), ce.GetShouldFollowEntity()));
                        ce.SetStoppingDist(0f);
                        ce.SetShouldFollowEntity(false);
                    }
                    actionState.vectorData = loc;
                }
                break;
            case Action.Animation:
                ce = actionState.actionObject.GetComponent<CharacterEntity>();
                if (ce == null) { Debug.LogError("Can't call an animation on an object that's not a Player/NPC: " + actionState.actionObject.name); return false; }
                break;
            case Action.TurnTowards:
            //    if (this.name.Contains("Captain")) Debug.LogError("******** start setting up captain TurnTowards: " + Time.time);
                if (SetTurnTowards(actionState.actionInfo, actionState) == false) { return false; }
              //  if (this.name.Contains("Captain")) Debug.LogError("******** end setting up captain TurnTowards: " + Time.time);
                break;
            case Action.TurnTowardsEachOther:
                if (actionState.actionInfo.Equals("Dialogue_NPC")) //actionState.actionInfo = DialogueNPC.name;
                {
                    ArticyFlow af = CurCallingObject.GetComponent<ArticyFlow>();
                    if(af == null) { Debug.LogError("there's no ArticyFlow on this BFP's calling object: " + CurCallingObject.name); return false; }
                    actionState.actionInfo = af.GetDialogueNPCName();
                }
                if (SetTurnTowards(actionState.actionInfo, actionState) == false) { return false; }
                GameObject actionObject2 = GameObject.Find(actionState.actionInfo);
                if (actionObject2 == null) { Debug.LogError("No object in the scene named: " + actionState.actionObject.name); return false; }
                ActionState actionState2 = new ActionState(actionState.action, actionState.actionObject.name, actionObject2);
                actionsStates.Add(actionState2);
                if (SetTurnTowards(actionState2.actionInfo, actionState2) == false) { return false; }
                actionState.action = Action.TurnTowards;
                actionState2.action = Action.TurnTowards;
                break;
            case Action.Wander:
                GameObject wanderManagerObject = GameObject.Find(actionState.actionInfo);
                if (wanderManagerObject == null) { Debug.LogError("There's no wander manager object in the scene called: " + actionState.actionInfo); return false; }
                WanderManager wm = wanderManagerObject.GetComponent<WanderManager>();
                if (wm == null) { Debug.LogError("There's no WanderManager component on this object: " + actionState.actionInfo); return false; }
                WanderPoint wp = wm.GetWanderPoint(actionState.actionObject);
                actionState.infoObject = wp.gameObject;
                ce = actionState.actionObject.GetComponent<CharacterEntity>();
                if (ce == null) { Debug.LogError("Can't tell an object that's not a Player/NPC/AmbientEntity to wander to a wander point: " + actionState.actionObject.name); return false; }
                actionState.vectorData = wp.transform.position;
                break;
            case Action.Bark:
                NPC ae = actionState.actionObject.GetComponent<NPC>();
                if (ae == null) { Debug.LogError("There's no NPC on the object you want a Bark action for: " + actionState.actionObject.name); return false; }
                ae.SetBarkText(actionState.actionInfo);
                break;
            case Action.Follow:
                GameObject followObject = GameObject.Find(actionState.actionInfo);
                if (followObject == null) { Debug.LogError("Can't follow an object that's not in the scene: " + actionState.actionInfo); return false; }
                ce = actionState.actionObject.GetComponent<CharacterEntity>();
                if (ce == null) { Debug.LogError("There's no CharacterEntity on the object you want to follow: " + actionState.actionObject.name); return false; }
                ce.SetEntityToFollow(followObject);
                actionState.delayDone = true;
                actionState.isActionDone = true;
                IdleOrFollow = true;
                break;
            case Action.Idle:
                if (this.name.Equals("Follow Test")) Debug.Log("setting IdleOrFollow to TRUE");
                IdleOrFollow = true;
                break;
            case Action.Teleport:
                Debug.Log("-----------------------------------------------------------------------Teleport"); 
                actionState.infoObject = GameObject.Find(actionState.actionInfo);
                if (actionState.infoObject == null) { Debug.LogError("The object to teleport to: " + actionState.actionInfo + " is not in the scene."); return false; }
                NavMeshAgent agent = actionState.actionObject.GetComponent<NavMeshAgent>();
                if (agent != null) agent.enabled = false;
                actionState.actionObject.transform.position = actionState.infoObject.transform.position;
                if (agent != null) agent.enabled = true;
                ce = actionState.actionObject.GetComponent<CharacterEntity>();
                if (ce != null) StartCoroutine(ce.CheckPostTeleportTransparency());/*ce.CheckPostTeleportTransparency();*/ else { Debug.LogError("No CharacterEntity on this: " + this.name); return false; }
                actionState.delayDone = true;
                actionState.isActionDone = true;                     
                break;
            default:                
                Debug.LogError("ERROR: we don't have code for this action: " + actionState.action);
                return false;
        }

        return true;
    }

    bool StartActionState(ActionState actionState)
    {
        actionState.timer = 0f;
        CharacterEntity ce = actionState.actionObject.GetComponent<CharacterEntity>();
        switch (actionState.action)
        {            
            case Action.CameraFollow:                
                CamFollow.SetupNewCamFollow(ce, actionState.vectorData, actionState.LerpRotEnd);
                actionState.isActionDone = true;
                break;
            case Action.WalkToLocation:
                ce.SetNavMeshDest(actionState.vectorData);
                break;
            case Action.WalkToObject:
                ce.SetNavMeshDest(actionState.infoObject.transform.position);
                break;
            case Action.Animation:
                actionState.actionTime = ce.PlayAnim(actionState.actionInfo);
                if (actionState.actionTime == -1) { Debug.LogError(actionState.actionObject + " does not have an animation called: " + actionState.actionInfo + " in it."); return false; }
                break;
            case Action.Wander:
                ce.SetNavMeshDest(actionState.vectorData);
                break;
            case Action.Bark:
                actionState.actionObject.GetComponent<NPC>().ToggleBarkText(true);
                break;
        }

        return true;
    }

    void StopActionState(ActionState actionState)
    {
        CharacterEntity ce;
        switch (actionState.action)
        {
            case Action.Animation:
                ce = actionState.actionObject.GetComponent<CharacterEntity>();
                ce.StopAnim();
                break;
            case Action.Bark:
                actionState.actionObject.GetComponent<NPC>().ToggleBarkText(false);
                break;
            case Action.WalkToLocation:
            case Action.WalkToObject:
                ce = actionState.actionObject.GetComponent<CharacterEntity>();
                ce.StopNavMeshMovement();
                break;
            case Action.Wander:
                ce = actionState.actionObject.GetComponent<CharacterEntity>();
                ce.StopNavMeshMovement();
                GameObject wanderManagerObject = GameObject.Find(actionState.actionInfo);
                WanderManager wm = wanderManagerObject.GetComponent<WanderManager>();
                wm.ReleaseWanderPoint(actionState.infoObject.GetComponent<WanderPoint>());                    
                break;
            case Action.Follow:
                ce = actionState.actionObject.GetComponent<CharacterEntity>();
                ce.SetEntityToFollow(null);
                break;
        }
        actionState.delayDone = true;
        actionState.isActionDone = true;
    }

    public void StopBehavior()
    {
        if (CurBehaviorCoroutine != null)
        {
            StopCoroutine(CurBehaviorCoroutine);
            CurBehaviorCoroutine = null;
        }
        foreach (KeyValuePair<CharacterEntity, EntitySaveData> entry in CurCALEntitySaveData)
        {
            entry.Key.SetStoppingDist(entry.Value.NavMeshStoppingDist);
            entry.Key.SetShouldFollowEntity(entry.Value.ShouldFollowEntity);
        }
        CurCallingObject = null;
        //Debug.Log(this.name + ": setting EA to false in StopBehavior");
        //ExecutingActions = false;
        ActionsToTake.Clear();
        foreach (ActionState actionState in ActionsStates)
        {
            StopActionState(actionState);
        }
    }

    bool SetTurnTowards(string objectNameToLookAt, ActionState actionState)
    {
        //Debug.Log("----------------------------------------------SetTurnTowards(). " + actionState.actionObject.name + " wants to look at: " + objectNameToLookAt);
        GameObject objectToLookAt = GameObject.Find(objectNameToLookAt);
        if (objectToLookAt == null) { Debug.LogError("No object in the scene to look at called: " + objectNameToLookAt); return false; }
        actionState.LerpRotStart = actionState.actionObject.transform.rotation;
        Transform t = objectToLookAt.transform;
        t.position = new Vector3(t.position.x, this.transform.position.y, t.position.z);
        actionState.actionObject.transform.LookAt(objectToLookAt.transform);
        actionState.LerpRotEnd = actionState.actionObject.transform.rotation;
        actionState.actionObject.transform.rotation = actionState.LerpRotStart;
        return true;
    }

    bool AllActionsDone(List<ActionState> actionStates)
    {
        bool allDone = true;
        foreach (ActionState actionState in actionStates)
        {
            if (actionState.isActionDone == false) allDone = false;
        }
        return allDone;
    }

    void PrintActionInfo(string actionName, Character_Action_FeatureFeature curActionData)
    {
        string s = "\tAction name: " + actionName + ", ";
        s += "Action type: " + curActionData.Action + ", ";
        s += "action info: " + curActionData.ActionInfo + ", ";
        s += "action Object: " + curActionData.ObjectsToAct;
        StaticStuff.PrintCAL(s);
    }
    
    class EntitySaveData
    {
        public Vector3 StartPos;
        public float NavMeshStoppingDist;
        public bool ShouldFollowEntity;
        public EntitySaveData(Vector3 pos, float dist, bool shouldFollow)
        {
            StartPos = pos;
            NavMeshStoppingDist = dist;
            ShouldFollowEntity = shouldFollow;
        }
    }
    class ActionState
    {
        public Action action;
        public string actionInfo;
        public GameObject actionObject;
        public GameObject infoObject;
        public float actionTime = 0;
        public float delayTime = 0f;
        public float timer = 0;
        public Quaternion LerpRotStart = Quaternion.identity;
        public Quaternion LerpRotEnd = Quaternion.identity;
        public Vector3 vectorData;
        public bool delayDone = false;
        public bool isActionDone = false;

        public ActionState(Action action, string actionInfo, GameObject actionObject)
        {
            this.action = action;
            this.actionInfo = actionInfo;
            this.actionObject = actionObject;
        }
    }

}
