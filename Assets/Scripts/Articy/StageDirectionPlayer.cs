using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.Features;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Articy.Unity;

public class StageDirectionPlayer : MonoBehaviour
{
    TheCaptainsChair CaptainsChair;
    ArticyFlow ArticyFlow;
    List<NPC> ShutOffAIs = new List<NPC>();    

    private void Awake()
    {
        CaptainsChair = FindObjectOfType<TheCaptainsChair>();
        this.ArticyFlow = GetComponent<ArticyFlow>();        
    }

    public void HandleStangeDirectionContainer(Stage_Directions_Container sdc)
    {
        foreach (OutgoingConnection oc in sdc.InputPins[0].Connections)
        {
            Stage_Directions sd = oc.Target as Stage_Directions;
            Condition c = oc.Target as Condition;
            if(c != null)
            {
               // Debug.Log("We need to eval this condition");
                bool val = c.Expression.CallScript();
                if (val == false) continue;
                sd = c.OutputPins[0].Connections[0].Target as Stage_Directions;
            }
            if (sd == null) { Debug.LogError("We're expecting a Stage_Directions here: " + oc.Target.GetType()); continue; }
            if( HandleStageDirection(sd) == true) 
            {
                ArticyScriptInstruction instruction = sd.OutputPins[0].Text;
                if(instruction != null) instruction.CallScript();
            }
        }
    }

    public bool HandleStageDirection(Stage_Directions sd) 
    {        
        if (sd != null)
        {
            Stage_DirectionFeature sdf = sd.Template.Stage_Direction;
            switch (sdf.Direction)
            {
                case Direction.AI_Off:
                    List<string> aisToShutOff = sdf.DirectionTargets.Split(',').ToList();
                    //Debug.Log("num ai's to turn off: " + aisToShutOff.Count);
                    foreach (string s in aisToShutOff)
                    {
                        NPC npc = CaptainsChair.GetNPCFromActorName(s);
                        if (npc == null) { Debug.LogError("There's no NPC associated with the provided name. " + s); return false; }
                        StopAIOnNPC(npc);
                    }
                    break;
                case Direction.AI_On:
                    List<string> aisToTurnOn = sdf.DirectionTargets.Split(',').ToList();
                   // Debug.Log("num ai's to turn on: " + aisToTurnOn.Count);
                    foreach (string s in aisToTurnOn)
                    {
                        NPC npc = CaptainsChair.GetNPCFromActorName(s);
                        if (npc == null) { Debug.LogError("There's no NPC associated with the provided name. " + s); return false; }
                        StartAIOnNPC(npc, true);
                    }
                    break;
                case Direction.Dialogue_Interact_On:                    
                    this.ArticyFlow.IsDialogueFragmentsInteractive = true;
                    break;
                case Direction.Dialogue_Interact_Off:
                    this.ArticyFlow.IsDialogueFragmentsInteractive = false;
                    break;
                case Direction.Background_Track_Change:
                    BackgroundMusicPlayer.Play(sdf.Direction_Info);
                    break;
                case Direction.SFX:
                    SoundFXPlayer.Play(sdf.Direction_Info);
                    break;
                case Direction.VFX:
                    Vector3 vfxPos = Vector3.zero;
                    string[] locInfo = sdf.DirectionTargets.Split(',');
                    if(locInfo.Length == 1)
                    {
                        GameObject posObj = GameObject.Find(sdf.DirectionTargets);
                        if (posObj == null) { Debug.LogError("Trying to play VFX but no location object in scene"); break; }
                        vfxPos = posObj.transform.position;
                    }
                    else
                    {
                        vfxPos = new Vector3(float.Parse(locInfo[0]), float.Parse(locInfo[1]), float.Parse(locInfo[2]));
                    }
                    VisualFXPlayer.Play(sdf.Direction_Info, vfxPos);                        
                    break;
                case Direction.Dialogue_Text_Speed:
                    if (sdf.Direction_Info.Equals("Default")) ArticyFlow.TypewriterSpeed = ArticyFlow.GetDefaultTypewriterSpeed(); //TypewriterSpeed = ConvoUI.DefaultTypewriterSpeed;
                    else ArticyFlow.TypewriterSpeed = float.Parse(sdf.Direction_Info);
                   // Debug.Log("TypewriterSpeed: " + ArticyFlow.TypewriterSpeed);
                    break;

                default:
                    Debug.LogError("Unknown how to handle this Stage_Direction type: " + sdf.Direction);
                    return false;
                    break;
            }      
        }
        return true;
    }

    public void StartAIOnNPC(NPC npc, bool removeFromShutOffAis = false)
    {
        npc.RestartBehavior();
        if (removeFromShutOffAis == true && ShutOffAIs.Contains(npc)) ShutOffAIs.Remove(npc);
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

    public void ShutOnAllAIs()
    {
        foreach (NPC npc in ShutOffAIs)
        {
            StartAIOnNPC(npc);
        }
        ShutOffAIs.Clear();
    }

    public string GetShutOffAINames()
    {
        string s = "";
        foreach (NPC npc in ShutOffAIs)
        {
            s += "\t" + npc.name + "\n";
        }
        return s;
    }
}
