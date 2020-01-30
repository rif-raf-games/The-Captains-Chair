using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Articy.Unity;
using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.Features;
using UnityEngine.UI;
using System.Linq;

public class ArticyFlow : MonoBehaviour, IArticyFlowPlayerCallbacks, IScriptMethodProvider
{
    public enum eArticyState { FREE_ROAM, DIALOGUE, AI, AMBIENT_TRIGGER, NUM_ARTICY_STATES };
    public eArticyState CurArticyState;

    // objet references
    TheCaptainsChair CaptainsChair;
    CCPlayer Player;
    ArticyFlowPlayer FlowPlayer;

    // flow stuff
    IFlowObject CurPauseObject = null;
    List<Branch> CurBranches = new List<Branch>();
    //DialogueFragment LastDFPlayed;
    Branch NextBranch = null;
    public ArticyObject NextFragment = null;
    //Character_Movement_FeatureFeature CurCMFeature = null;

    // list of characters in the scene for quick reference
   /* List<CharacterEntity> CharacterEntities = new List<CharacterEntity>(); // this is temp until I update the scripts    
    public List<CharacterEntity> GetCharacterEntities() { return CharacterEntities; }*/

    // UI's for cutscenes and conversations
   // public CutSceneUI CutSceneUI;
    public ConvoUI ConvoUI;

    // Articy stuff
    public bool IsCalledInForecast { get; set; }
                   
    //debug
    public Text DebugText;

    void Start()
    {
        CurArticyState = eArticyState.NUM_ARTICY_STATES;

        Player = GameObject.FindObjectOfType<CCPlayer>();
        CaptainsChair = GameObject.FindObjectOfType<TheCaptainsChair>();
        FlowPlayer = this.GetComponent<ArticyFlowPlayer>();
        //CharacterEntities = GameObject.FindObjectsOfType<CharacterEntity>().ToList();

        ArticyDatabase.DefaultGlobalVariables.Notifications.AddListener("*.*", MyGameStateVariablesChanged);

        ActiveCALPauseObjects.Clear();
    }

    #region ARTICY
    /// <summary>
    /// Called from Articy when a variable changes
    /// </summary>    
    void MyGameStateVariablesChanged(string aVariableName, object aValue)
    {
        //Debug.Log("aVariableName: " + aVariableName + " changed to: " + aValue.ToString());
        CaptainsChair.SaveSaveData();
    }

    /// <summary>
    /// This is called from an Articy fragment defined in the project file
    /// </summary>
    public void OpenCaptainsDoor()
    {
        if (IsCalledInForecast == false)
        {
            Debug.Log("-------------------------------------------------------------- called OpenCaptainsDoor() but we're changing functionality");
        }
        else
        {
            Debug.Log("-------------------------------------------------------------- OpenCaptainDoor(): Do NOT open door, we're just forecasting");
        }
    }

    public void DeleteSaveData()
    {
        if (IsCalledInForecast == false)
        {
            CaptainsChair.DeleteSaveData();
        }        
    }
    #endregion

    #region ARTICY_FLOW       
    
    public void StopForPuppetShow()
    {
        SetNextBranch(null);
        ActiveCALPauseObjects.Clear();
    }
    public void SetNextBranch(Branch nextBranch)
    {
        NextBranch = nextBranch;
    }

    /// <summary>
    /// Callback from Articy when we've reached a pause point
    /// </summary>    
    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() START ************* time: " + Time.time, this);
        if(this.name.Equals("Carver") || this.name.Equals("Grunfeld"))
        {
          //  Debug.Log(this.name + " OnFlowPlayerPaused() object type: " + aObject.GetType().ToString() + "num Updates: " + numUpdates);
        }
        //Debug.Log("************** OnFlowPlayerPaused() START *************");
        CurPauseObject = null;
        if(aObject == null)
        {
            //StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() END aObject WAS null***************");
            StaticStuff.PrintFlowPaused("this: " + this.name + " -************** OnFlowPlayerPaused() END aObject WAS null***************", this);
            return;
        }
        CurPauseObject = aObject;        
        StaticStuff.PrintFlowPaused("OnFlowPlayerPaused() IFlowObject Type: " + aObject.GetType() + ", with TechnicalName: " + ((ArticyObject)aObject).TechnicalName, this);
        //Debug.Log("OnFlowPlayerPaused() IFlowObject Type: " + aObject.GetType() + ", with TechnicalName: " + ((ArticyObject)aObject).TechnicalName);
        // keep track of the technical names of the nodes we've visited
        if(CurArticyState != eArticyState.FREE_ROAM && CurArticyState != eArticyState.NUM_ARTICY_STATES)
        {
           // string s = "#################################################### adding to list: " + ((ArticyObject)aObject).TechnicalName + ", of type: "+ aObject.GetType();
           // DialogueFragment df = aObject as DialogueFragment;
           // FlowFragment ff = aObject as FlowFragment;
           // if (df != null) s += ", a dialogue frag: " + df.Text;
           // else if (ff != null) s += ", info: " + ff.DisplayName;
            //Debug.Log(s);
            FlowFragsVisited.Add(((ArticyObject)aObject).TechnicalName);
        }                      
        StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() END aObject was NOT null*************** time: " + Time.time, this);
        //Debug.Log("************** OnFlowPlayerPaused() END aObject was NOT null***************");
    }

    void StartAIOnNPC(NPC npc)
    {
        npc.RestartBehavior();
        //ArticyFlowPlayer npcAFP = npc.GetComponent<ArticyFlowPlayer>();
        //npcAFP.StartOn = npc.ArticyAIReference.GetObject();
        // npcAFP.Play();
    }

    List<NPC> ShutOffAIs = new List<NPC>();
    void StopAIOnNPC(NPC npc)
    {
        if(ShutOffAIs.Contains(npc))
        {
            Debug.LogWarning("This npc is already on the list so we're bailing: " + npc.name);
            return;
        }
        BehaviorFlowPlayer bfp = npc.GetComponent<BehaviorFlowPlayer>();
        bfp.StopBehavior();
       // ArticyFlowPlayer npcAFP = npc.GetComponent<ArticyFlowPlayer>();
        //ArticyFlow npcAF = npc.GetComponent<ArticyFlow>();
       // CharacterActionList npcCAL = npc.GetComponent<CharacterActionList>();
       // npcAF.StopForPuppetShow();
       // npcCAL.StopCAL();
      //  npcAFP.StartOn = CaptainsChair.FlowPauseTarget.GetObject();
        //npcAFP.Play();
        Debug.Log("adding this to the shut off list: " + npc.name);
        ShutOffAIs.Add(npc);
    }
    
    int numUpdates = 1;
    /// <summary>
    /// Callback from Articy when it's calculated the available branches
    /// </summary>
 
    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
        StaticStuff.PrintFlowBranchesUpdate("************** OnBranchesUpdated() START ************* time: " + Time.time, this);        
        StaticStuff.PrintFlowBranchesUpdate("Num branches: " + aBranches.Count, this);
        // Debug.Log("************** OnBranchesUpdated() START *************");
        //Debug.Log("Num branches: " + aBranches.Count);
        if (this.name.Equals("Carver") || this.name.Equals("Grunfeld"))
        {
           // Debug.Log(this.name + " OnBranchesUpdated() first branch : " + aBranches[0].Target.GetType().ToString());
        }

        CurBranches.Clear();
        
        int i = 0;
        foreach (Branch b in aBranches)
        {
            StaticStuff.PrintFlowBranchesUpdate("branch: " + i + " is type: " + b.Target.GetType(), this);                       
            if (b.IsValid == false) Debug.LogWarning("Invalid branch in OnBranchesUpdate(): " + b.DefaultDescription);
            CurBranches.Add(b);
            i++;
        }
        DialogueFragment df = CurPauseObject as DialogueFragment;        
        if(df != null )
        {
            StaticStuff.PrintFlowBranchesUpdate("We're on a dialogue fragment, so set the text based on current flow state.", this);
            //LastDFPlayed = df;
            switch (CurArticyState)
            {               
                case eArticyState.DIALOGUE:
                    if (ActiveCALPauseObjects.Count == 0) Player.StopNavMeshMovement();
                    if(DialogueNPC != null && df.Speaker == null)
                    {
                        df.Speaker = DialogueNPC.ArticyEntityReference.GetObject();
                    }
                    ConvoUI.ShowDialogueFragment(df, CurPauseObject, CurBranches);
                    break;
                default:
                    Debug.LogError("In invalid state for DialogueFragment: " + CurArticyState);
                    break;
            }
        }
        else if (CurPauseObject.GetType().Equals(typeof(Character_Action_List_Template)))
        {
            //StaticStuff.PrintFlowBranchesUpdate("-----------------------------------We've arrived at a Character Action List so let's see what's up", this);                
            Debug.Log("-----------------------------------We've arrived at a Character Action List so let's see what's up", this);
            SetNextBranch(null);
            OutputPin outputPin = (CurPauseObject as FlowFragment).OutputPins[0];
            NextFragment = (outputPin.Connections[0].Target as ArticyObject);
            Debug.Log(this.name + ": NextFragment: " + NextFragment.TechnicalName);            
            Character_Action_List_Template CurCAL = CurPauseObject as Character_Action_List_Template;
            Character_Action_List_FeatureFeature CurCALObject = CurCAL.Template.Character_Action_List_Feature;
            ActiveCALPauseObjects = CurCALObject.PauseFrags;
            //  Debug.Log(this.name + " is about to play it's CAL with a calCount of: " + calCount);
            // Debug.Log(this.name + " is it's CAL currently active?: " + GetComponent<CharacterActionList>().IsActive);
            //if(GetComponent<CharacterActionList>().IsActive == true)
            //  {
            //     Debug.LogError(this.name + " has a Behavior active and we're trying to start it again before the last one finished");
            // }
            Debug.Log(this.name + " is about to start their Behavior.  Time: " + Time.time);

            //Debug.LogWarning("time to replace this with a behavior player");
            GetComponent<BehaviorFlowPlayer>().StartBehaviorFlow(CurCAL, this.gameObject/*, DialogueNPC*/);
            calCount++;
            // Debug.Log(this.name + " now has a calCount of: " + calCount);
        }
        else if (CurBranches.Count == 1)
        {   // We're paused and there's only one valid branch available. This is common so have it's own section                 
            if (CurPauseObject.GetType().Equals(typeof(Dialogue)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're about to start a Dialogue but it may NOT always start with a dialogue fragment.", this);
                CurArticyState = eArticyState.DIALOGUE;
                SetNextBranch(CurBranches[0]);
            }
            else if(CurPauseObject.GetType().Equals(typeof(Jump)))
            {
                StaticStuff.PrintFlowBranchesUpdate("we're on a Jump, so just jump to where it's supposed to go.", this);
                SetNextBranch(CurBranches[0]);
            }
            else if (CurPauseObject.GetType().Equals(typeof(Hub)) )//&& CurBranches[0].Target.GetType().Equals(typeof(OutputPin)))
            {
                if(CurBranches[0].Target.GetType().Equals(typeof(OutputPin)))
                {
                    StaticStuff.PrintFlowBranchesUpdate("*****************************************We're paused on a Hub with no Target so we're in Free Roam.", this);
                    //Debug.LogWarning("*****************************************We're paused on a Hub with no Target so we're in Free Roam.", this);
                    //Debug.LogWarning("We're about to clear the FlowFragsVisited list: NOT in a dialogue, on a hub with an output pis as the branch, don't Play() anything but go back to FREE_ROAM");
                    if (CurArticyState == eArticyState.DIALOGUE)
                    {
                       // Debug.LogWarning("Coming out of a dialogue");
                        Player.ToggleMovementBlocked(false);
                       // Player.GetComponent<CapsuleCollider>().enabled = true;
                        if (DialogueNPC != null)
                        {
                            StartAIOnNPC(DialogueNPC);
                            //DialogueNPC.GetComponent<BoxCollider>().enabled = true;
                            DialogueNPC = null;
                        }
                        foreach (NPC npc in ShutOffAIs)
                        {
                            StartAIOnNPC(npc);
                        }
                        ShutOffAIs.Clear();
                    }
                    else if(CurArticyState == eArticyState.AMBIENT_TRIGGER)
                    {
                        //Debug.LogWarning("Coming out of an ambient trigger");
                        foreach (NPC npc in ShutOffAIs)
                        {
                            StartAIOnNPC(npc);
                        }
                        ShutOffAIs.Clear();
                    }
                    CurArticyState = eArticyState.FREE_ROAM;
                    FlowFragsVisited.Clear();
                }
                else
                {
                    Debug.Log("We're paused on a hub that has an actual target so for now that means we're in a dialogue that's actually going to play state: " + CurArticyState);
                    if(CurArticyState == eArticyState.DIALOGUE)
                    {
                        DialogueNPC = null;
                        if (DialogueStartAttachments.Count != 0)
                        {
                            Debug.Log("we've got at least one attachment");
                            foreach (ArticyObject ao in DialogueStartAttachments)
                            {
                                Entity e = ao as Entity;
                                if (e.DisplayName.Equals("Dialogue_NPC"))
                                {
                                    DialogueNPC = DialogueStartCollider.GetComponent<NPC>();
                                    if (DialogueNPC == null) { Debug.LogError("There's no NPC on the Dialgue_NPC you collided with"); return; }
                                    StopAIOnNPC(DialogueNPC);
                                    // DialogueNPC.GetComponent<BoxCollider>().enabled = false;
                                }
                                else
                                {
                                    Debug.Log("non dialogue NPC: " + e.DisplayName);
                                }
                            }
                        }                        
                    }
                    else if(CurArticyState == eArticyState.AMBIENT_TRIGGER)
                    {
                        Debug.Log("Start the AMBIENT_TRIGGER");
                    }
                    FlowFragsVisited.Clear();
                    SetNextBranch(CurBranches[0]);
                }
                                                
            }
            
            else if(CurPauseObject.GetType().Equals(typeof(AI_Template)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're starting an AI to get it going", this);
                CurArticyState = eArticyState.AI;                
                SetNextBranch(CurBranches[0]);
                FlowFragsVisited.Clear();
            }
            else if(CurPauseObject.GetType().Equals(typeof(Ambient_Trigger)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're about to get an Ambient_Trigger going", this);
                CurArticyState = eArticyState.AMBIENT_TRIGGER;
                SetNextBranch(CurBranches[0]);
            }
            else if(CurPauseObject.GetType().Equals(typeof(Stage_Directions)))
            {
                Stage_Directions sd = CurPauseObject as Stage_Directions;
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
                            StartAIOnNPC(npc);
                            if (ShutOffAIs.Contains(npc)) ShutOffAIs.Remove(npc);
                        }
                    }
                }
                SetNextBranch(CurBranches[0]);
            }
            else
            {
                Debug.LogWarning("We haven't supported this single branch situation yet. CurPauseObject: " + CurPauseObject.GetType() + ", branch: " + CurBranches[0].Target.GetType());
            }            
        }
        else if(CurPauseObject == null && CurBranches.Count == 0)
        { 
            string s = "===========================================We've got a null CurPauseObject and Branches count is 0 so lets see what state we're in.\n";
            if(CurArticyState == eArticyState.FREE_ROAM)
            {
                s += "we're in FREE_ROAM, so we're most likely an AI that's been put on a pause object, so just chill";
            }
            else if(CurArticyState == eArticyState.AMBIENT_TRIGGER)
            {
                s += "We're ending an Ambient_Trigger, so get the AI's back to normal";                
                foreach(NPC npc in ShutOffAIs)
                {
                    StartAIOnNPC(npc);
                }
                ShutOffAIs.Clear();                
            } 
            else
            {
                Debug.LogWarning("we're on a null CurPauseObject and 0 branches but we're not sure how to handle this particular case yet");
            }
            Debug.Log(s);
            SetNextBranch(null);
        }
        else
        {   
            Debug.LogWarning("this: " + this.name + " - We haven't supported this case yet.");
        }

        StaticStuff.PrintFlowBranchesUpdate("************** OnBranchesUpdated() END *************** time: " + Time.time, this);
        //Debug.Log("************** OnBranchesUpdated() END ***************");
    }
    int calCount = 0;
    void SetArticyState(eArticyState newState)
    {
        CurArticyState = newState;
    }

    #endregion

    public void StartAmbientFlow(Ambient_Trigger ambientTrigger)
    {
        Debug.Log("************************************ Want to start an ambient flow: " + ambientTrigger.name);
        FlowPlayer.StartOn = ambientTrigger;
    }
    NPC DialogueNPC = null;
    List<NPC> NPCsToPutOnHold = new List<NPC>();
    List<string> FlowFragsVisited = new List<string>();
    GameObject DialogueStartCollider = null;
    List<ArticyObject> DialogueStartAttachments;
    public void StartDialogue(Dialogue convoStart, GameObject collider)
    {        
        Debug.Log("************************************ Start Dialogue() with technical name: " + convoStart.TechnicalName + " on GameObject: " + this.gameObject.name + " but DON'T DO ANY CODE STUFF UNTIL WE KNOW WE'RE ACTUALLY COMMITTING  time: " + Time.time);
        DialogueStartCollider = collider;
        DialogueStartAttachments = convoStart.Attachments;
       // Player.GetComponent<CapsuleCollider>().enabled = false;
        FlowPlayer.StartOn = convoStart;
        Player.ToggleMovementBlocked(true);        
    }
    
    // monote
    // have AI Behaviors have their pause frag be blank 
    // have delay range work if it's just a single 0
    // move the convo UI stuff out of this and somewhere else because you only need 1
    // add timer to bark text so it's not always 5 second
    // add code to places where you don't need to drag references
    // expansions
    // have ambient triggers work on a random character

    List<ArticyObject> ActiveCALPauseObjects = new List<ArticyObject>(); 
    #region CHARACTER_ACTION_LIST    

    /// <summary>
    /// End the current Character Action List.  This is called from the CharacterActionList component
    /// </summary>
    public void EndCAL()
    {
        Debug.Log(this.name + " EndBehavior() with a time of: " + Time.time);
        Debug.Log("cur pause object type: " + CurPauseObject.GetType().ToString());
        ActiveCALPauseObjects.Clear();        
       // Debug.Log(this.name + " end of EndCal() with a calCount of: " + calCount);
    }
    #endregion

    #region UI

    

    

    public void UIButtonCallback(int buttonIndex)
    {
        StaticStuff.PrintUI("UIButtonCallback() buttonIndex: " + buttonIndex);
        //Debug.Log("UIButtonCallback() buttonIndex: " + buttonIndex);
        //NextBranch = CurBranches[buttonIndex];
        SetNextBranch(CurBranches[buttonIndex]);
        //StaticStuff.PrintUI("NextBranchType: " + NextBranch.Target.GetType());
       // Debug.Log("NextBranchType: " + NextBranch.Target.GetType());
        DialogueFragment df = CurBranches[buttonIndex].Target as DialogueFragment;        
        if(df != null)
        {
            StaticStuff.PrintUI("Chosen branch is a dialogue fragment, so just let the engine handle the next phase.");
            //Debug.Log("Chosen branch is a dialogue fragment, so just let the engine handle the next phase.");
        }
        else if (CurBranches[buttonIndex].Target.GetType().Equals(typeof(Character_Action_List_Template)))
        {
            //StaticStuff.PrintUI("Next thing is a Character_Actions_List_Template, so shut off the UI and let the action list do it's thing.");
            Debug.Log("---------------------------Next thing is a Character_Actions_List_Template, so shut off the UI and let the action list do it's thing.");
            EndUIs();
        }
        else
        {
            StaticStuff.PrintUI("Chosen branch isn't a DialogueFragment or a Character_Movement so for now just assume we're done talking and shut off the UI");
            //Debug.Log("----------------------------------- Chosen branch isn't a DialogueFragment or a Character_Movement so for now just assume we're done talking and shut off the UI");
            EndUIs();
        }        
    }

    void EndUIs()
    {
        //CutSceneUI.EndCutScene();
        ConvoUI.EndConversation();
    }
    #endregion

    bool WaitingOnActionList()
    {
        if (ActiveCALPauseObjects.Count == 0) return false;
        else
        {
            foreach(ArticyObject ao in ActiveCALPauseObjects)
            {
                if (FlowFragsVisited.Contains(ao.TechnicalName)) return true;
            }
            return false;            
        }        
    }
    // Update is called once per frame
    void Update()
    {        
        bool waitingOnAL = WaitingOnActionList();
       // if (NextFragment != null) Debug.Log("NextFragment: " + NextFragment.TechnicalName);
        if (DebugText != null)
        {
            DebugText.text = this.name + "\n";
            DebugText.text += CurArticyState.ToString() + "\n";
            //DebugText.text += "CALActive?: " + CALActive + "\n";
            if (NextBranch == null) DebugText.text += "NextBranch is null\n";
            else DebugText.text += "NextBranch is: " + NextBranch.DefaultDescription + "\n";
            if (NextFragment == null) DebugText.text += "NextFragment is null\n";
            else DebugText.text += "NextFragment is: " + NextFragment.TechnicalName + "\n";
            DebugText.text += "num active CAL pause objects: " + ActiveCALPauseObjects.Count + "\n";
            foreach (ArticyObject ao in ActiveCALPauseObjects) DebugText.text += "\t" + ao.TechnicalName + "\n";
            DebugText.text += "WaitingOnActionList()? " + waitingOnAL + "\n";
            foreach (string s in FlowFragsVisited)
            {
               // DebugText.text += s + "\n";
            }
            DebugText.text += "ShutOffAI's:\n";
            foreach (NPC npc in ShutOffAIs)
            {
                DebugText.text += "\t" + npc.name + "\n";
            }
        }
        if (NextBranch != null && waitingOnAL == false)
        {
            StaticStuff.PrintFlowBranchesUpdate(this.name + ": going to next branch via Update", this);
            Branch b = NextBranch;            
            SetNextBranch(null);                        
            FlowPlayer.Play(b);
        }    
        else if(NextFragment != null && waitingOnAL == false)
        {
            StaticStuff.PrintFlowBranchesUpdate(this.name + ": going to next FRAG via Update", this);
            ArticyObject ff = NextFragment;
            Debug.Log("set NextFragment to null");
            NextFragment = null;
            FlowPlayer.StartOn = ff;
        }
    }
}