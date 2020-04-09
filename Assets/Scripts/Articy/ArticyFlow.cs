﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Articy.Unity;
using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.Features;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using Articy.The_Captain_s_Chair.GlobalVariables;
using UnityEngine.Analytics;
using System;

public class ArticyFlow : MonoBehaviour, IArticyFlowPlayerCallbacks, IScriptMethodProvider
{
    public enum eArticyState { FREE_ROAM, DIALOGUE, AI, AMBIENT_TRIGGER, NUM_ARTICY_STATES };
    public eArticyState CurArticyState;

    // flow stuff
    ArticyFlowPlayer FlowPlayer;    // This is the Articy behind-the-scenes stuff that communicates with this file
    IFlowObject CurPauseObject = null;  // Current fragment we're paused on, as told to us by ArticyFlowPlayer
    List<Branch> CurBranches = new List<Branch>();  // Branches available from CurPauseObject
    Branch NextBranch = null;   // Next branch we're going to tell the flow player to continue on
    ArticyObject NextFragment = null;   // This is our stuff since we have to temp break the flow for Action Lists
    StageDirectionPlayer StageDirectionPlayer;  // Player for stage directions
    ArticyScriptInstruction DialogueEndInstruction = null;  // If we take over the flow for a CAL at the end of a Dialogue, keep track of that dialogue's end insturction for execution after the CAL
    Character_Action_List_Template CurCAL = null;   // Current CAL we're working showing

    // The objects you have to wait until you have visited before the dialogue will continue if you're in the middle of a CAL
    List<ArticyObject> ActiveCALPauseObjects = new List<ArticyObject>(); 
    List<string> FlowFragsVisited = new List<string>(); // List of flow fragments visited to check against ActiveCALPauseObjects
    
    NPC DialogueNPC = null; // The NPC you're in conversation with        

    // game stuff
    TheCaptainsChair CaptainsChair; // Core game
    CCPlayer Player;                // Captain/Player

    // UI stuff
    public ConvoUI ConvoUI; 
    public bool IsDialogueFragmentsInteractive { get; set; }    // Whether or not the UI dialogue is interactive or not
    public float TypewriterSpeed { get; set; }  // How fast the UI text types on screen    
               
    // Articy function callback stuff
    public bool IsCalledInForecast { get; set; } // This is required by Articy for code callbacks from the Articy flow stuff
                   
    [Header("Debug")]
    public Text DebugText;

    void Start()
    {        
        SetArticyState(eArticyState.NUM_ARTICY_STATES);
        Player = GameObject.FindObjectOfType<CCPlayer>();
        CaptainsChair = GameObject.FindObjectOfType<TheCaptainsChair>();
        FlowPlayer = this.GetComponent<ArticyFlowPlayer>();
        StageDirectionPlayer = FindObjectOfType<StageDirectionPlayer>();
        TypewriterSpeed = ConvoUI.DefaultTypewriterSpeed;
        ArticyDatabase.DefaultGlobalVariables.Notifications.AddListener("*.*", MyGameStateVariablesChanged);
        ActiveCALPauseObjects.Clear();
    }

    /// <summary>
    /// We now determine manually whether or not we're going to start a dialogue in order to solve lots of potential
    /// conflicts like more than 1 dialogue starting at once.  
    /// </summary>
    public void CheckIfDialogueShouldStart(Dialogue convoStart, GameObject collider)
    {
        Debug.Log("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ CheckDialogue() with technical name: " + convoStart.TechnicalName + " on GameObject: " + this.gameObject.name + " but DON'T DO ANY CODE STUFF UNTIL WE KNOW WE'RE ACTUALLY COMMITTING  time: " + Time.time);
        
        if (CurArticyState == eArticyState.DIALOGUE)
        {   // we're already in a dialogue, so bail
            return;
        }
        bool shouldBail;
        Condition c = (convoStart.InputPins[0].Connections[0].Target) as Condition;
        if (c == null)
        {
            // if no conditional just assume we're gonna go through
            shouldBail = false;
        }
        else
        {   // evaulate the expression to findo out if we should bail
            shouldBail = c.Expression.CallScript();
        }
        
        if (shouldBail == true)
        {
            // for whatever reason determined above we're gonna bail so do it
            return;
        }
        
        // if we made it this far then we're going to start the dialogue so get ready        
        SetArticyState(eArticyState.DIALOGUE);                
        // the DialogueNPC is who you are talking to during a dialogue with a random NPC.  Need more info about the NPC
        // because it's not built into the Dialogue Fragments since the dialogue can happen with random people
        DialogueNPC = null;
        if (convoStart.Attachments.Count != 0)
        {   // if we have attachments, then we're starting a dialogue with an NPC so get that NPC's references            
            foreach (ArticyObject ao in convoStart.Attachments)
            {
                Entity e = ao as Entity;
                if (e.DisplayName.Equals("Dialogue_NPC"))
                {
                    DialogueNPC = collider.GetComponent<NPC>();
                    if (DialogueNPC == null) { Debug.LogError("There's no NPC on the Dialgue_NPC you collided with"); return; }
                    StageDirectionPlayer.StopAIOnNPC(DialogueNPC);                    
                }                
            }
        }

        // start the dialogue
        IsDialogueFragmentsInteractive = true;
        FlowPlayer.StartOn = convoStart;
        if (Player != null) Player.ToggleMovementBlocked(true); 
    }

    #region ARTICY_FLOW_CALLBACKS    
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
            FlowFragsVisited.Add(((ArticyObject)aObject).TechnicalName);
        }                      
        //StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() END aObject was NOT null*************** time: " + Time.time, this);
        //Debug.Log("************** OnFlowPlayerPaused() END aObject was NOT null***************");
    }

    /*void OnGUI()
    {
        if(GUI.Button(new Rect(100,100,100,100), "feh"))
        {
            Debug.Log(Environment.StackTrace);
        }
    }*/
    /// <summary>
    /// Callback from Articy when it's calculated the available branches
    /// </summary>
    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
        StaticStuff.PrintFlowBranchesUpdate("************** OnBranchesUpdated() START ************* time: " + Time.time, this);
        if (CurPauseObject == null) StaticStuff.PrintFlowBranchesUpdate("CurPauseObject is null", this);
        else StaticStuff.PrintFlowBranchesUpdate("CurPauseObject Type: " + CurPauseObject.GetType() + ", with TechnicalName: " + ((ArticyObject)CurPauseObject).TechnicalName, this);
        StaticStuff.PrintFlowBranchesUpdate("Num branches: " + aBranches.Count, this);

        CurBranches.Clear();
        if (CurPauseObject == null) StaticStuff.TrackEvent("Null CurPauseObject in OnBranchesUpdated()");
        
        // add all the aBranches to CurBranches but do some info sharing and error checking
        int i = 0;
        foreach (Branch b in aBranches)
        {
            StaticStuff.PrintFlowBranchesUpdate("branch: " + i + " is type: " + b.Target.GetType(), this);
            if (b.IsValid == false) Debug.LogWarning("Invalid branch in OnBranchesUpdate(): " + b.DefaultDescription);
            CurBranches.Add(b);
            i++;
        }

        // Start checking to see what we're paused on
        DialogueFragment df = CurPauseObject as DialogueFragment;
        if (df != null)
        {   // We're paused on a DialogueFragment so get ready to show some UI
            StaticStuff.PrintFlowBranchesUpdate("We're on a dialogue fragment, so set the text based on current flow state.", this);            
            switch (CurArticyState)
            {
                case eArticyState.DIALOGUE:
                    // if we're not waiting for a future node to be reached during a CAL and the Player ref isn't null then stop player's navmesh movement
                    if (ActiveCALPauseObjects.Count == 0 && Player != null) Player.StopNavMeshMovement();
                    // if we have a DialogueNPC then make sure to grab set the Speaker dynamically since it won't be part of the fragment
                    if (DialogueNPC != null && df.Speaker == null)
                    {
                        df.Speaker = DialogueNPC.ArticyEntityReference.GetObject();
                    }
                    ConvoUI.ShowDialogueFragment(df, CurPauseObject, CurBranches, IsDialogueFragmentsInteractive, TypewriterSpeed); // show the UI
                    break;
                default:
                    Dictionary<string, object> trackingParameters = new Dictionary<string, object>();
                    trackingParameters.Add("CurArticyState", CurArticyState.ToString());
                    StaticStuff.TrackEvent("OnBranchesUpdated() invalid state for DF", trackingParameters);
                    Debug.LogError("In invalid state for DialogueFragment: " + CurArticyState);
                    break;
            }
        }
        else if (CurPauseObject.GetType().Equals(typeof(Character_Action_List_Template)))
        {            
            StaticStuff.PrintFlowBranchesUpdate("-----------------------------------We've arrived at a Character Action List so let's see what's up", this);
            SetNextBranch(null);        // We temp pause the flow to handle the action list
            // grab the next fragment from the current fragment's output pin manually since we're temporarily pausing the flow
            CurCAL = CurPauseObject as Character_Action_List_Template;
            OutputPin outputPin = (CurPauseObject as FlowFragment).OutputPins[0];
            NextFragment = (outputPin.Connections[0].Target as ArticyObject);
            StaticStuff.PrintFlowBranchesUpdate(this.name + ": NextFragment type: " + outputPin.Connections[0].Target.GetType(), this);
            StaticStuff.PrintFlowBranchesUpdate(this.name + ": NextFragment: " + NextFragment.TechnicalName, this);
            if (outputPin.Connections[0].Target as Dialogue != null)
            {   // if the next fragment is a dialogue, then we're at the end of this Dialogue so make sure you keep the instruction to execute around
                StaticStuff.PrintFlowBranchesUpdate("next is a dialogue", this);
                Dialogue d = outputPin.Connections[0].Target as Dialogue;
                DialogueEndInstruction = d.OutputPins[0].Text;
                NextFragment = d.OutputPins[0].Connections[0].Target as ArticyObject;
            }
            // Get the pause frags if necessary so we know how much further along the dialogue can go before we pause input again            
            Character_Action_List_FeatureFeature CurCALObject = CurCAL.Template.Character_Action_List_Feature;
            ActiveCALPauseObjects = CurCALObject.PauseFrags;
            StaticStuff.PrintFlowBranchesUpdate(this.name + " is about to start their Behavior.  Time: " + Time.time, this);
            // get the behavior flow (action list) going
            GetComponent<BehaviorFlowPlayer>().StartBehaviorFlow(CurCAL, this.gameObject);
        }
        else if (CurPauseObject.GetType().Equals(typeof(Stage_Directions_Container)))
        {   // for stage direction containers we again get the next fragment automatically since we're temp taking over for this fragment        
            Stage_Directions_Container sdc = CurPauseObject as Stage_Directions_Container;
            NextFragment = (sdc.OutputPins[0].Connections[0].Target as ArticyObject);            
            StageDirectionPlayer.HandleStangeDirectionContainer(sdc);            
        }
        else if (CurBranches.Count == 1)
        {   // We're paused and there's only one valid branch available. This is common so have it's own section                 
            if (CurPauseObject.GetType().Equals(typeof(Dialogue)))
            {   // Paused on a Dialogue, so set the state and just move along automatically to the start of the Dialogue
                StaticStuff.PrintFlowBranchesUpdate("We're about to start a Dialogue but it may NOT always start with a dialogue fragment.", this);                
                SetArticyState(eArticyState.DIALOGUE);
                SetNextBranch(CurBranches[0]);
            }
            else if (CurPauseObject.GetType().Equals(typeof(Jump)))
            {   // We're on a Jump, so just go automatically 
                StaticStuff.PrintFlowBranchesUpdate("we're on a Jump, so just jump to where it's supposed to go.", this);
                SetNextBranch(CurBranches[0]);
            }
            else if (CurPauseObject.GetType().Equals(typeof(Hub)))
            {   // We're on a Hub so figure out what to do
                if (CurBranches[0].Target.GetType().Equals(typeof(OutputPin)))
                {   // if the first target is an output pin, then we're done with the Dialogue and are ready to go back to free roam
                    StaticStuff.PrintFlowBranchesUpdate("*****************************************We're paused on a Hub with no Target so we're in Free Roam.", this);                    
                    if (CurArticyState == eArticyState.DIALOGUE)
                    {   // we're leaving a Dialogue                        
                        if (Player != null) Player.ToggleMovementBlocked(false); // set the player free if there is one (if not we're in a mini game)                        
                        if (DialogueNPC != null)
                        {   // get the DialogueNPC back to normal if there was one
                            StageDirectionPlayer.StartAIOnNPC(DialogueNPC);
                            DialogueNPC = null;
                        }
                        StageDirectionPlayer.ShutOnAllAIs();    // make sure all AI's shut off during the dialogue are turned back on
                    }
                    else if (CurArticyState == eArticyState.AMBIENT_TRIGGER)
                    {
                        Debug.LogWarning("We're leaving an Ambient Trigger, and with all the recent changes this might not be being handled correctly so double check");                        
                        StageDirectionPlayer.ShutOnAllAIs();
                    }
                    //CurArticyState = eArticyState.FREE_ROAM;
                    SetArticyState(eArticyState.FREE_ROAM);
                    FlowFragsVisited.Clear();
                }
                else
                {   // We're on a Hub that has an actual target, so that means we're in a dialogue that's going to continue                    
                    Debug.LogWarning("Tell Mo if you see this and let him know where you are in the game so he can look into it more");
                    FlowFragsVisited.Clear();
                    SetNextBranch(CurBranches[0]);
                }
            }
            else if (CurPauseObject.GetType().Equals(typeof(AI_Template)))
            {
                StaticStuff.TrackEvent("AI_Templates should not be here");                
                StaticStuff.PrintFlowBranchesUpdate("We're starting an AI to get it going", this);                             
                SetArticyState(eArticyState.AI);
                SetNextBranch(CurBranches[0]);
                FlowFragsVisited.Clear();
            }
            else if (CurPauseObject.GetType().Equals(typeof(Ambient_Trigger)))
            {
                StaticStuff.TrackEvent("Ambient_Trigger should not be here");                
                StaticStuff.PrintFlowBranchesUpdate("We're about to get an Ambient_Trigger going", this);                
                SetArticyState(eArticyState.AMBIENT_TRIGGER);
                SetNextBranch(CurBranches[0]);
            }            
            else if (CurPauseObject.GetType().Equals(typeof(Scene_Jump)))
            {   // Jump to the scene specified
                Scene_Jump sj = CurPauseObject as Scene_Jump;
                SceneManager.LoadScene(sj.Template.Next_Game_Scene.Scene_Name);
            }
            else if (CurPauseObject.GetType().Equals(typeof(Mini_Game_Jump)))
            {   // we're going to a mini game, so fill up the mini game info container with the current pause object's information, then start the mini game
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
        else if (CurPauseObject == null && CurBranches.Count == 0)
        {
            StaticStuff.TrackEvent("Should not be null CurPauseObj and null CurBranches");           
            string s = "===========================================We've got a null CurPauseObject and Branches count is 0 so lets see what state we're in.\n";
            if (CurArticyState == eArticyState.FREE_ROAM)
            {
                s += "we're in FREE_ROAM, so we're most likely an AI that's been put on a pause object, so just chill";
            }
            else if (CurArticyState == eArticyState.AMBIENT_TRIGGER)
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
            foreach (Branch b in CurBranches)
            {
                s += "/t" + b.Target.GetType() + "\n";
            }
            Debug.LogWarning(s);            
        }

        StaticStuff.PrintFlowBranchesUpdate("************** OnBranchesUpdated() END *************** time: " + Time.time, this);        
    }

    /// <summary>
    /// This is called once the CAL is finished to let the Articy stuff know
    /// </summary>
    public void EndCAL(string callerName, string behaviorName)
    {        
        CurCAL.OutputPins[0].Text.CallScript(); // make sure to call any scripts on the output pin of the CAL fragment
        if (DialogueEndInstruction != null)
        {   // If this CAL was also the end of the dialogue, make sure the dialogue's end instruction is called)
            DialogueEndInstruction.CallScript();
            DialogueEndInstruction = null;
        }
        ActiveCALPauseObjects.Clear();  // no need for this anymore
    }

    /// <summary>
    /// Sets the NextBranch var and allows an easy way to keep track of state changes
    /// </summary>
    /// <param name="nextBranch"></param>
    public void SetNextBranch(Branch nextBranch)
    {
        NextBranch = nextBranch;
    }

    /// <summary>
    /// Sets the new articy state and handles any other issues necessaryo
    /// </summary>
    /// <param name="newState"></param>
    void SetArticyState(eArticyState newState)
    {
        CurArticyState = newState;
        if (Player != null && newState == eArticyState.DIALOGUE)
        {
            Player.SetPlayerControlStartDialogue();
        }
        else if (Player != null && newState == eArticyState.FREE_ROAM)
        {
            Player.SetPlayerControlEndDialogue();
        }
    }

    /// <summary>
    /// Determins whether we can proceed with the flow or not due to elements on the pause object list. 
    /// </summary>
    /// <returns></returns>
    bool WaitingOnActionList()
    {
        if (ActiveCALPauseObjects.Count == 0) return false; // no pause objects so we're not waiting
        else
        {
            foreach (ArticyObject ao in ActiveCALPauseObjects)
            {
                if (FlowFragsVisited.Contains(ao.TechnicalName)) return true; // You've visited a fragement on the list, so wait until it's cleared
            }
            return false; // haven't visited any fragements on the pause list yet so keep going
        }
    }
    // Update is called once per frame
    void Update()
    {
        bool waitingOnAL = WaitingOnActionList();
        FlowDebug(waitingOnAL);

        if (NextBranch != null && waitingOnAL == false)
        {   // we have a NextBranch assigned and we're not waiting on the Action List so continue the flow on the NextBranch
            StaticStuff.PrintFlowBranchesUpdate(this.name + ": going to next branch via Update", this);            
            Branch b = NextBranch;
            SetNextBranch(null);
            FlowPlayer.Play(b);
        }
        else if (NextFragment != null && waitingOnAL == false)
        {   // we have a NextFragment assigned and we're not waiting on the Action List so continue the flow on the NextFragment
            StaticStuff.PrintFlowBranchesUpdate(this.name + ": going to next FRAG via Update", this);            
            ArticyObject nf = NextFragment;            
            NextFragment = null;
            FlowPlayer.StartOn = nf;
        }
    }

    void FlowDebug(bool waitingOnAL )
    {
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
    }
    #endregion

    #region UI_DIALOGUE    

    /// <summary>
    /// Callback function for the buttons on the UI
    /// </summary>
    public void UIButtonCallback(int buttonIndex)
    {
        StaticStuff.PrintUI("UIButtonCallback() buttonIndex: " + buttonIndex);        
        SetNextBranch(CurBranches[buttonIndex]); // select the next branch from the list based on the button index
        DialogueFragment df = CurBranches[buttonIndex].Target as DialogueFragment;
        if (df != null)
        {            
            StaticStuff.PrintUI("Chosen branch is a dialogue fragment, so just let the engine handle the next phase.");            
        }
        else if (CurBranches[buttonIndex].Target.GetType().Equals(typeof(Character_Action_List_Template)))
        {
            StaticStuff.PrintUI("Next thing is a Character_Actions_List_Template, so shut off the UI and let the action list do it's thing.");
            //Debug.Log("---------------------------Next thing is a Character_Actions_List_Template, so shut off the UI and let the action list do it's thing.");
            ConvoUI.EndConversation();
        }
        else
        {
            StaticStuff.PrintUI("Chosen branch isn't a DialogueFragment or a Character_Movement so for now just assume we're done talking and shut off the UI");            
            //Debug.Log("----------------------------------- Chosen branch isn't a DialogueFragment or a Character_Movement so for now just assume we're done talking and shut off the UI");
            ConvoUI.EndConversation();
        }
    }

    /// <summary>
    /// Bails on the current dialogue right away.  Currently called from MiniGames once they've finished a puzzle
    /// </summary>
    public void QuitCurDialogue()
    {
        SetArticyState(eArticyState.FREE_ROAM);
        CurPauseObject = null;
        NextFragment = null;
        NextBranch = null;
        FlowFragsVisited.Clear();
        ActiveCALPauseObjects.Clear();
        if (Player != null) Player.ToggleMovementBlocked(false);
        ConvoUI.EndConversation();
    }    
    #endregion
    
    #region ARTICY_VAR_FUNCTION_CALLBACKS
    /// <summary>
    /// Called from Articy when a variable changes
    /// </summary>    
    void MyGameStateVariablesChanged(string aVariableName, object aValue)
    {
        //Debug.Log("aVariableName: " + aVariableName + " changed to: " + aValue.ToString());
        if (CaptainsChair != null) StaticStuff.SaveSaveData();
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
    #endregion

    #region MISC
    /// <summary>
    /// Wrapper function to send things to this ArticyFlow's StageDirectionPlayer
    /// </summary>
    public void SendToStageDirections(Stage_Directions_Container sdc)
    {
        StageDirectionPlayer.HandleStangeDirectionContainer(sdc);
    }

    /// <summary>
    /// Returns the name of the DialogueNPC for any converstions involving a random NPC
    /// </summary>
    /// <returns></returns>
    public string GetDialogueNPCName()
    {
        if (DialogueNPC == null) { Debug.LogError("Trying to get the DialogueNPC name but the DialogueNPC is null."); return "Null NPC"; }
        return DialogueNPC.name;
    }

    public bool IsInFreeRoam()
    {
        return (CurArticyState == eArticyState.FREE_ROAM);
    }
    #endregion    

    public float GetDefaultTypewriterSpeed() { return ConvoUI.DefaultTypewriterSpeed; }
}

