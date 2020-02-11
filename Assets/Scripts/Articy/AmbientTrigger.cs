using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Articy.The_Captain_s_Chair;
using Articy.Unity;
using System.Linq;

public class AmbientTrigger : MonoBehaviour
{
    List<NPC> ShutOffAIs = new List<NPC>();
    TheCaptainsChair CaptainsChair;
    Character_Action_List_Template Behavior;

    void Start()
    {
        CaptainsChair = GameObject.FindObjectOfType<TheCaptainsChair>();
    }
    
    public void StopAIOnNPC(NPC npc)
    {
        if (ShutOffAIs.Contains(npc))
        {
            Debug.LogWarning("This npc is already on the list so we're bailing: " + npc.name);
            return;
        }
        BehaviorFlowPlayer bfp = npc.GetComponent<BehaviorFlowPlayer>();
        bfp.StopBehavior();
        Debug.Log("adding this to the shut off list: " + npc.name);
        ShutOffAIs.Add(npc);
    }
    public void StartAIOnNPC(NPC npc, bool removeFromShutOffAis = false)
    {
        npc.RestartBehavior();
        if (removeFromShutOffAis == true && ShutOffAIs.Contains(npc)) ShutOffAIs.Remove(npc);
    }

    public void ProcessAmbientTrigger(Ambient_Trigger at)
    {
        Condition c = (at.InputPins[0].Connections[0].Target) as Condition;
        if (c == null) { Debug.LogError("The first node in this Ambient_Triger isn't a Condition"); return; }
        bool o = c.Expression.CallScript();
        Debug.Log("o: " + o);
        if(o==true)
        {
            Debug.Log("we've already done this Ambient_Trigger so bail");
            return;
        }

        ArticyObject target = c.OutputPins[1].Connections[0].Target;
        Stage_Directions sd = target as Stage_Directions;
        Behavior = target as Character_Action_List_Template;

        if (sd != null)
        {
            Debug.Log("this ambient trigger is starting off with Stage_Directions");
            HandleStageDirections(sd);
            target = sd.OutputPins[0].Connections[0].Target;
            Debug.Log("next target is: " + target.GetType());
            Behavior = target as Character_Action_List_Template;
        }
        if(Behavior == null ) { Debug.LogError("there's no Character_Action_List in this Ambient Trigger"); return; }
        BehaviorFlowPlayer bfp = GetComponent<BehaviorFlowPlayer>();
        if(bfp == null) { Debug.LogError("There's no BehaviorFlowPlayer on this AmbientTrigger."); return; }
        bfp.StartBehaviorFlow(Behavior, this.gameObject);
    }

    public void EndCAL()
    {
        Debug.Log("ending CAL on an ambient trigger");
        foreach(NPC npc in ShutOffAIs)
        {
            StartAIOnNPC(npc);
        }
        ShutOffAIs.Clear();
        Behavior.OutputPins[0].Text.CallScript();
        Behavior = null;
    }

    public void HandleStageDirections(Stage_Directions sd)
    {
        if (sd != null)
        {
            if (sd.Template.Stage_Direction_String_Lists.AITurnOff != "")
            {
                List<string> aisToShutOff = sd.Template.Stage_Direction_String_Lists.AITurnOff.Split(',').ToList();
                // Debug.Log("num ai's: " + aisToShutOff.Count);
                foreach (string s in aisToShutOff)
                {
                    NPC npc = CaptainsChair.GetNPCFromActorName(s);
                    if (npc == null) { Debug.LogError("There's no NPC associated with the provided name. " + s); return; }
                    StopAIOnNPC(npc);
                }
            }
            if (sd.Template.Stage_Direction_String_Lists.AITurnOn != "")
            {
                List<string> aisToTurnOn = sd.Template.Stage_Direction_String_Lists.AITurnOn.Split(',').ToList();
                // Debug.Log("num ai's: " + aisToTurnOn.Count);
                foreach (string s in aisToTurnOn)
                {
                    NPC npc = CaptainsChair.GetNPCFromActorName(s);
                    if (npc == null) { Debug.LogError("There's no NPC associated with the provided name. " + s); return; }
                    StartAIOnNPC(npc, true);
                }
            }
        }
    }
}
