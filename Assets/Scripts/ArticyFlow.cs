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
    public enum ArticyState { FREE_ROAM, CUT_SCENE, CONVERSATION, NUM_ARTICY_STATES };
    public ArticyState CurArticyState;

    // objet references
    TheCaptainsChair CaptainsChair;
    CCPlayer Player;
    ArticyFlowPlayer FlowPlayer;

    // flow stuff
    IFlowObject CurPauseObject = null;
    List<Branch> CurBranches = new List<Branch>();
    DialogueFragment LastDFPlayed;
    Branch NextBranch = null;
    //Character_Movement_FeatureFeature CurCMFeature = null;

    // list of characters in the scene for quick reference
    List<CharacterEntity> CharacterEntities = new List<CharacterEntity>(); // this is temp until I update the scripts    
    public List<CharacterEntity> GetCharacterEntities() { return CharacterEntities; }

    // UI's for cutscenes and conversations
    public CutSceneUI CutSceneUI;
    public ConvoUI ConvoUI;

    // Articy stuff
    public bool IsCalledInForecast { get; set; }
                   
    //debug
    public Text DebugText;

    void Start()
    {
        CurArticyState = ArticyState.NUM_ARTICY_STATES;

        Player = GameObject.FindObjectOfType<CCPlayer>();
        CaptainsChair = GameObject.FindObjectOfType<TheCaptainsChair>();
        FlowPlayer = this.GetComponent<ArticyFlowPlayer>();
        CharacterEntities = GameObject.FindObjectsOfType<CharacterEntity>().ToList();

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
    /// <summary>
    /// Checks to see if the Player has reached the specified point and the last dialogue flow fragment
    /// in the Character_Movement setup has been reached
    /// </summary>    
   /* IEnumerator Character_Movement_Check()
    {        
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        bool isFinished = false;
        while(isFinished == false)
        {            
            // right now it's just the player, but we should add a new class that keeps track of the game object as well
            if(LastDFPlayed == CurCMFeature.DialogueFragmentThatMustComplete && Player.NavMeshDone())
            {
                Debug.Log("----------------------- We're Done");
                FlowPlayer.StartOn = CurCMFeature.StartingDialogueFragment;                
                isFinished = true;
                CurCMFeature = null;
                LastDFPlayed = null;               
            }
            if(isFinished==false) yield return new WaitForEndOfFrame();
        }
    }    */

    void SetNextBranch(Branch nextBranch)
    {
        NextBranch = nextBranch;
    }

    /// <summary>
    /// Callback from Articy when we've reached a pause point
    /// </summary>    
    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() START *************");
        //Debug.Log("************** OnFlowPlayerPaused() START *************");
        CurPauseObject = null;
        if(aObject == null)
        {            
            //StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() END aObject WAS null***************");
            Debug.LogWarning("************** OnFlowPlayerPaused() END aObject WAS null***************");
            return;
        }
        CurPauseObject = aObject;
        StaticStuff.PrintFlowPaused("OnFlowPlayerPaused() IFlowObject Type: " + aObject.GetType() + ", with TechnicalName: " + ((ArticyObject)aObject).TechnicalName);
        //Debug.Log("OnFlowPlayerPaused() IFlowObject Type: " + aObject.GetType() + ", with TechnicalName: " + ((ArticyObject)aObject).TechnicalName);
        // keep track of the technical names of the nodes we've visited
        if(CurArticyState != ArticyState.FREE_ROAM && CurArticyState != ArticyState.NUM_ARTICY_STATES)
        {
            string s = "#################################################### adding to list: " + ((ArticyObject)aObject).TechnicalName + ", of type: "+ aObject.GetType();
            DialogueFragment df = aObject as DialogueFragment;
            FlowFragment ff = aObject as FlowFragment;
            if (df != null) s += ", a dialogue frag: " + df.Text;
            else if (ff != null) s += ", info: " + ff.DisplayName;
            //Debug.Log(s);
            FlowFragsVisited.Add(((ArticyObject)aObject).TechnicalName);
        }                      
        StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() END aObject was NOT null***************");
        //Debug.Log("************** OnFlowPlayerPaused() END aObject was NOT null***************");
    }
    
    /// <summary>
    /// Callback from Articy when it's calculated the available branches
    /// </summary>    
    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
        StaticStuff.PrintFlowBranchesUpdate("************** OnBranchesUpdated() START *************");
        StaticStuff.PrintFlowBranchesUpdate("Num branches: " + aBranches.Count);
       // Debug.Log("************** OnBranchesUpdated() START *************");
        //Debug.Log("Num branches: " + aBranches.Count);

        CurBranches.Clear();
        
        int i = 0;
        foreach (Branch b in aBranches)
        {
            StaticStuff.PrintFlowBranchesUpdate("branch: " + i + " is type: " + b.Target.GetType());
            //Debug.Log("branch: " + i + " is type: " + b.Target.GetType() + ", desc: " + b.DefaultDescription);
            string s = "branch: " + i + " is type: " + b.Target.GetType() + ", desc: " + b.DefaultDescription;
            DialogueFragment d = b.Target as DialogueFragment;
            if (d != null) s += ", tech name: " + d.TechnicalName;
            //Debug.Log(s);
            if (b.IsValid == false) Debug.LogWarning("Invalid branch in OnBranchesUpdate(): " + b.DefaultDescription);
            if (b.Target == CurPauseObject)
            {
                Debug.LogWarning("ERROR: We should NOT have a Branch that points to itself");
                //NextBranch = b;
                SetNextBranch(b);
                return;
            }

            CurBranches.Add(b);
        }
        DialogueFragment df = CurPauseObject as DialogueFragment;        
        if(df != null )
        {
            StaticStuff.PrintFlowBranchesUpdate("We're on a dialogue fragment, so set the text based on current flow state.");
            LastDFPlayed = df;
            switch (CurArticyState)
            {
                case ArticyState.CUT_SCENE:
                    CutSceneUI.SetCutsceneNode(CurPauseObject as DialogueFragment);
                    break;
                case ArticyState.CONVERSATION:
                    if (ActiveCALPauseObjects.Count == 0) Player.StopNavMeshMovement();                    
                    ConvoUI.ShowDialogueFragment(CurPauseObject as DialogueFragment, CurPauseObject, aBranches);
                    break;
                default:
                    Debug.LogError("In invalid state for DialogueFragment: " + CurArticyState);
                    break;
            }
        }
        else if (aBranches.Count == 1)
        {   // We're paused and there's only one valid branch available. This is common so have it's own section
            if (CurBranches[0].Target.GetType().Equals(typeof(Scene)))
            {
                StaticStuff.PrintFlowBranchesUpdate("The target is a Scene, so Play() it.");
                //NextBranch = CurBranches[0];
                SetNextBranch(CurBranches[0]);
            }
            else if (CurPauseObject.GetType().Equals(typeof(Cut_Scene)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're starting a cut scene.");
                StartCutScene(CurPauseObject as Cut_Scene);
            }
            else if (CurPauseObject.GetType().Equals(typeof(Dialogue)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're about to start a Dialogue.");
                //NextBranch = CurBranches[0];
                SetNextBranch(CurBranches[0]);
            }
            else if (CurBranches[0].Target.GetType().Equals(typeof(Hub)))
            {
                StaticStuff.PrintFlowBranchesUpdate("The target is a Hub, so Play() it.");                
                //NextBranch = CurBranches[0];
                SetNextBranch(CurBranches[0]);
            }
            else if (CurBranches[0].Target.GetType().Equals(typeof(Cut_Scene)))
            {
                StaticStuff.PrintFlowBranchesUpdate("The target is a Cut_Scene, so Play() it.");
               // NextBranch = CurBranches[0];
                SetNextBranch(CurBranches[0]);
            }
            else if (CurPauseObject.GetType().Equals(typeof(Hub)) && CurBranches[0].Target.GetType().Equals(typeof(OutputPin)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're paused on a Hub with no Target so we're in Free Roam.  Don't Play() anything.");
                FlowFragsVisited.Clear();
                CurArticyState = ArticyState.FREE_ROAM;
            }
            else if (CurPauseObject.GetType().Equals(typeof(Character_Action_List_Template)))
            {               
                Debug.Log("-----------------------------------We've arrived at a Character Action List so let's see what's up");
                //Debug.Log("Num Branches: " + CurBranches.Count);
                NextBranch = CurBranches[0];                
                Player.ToggleMovementBlocked(true);
                Character_Action_List_FeatureFeature CurCALObject = (CurPauseObject as Character_Action_List_Template).Template.Character_Action_List_Feature;
                ActiveCALPauseObjects = CurCALObject.PauseFrags;
                //Debug.Log("num pause frags: " + ActiveCALPauseObjects.Count);
                GetComponent<CharacterActionList>().BeginCAL(CurCALObject);                
            }
            else
            {
                Debug.LogWarning("We haven't supported this single branch situation yet. CurPauseObject: " + CurPauseObject.GetType() + ", branch: " + CurBranches[0].Target.GetType());
            }            
        }
        else
        {
            Debug.LogWarning("We haven't supported this case yet.");
        }

        StaticStuff.PrintFlowBranchesUpdate("************** OnBranchesUpdated() END ***************");
        //Debug.Log("************** OnBranchesUpdated() END ***************");
    }
    #endregion

    List<ArticyObject> ActiveCALPauseObjects = new List<ArticyObject>(); 
    #region CHARACTER_ACTION_LIST    

    /// <summary>
    /// End the current Character Action List.  This is called from the CharacterActionList component
    /// </summary>
    public void EndCAL()
    {        
        ActiveCALPauseObjects.Clear();        
    }
    #endregion    

    #region UI
    public void StartCutScene(Cut_Scene cutScene)
    {
        CurArticyState = ArticyState.CUT_SCENE;        
        CutSceneUI.StartCutScene(cutScene);
        //NextBranch = CurBranches[0];
        SetNextBranch(CurBranches[0]);
    }

    List<string> FlowFragsVisited = new List<string>();
    public void StartConvo(Dialogue convoStart)
    {
        Debug.Log("************************************ START CONVO on frag with technical name: " + convoStart.TechnicalName);
        FlowFragsVisited.Clear();
        CurArticyState = ArticyState.CONVERSATION;
        FlowPlayer.StartOn = convoStart;
    }

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
        CutSceneUI.EndCutScene();
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
        if (NextBranch != null && WaitingOnActionList() == false)
        {
            Branch b = NextBranch;
            //NextBranch = null;
            SetNextBranch(null);
            FlowPlayer.Play(b);
        }
        if (DebugText != null)
        {
            DebugText.text = CurArticyState.ToString() + "\n";
            //DebugText.text += "CALActive?: " + CALActive + "\n";
            if (NextBranch == null) DebugText.text += "NextBranch is null\n";
            else DebugText.text += "WaitingOnActionList()? " + WaitingOnActionList() + "\n";
            foreach (string s in FlowFragsVisited)
            {
                DebugText.text += s + "\n";
            }
            //DebugText.text += Player.IsMovementBlocked() + "\n";
        }
    }
}