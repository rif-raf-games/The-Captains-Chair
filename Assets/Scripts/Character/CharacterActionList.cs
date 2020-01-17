using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Articy.Unity;
using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.Features;
using UnityEngine.UI;
using System.Linq;

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

public class CharacterActionList : MonoBehaviour
{
    CamFollow CamFollow;

    GameObject CurCallingObject = null;
    Character_Action_List_FeatureFeature CurCALObject = null;
    Dictionary<CharacterEntity, EntitySaveData> CurCALEntitySaveData = new Dictionary<CharacterEntity, EntitySaveData>();
    int CurCALIndex = 0;    

    private void Start()
    {
        CamFollow = FindObjectOfType<CamFollow>();
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

    
        
    bool AllActionsDone(List<ActionState> actionStates)
    {
        bool allDone = true;
        foreach(ActionState actionState in actionStates)
        {
            if (actionState.isActionDone == false) allDone = false;
        }
        return allDone;
    }
    
    public void BeginCAL(Character_Action_List_FeatureFeature calObject, GameObject callingObject)
    {
        if(callingObject == null) { Debug.LogError("CharacterActionList.BeginCal(): null callingObject."); return; }
        Debug.Log("------------------------------------------------------------BeginCal(): callingObject: " + callingObject);
        CurCALIndex = 0;
        CurCallingObject = callingObject;
        CurCALObject = calObject;
        CurCALEntitySaveData.Clear();
        StartCoroutine(PlayCAL());
    }

    bool SetupActionState(ActionState actionState, List<ActionState> actionsStates, string delayRangeString)
    {
        CharacterEntity ce;

        if (delayRangeString.Equals(""))
        {
            Debug.Log("delay blank, so no delay");
            actionState.delayDone = true;
        }
        else
        {
            string[] delayRange = delayRangeString.Split(',');
            actionState.delayTime = Random.Range(float.Parse(delayRange[0]), float.Parse(delayRange[1]));
            actionState.delayDone = false;
            Debug.Log("have a delay of " + actionState.delayTime.ToString("F2") + " seconds.");
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
                AmbientEntity ae = actionState.actionObject.GetComponent<AmbientEntity>();
                if(ae == null) { Debug.LogError("There's no AmbientEntity on the object you want a Bark action for: " + actionState.actionObject.name); return false; }
                ae.SetBarkText(actionState.actionInfo);
                break;
            default:
                Debug.LogError("ERROR: we don't have code for this action: " + actionState.action);
                return false;
        }

        return true;
    }
    //IndexOutOfRangeException: Index was outside the bounds of the array.

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
                actionState.actionObject.GetComponent<AmbientEntity>().ToggleBarkText(true);                
                break;
        }

        return true;
    }
   
    IEnumerator PlayCAL()
    {                
        Debug.Log("PlayCAL() going to do " + CurCALObject.ActionsStrip.Count + " action groups");
        foreach(ArticyObject ao in CurCALObject.ActionsStrip)
        {
            List<Character_Action_Template> actionsToTake = new List<Character_Action_Template>();
            // it's either an Action Group or just an Action
            Character_Action_Group_Template cagt = ao as Character_Action_Group_Template;
            Character_Action_Template cat = ao as Character_Action_Template;
            if (cagt != null)
            {
                //Debug.Log("element is an action group: " + cag.DisplayName);
                Character_Action_Group_FeatureFeature actionsFeature = cagt.Template.Character_Action_Group_Feature;
                foreach(Character_Action_Template catt in actionsFeature.ActionStrip)
                {
                    actionsToTake.Add(catt);
                }
            }
            else if(cat != null)
            {
                //Debug.Log("element is an action: " + cat.DisplayName);
                actionsToTake.Add(cat);
            }
            
            List<ActionState> actionsStates = new List<ActionState>();                                  
            foreach(Character_Action_Template actionTemplate in actionsToTake)
            {   // Set up the initial state of the data  
                Character_Action_FeatureFeature curActionData = actionTemplate.Template.Character_Action_Feature;
                PrintActionInfo(actionTemplate.DisplayName, curActionData);
                string[] actionObjects = curActionData.ObjectsToAct.Split(',');
                //Debug.Log("num objects to act: " + actionObjects.Count());
                foreach(string objectName in actionObjects)
                {
                    GameObject actionObject;
                    if (CurCallingObject.GetComponent<AmbientEntity>() != null)
                    {
                        actionObject = CurCallingObject;
                    }
                    else
                    {
                        actionObject = GameObject.Find(objectName);
                    }                    
                    if (actionObject == null) { Debug.LogError("No object in the scene called: " + objectName); yield break; }
                    ActionState actionState = new ActionState(curActionData.Action, curActionData.ActionInfo, actionObject);
                    actionsStates.Add(actionState);
                                   
                    /*if(curActionData.Delay_Range.Equals(""))                    
                    {
                        Debug.Log("delay blank, so no delay");
                        actionState.delayDone = true;
                    }
                    else
                    {
                        string[] delayRange = curActionData.Delay_Range.Split(',');
                        actionState.delayTime = Random.Range(float.Parse(delayRange[0]), float.Parse(delayRange[1]));
                        actionState.delayDone = false;
                        Debug.Log("have a delay of " + actionState.delayTime.ToString("F2") + " seconds.");
                    }*/
                    
                    if (SetupActionState(actionState, actionsStates, curActionData.Delay_Range) == false) yield break;
                    if (actionState.delayDone == true)
                    {
                        if (StartActionState(actionState) == false) yield break;
                    }
                    /*CharacterEntity ce;                    
                    switch(actionState.action)
                    {
                        case Action.CameraFollow:
                            ce = actionState.actionObject.GetComponent<CharacterEntity>();
                            if (ce == null) { Debug.LogError("Camera can't follow something that's not a Player/NPC: " + actionState.actionObject.name); yield break; }
                            string[] offset = actionState.actionInfo.Split(',');                            
                            actionState.vectorData = new Vector3(float.Parse(offset[0]), float.Parse(offset[1]), float.Parse(offset[2]));                            
                            break;
                        case Action.WalkToObject:
                            ce = actionState.actionObject.GetComponent<CharacterEntity>();
                            if (ce == null) { Debug.LogError("Can't tell an object that's not a Player/NPC to walk to an object: " + actionState.actionObject.name); yield break; }
                            actionState.infoObject = GameObject.Find(actionState.actionInfo);
                            if (actionState.infoObject == null) { Debug.LogError("The object to walk to: " + actionState.actionInfo + " is not in the scene."); yield break; }
                            if (CurCALEntitySaveData.ContainsKey(ce) == false)
                            {
                                CurCALEntitySaveData.Add(ce, new EntitySaveData(ce.transform.position, ce.GetStoppingDist(), ce.GetShouldFollowEntity()));                                
                                ce.SetShouldFollowEntity(false);
                            }                                                        
                            break;
                        case Action.WalkToLocation:
                            ce = actionState.actionObject.GetComponent<CharacterEntity>();
                            if (ce == null) { Debug.LogError("Can't tell an object that's not a Player/NPC to walk to a location: " + actionState.actionObject.name); yield break; }
                            // if (curActionData.ActionInfo == "Start_Position")
                            if (actionState.actionInfo == "Start_Position")
                            {
                                if (CurCALEntitySaveData.ContainsKey(ce) == false) { Debug.LogError("This entity object isn't in the save data list. Make sure Start_Position isn't the first ActionInfo for walking on this entity: " + actionState.actionObject.name); yield break; } // moaction
                                actionState.vectorData = CurCALEntitySaveData[ce].StartPos;                                
                            }
                            else
                            {
                                //string[] pos = curActionData.ActionInfo.Split(',');
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
                            if (ce == null) { Debug.LogError("Can't call an animation on an object that's not a Player/NPC: " + actionState.actionObject.name); yield break; }
                            actionState.actionTime = ce.PlayAnim(actionState.actionInfo); // moaction
                            if (actionState.actionTime == -1) { Debug.LogError(actionState.actionObject + " does not have an animation called: " + actionState.actionInfo + " in it."); yield break; }
                            break;
                        case Action.TurnTowards:                                                        
                            if( SetTurnTowards(actionState.actionInfo, actionState) == false) { yield break; }                                                        
                            break;
                        case Action.TurnTowardsEachOther:
                            if (SetTurnTowards(actionState.actionInfo, actionState) == false) { yield break; }
                            GameObject actionObject2 = GameObject.Find(actionState.actionInfo);
                            if (actionObject2 == null) { Debug.LogError("No object in the scene named: " + actionState.actionObject.name); yield break; }
                            ActionState actionState2 = new ActionState(actionState.action, actionState.actionObject.name, actionObject2);
                            actionsStates.Add(actionState2);                            
                            if (SetTurnTowards(actionState2.actionInfo, actionState2) == false) { yield break; }
                            actionState.action = Action.TurnTowards;
                            actionState2.action = Action.TurnTowards;                            
                            break;
                        case Action.Wander:
                            GameObject wanderManagerObject = GameObject.Find(actionState.actionInfo);
                            if(wanderManagerObject == null) { Debug.LogError("There's no wander manager object in the scene called: " + actionState.actionInfo); yield break; }
                            WanderManager wm = wanderManagerObject.GetComponent<WanderManager>();
                            if(wm == null ) { Debug.LogError("There's no WanderManager component on this object: " + actionState.actionInfo); yield break; }
                            WanderPoint wp = wm.GetWanderPoint();
                            ce = actionState.actionObject.GetComponent<CharacterEntity>();
                            if (ce == null) { Debug.LogError("Can't tell an object that's not a Player/NPC/AmbientEntity to wander to a wander point: " + actionState.actionObject.name); yield break; }
                            ce.SetNavMeshDest(wp.transform.position); // moaction
                            actionState.action = Action.WalkToLocation;
                            break;
                        default:
                            Debug.LogError("ERROR: we don't have code for this action: " + actionState.action);
                            yield break;
                    }*/
                }                                                                
            } 
            // now get the loop going
            while(AllActionsDone(actionsStates) == false)
            {                
                foreach(ActionState actionState in actionsStates)
                {
                    if(actionState.isActionDone == false)
                    {                        
                        if(actionState.delayDone == false)
                        {
                            if (DebugText != null)
                            {
                                if (CurCallingObject == null) Debug.LogError("WTF3");
                                DebugText.text = CurCallingObject.name + " is delaying for: " + actionState.delayTime.ToString("F2") + ", we're at: " + actionState.timer.ToString("F2");
                            }
                            actionState.timer += Time.deltaTime;
                            if(actionState.timer >= actionState.delayTime)
                            {
                                actionState.delayDone = true;
                                if (StartActionState(actionState) == false) yield break;
                                /*actionState.timer = 0f;
                                switch(actionState.action)
                                {
                                    case Action.CameraFollow:
                                        CamFollow.SetupNewCamFollow(actionState.actionObject.GetComponent<CharacterEntity>(), actionState.vectorData);                                        
                                        actionState.isActionDone = true;
                                        break;
                                    case Action.WalkToLocation:
                                        actionState.actionObject.GetComponent<CharacterEntity>().SetNavMeshDest(actionState.vectorData);
                                        break;
                                    case Action.WalkToObject:
                                        actionState.actionObject.GetComponent<CharacterEntity>().SetNavMeshDest(actionState.infoObject.transform.position);
                                        break;
                                }*/
                            }
                            break;
                        }
                        else
                        {
                            if (DebugText != null)
                            {
                                if (CurCallingObject == null) Debug.LogError("WTF");
                                if (actionState == null) Debug.Log("WTF2");
                                DebugText.text = CurCallingObject.name + " is now on to the action!: " + actionState.action;

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
                                if(actionState.timer >= actionState.actionTime)
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
                                if(actionState.isActionDone == true)
                                {
                                    GameObject wanderManagerObject = GameObject.Find(actionState.actionInfo);
                                    WanderManager wm = wanderManagerObject.GetComponent<WanderManager>();
                                    wm.ReleaseWanderPoint(actionState.infoObject.GetComponent<WanderPoint>());
                                }
                                break;
                            case Action.Bark:
                                actionState.timer += Time.deltaTime;
                                if(actionState.timer > 5f)
                                {
                                    actionState.actionObject.GetComponent<AmbientEntity>().ToggleBarkText(false);
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
           // Debug.Log("all actions done");            
        }

        //Debug.Log("CAL done");
        foreach (KeyValuePair<CharacterEntity, EntitySaveData> entry in CurCALEntitySaveData)
        {
            entry.Key.SetStoppingDist(entry.Value.NavMeshStoppingDist);
            entry.Key.SetShouldFollowEntity(entry.Value.ShouldFollowEntity);
        }
        ArticyFlow af = CurCallingObject.GetComponent<ArticyFlow>();
        AmbientEntity ae = CurCallingObject.GetComponent<AmbientEntity>();
        CurCALObject = null;
        CurCallingObject = null;

        if (af != null )
        {            
            af.EndCAL();
        }
        else if(ae != null)
        {
            ae.CALDone();
        }
        else
        {
            Debug.LogError("Calling CharacterActionList.BeginCAL() from an object that neither has an ArticyFlow or AmbientEntity component: " + CurCallingObject.name);
            yield break;
        }
        
    }

    bool SetTurnTowards(string objectNameToLookAt, ActionState actionState )
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


    void PrintActionInfo(string actionName, Character_Action_FeatureFeature curActionData)
    {
        string s = "\tAction name: " + actionName + ", ";
        s += "Action type: " + curActionData.Action + ", ";
        s += "action info: " + curActionData.ActionInfo + ", ";
        s += "action Object: " + curActionData.ObjectsToAct;
        Debug.Log(s);
    }
    public Text DebugText;    
    
}
