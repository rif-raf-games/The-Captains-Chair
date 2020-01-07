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
    
    Character_Action_List_FeatureFeature CurCALFeature = null;
    Dictionary<CharacterEntity, EntitySaveData> CurCALEntitySaveData = new Dictionary<CharacterEntity, EntitySaveData>();
    int CurCALIndex = 0;
    Branch CALNextBranch = null;

    private void Start()
    {
        CamFollow = FindObjectOfType<CamFollow>();        
    }

    public void BeginCAL(IFlowObject calObject, Branch calNextBranch)
    {        
        CurCALIndex = 0;
        CALNextBranch = calNextBranch;
        CurCALFeature = (calObject as Character_Action_List_Template).Template.Character_Action_List_Feature;
        CurCALEntitySaveData.Clear();
        StartCoroutine(PlayCAL());
    }

    IEnumerator PlayCAL()
    {
        bool isCALDone = false;
        while (isCALDone == false)
        {
            float animTime = -1f;
            bool isActionDone = false;
            bool isMoveDone = false;
            bool isRotDone = false;
            float timer = 0f;
            Quaternion LerpRotStart = Quaternion.identity;
            Quaternion LerpRotEnd = Quaternion.identity;

            Character_Action_Template curActionTemplate = CurCALFeature.ActionStrip[CurCALIndex] as Character_Action_Template;
            Character_Action_FeatureFeature curActionData = curActionTemplate.Template.Character_Action_Feature;
            GameObject objectToAct = GameObject.Find(curActionData.ObjectToAct);      // mochange                      
            if (objectToAct == null) { Debug.LogError("No object in the scene called: " + curActionData.ObjectToAct); yield break; }
            CharacterEntity curEntityObject = objectToAct.GetComponent<CharacterEntity>();
            if (curEntityObject == null) { Debug.LogError("There's no CharacterEntity object on the ObjectToAct: " + curActionData.ObjectToAct); yield break; }
            PrintCurActionDataInfo(curActionData);

            // set up the current action
            switch (curActionData.Action)
            {
                case Action.Walk:
                    if (curActionData.ActionInfo == "Start_Position")
                    {
                        if (CurCALEntitySaveData.ContainsKey(curEntityObject) == false) { Debug.LogError("This entity object isn't in the save data list. Make sure Start_Position isn't the first ActionInfo for walking on this entity: " + curEntityObject.name); yield break; }
                        curEntityObject.SetNavMeshDest(CurCALEntitySaveData[curEntityObject].StartPos);
                    }
                    else
                    {
                        string[] pos = curActionData.ActionInfo.Split(',');
                        Vector3 loc = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
                        if (CurCALEntitySaveData.ContainsKey(curEntityObject) == false)
                        {
                            CurCALEntitySaveData.Add(curEntityObject, new EntitySaveData(curEntityObject.transform.position, curEntityObject.GetStoppingDist(), curEntityObject.GetShouldFollowEntity()));
                            curEntityObject.SetStoppingDist(0f);
                            curEntityObject.ToggleFollowEntity(false);
                        }
                        curEntityObject.SetNavMeshDest(loc);
                    }
                    if (curActionData.BeginCameraFollow == true)
                    {
                        string[] vec = curActionData.CamFollowOffset.Split(',');
                        Vector3 dist = new Vector3(float.Parse(vec[0]), float.Parse(vec[1]), float.Parse(vec[2]));
                        CamFollow.SetupNewCamFollow(curEntityObject, dist);
                    }
                    break;

                case Action.Animation:
                    animTime = curEntityObject.PlayAnim(curActionData.ActionInfo);
                    if (animTime == -1) { Debug.LogError("ERROR: " + curActionData.ActionInfo + " is not in the " + curEntityObject.name + "'s Animtor"); yield break; }
                    break;
                case Action.NoAction:
                    if (curActionData.BeginCameraFollow == true)
                    {
                        string[] vec = curActionData.CamFollowOffset.Split(',');
                        Vector3 dist = new Vector3(float.Parse(vec[0]), float.Parse(vec[1]), float.Parse(vec[2]));
                        CamFollow.SetupNewCamFollow(curEntityObject, dist);
                    }
                    isActionDone = true;
                    break;

                default:
                    Debug.LogError("This Action is not supported: " + curActionData.Action);
                    yield break;
            }

            // perform the current action                        
            while (isActionDone == false)
            {
                switch (curActionData.Action)
                {
                    case Action.Walk:
                        if (isMoveDone == false)
                        {
                            isMoveDone = curEntityObject.NavMeshDone();
                            if (isMoveDone == true)
                            {
                                if (curActionData.LookAtEntityWhenDone == true)
                                {
                                    GameObject objectToLookAt = GameObject.Find(curActionData.ObjectToLookAt);                                      
                                    if (objectToLookAt == null) { Debug.LogError("No object in the scene to look at called: " + curActionData.ObjectToLookAt); yield break; }
                                    CharacterEntity entityToLookAt = objectToLookAt.GetComponent<CharacterEntity>();
                                    if (entityToLookAt == null) { Debug.LogError("no object in the scene with this name: " + curActionData.ObjectToLookAt + "that has a CharacterEntity component"); yield break; }

                                    LerpRotStart = curEntityObject.transform.rotation;
                                    curEntityObject.transform.LookAt(entityToLookAt.transform);
                                    LerpRotEnd = curEntityObject.transform.rotation;
                                    curEntityObject.transform.rotation = LerpRotStart;
                                }
                                else
                                {
                                    isRotDone = true;
                                }
                            }
                        }
                        else if (isRotDone == false)
                        {
                            timer += Time.deltaTime * 5f;
                            curEntityObject.transform.rotation = Quaternion.Lerp(LerpRotStart, LerpRotEnd, timer);
                            if (timer >= 1f)
                            {
                                curEntityObject.transform.rotation = LerpRotEnd;
                                isRotDone = true;
                            }
                        }
                        if (isMoveDone && isRotDone) isActionDone = true;
                        break;

                    case Action.Animation:
                        timer += Time.deltaTime;
                        if (timer > animTime)
                        {
                            Debug.Log("anim is done");
                            isActionDone = true;
                        }
                        break;
                }
                yield return new WaitForEndOfFrame();
            }

            Debug.Log("action done");
            CurCALIndex++;
            if (CurCALIndex >= CurCALFeature.ActionStrip.Count)
            {
                Debug.Log("done with this action list.");
                isCALDone = true;
            }
            yield return new WaitForEndOfFrame();
        }

        foreach (KeyValuePair<CharacterEntity, EntitySaveData> entry in CurCALEntitySaveData)
        {
            entry.Key.SetStoppingDist(entry.Value.NavMeshStoppingDist);
            entry.Key.SetShouldFollowEntity(entry.Value.ShouldFollowEntity);
        }

        GetComponent<ArticyFlow>().EndCAL(CALNextBranch);
        CurCALFeature = null;
        CALNextBranch = null;
        
        
        /* DialogueFragment df = CALNextBranch.Target as DialogueFragment;

         if (df != null)
         {
             Debug.Log("ready to head back to conversation");
             CurArticyState = ArticyState.CONVERSATION;
         }
         else
         {
             Debug.LogWarning("We haven't added support for Action Lists continuing on anything other than DialogueFragments so see what happens.");
         }
         foreach (KeyValuePair<CharacterEntity, EntitySaveData> entry in CurCALEntitySaveData)
         {
             entry.Key.SetStoppingDist(entry.Value.NavMeshStoppingDist);
             entry.Key.SetShouldFollowEntity(entry.Value.ShouldFollowEntity);
         }
         NextBranch = CALNextBranch;
         CurCALFeature = null;
         CALNextBranch = null;*/
    }

    CharacterEntity GetEntityObjectFromActionEntity(ArticyObject characterEntity)
    {        
        foreach (CharacterEntity go in GetComponent<ArticyFlow>().GetCharacterEntities())
        {
            ArticyReference ar = go.GetComponent<ArticyReference>();
            if (ar == null) { Debug.LogError("Our Player has no ArticyReference?"); return null; }
            if (ar.reference.GetObject() == characterEntity)
            {
                return go;
            }
        }
        return null;
    }

    void PrintCurActionDataInfo(Character_Action_FeatureFeature curActionData)
    {
        string s = "-------Begin Action with object: " + curActionData.ObjectToAct + ", Action: " + curActionData.Action + ", info: " + curActionData.ActionInfo + "\n";
        s += "Begin Cam Follow: " + curActionData.BeginCameraFollow + ", Cam Follow Offset: " + curActionData.CamFollowOffset + "\n";
        s += "Look at entity when done: " + curActionData.LookAtEntityWhenDone;
        if(curActionData.LookAtEntityWhenDone == true) s += ", object to look at: " + curActionData.ObjectToLookAt;        
        Debug.Log(s);
    }



    string GetCharacterEntityDisplayName(ArticyObject entity)
    {
        Debug.Log("GetName(): " + entity.GetType());
        if (entity.GetType().Equals(typeof(npcCrew))) return (entity as npcCrew).DisplayName;
        else if (entity.GetType().Equals(typeof(PC))) return (entity as PC).DisplayName;
        else return "Unknown type: " + entity.GetType().ToString();
    }
}
