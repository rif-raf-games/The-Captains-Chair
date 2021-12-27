//#define USE_RR_ONGU
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
using UnityEngine.Analytics;
using System;
using System.Collections;
using Articy.Unity.Interfaces;

public class ArticyFlow : MonoBehaviour, IArticyFlowPlayerCallbacks, IScriptMethodProvider
{
    public enum eArticyState { FREE_ROAM, DIALOGUE, AI, AMBIENT_TRIGGER, NUM_ARTICY_STATES };
    public eArticyState CurArticyState;

    // flow stuff
    public ArticyFlowPlayer FlowPlayer;    // This is the Articy behind-the-scenes stuff that communicates with this file
    public IFlowObject CurPauseObject = null;  // Current fragment we're paused on, as told to us by ArticyFlowPlayer
    public List<Branch> CurBranches = new List<Branch>();  // Branches available from CurPauseObject
    public Branch NextBranch = null;   // Next branch we're going to tell the flow player to continue on
    public ArticyObject NextFragment = null;   // This is our stuff since we have to temp break the flow for Action Lists
    StageDirectionPlayer StageDirectionPlayer;  // Player for stage directions
    ArticyScriptInstruction DialogueEndInstruction = null;  // If we take over the flow for a CAL at the end of a Dialogue, keep track of that dialogue's end insturction for execution after the CAL
    Character_Action_List_Template CurCAL = null;   // Current CAL we're working showing

    // The objects you have to wait until you have visited before the dialogue will continue if you're in the middle of a CAL
    List<ArticyObject> ActiveCALPauseObjects = new List<ArticyObject>(); 
    List<string> FlowFragsVisited = new List<string>(); // List of flow fragments visited to check against ActiveCALPauseObjects
    
    NPC DialogueNPC = null; // The NPC you're in conversation with    

    MiniGameMCP MiniGameMCP; // if we're in a mini game we'll have a mini game mcp

    // game stuff
    TheCaptainsChair CaptainsChair; // Core game
    CCPlayer Player;                // Captain/Player
    MCP MCP;
    // UI stuff
    ConvoUI ConvoUI; 
    public bool IsDialogueFragmentsInteractive { get; set; }    // Whether or not the UI dialogue is interactive or not
    public float TypewriterSpeed;/* { get; set; }*/  // How fast the UI text types on screen    
               
    // Articy function callback stuff
    public bool IsCalledInForecast { get; set; } // This is required by Articy for code callbacks from the Articy flow stuff
                   
    [Header("Debug")]
    public Text DebugText;

    public bool StartCalled = false;

    bool IsSaveOnlyDialogue;

    private void Awake()
    {
        IsDialogueFragmentsInteractive = true;
    }
    //public bool AwakeCalled = false;
    void Start()
    {
       // Debug.Log("ArticyFlow.Start(): " + this.gameObject.GetInstanceID());
        SetArticyState(eArticyState.NUM_ARTICY_STATES);
        Player = GameObject.FindObjectOfType<CCPlayer>();
        CaptainsChair = GameObject.FindObjectOfType<TheCaptainsChair>();
        FlowPlayer = this.GetComponent<ArticyFlowPlayer>();
        StageDirectionPlayer = FindObjectOfType<StageDirectionPlayer>();
        this.MiniGameMCP = FindObjectOfType<MiniGameMCP>();
        ConvoUI = FindObjectOfType<MCP>().GetConvoUI();
        TypewriterSpeed = ConvoUI.DefaultTypewriterSpeed;
        ArticyDatabase.DefaultGlobalVariables.Notifications.AddListener("*.*", MyGameStateVariablesChanged);
        ActiveCALPauseObjects.Clear();
        this.MCP = FindObjectOfType<MCP>();
        if (this.MCP == null) Debug.LogError("ERROR: no MCP");
        StartCalled = true;
    }
    
    
    /// <summary>
    /// We now determine manually whether or not we're going to start a dialogue in order to solve lots of potential
    /// conflicts like more than 1 dialogue starting at once.  
    /// </summary>
    public bool CheckIfDialogueShouldStart(Dialogue convoStart, GameObject collider)
    {
       // Debug.Log("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ CheckDialogue() with technical name: " + convoStart.TechnicalName + " on GameObject: " + this.gameObject.name + " but DON'T DO ANY CODE STUFF UNTIL WE KNOW WE'RE ACTUALLY COMMITTING  time: " + Time.time);// + ", convoStart: " + convoStart.name + ", collider: " + collider.name);
        if (CurArticyState == eArticyState.DIALOGUE)
        {   // we're already in a dialogue, so bail
            return false;
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
            return false;
        }

        ArticyObject firstNode = convoStart.InputPins[0].Connections[0].Target;
        Save_Point sp = convoStart.InputPins[0].Connections[0].Target as Save_Point;
        IsSaveOnlyDialogue = sp != null;                
        if(IsSaveOnlyDialogue == true)
        {
            //Debug.Log("IsSaveOnlyDialogue is true to just save and bail");
            StaticStuff.SaveCurrentProfile("ArticyFlow.CheckIfDialogueShouldStart() Dialogue is a Save_Point so save and bail");
            return false;
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
                    if (DialogueNPC == null) { Debug.LogError("There's no NPC on the Dialgue_NPC you collided with"); return false; }
                    StageDirectionPlayer.StopAIOnNPC(DialogueNPC);                    
                }                
            }
        }

        // start the dialogue
        //IsDialogueFragmentsInteractive = true;
        //  FindObjectOfType<MCP>().ToggleInGameUI(false);       
        
        FlowPlayer.StartOn = convoStart;
        if (Player != null) Player.ToggleMovementBlocked(true);
        this.MCP.ShutOffAllUI(); // ArticyFlow.CheckIfDialogueShouldStart() == true

        return true;
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

    

    List<ArticyObject> GetValidArticyObjects(ArticyObject curAO)
    {
        List<ArticyObject> validBranches = new List<ArticyObject>();
        Jump jump = curAO as Jump;
        if(jump != null)
        {            
            validBranches.Add(jump.Target);           
        }
        else
        {
            List<IOutputPin> oPins = (curAO as IOutputPinsOwner).GetOutputPins();
            OutputPin cpoOutputPin = (oPins[0] as OutputPin);
           // Debug.LogWarning("Executing this script: " + cpoOutputPin.Text.RawScript);
            cpoOutputPin.Text.CallScript();
            // now evaluate all the targets to see which ones are valid
            validBranches = new List<ArticyObject>();
           // Debug.LogWarning("this pin has " + cpoOutputPin.Connections.Count + " connections.");
            foreach (OutgoingConnection outCon in cpoOutputPin.Connections)
            {
                ArticyObject target = outCon.Target;
                List<IInputPin> iPins = (target as IInputPinsOwner).GetInputPins();
                if (iPins.Count != 1) Debug.LogWarning("You should only have 1 input pin on dialogue nodes: " + target.TechnicalName);
                InputPin targetInputPin = iPins[0] as InputPin;

                bool val = targetInputPin.Text.CallScript();
               // Debug.LogWarning("This target's input pin conditional: " + targetInputPin.Text.RawScript + " and it's eval is: " + val);
                if (val == true) validBranches.Add(target);
            }
        }        
        return validBranches;
    }

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
        if (CurPauseObject == null) Debug.LogError("CurPauseObject is null");//StaticStuff.TrackEvent("Null CurPauseObject in OnBranchesUpdated()");        
        
        // add all the aBranches to CurBranches but do some info sharing and error checking
        int i = 0;
        foreach (Branch b in aBranches)
        {
            StaticStuff.PrintFlowBranchesUpdate("branch: " + i + " is type: " + b.Target.GetType(), this);
            if (b.IsValid == false) Debug.LogWarning("Invalid branch in OnBranchesUpdate(): " + b.DefaultDescription);                        
            CurBranches.Add(b);

            if (b.Target.GetType() == typeof(Articy.The_Captain_s_Chair.OutputPin))
            {
                string s = "OnBranchesUpdate() branch hash code: " + b.GetHashCode() + ", CurBranches hash code: " + CurBranches.GetHashCode() + ", this hash code: " + this.GetHashCode();
                int numAF = FindObjectsOfType<ArticyFlow>().Length;
                int numAFP = FindObjectsOfType<ArticyFlowPlayer>().Length;
                s += ", numAF: " + numAF + ", numAFP: " + numAFP;
                //Debug.LogError(s);
            }
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
            if(StageDirectionPlayer != null) StageDirectionPlayer.HandleStangeDirectionContainer(sdc);            
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
                        if (Player != null && IsSaveOnlyDialogue == false) Player.ToggleMovementBlocked(false); // set the player free if there is one (if not we're in a mini game)                        
                        if (DialogueNPC != null)
                        {   // get the DialogueNPC back to normal if there was one
                            StageDirectionPlayer.StartAIOnNPC(DialogueNPC);
                            DialogueNPC = null;
                        }
                        if(StageDirectionPlayer != null) StageDirectionPlayer.ShutOnAllAIs();    // make sure all AI's shut off during the dialogue are turned back on
                    }
                    else if (CurArticyState == eArticyState.AMBIENT_TRIGGER)
                    {
                        Debug.LogWarning("We're leaving an Ambient Trigger, and with all the recent changes this might not be being handled correctly so double check");                        
                        StageDirectionPlayer.ShutOnAllAIs();
                    }
                    
                    SetArticyState(eArticyState.FREE_ROAM);     // moiap - this is ArticyFlow.OnBranchesUpdated where we're calling EndCoversation               
                    ConvoUI.EndConversation();
                    FlowFragsVisited.Clear();
                    if (this.MiniGameMCP != null) this.MiniGameMCP.CurrentDiaogueEnded();                   
                }
                else
                {   // We're on a Hub that has an actual target, so that means we're in a dialogue that's going to continue                    
                    //Debug.LogWarning("Tell Mo if you see this and let him know where you are in the game so he can look into it more");
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
                if(this.MCP.SaveNextObjectForIAP == true)
                {
                    Debug.Log("OK we want to hold the dialogue until we handle the IAP stuff so save the current pause object");
                    this.MCP.IAPPauseObject = CurPauseObject;                    
                    this.MCP.StartIAPPanel();
                    return;
                }
                Scene_Jump sj = CurPauseObject as Scene_Jump;
                //SceneManager.Load Scene(sj.Template.Next_Game_Scene.Scene_Name);
                this.ConvoUI.gameObject.SetActive(false); // don't use EndConversation because that also shuts off the burger menu temporarily.  This UI needs an enema
                //Debug.Log("---- about to do a regular scene jump");
                FindObjectOfType<MCP>().LoadNextScene(sj.Template.LoadingScreen.SceneToLoad, sj);
                //FindObjectOfType<MCP>().LoadNextScene(sj.Template.Next_Game_Scene.Scene_Name, sj); 
            }
            else if (CurPauseObject.GetType().Equals(typeof(Mini_Game_Jump)))
            {
                // we're going to a mini game, so fill up the mini game info container with the current pause object's information, then start the mini game                               
                Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");
                Mini_Game_Jump curJump = CurPauseObject as Mini_Game_Jump;
                //Debug.Log("Set Up Mini game: " + curJump.TechnicalName + ", is SaveFragment null: " + (curJump.Template.Success_Save_Fragment.SaveFragment == null));
                //jumpSave.Template.Mini_Game_Scene.Scene_Name = curJump.Template.Mini_Game_Scene.Scene_Name;
                jumpSave.Template.LoadingScreen.SceneToLoad = curJump.Template.LoadingScreen.SceneToLoad;
                jumpSave.Template.LoadingScreen.LoadingImages = curJump.Template.LoadingScreen.LoadingImages;
                jumpSave.Template.LoadingScreen.DisplayTime = curJump.Template.LoadingScreen.DisplayTime;
                jumpSave.Template.LoadingScreen.FadeTime = curJump.Template.LoadingScreen.FadeTime;

                jumpSave.Template.Mini_Game_Puzzles_To_Play.Puzzle_Numbers = curJump.Template.Mini_Game_Puzzles_To_Play.Puzzle_Numbers;

                jumpSave.Template.Dialogue_List.DialoguesToPlay = curJump.Template.Dialogue_List.DialoguesToPlay;

                jumpSave.Template.Success_Mini_Game_Result.SceneName = curJump.Template.Success_Mini_Game_Result.SceneName;
                jumpSave.Template.Success_Mini_Game_Result.LoadingImages = curJump.Template.Success_Mini_Game_Result.LoadingImages;
                jumpSave.Template.Success_Mini_Game_Result.DisplayTime = curJump.Template.Success_Mini_Game_Result.DisplayTime;
                jumpSave.Template.Success_Mini_Game_Result.FadeTime = curJump.Template.Success_Mini_Game_Result.FadeTime;
                jumpSave.Template.Success_Mini_Game_Result.Dialogue = curJump.Template.Success_Mini_Game_Result.Dialogue;

                jumpSave.Template.Quit_Mini_Game_Result.SceneName = curJump.Template.Quit_Mini_Game_Result.SceneName;
                jumpSave.Template.Quit_Mini_Game_Result.LoadingImages = curJump.Template.Quit_Mini_Game_Result.LoadingImages;
                jumpSave.Template.Quit_Mini_Game_Result.DisplayTime = curJump.Template.Quit_Mini_Game_Result.DisplayTime;
                jumpSave.Template.Quit_Mini_Game_Result.FadeTime = curJump.Template.Quit_Mini_Game_Result.FadeTime;
                jumpSave.Template.Quit_Mini_Game_Result.Dialogue = curJump.Template.Quit_Mini_Game_Result.Dialogue;

                jumpSave.Template.Success_Save_Fragment.SaveFragment = curJump.Template.Success_Save_Fragment.SaveFragment;
                jumpSave.Template.payment.Payment_Value = curJump.Template.payment.Payment_Value;

                ArticyGlobalVariables.Default.Mini_Games.Coming_From_Main_Game = true;
                ArticyGlobalVariables.Default.Mini_Games.Returning_From_Mini_Game = false;
                ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success = false;
                //Debug.Log("About to start a mini game: " + curJump.Template.LoadingScreen.SceneToLoad);                
                //FindObjectOfType<MCP>().LoadNextScene(curJump.Template.Mini_Game_Scene.Scene_Name, null, curJump); // MGJ: ArticyFlow.OnBranchesUpdate() CurPauseObject = Mini_Game_Jump
                /*FindObjectOfType<MCP>().*/
                FindObjectOfType<MCP>().LoadNextScene(curJump.Template.LoadingScreen.SceneToLoad, null, curJump); // MGJ: ArticyFlow.OnBranchesUpdate() CurPauseObject = Mini_Game_Jump
            }
            else if(CurPauseObject.GetType().Equals(typeof(Save_Point)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're on a Save_Point, so parse the data and save it", this);
                Debug.Log("We're on a Save_Point, so parse the data and save it");
                HandleSavePoint(CurPauseObject as Save_Point);                
                SetNextBranch(CurBranches[0]);
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
    /// Save_Point fragments can show up in different ways so have a separate function to handle the process
    /// </summary>
    /// <param name="savePoint"></param>
    public void HandleSavePoint(Save_Point savePoint)
    {        
        if(savePoint == null)
        {
            Debug.LogError("The save info is not set up in the articy nodes yet so we're temporarily skipping saving at this moment.");
            return;
        }
       // Debug.Log("HandleSavePoint() Sav_Var: " + savePoint.Template.Save_Info.Sav_Var);

        ArticyGlobalVariables.Default.Save_Info.Return_Scene = savePoint.Template.Save_Info.ReturnScene;

        //Debug.LogError("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! mosavepos01 HandleSavePoint()");
        if(savePoint.Template.Save_Info.PositionsToSave != "")
        {            
           // Debug.LogError("we want to save these positions: " + savePoint.Template.Save_Info.PositionsToSave);
            ArticyGlobalVariables.Default.Save_Info.Positions_To_Save = savePoint.Template.Save_Info.PositionsToSave;
            string[] characters = savePoint.Template.Save_Info.PositionsToSave.Split(',');
            string charPositions = "";            
            foreach (string name in characters)
            {
                string pos;
               // Debug.LogError("looking for character: " + name);
                GameObject go = GameObject.Find(name);
                if (go == null)
                {
                    Debug.LogError("ERROR: this character: " + name + " is not in the scene");
                    pos = "0,0,0";
                }
                pos = go.transform.position.x.ToString("F5") + "," + go.transform.position.y.ToString("F5") + "," + go.transform.position.z.ToString("F5");
              //  Debug.LogError("saving position: " + pos);
                charPositions += pos + ",";
            }
            charPositions = charPositions.Remove(charPositions.Length - 1, 1);
            ArticyGlobalVariables.Default.Save_Info.Saved_Positions = charPositions;                        
        }
        else
        {
            ArticyGlobalVariables.Default.Save_Info.Positions_To_Save = "";
            ArticyGlobalVariables.Default.Save_Info.Saved_Positions = "";
        }

        List<Elevator> elevators = FindObjectsOfType<Elevator>().ToList();
        if (elevators.Count > 0)
        {
            string elevatorPositions = "";
            List<Elevator> sortedList = elevators.OrderBy(o => o.name).ToList<Elevator>();
            foreach (Elevator elevator in sortedList)
            {
                // Debug.Log("elevator: " + elevator.name + " is on floor: " + elevator.CurrentFloor);
                elevatorPositions += elevator.CurrentFloor.ToString() + ",";
            }
            elevatorPositions = elevatorPositions.Remove(elevatorPositions.Length - 1);
            ArticyGlobalVariables.Default.Save_Info.Majestic_Elevators = elevatorPositions;
            //  Debug.Log("elevatorPosition: " + elevatorPositions);
        }

        if (savePoint.Template.Save_Info.AnalyticsToTrack != "")
        {
            string[] analytics = savePoint.Template.Save_Info.AnalyticsToTrack.Split(',');
            foreach (string a in analytics)
            {
                string x = ArticyGlobalVariables.Default.GetVariableByString<string>(a);
                Dictionary<string, object> trackingParameters = new Dictionary<string, object>();
                trackingParameters.Add("value at save", x);
                StaticStuff.TrackEvent(a, trackingParameters);
            }
        }  
        if(savePoint.Template.Save_Info.Sav_Var != "")
        {
            string saveVar = savePoint.Template.Save_Info.Sav_Var;
            Debug.Log("saveVar: " + saveVar);
            ArticyGlobalVariables.Default.SetVariableByString(saveVar, true);            
        }

        StaticStuff.SaveCurrentProfile("ArticyFlow.HandleSavePoint()");        
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
       // Debug.Log("SetArticyState: " + newState + ", " + this.gameObject.GetInstanceID());
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

    public DialogueFragment DebugDF = null;
    public bool SHOW_SKIP_BUTTON = false;
    public void SkipDialogue()
    {
        DialogueFragment df = CurPauseObject as DialogueFragment;
        if (df == null)
        {
            Debug.Log("SkipDialogue(): df is null so bail");
            return;
        }

        DebugDF = df;
        ArticyObject curAO = CurPauseObject as ArticyObject;
        //  int numCheck = 0;
        bool iSaySo = true;
        //Debug.Log("*************************************skip");
        while (iSaySo == true)
        {
            // numCheck++;
            // Debug.LogWarning("curAO Type: " + curAO.GetType() + ", with TechnicalName: " + curAO.TechnicalName, this);

            List<ArticyObject> validArticyObjects = GetValidArticyObjects(curAO);
            if (validArticyObjects.Count == 0)
            {
                //  Debug.LogWarning("No valid branches so we must be at the end: " + curAO.TechnicalName);
                iSaySo = false;
                CurPauseObject = DebugDF as IFlowObject;
                CurBranches.Clear();
                validArticyObjects = GetValidArticyObjects(DebugDF as ArticyObject);
                if (validArticyObjects.Count == 1 && (validArticyObjects[0] as Dialogue) != null)
                {
                    validArticyObjects = GetValidArticyObjects(validArticyObjects[0]);
                }
                // Debug.LogWarning("num valid branches at end: " + validArticyObjects.Count);
                ActiveCALPauseObjects.Clear();
                FlowFragsVisited.Clear();
                SetNextBranch(null);
                NextFragment = null;
                ConvoUI.ShowDialogueFragment(DebugDF, CurPauseObject, null, IsDialogueFragmentsInteractive, TypewriterSpeed, validArticyObjects); // show the UI                        
                ConvoUI.TurnOnValidButtons();
            }
            else
            {
                int choice = UnityEngine.Random.Range(0, validArticyObjects.Count);
                //  Debug.LogWarning("You chose option index: " + choice);
                curAO = validArticyObjects[choice];
                if (curAO as DialogueFragment != null)
                {
                    // Debug.Log("We now have a new DebugDF: " + curAO.TechnicalName);
                    DebugDF = curAO as DialogueFragment;
                }
            }
        }
    }
#if USE_RR_ONGU
    void OnGUI()
    {
      //  if (SHOW_SKIP_BUTTON == false) return;
        if (CurArticyState == eArticyState.DIALOGUE && CurPauseObject != null)
        {
            DialogueFragment df = CurPauseObject as DialogueFragment;
            if (df == null) return;

            if (GUI.Button(new Rect(Screen.width - 100, Screen.height / 2 - 50, 100, 100), "Skip"))
            {
                DebugDF = df;
                ArticyObject curAO = CurPauseObject as ArticyObject;
              //  int numCheck = 0;
                bool iSaySo = true;
                //Debug.Log("*************************************skip");
                while (iSaySo == true)
                {
                   // numCheck++;
                   // Debug.LogWarning("curAO Type: " + curAO.GetType() + ", with TechnicalName: " + curAO.TechnicalName, this);

                    List<ArticyObject> validArticyObjects = GetValidArticyObjects(curAO);
                    if (validArticyObjects.Count == 0)
                    {
                      //  Debug.LogWarning("No valid branches so we must be at the end: " + curAO.TechnicalName);
                        iSaySo = false;
                        CurPauseObject = DebugDF as IFlowObject;
                        CurBranches.Clear();
                        validArticyObjects = GetValidArticyObjects(DebugDF as ArticyObject);
                        if(validArticyObjects.Count == 1 && (validArticyObjects[0] as Dialogue) != null)
                        {                            
                            validArticyObjects = GetValidArticyObjects(validArticyObjects[0]);
                        }
                       // Debug.LogWarning("num valid branches at end: " + validArticyObjects.Count);
                        ActiveCALPauseObjects.Clear();
                        FlowFragsVisited.Clear();
                        SetNextBranch(null);
                        NextFragment = null;
                        ConvoUI.ShowDialogueFragment(DebugDF, CurPauseObject, null, IsDialogueFragmentsInteractive, TypewriterSpeed, validArticyObjects); // show the UI                        
                        ConvoUI.TurnOnValidButtons();
                    }
                    else
                    {
                        int choice = UnityEngine.Random.Range(0, validArticyObjects.Count);
                      //  Debug.LogWarning("You chose option index: " + choice);
                        curAO = validArticyObjects[choice];
                        if (curAO as DialogueFragment != null)
                        {
                           // Debug.Log("We now have a new DebugDF: " + curAO.TechnicalName);
                            DebugDF = curAO as DialogueFragment;
                        }
                    }
                }
            }
        }
    } 

#endif
    MCP OurMCP;
    bool WaitingOnALLastFrame = false;
    // Update is called once per frame
    void Update()
    {        
        bool waitingOnAL = WaitingOnActionList();
        FlowDebug(waitingOnAL);        

        if (waitingOnAL == true)
        {
           // Debug.Log("1");
           // ConvoUI.ShutOffButtons();
        }
        else
        {
            if(WaitingOnALLastFrame == true && waitingOnAL == false)
            {
              //  Debug.Log("2");
                //  ConvoUI.SetupValidButtons();
            }            
        }
        WaitingOnALLastFrame = waitingOnAL;
        
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
#endregion
#region UI_DIALOGUE    
    public OutputPin MyOutputPin;
    public void ConvoButtonClicked(int buttonIndex, ArticyObject nextFragment = null)
    {        
        StaticStuff.PrintUI("ConvoButtonClicked() buttonIndex: " + buttonIndex);
        
        DialogueFragment df;
        IFlowObject target;
        if(nextFragment != null)
        {
            NextFragment = nextFragment;
            df = nextFragment as DialogueFragment;
            target = nextFragment as IFlowObject;            
        }
        else
        {
            //Branch b = CurBranches[buttonIndex];
            SetNextBranch(CurBranches[buttonIndex]); // select the next branch from the list based on the button index
            df = CurBranches[buttonIndex].Target as DialogueFragment;
            target = CurBranches[buttonIndex].Target;
        }
        
        if (df != null)
        {
            StaticStuff.PrintUI("Chosen branch is a dialogue fragment, so just let the engine handle the next phase.");
            Debug.Log("--------------------------- Chosen branch is a dialogue fragment, so just let the engine handle the next phase.");
        }
        else if (target.GetType().Equals(typeof(Character_Action_List_Template)))
        {
            StaticStuff.PrintUI("Next thing is a Character_Actions_List_Template, so shut off the UI and let the action list do it's thing.");
            Debug.Log("---------------------------Next thing is a Character_Actions_List_Template, so shut off the UI and let the action list do it's thing.");
            //ConvoUI.EndConversation();
        }
        else
        {
           // Debug.Log("type is: " + CurBranches[buttonIndex].Target.GetType());                                          
            StaticStuff.PrintUI("Chosen branch isn't a DialogueFragment or a Character_Movement so for now just assume we're done talking and shut off the UI");
            Debug.Log("----------------------------------- Chosen branch isn't a DialogueFragment or a Character_Movement so for now just assume we're done talking and shut off the UI");
           // ConvoUI.EndConversation();            
        }
    }
    string DefaultDes = "none";
    private void LateUpdate()
    {
        if (CurBranches == null) return;
        if (CurBranches.Count == 0) return;        
    }

    void FlowDebug(bool waitingOnAL)
    {
        /*if (DebugText != null)
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
            DebugText.text += "ShutOffAI's:\n";
            DebugText.text += StageDirectionPlayer.GetShutOffAINames();
        }*/
    }
    

    /// <summary>
    /// Bails on the current dialogue right away.  Currently called from MiniGames once they've finished a puzzle
    /// </summary>
    public void EndMiniGameDialogues()
    {
        //Debug.LogError("fix");
        SetArticyState(eArticyState.FREE_ROAM);        
        ConvoUI.EndConversation();
        CurPauseObject = null;
        NextFragment = null;
        NextBranch = null;
        FlowFragsVisited.Clear();
        ActiveCALPauseObjects.Clear();
        if (Player != null) Player.ToggleMovementBlocked(false);        
    }    
#endregion
    
#region ARTICY_VAR_FUNCTION_CALLBACKS
    /// <summary>
    /// Called from Articy when a variable changes
    /// </summary>    
    void MyGameStateVariablesChanged(string aVariableName, object aValue)
    {
        //Debug.Log("aVariableName: " + aVariableName + " changed to: " + aValue.ToString());
        //if (CaptainsChair != null) StaticStuff.SaveSaveData();
    }

    /// <summary>
    /// This is called from an Articy fragment defined in the project file
    /// </summary>
    public void ArticyTrophyCallback(string trophyID)
    {
        if (IsCalledInForecast == false)
        {
            Debug.Log("called ArticyTropyCallback(). IsCalledInForecast == false so do the thing: " + trophyID);
        }
        else
        {
            Debug.Log("called ArticyTropyCallback(). IsCalledInForecast == true so do NOT do the thing: " + trophyID);
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

    //public Text debugText;    
}

