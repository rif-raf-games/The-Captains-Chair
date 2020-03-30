using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Articy.The_Captain_s_Chair;
using Articy.Unity;
using System.Linq;
using Articy.The_Captain_s_Chair.Features;

// NOTE: THIS SHOULD BE REDONE!!!  This is using a lot of crap shared with ArticyFlow so figure it out (create an NPC AI pool you can use anywhere, etc)
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
        Stage_Directions_Container sdc = target as Stage_Directions_Container;
        Behavior = target as Character_Action_List_Template;

        if (sdc != null)
        {
            Debug.Log("this ambient trigger is starting off with Stage_Directions_Container");
            foreach (OutgoingConnection oc in sdc.InputPins[0].Connections)
            {
                Stage_Directions sd = oc.Target as Stage_Directions;
                if (sd == null) { Debug.LogError("We're expecting a Stage_Directions here: " + oc.Target.GetType()); continue; }
                HandleStageDirections(sd);
            }
            target = sdc.OutputPins[0].Connections[0].Target;
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
            Stage_DirectionFeature sdf = sd.Template.Stage_Direction;
            switch (sdf.Direction)
            {
                case Direction.AI_Off:
                    List<string> aisToShutOff = sdf.DirectionTargets.Split(',').ToList();
                    Debug.Log("num ai's to turn off: " + aisToShutOff.Count);
                    foreach (string s in aisToShutOff)
                    {
                        NPC npc = CaptainsChair.GetNPCFromActorName(s);
                        if (npc == null) { Debug.LogError("There's no NPC associated with the provided name. " + s); continue; }
                        StopAIOnNPC(npc);
                    }
                    break;
                case Direction.AI_On:
                    List<string> aisToTurnOn = sdf.DirectionTargets.Split(',').ToList();
                    Debug.Log("num ai's to turn on: " + aisToTurnOn.Count);
                    foreach (string s in aisToTurnOn)
                    {
                        NPC npc = CaptainsChair.GetNPCFromActorName(s);
                        if (npc == null) { Debug.LogError("There's no NPC associated with the provided name. " + s); return; }
                        StartAIOnNPC(npc, true);
                    }
                    break;
                case Direction.SFX:
                    SoundFXPlayer.Play(sdf.Direction_Info);
                    break;
                default:
                    Debug.LogError("Unknown how to handle this direction: " + sdf.Direction + " on an Ambient Trigger");
                    break;
            }
        }
    }
}
