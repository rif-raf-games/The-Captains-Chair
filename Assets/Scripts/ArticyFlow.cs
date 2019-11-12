using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Articy.Unity;
using Articy.The_Captain_s_Chair;

public class ArticyFlow : MonoBehaviour, IArticyFlowPlayerCallbacks, IScriptMethodProvider
{
    public enum ArticyState { FREE_ROAM, CUT_SCENE, CONVERSATION, NUM_ARTICY_STATES };
    public ArticyState CurArticyState;

    public TheCaptainsChair CaptainsChair;

    ArticyFlowPlayer FlowPlayer;
    IFlowObject CurPauseObject = null;
    List<Branch> CurBranches = new List<Branch>();
    Branch NextBranch = null;

    public CutSceneUI CutSceneUI;
    public ConvoUI ConvoUI;

    public bool IsCalledInForecast { get; set; }
    
    public void OpenCaptainsDoor()
    {
        if (IsCalledInForecast == false)
        {
            Debug.Log("-------------------------------------------------------------- OpenCaptainDoor(): Open the door");
            CaptainsChair.OpenCaptainsDoor();
        }
        else
        {
            Debug.Log("-------------------------------------------------------------- OpenCaptainDoor(): Do NOT open door, we're just forecasting");
        }
            
    }

    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() START *************");
        if(aObject == null)
        {
            Debug.LogWarning("We have a null iFlowObject in OnFlowPlayerPaused(), so we're at a dangling end point somewhere that needs to get sorted out.");
            return;
        }
        StaticStuff.PrintFlowPaused("OnFlowPlayerPaused() IFlowObject Type: " + aObject.GetType() + ", with TechnicalName: " + ((ArticyObject)aObject).TechnicalName);

        CurPauseObject = aObject;

        StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() END ***************");
    }
    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
        StaticStuff.PrintFlowBranchesUpdate("************** OnBranchesUpdated() START *************");
        StaticStuff.PrintFlowBranchesUpdate("Num branches: " + aBranches.Count);

        CurBranches.Clear();
        int i = 0;
        foreach (Branch b in aBranches)
        {
            StaticStuff.PrintFlowBranchesUpdate("branch: " + i + " is type: " + b.Target.GetType());
            if (b.IsValid == false) Debug.LogWarning("Invalid branch in OnBranchesUpdate(): " + b.DefaultDescription);
            CurBranches.Add(b);
        }
        DialogueFragment df = CurPauseObject as DialogueFragment;
        //if (df == null) Debug.Log("we do not have a dialogue fragment");
        //else Debug.Log("we have a dialogue fragment");
        //if (CurPauseObject.GetType().Equals(typeof(DialogueFragment)))
        if(df != null )
        {
            StaticStuff.PrintFlowBranchesUpdate("We're on a dialogue fragment, so set the text based on current flow state.");
            switch (CurArticyState)
            {
                case ArticyState.CUT_SCENE:
                    CutSceneUI.SetCutsceneNode(CurPauseObject as DialogueFragment);
                    break;
                case ArticyState.CONVERSATION:
                    ConvoUI.ShowDialogueFragment(CurPauseObject as DialogueFragment, CurPauseObject, aBranches);
                    break;
                default:
                    Debug.LogError("In invalid state for DialogueFragment: " + CurArticyState);
                    break;
            }
        }
        else if (aBranches.Count == 1)
        {   // We're paused and there's only one valid branch available. This is common so have it's own section
            if(CurBranches[0].Target.GetType().Equals(typeof(Scene)))
            {
                StaticStuff.PrintFlowBranchesUpdate("The target is a Scene, so Play() it.");
                NextBranch = CurBranches[0];
            }
            else if (CurPauseObject.GetType().Equals(typeof(Cut_Scene)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're starting a cut scene.");
                StartCutScene(CurPauseObject as Cut_Scene);
            }
            else if(CurPauseObject.GetType().Equals(typeof(Dialogue)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're about to start a Dialogue.");
                NextBranch = CurBranches[0];
            }            
            else if(CurBranches[0].Target.GetType().Equals(typeof(Hub)))
            {
                StaticStuff.PrintFlowBranchesUpdate("The target is a Hub, so Play() it.");
                NextBranch = CurBranches[0];
            }
            else if (CurBranches[0].Target.GetType().Equals(typeof(Cut_Scene)))
            {
                StaticStuff.PrintFlowBranchesUpdate("The target is a Cut_Scene, so Play() it.");
                NextBranch = CurBranches[0];
            }
            else if(CurPauseObject.GetType().Equals(typeof(Hub)) && CurBranches[0].Target.GetType().Equals(typeof(OutputPin)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're paused on a Hub with no Target so we're in Free Roam.  Don't Play() anything.");
                CurArticyState = ArticyState.FREE_ROAM;
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
    }

    public void StartCutScene(Cut_Scene cutScene)
    {
        CurArticyState = ArticyState.CUT_SCENE;        
        CutSceneUI.StartCutScene(cutScene);
        NextBranch = CurBranches[0];
    }

    public void StartConvo(Dialogue convoStart)
    {
        StaticStuff.PrintUI("Start Conversations");
        CurArticyState = ArticyState.CONVERSATION;
        FlowPlayer.StartOn = convoStart;
    }

    public void UIButtonCallback(int buttonIndex)
    {
        StaticStuff.PrintUI("UIButtonCallback() buttonIndex: " + buttonIndex);
        NextBranch = CurBranches[buttonIndex];
        if (CurBranches[buttonIndex].Target.GetType().Equals(typeof(DialogueFragment)) == false)
        {
            StaticStuff.PrintUI("Chosen branch isn't a dialogue fragment, so for now assume we're done talking and shut off the UI");
            CutSceneUI.EndCutScene();
            ConvoUI.EndConversation();
        }
        else
        {
            StaticStuff.PrintUI("Chosen branch is a dialogue fragment, so just let the engine handle the next phase.");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        FlowPlayer = this.GetComponent<ArticyFlowPlayer>();
        CurArticyState = ArticyState.NUM_ARTICY_STATES;
    }

    // Update is called once per frame
    void Update()
    {
        if (NextBranch != null)
        {
            Branch b = NextBranch;
            NextBranch = null;
            FlowPlayer.Play(b);
        }
    }
}
