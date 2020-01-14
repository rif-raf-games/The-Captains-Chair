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
        public float actionTime = -1;
        public float timer = -1;
        public Quaternion LerpRotStart = Quaternion.identity;
        public Quaternion LerpRotEnd = Quaternion.identity;
        public bool isDone = false;

        public ActionState(Action action, string actionInfo, GameObject actionObject/*, Vector3 startPosition*/)
        {
            this.action = action;
            this.actionInfo = actionInfo;
            this.actionObject = actionObject;
            //this.startPosition = startPosition;
        }
    }

    
        
    bool AllActionsDone(List<ActionState> actionStates)
    {
        bool allDone = true;
        foreach(ActionState actionState in actionStates)
        {
            if (actionState.isDone == false) allDone = false;
        }
        return allDone;
    }
    public void BeginCAL(Character_Action_List_FeatureFeature calObject)
    {
        CurCALIndex = 0;
        CurCALObject = calObject;
        CurCALEntitySaveData.Clear();
        StartCoroutine(PlayCAL());
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
               // PrintActionInfo(actionTemplate.DisplayName, curActionData);
                GameObject actionObject = GameObject.Find(curActionData.ObjectToAct);                
                if (actionObject == null ){ Debug.LogError("No object in the scene called: " + curActionData.ObjectToAct); yield break; }
                ActionState actionState = new ActionState(curActionData.Action, curActionData.ActionInfo, actionObject/*, actionObject.transform.position*/);
                actionsStates.Add(actionState);
                CharacterEntity ce;
                switch (curActionData.Action)
                {
                    case Action.CameraFollow:                        
                        ce = actionState.actionObject.GetComponent<CharacterEntity>();
                        if (ce == null) { Debug.LogError("Camera can't follow something that's not a Player/NPC: " + actionState.actionObject.name); yield break; }
                        string[] offset = actionState.actionInfo.Split(',');
                        Vector3 offsetVec = new Vector3(float.Parse(offset[0]), float.Parse(offset[1]), float.Parse(offset[2]));
                        CamFollow.SetupNewCamFollow(ce, offsetVec);
                        actionState.isDone = true;
                        break;
                    case Action.WalkToObject:
                        ce = actionState.actionObject.GetComponent<CharacterEntity>();
                        if (ce == null) { Debug.LogError("Can't tell an object that's not a Player/NPC to walk to an object: " + actionState.actionObject.name); yield break; }
                        GameObject objectToWalkTo = GameObject.Find(actionState.actionInfo);
                        if(objectToWalkTo == null) { Debug.LogError("The object to walk to: " + actionState.actionInfo + " is not in the scene."); yield break; }
                        if (CurCALEntitySaveData.ContainsKey(ce) == false)
                        {
                            CurCALEntitySaveData.Add(ce, new EntitySaveData(ce.transform.position, ce.GetStoppingDist(), ce.GetShouldFollowEntity()));
                            //ce.SetStoppingDist(0f);
                            ce.SetShouldFollowEntity(false);
                        }
                        ce.SetNavMeshDest(objectToWalkTo.transform.position);
                        break;
                    case Action.WalkToLocation:                        
                        ce = actionState.actionObject.GetComponent<CharacterEntity>();
                        if (ce == null) { Debug.LogError("Can't tell an object that's not a Player/NPC to walk to a location: " + actionState.actionObject.name); yield break; }
                        if (curActionData.ActionInfo == "Start_Position")
                        {
                            if (CurCALEntitySaveData.ContainsKey(ce) == false) { Debug.LogError("This entity object isn't in the save data list. Make sure Start_Position isn't the first ActionInfo for walking on this entity: " + actionState.actionObject.name); yield break; }
                            ce.SetNavMeshDest(CurCALEntitySaveData[ce].StartPos);                            
                        }
                        else 
                        {
                            string[] pos = curActionData.ActionInfo.Split(',');
                            Vector3 loc = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
                            if (CurCALEntitySaveData.ContainsKey(ce) == false)
                            {
                                CurCALEntitySaveData.Add(ce, new EntitySaveData(ce.transform.position, ce.GetStoppingDist(), ce.GetShouldFollowEntity()));
                                ce.SetStoppingDist(0f);
                                ce.SetShouldFollowEntity(false);
                            }                            
                            ce.SetNavMeshDest(loc);
                        }                        
                            break;
                    case Action.Animation:
                        ce = actionState.actionObject.GetComponent<CharacterEntity>();
                        if(ce == null) { Debug.LogError("Can't call an animation on an object that's not a Player/NPC: " + actionState.actionObject.name); yield break;}
                        actionState.actionTime = ce.PlayAnim(actionState.actionInfo);
                        if(actionState.actionTime == -1) { Debug.LogError(actionState.actionObject + " does not have an animation called: " + actionState.actionInfo + " in it."); yield break; }
                        break;
                    case Action.TurnTowards:
                        GameObject objectToLookAt = GameObject.Find(curActionData.ActionInfo);
                        if (objectToLookAt == null) { Debug.LogError("No object in the scene to look at called: " + curActionData.ActionInfo); yield break; }
                        actionState.LerpRotStart = actionState.actionObject.transform.rotation;
                        actionState.actionObject.transform.LookAt(objectToLookAt.transform);
                        actionState.LerpRotEnd = actionState.actionObject.transform.rotation;
                        actionState.actionObject.transform.rotation = actionState.LerpRotStart;
                       // Debug.Log(actionState.LerpRotStart.eulerAngles + ", end: " + actionState.LerpRotEnd.eulerAngles);
                        break;
                }
                
            } 
            // now get the loop going
            while(AllActionsDone(actionsStates) == false)
            {                
                foreach(ActionState actionState in actionsStates)
                {
                    if(actionState.isDone == false)
                    {                        
                        switch (actionState.action)
                        {
                            case Action.WalkToLocation:
                                actionState.isDone = actionState.actionObject.GetComponent<CharacterEntity>().NavMeshDone();
                                break;
                            case Action.WalkToObject:
                                actionState.isDone = actionState.actionObject.GetComponent<CharacterEntity>().NavMeshDone();                              
                                break;
                            case Action.Animation:
                                actionState.timer += Time.deltaTime;
                                if(actionState.timer >= actionState.actionTime)
                                {
                                    actionState.isDone = true;
                                }
                                break;
                            case Action.TurnTowards:                                
                                actionState.timer += Time.deltaTime * 5f;
                                actionState.actionObject.transform.rotation = Quaternion.Lerp(actionState.LerpRotStart, actionState.LerpRotEnd, actionState.timer);                               
                                if (actionState.timer > 1f)
                                {
                                    actionState.actionObject.transform.rotation = actionState.LerpRotEnd;
                                    actionState.isDone = true;
                                }
                                break;
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
        GetComponent<ArticyFlow>().EndCAL();
        CurCALObject = null;        
    }

    void PrintActionInfo(string actionName, Character_Action_FeatureFeature curActionData)
    {
        string s = "\tAction name: " + actionName + ", ";
        s += "Action type: " + curActionData.Action + ", ";
        s += "action info: " + curActionData.ActionInfo + ", ";
        s += "action Object: " + curActionData.ObjectToAct;
        Debug.Log(s);
    }
    public Text DebugText;    
    
}
