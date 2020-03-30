using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Articy.Unity;
using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.Features;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using Articy.The_Captain_s_Chair.GlobalVariables;

public class ArticyFlow : MonoBehaviour, IArticyFlowPlayerCallbacks, IScriptMethodProvider
{
    public enum eArticyState { FREE_ROAM, DIALOGUE, AI, AMBIENT_TRIGGER, NUM_ARTICY_STATES };
    public eArticyState CurArticyState;

    // objet references
    TheCaptainsChair CaptainsChair;
    CCPlayer Player;
    ArticyFlowPlayer FlowPlayer;
    StageDirectionPlayer StageDirectionPlayer;

    // flow stuff
    IFlowObject CurPauseObject = null;
    List<Branch> CurBranches = new List<Branch>();    
    public bool IsDialogueFragmentsInteractive { get; set; }
    public float TypewriterSpeed { get; set; }        
    bool IsMoAMoron { get; set; }

    Branch NextBranch = null;
    ArticyObject NextFragment = null;

    public bool MoTest1 = false;
    public bool MoTest2 { get; set; }
    // UI's for cutscenes and conversations   
    public ConvoUI ConvoUI; public float GetDefaultTypewriterSpeed() { return ConvoUI.DefaultTypewriterSpeed; }

    // Articy stuff
    public bool IsCalledInForecast { get; set; }
                   
    //debug
    public Text DebugText;

    void Start()
    {
        //CurArticyState = eArticyState.NUM_ARTICY_STATES;
        SetArticyState(eArticyState.NUM_ARTICY_STATES);
        Player = GameObject.FindObjectOfType<CCPlayer>();
        CaptainsChair = GameObject.FindObjectOfType<TheCaptainsChair>();
        FlowPlayer = this.GetComponent<ArticyFlowPlayer>();
        StageDirectionPlayer = FindObjectOfType<StageDirectionPlayer>();

        TypewriterSpeed = ConvoUI.DefaultTypewriterSpeed;

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
        if(CaptainsChair != null) CaptainsChair.SaveSaveData();
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
      //  StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() START ************* time: " + Time.time, this);        
        CurPauseObject = null;
        if(aObject == null)
        {
            //StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() END aObject WAS null***************");
           // StaticStuff.PrintFlowPaused("this: " + this.name + " -************** OnFlowPlayerPaused() END aObject WAS null***************", this);
            return;
        }
        CurPauseObject = aObject;        
       // StaticStuff.PrintFlowPaused("OnFlowPlayerPaused() IFlowObject Type: " + aObject.GetType() + ", with TechnicalName: " + ((ArticyObject)aObject).TechnicalName, this);
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
        //StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() END aObject was NOT null*************** time: " + Time.time, this);
        //Debug.Log("************** OnFlowPlayerPaused() END aObject was NOT null***************");
    }

    public void SendToStageDirections(Stage_Directions_Container sdc)
    {
        StageDirectionPlayer.HandleStangeDirectionContainer(sdc);
    }   
                
    /// <summary>
    /// Callback from Articy when it's calculated the available branches
    /// </summary>
    ArticyScriptInstruction DialogueEndInstruction = null;
    Character_Action_List_Template CurCAL = null;
    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
        StaticStuff.PrintFlowBranchesUpdate("************** OnBranchesUpdated() START ************* time: " + Time.time, this);
        if (CurPauseObject == null) StaticStuff.PrintFlowBranchesUpdate("CurPauseObject is null", this);
        else StaticStuff.PrintFlowBranchesUpdate("CurPauseObject Type: " + CurPauseObject.GetType() + ", with TechnicalName: " + ((ArticyObject)CurPauseObject).TechnicalName, this);
        StaticStuff.PrintFlowBranchesUpdate("Num branches: " + aBranches.Count, this);        

        CurBranches.Clear();

        if (CurPauseObject == null)
            Debug.LogError("Null CurPauseObject we must be in a mini game");
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
                    if (ActiveCALPauseObjects.Count == 0 && Player != null) Player.StopNavMeshMovement();
                    if(DialogueNPC != null && df.Speaker == null)
                    {
                        df.Speaker = DialogueNPC.ArticyEntityReference.GetObject();
                    }
                    ConvoUI.ShowDialogueFragment(df, CurPauseObject, CurBranches, IsDialogueFragmentsInteractive, TypewriterSpeed);
                    break;
                default:
                    Debug.LogError("In invalid state for DialogueFragment: " + CurArticyState);
                    break;
            }
        }
        else if (CurPauseObject.GetType().Equals(typeof(Character_Action_List_Template)))
        {
            //StaticStuff.PrintFlowBranchesUpdate("-----------------------------------We've arrived at a Character Action List so let's see what's up", this);                
            StaticStuff.PrintFlowBranchesUpdate("-----------------------------------We've arrived at a Character Action List so let's see what's up", this);
            SetNextBranch(null);
            CurCAL = CurPauseObject as Character_Action_List_Template;
            OutputPin outputPin = (CurPauseObject as FlowFragment).OutputPins[0];
            NextFragment = (outputPin.Connections[0].Target as ArticyObject);
            StaticStuff.PrintFlowBranchesUpdate(this.name + ": NextFragment type: " + outputPin.Connections[0].Target.GetType(), this);
            StaticStuff.PrintFlowBranchesUpdate(this.name + ": NextFragment: " + NextFragment.TechnicalName, this);
            if(outputPin.Connections[0].Target as Dialogue != null)
            {
                StaticStuff.PrintFlowBranchesUpdate("next is a dialogue", this);
                Dialogue d = outputPin.Connections[0].Target as Dialogue;
                DialogueEndInstruction = d.OutputPins[0].Text;
                NextFragment = d.OutputPins[0].Connections[0].Target as ArticyObject;
            }
            
            //Character_Action_List_Template CurCAL = CurPauseObject as Character_Action_List_Template;
            Character_Action_List_FeatureFeature CurCALObject = CurCAL.Template.Character_Action_List_Feature;
            ActiveCALPauseObjects = CurCALObject.PauseFrags;
            StaticStuff.PrintFlowBranchesUpdate(this.name + " is about to start their Behavior.  Time: " + Time.time, this);
            //Debug.LogWarning("time to replace this with a behavior player");
            GetComponent<BehaviorFlowPlayer>().StartBehaviorFlow(CurCAL, this.gameObject);            
        }
        else if(CurPauseObject.GetType().Equals(typeof(Stage_Directions_Container)))
        {
            //NextFragment = (outputPin.Connections[0].Target as ArticyObject);
            Stage_Directions_Container sdc = CurPauseObject as Stage_Directions_Container;
            NextFragment = (sdc.OutputPins[0].Connections[0].Target as ArticyObject);
            // all of the branches are the stage directions we need to implement
            Debug.Log("Have a Stage_Directions_Container so all the branches are the stage directions.  Num: " + CurBranches.Count);
            foreach(Branch b in CurBranches)
            {                
                Stage_Directions sd = b.Target as Stage_Directions;                
                if(sd == null)
                {
                    Debug.LogError("There's something other than a Stage_Directions linked on this Stage_Directions_Container.");
                    continue;
                }
                StageDirectionPlayer.HandleStageDirection(sd); //mosd - ArticyFlow.cs OnBranchesUpdated(), CurPauseObject == Stage_Directions_Container
            }            
        }
        else if (CurBranches.Count == 1)
        {   // We're paused and there's only one valid branch available. This is common so have it's own section                 
            if (CurPauseObject.GetType().Equals(typeof(Dialogue)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're about to start a Dialogue but it may NOT always start with a dialogue fragment.", this);
                //Debug.LogWarning("We're about to start a Dialogue but it may NOT always start with a dialogue fragment.");
                //CurArticyState = eArticyState.DIALOGUE;
                SetArticyState(eArticyState.DIALOGUE);
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
                        if(Player != null ) Player.ToggleMovementBlocked(false);
                       // Player.GetComponent<CapsuleCollider>().enabled = true;
                        if (DialogueNPC != null)
                        {
                            StageDirectionPlayer.StartAIOnNPC(DialogueNPC);                            
                            DialogueNPC = null;
                        }
                        StageDirectionPlayer.ShutOnAllAIs();                        
                    }
                    else if(CurArticyState == eArticyState.AMBIENT_TRIGGER)
                    {
                        //Debug.LogWarning("Coming out of an ambient trigger");
                        // monote - this could be a problem because you might not want all AI's shut on after an Ambient Trigger because a Dialogue might be going on. REDO AMBIENT_TRIGGERS
                        StageDirectionPlayer.ShutOnAllAIs(); 
                    }
                    //CurArticyState = eArticyState.FREE_ROAM;
                    SetArticyState(eArticyState.FREE_ROAM);
                    FlowFragsVisited.Clear();
                }
                else
                {
                    Debug.Log("We're paused on a hub that has an actual target so for now that means we're in a dialogue that's actually going to play state: " + CurArticyState);
                    if(CurArticyState == eArticyState.DIALOGUE)
                    {
                        Debug.Log("Start the DIALOGUE but not sure if we need this with the new determining the dialogue is valid system");                                                
                    }
                    else if(CurArticyState == eArticyState.AMBIENT_TRIGGER)
                    {
                        Debug.Log("Start the AMBIENT_TRIGGER but not sure if we need this with the new determining the dialogue is valid system");
                    }
                    FlowFragsVisited.Clear();
                    SetNextBranch(CurBranches[0]);
                }
                                                
            }
            
            else if(CurPauseObject.GetType().Equals(typeof(AI_Template)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're starting an AI to get it going", this);
                //CurArticyState = eArticyState.AI;                
                SetArticyState(eArticyState.AI);
                SetNextBranch(CurBranches[0]);
                FlowFragsVisited.Clear();
            }
            else if(CurPauseObject.GetType().Equals(typeof(Ambient_Trigger)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're about to get an Ambient_Trigger going", this);
                //CurArticyState = eArticyState.AMBIENT_TRIGGER;
                SetArticyState(eArticyState.AMBIENT_TRIGGER);
                SetNextBranch(CurBranches[0]);
            }            
            else if(CurPauseObject.GetType().Equals(typeof(Stage_Directions)))
            {
                Debug.LogError("We should not be seeing a Stage_Directions element at this point here.");
                //Stage_Directions sd = CurPauseObject as Stage_Directions;
                //HandleStageDirections(sd);
                //SetNextBranch(CurBranches[0]);
            }
            else if(CurPauseObject.GetType().Equals(typeof(Scene_Jump)))
            {
                Scene_Jump sj = CurPauseObject as Scene_Jump;
                SceneManager.LoadScene(sj.Template.Next_Game_Scene.Scene_Name);                
            }
            else if(CurPauseObject.GetType().Equals(typeof(Mini_Game_Jump)))
            {
                Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");
                Mini_Game_Jump curJump = CurPauseObject as Mini_Game_Jump;
                jumpSave.Template.Mini_Game_Scene.Scene_Name = curJump.Template.Mini_Game_Scene.Scene_Name;
                jumpSave.Template.Mini_Game_Puzzles_To_Play.Puzzle_Numbers = curJump.Template.Mini_Game_Puzzles_To_Play.Puzzle_Numbers;
                jumpSave.Template.Dialogue_List.DialoguesToPlay = curJump.Template.Dialogue_List.DialoguesToPlay;
                jumpSave.Template.Next_Game_Scene.Scene_Name = curJump.Template.Next_Game_Scene.Scene_Name;
                jumpSave.Template.Flow_Start_Success.ReferenceSlot = curJump.Template.Flow_Start_Success.ReferenceSlot;
                jumpSave.Template.Flow_Start_Fail.ReferenceSlot = curJump.Template.Flow_Start_Fail.ReferenceSlot;
                ArticyGlobalVariables.Default.Mini_Games.Coming_From_Main_Game = true;
                Debug.Log("About to start a mini game: " + curJump.Template.Mini_Game_Scene.Scene_Name);
                SceneManager.LoadScene(curJump.Template.Mini_Game_Scene.Scene_Name);
                
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
                StageDirectionPlayer.ShutOnAllAIs();                            
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
            string s = "this: " + this.name + " - We haven't supported this case yet:\n";
            if (CurPauseObject == null) s += "NULL CurPauseObject\n";
            else s += "CurPauseObject type: " + CurPauseObject.GetType().ToString() + "\n";
            s += "num branches: " + CurBranches.Count + "\n";
            foreach(Branch b in CurBranches)
            {
                s += "/t" + b.Target.GetType() + "\n";
            }
            Debug.LogWarning(s);
            //Debug.LogWarning("this: " + this.name + " - We haven't supported this case yet.");
        }

        StaticStuff.PrintFlowBranchesUpdate("************** OnBranchesUpdated() END *************** time: " + Time.time, this);
        //Debug.Log("************** OnBranchesUpdated() END ***************");
    }
    
    void SetArticyState(eArticyState newState)
    {
        CurArticyState = newState;
        if(Player != null && newState == eArticyState.DIALOGUE)
        {
            Player.StartDialogue();
        }
        else if (Player != null && newState == eArticyState.FREE_ROAM)
        {
            Player.EndDialogue();
        }
    }
    /// <summary>
    /// End the current Character Action List.  This is called from the CharacterActionList component
    /// </summary>
    public void EndCAL(string callerName, string behaviorName)
    {
        Debug.Log("----------------------------------------------------------------------------- " + callerName + " EndBehavior() " + behaviorName + ": with a time of: " + Time.time);
        Debug.Log("cur pause object type: " + CurPauseObject.GetType().ToString());
        CurCAL.OutputPins[0].Text.CallScript();        
        if (DialogueEndInstruction  != null)
        {
            Debug.Log("execute dialogue end instructions");
            DialogueEndInstruction.CallScript();
            DialogueEndInstruction = null;
        }
        ActiveCALPauseObjects.Clear();
    }
    #endregion

    public void StartAmbientFlow(Ambient_Trigger ambientTrigger)
    {
        Debug.Log("************************************ Want to start an ambient flow: " + ambientTrigger.name);
        // traverse the Ambient_Trigger until you find out if we're committing to it
        FlowPlayer.StartOn = ambientTrigger;
    }
    NPC DialogueNPC = null;
    public string GetDialogueNPCName()
    {
        if(DialogueNPC == null) { Debug.LogError("Trying to get the DialogueNPC name but the DialogueNPC is null."); return "Null NPC"; }
        return DialogueNPC.name;
    }
    
    public bool IsInFreeRoam()
    {
        return (CurArticyState == eArticyState.FREE_ROAM);
    }

    public void QuitCurDialogue()
    {
        //CurArticyState = eArticyState.FREE_ROAM;
        SetArticyState(eArticyState.FREE_ROAM);
        CurPauseObject = null;
        NextFragment = null;
        NextBranch = null;
        FlowFragsVisited.Clear();
        ActiveCALPauseObjects.Clear();
        if (Player != null) Player.ToggleMovementBlocked(false);
        ConvoUI.EndConversation();
    }

    List<string> FlowFragsVisited = new List<string>();
    GameObject DialogueStartCollider = null;
    List<ArticyObject> DialogueStartAttachments;
    public void CheckDialogue(Dialogue convoStart, GameObject collider)
    {        
        Debug.Log("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ CheckDialogue() with technical name: " + convoStart.TechnicalName + " on GameObject: " + this.gameObject.name + " but DON'T DO ANY CODE STUFF UNTIL WE KNOW WE'RE ACTUALLY COMMITTING  time: " + Time.time);
        if (CurArticyState == eArticyState.DIALOGUE)
        {
            Debug.Log("We're already in a Dialogue so bail on this one");
            return;
        }
        bool o;
        Condition c = (convoStart.InputPins[0].Connections[0].Target) as Condition;
        if (c == null) 
        {
            // if no conditional just assume we're gonna go through
            o = false;
        }
        else
        {
            o = c.Expression.CallScript();
        }        
        //Debug.Log("Have we done this dialogue?: " + o);
        if (o == true)
        {
            Debug.Log("we've already done this Dialogue so bail");
            return;
        }
       // Debug.Log("we've determined that this dialouge is ready to go so rock it");
        //CurArticyState = eArticyState.DIALOGUE;
        SetArticyState(eArticyState.DIALOGUE);
        DialogueStartCollider = collider;
        DialogueStartAttachments = convoStart.Attachments;

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
                    StageDirectionPlayer.StopAIOnNPC(DialogueNPC);
                    // DialogueNPC.GetComponent<BoxCollider>().enabled = false;
                }
                else
                {
                    Debug.Log("non dialogue NPC: " + e.DisplayName);
                }
            }
        }

        IsDialogueFragmentsInteractive = true;
        FlowPlayer.StartOn = convoStart;
        if(Player != null) Player.ToggleMovementBlocked(true); // mini games now have an articy flow player but not a CCplayer
    }        

    List<ArticyObject> ActiveCALPauseObjects = new List<ArticyObject>(); 
    #region CHARACTER_ACTION_LIST    

    
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
            StaticStuff.PrintUI("Next thing is a Character_Actions_List_Template, so shut off the UI and let the action list do it's thing.");
            //Debug.Log("---------------------------Next thing is a Character_Actions_List_Template, so shut off the UI and let the action list do it's thing.");
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
            DebugText.text += StageDirectionPlayer.GetShutOffAINames();
        }
        if (NextBranch != null && waitingOnAL == false)
        {            
            StaticStuff.PrintFlowBranchesUpdate(this.name + ": going to next branch via Update", this);
           // Debug.Log(this.name + ": going to next branch via Update");
            Branch b = NextBranch;            
            SetNextBranch(null);                        
            FlowPlayer.Play(b);
        }    
        else if(NextFragment != null && waitingOnAL == false)
        {
            StaticStuff.PrintFlowBranchesUpdate(this.name + ": going to next FRAG via Update", this);
           // Debug.Log(this.name + ": going to next FRAG via Update");
            ArticyObject ff = NextFragment;
            StaticStuff.PrintFlowBranchesUpdate("set NextFragment to null", this);
            NextFragment = null;
            FlowPlayer.StartOn = ff;
        }
    }
}

