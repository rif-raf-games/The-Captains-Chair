using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Articy.Unity;
using Articy.The_Captain_s_Chair;
using UnityEngine.UI;

public class MRLab : MonoBehaviour, IArticyFlowPlayerCallbacks
{
    IFlowObject CurPauseObject = null;
    Branch NextBranch = null;
    public ArticyFlowPlayer FlowPlayer;

    public Text SpeakerText;
    public GameObject DialogueUI;
    public GameObject[] DialogueOptions;
    public MRLabPlayer Player;
    List<Branch> CurBranches = new List<Branch>();
    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        Debug.Log("************** OnFlowPlayerPaused() START *************");
        Debug.Log("OnFlowPlayerPaused() IFlowObject Type: " + aObject.GetType() + ", with TechnicalName: " + ((ArticyObject)aObject).TechnicalName);
        CurPauseObject = aObject;
        Debug.Log("************** OnFlowPlayerPaused() END ***************");
    }

    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
        Debug.Log("************** OnBranchesUpdated() START *************");
        Debug.Log("Num branches: " + aBranches.Count);

        CurBranches.Clear();
        foreach (Branch b in aBranches) CurBranches.Add(b);
            if (aBranches.Count == 1 && aBranches[0].IsValid && aBranches[0].Target.GetType().Equals(typeof(Hub)))
        {
            Debug.Log("only one valid branch and it's a hub called: " + aBranches[0].DefaultDescription + " so Play() it");
            NextBranch = aBranches[0];
        }
        else
        {
            if (aBranches.Count == 1 && CurPauseObject.GetType().Equals(typeof(Hub)))
            {
                if (aBranches[0].Target.GetType().Equals(typeof(OutputPin)))
                {
                    Debug.Log("we're on a hub with only an output pin, so we're in free roam waiting for a collision trigger");
                }
                else
                {
                    Debug.Log("only valid output is something else that an OutputPin...so ROCK IT via Play(NextBranch)");
                    NextBranch = aBranches[0];
                }
            }
            else
            {
                Debug.Log("--------------------------NOT in a situation where we have 1 valid Hub branch, so find out what to do.");
                if (CurPauseObject.GetType().Equals(typeof(DialogueFragment)))
                {
                    Debug.Log("We're on a dialogue fragment, so set up the dialogue UI");
                    ShowDialogueFragment(CurPauseObject as DialogueFragment, aBranches);
                }
                else
                {
                    Debug.LogWarning("Not sure what to do here");
                }
            }
        }
        Debug.Log("************** OnBranchesUpdated() END ***************");
    }

    void ShowDialogueFragment(DialogueFragment dialogueFrag, IList<Branch> dialogueOptions)
    {
        Debug.Log("going to set up a dialogue fragment with speaker: " + dialogueFrag.Speaker + " with text: " + dialogueFrag.Text);
        Debug.Log("this dialogue fragment has: " + dialogueOptions.Count + " options");
        DialogueUI.gameObject.SetActive(true);
        SpeakerText.text = dialogueFrag.Text;

        foreach (GameObject go in DialogueOptions) go.SetActive(false);

        for (int i = 0; i < dialogueOptions.Count; i++)
        {
            DialogueOptions[i].SetActive(true);
            DialogueFragment df = dialogueOptions[i].Target as DialogueFragment;
            if (df != null)
            {
                Debug.Log("a");
                DialogueOptions[i].GetComponentInChildren<Text>().text = df.MenuText;
            }
            else
            {
                DialogueOptions[i].GetComponentInChildren<Text>().text = "Continue";
                /*string buttonText = dialogueFrag.MenuText;
                Debug.Log("ok we are not going to a dialogue fragment, so get the button text by the template: " + buttonText);
                if (buttonText.Equals("") == false)
                {
                    Debug.Log("b");
                    DialogueOptions[i].GetComponentInChildren<Text>().text = buttonText;
                }
                else
                {
                    Debug.Log("c");
                    //string val = ArticyDatabase.DefaultGlobalVariables.GetVariableByString<string>("Misc_Globals.defaultDialogueFragmentButtonText");
                    DialogueOptions[i].GetComponentInChildren<Text>().text = "";
                }*/
            }
        }

        Player.ToggleMovementBlocked(true);
    }
    public void DialogueButtonCallback(int buttonIndex)
    {
        Debug.Log("DialogueButtonCallback() buttonIndex: " + buttonIndex);
        NextBranch = CurBranches[buttonIndex];
        //base.PrintBranchInfo(CurBranches[buttonIndex], "DialogueButtonCallback");
        if (CurBranches[buttonIndex].Target.GetType().Equals(typeof(DialogueFragment)) == false)
        {
            //Debug.Log("we're done with the current dialogue tree, so shut off the UI and let the flow handle itself");
            Debug.Log("Chosen branch isn't a dialogue fragment, so for now assume we're done talking and shut off the UI");
            ShutOffDialogueUI();
        }
        else
        {
            //Debug.LogWarning("Need to account for a flow fragment off of a dialogue UI button press");
            Debug.Log("Chosen branch is a dialogue fragment, so just let the engine handle the next phase.");
        }
    }

    void ShutOffDialogueUI()
    {
        DialogueUI.gameObject.SetActive(false);

        Player.ToggleMovementBlocked(false);
    }

    void Update()
    {
        if (NextBranch != null)
        {
            Branch b = NextBranch;
            NextBranch = null;
            FlowPlayer.Play(b);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        DialogueUI.gameObject.SetActive(false);
    }

    // Update is called once per frame
    
}
