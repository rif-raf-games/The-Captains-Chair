using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Articy.The_Captain_s_Chair.Features;
using Articy.The_Captain_s_Chair;
using Articy.Unity;
using UnityEngine.UI;

public class BehaviorFlowPlayer : MonoBehaviour
{
    public Text DebugText;


    CamFollow CamFollow;
    Dictionary<CharacterEntity, EntitySaveData> CurCALEntitySaveData = new Dictionary<CharacterEntity, EntitySaveData>();
    List<Character_Action_Template> CurActions = new List<Character_Action_Template>();
    List<ActionState> ActionsStates = new List<ActionState>();
    Character_Action_List_Template CurBehavior;
    GameObject CurCallingObject;

    private void Start()
    {
        CamFollow = FindObjectOfType<CamFollow>();
    }

    public void StartBehaviorFlow(Character_Action_List_Template behavior, GameObject callingObject)
    {
        //Debug.Log(this.name + ": BehaviorFlowPlayer.StartBehaviorFlow(): " + behavior.DisplayName);
        CurBehavior = behavior;
        CurCallingObject = callingObject;
        CurActions.Clear();
        
        List<InputPin> inputPins = behavior.InputPins;
        InputPin firstPin = inputPins[0];        
        List<FlowFragment> newValidTargets = new List<FlowFragment>();        

        foreach (OutgoingConnection outCon in firstPin.Connections)
        {
            //Debug.Log("this outCon has a target type: " + outCon.Target.GetType());
            FlowFragment target = outCon.Target as FlowFragment;
            InputPin targetInputPin = target.InputPins[0];
            if (targetInputPin.Text.CallScript() == true)
            {
                if (newValidTargets.Contains(target) == false) newValidTargets.Add(target);
            }
        }

        //Debug.Log(this.name + ": we've got " + newValidTargets.Count + " new valid targets to start with");
        PrepActions(newValidTargets);
    }

    Coroutine CurBehaviorCoroutine;
    List<Character_Action_Template> ActionsToTake = new List<Character_Action_Template>();
    void PrepActions(List<FlowFragment> flowList)
    {
        //Debug.Log(this.name + ": PrepActions()");
        ActionsToTake.Clear();
        foreach (FlowFragment ao in flowList)
        {
            if (ao as Character_Action_Template != null)
            {
                ActionsToTake.Add(ao as Character_Action_Template);
            }
            else if(ao as Character_Action_List_Template != null)
            {
                //Debug.Log(this.name + ": we've got a Character_Action_List_Template is our fragment, which is the end of the flow so this SHOULD be the only node connected so we should be ending after this");
            }
            else
            {
                Debug.LogWarning(this.name + ": we don't know what to do with this yet: " + ao.GetType().ToString());
            }
        }

        if(ActionsToTake.Count == 0)
        {
            //Debug.Log(this.name + ": we've got no ActionsToTake so bail");
            foreach (KeyValuePair<CharacterEntity, EntitySaveData> entry in CurCALEntitySaveData)
            {
                entry.Key.SetStoppingDist(entry.Value.NavMeshStoppingDist);
                entry.Key.SetShouldFollowEntity(entry.Value.ShouldFollowEntity);
            }
            CurBehavior.OutputPins[0].Text.CallScript();
            CurBehavior = null;
            CurCallingObject = null;
            CurBehaviorCoroutine = null;
            if (GetComponent<NPC>() != null) GetComponent<NPC>().EndCAL();
            if (GetComponent<ArticyFlow>() != null) GetComponent<ArticyFlow>().EndCAL();
            
            return;
        }

        CurBehaviorCoroutine = StartCoroutine(ExecuteActions());
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
            case Action.Wander:
                ce = actionState.actionObject.GetComponent<CharacterEntity>();
                ce.StopNavMeshMovement();
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

        foreach (ActionState actionState in ActionsStates)
        {
            StopActionState(actionState);
        }
    }

    void CurrentActionsDone()
    {
       // Debug.Log(this.name + ": CurrentActionsDone()");
        List<FlowFragment> newValidTargets = new List<FlowFragment>();
        foreach (Character_Action_Template action in ActionsToTake)
        {
            OutputPin outputPin = action.OutputPins[0];
            foreach(OutgoingConnection outCon in outputPin.Connections)
            {
                FlowFragment target = outCon.Target as FlowFragment;
                InputPin targetInputPin = target.InputPins[0];
                if(targetInputPin.Text.CallScript() == true)
                {
                    if (newValidTargets.Contains(target) == false) newValidTargets.Add(target);
                }
            }
        }
       // Debug.Log(this.name + ": we've got " + newValidTargets.Count + " new targets after the last set");
        if(newValidTargets.Count == 0)
        {
            Debug.LogError(this.name + "we should always have at least 1 target, even if it's the end of the flow, so check the flow on this npc please: " + this.name);
            return;
        }
        PrepActions(newValidTargets);
    }
    
    IEnumerator ExecuteActions()
    {        
        //Debug.Log(this.name + ": ExectueActions()");
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
                //if (CurCallingObject.GetComponent<NPC>() != null)
                if(objectName.Equals("Self"))
                {
                    actionObject = this.gameObject;
                }
                else
                {
                    actionObject = GameObject.Find(objectName);
                }
                if (actionObject == null) { Debug.LogError(this.name + "No object in the scene called: " + objectName); yield break; }
                ActionState actionState = new ActionState(curActionData.Action, curActionData.ActionInfo, actionObject);
                ActionsStates.Add(actionState);

                if (SetupActionState(actionState, ActionsStates, curActionData.Delay_Range) == false) yield break;
                if (actionState.delayDone == true)
                {
                    if (StartActionState(actionState) == false) yield break;
                }
            }            
        }
        while (AllActionsDone(ActionsStates) == false)
        {                        
            foreach (ActionState actionState in ActionsStates)
            {                                
                if (actionState.isActionDone == false)
                {                    
                    if (actionState.delayDone == false)
                    {                        
                        if (DebugText != null)
                        {                            
                            DebugText.text = CurCallingObject.name + " is delaying for: " + actionState.delayTime.ToString("F2") + ", we're at: " + actionState.timer.ToString("F2");// + ", active?: " + IsActive;
                        }                        
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
                        if (DebugText != null)
                        {
                            DebugText.text = CurCallingObject.name + " is now on to the action!: " + actionState.action;// + ", active?: " + IsActive;
                        }
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
                            if (actionState.timer > 5f)
                            {
                                actionState.actionObject.GetComponent<NPC>().ToggleBarkText(false);
                                actionState.isActionDone = true;
                            }
                            break;
                        case Action.TurnTowardsEachOther:
                            Debug.LogError("We should never get to Action.TurnTowardsEachOther here because the Action(s) for each ActionState on the characters turning towards each other were both changed to Action.TurnTowards");
                            yield break;
                    }
                }
            }
            
            yield return new WaitForEndOfFrame();
        }

        //Debug.Log(this.name + ": all actions are done");
        CurrentActionsDone();
    }

    bool SetupActionState(ActionState actionState, List<ActionState> actionsStates, string delayRangeString)
    {
        CharacterEntity ce;

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
                break;
            case Action.WalkToObject:
                ce = actionState.actionObject.GetComponent<CharacterEntity>();
                if (ce == null) { Debug.LogError("Can't tell an object that's not a Player/NPC to walk to an object: " + actionState.actionObject.name); return false; }
                actionState.infoObject = GameObject.Find(actionState.actionInfo);
                if (actionState.infoObject == null) { Debug.LogError("The object to walk to: " + actionState.actionInfo + " is not in the scene."); return false; }
                if (CurCALEntitySaveData.ContainsKey(ce) == false)
                {
                    CurCALEntitySaveData.Add(ce, new EntitySaveData(ce.transform.position, ce.GetStoppingDist(), ce.GetShouldFollowEntity()));
                    ce.SetShouldFollowEntity(false);
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
                if (SetTurnTowards(actionState.actionInfo, actionState) == false) { return false; }
                break;
            case Action.TurnTowardsEachOther:
                //if (actionState.actionInfo.Equals("Dialogue_NPC")) actionState.actionInfo = DialogueNPC.name;
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
                CamFollow.SetupNewCamFollow(ce, actionState.vectorData);
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

    bool SetTurnTowards(string objectNameToLookAt, ActionState actionState)
    {
        //Debug.Log("----------------------------------------------SetTurnTowards(). " + actionState.actionObject.name + " wants to look at: " + objectNameToLookAt);
        GameObject objectToLookAt = GameObject.Find(objectNameToLookAt);
        if (objectToLookAt == null) { Debug.LogError("No object in the scene to look at called: " + objectNameToLookAt); return false; }
        actionState.LerpRotStart = actionState.actionObject.transform.rotation;
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
