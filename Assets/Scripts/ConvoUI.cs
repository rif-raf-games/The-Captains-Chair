using Articy.The_Captain_s_Chair;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConvoUI : MonoBehaviour
{
    public Text SpeakerName;
    public Text SpeakerText;
    public GameObject[] DialogueOptions;

    TheCaptainsChair CapChair;

    public CCPlayer Player;

    public void ShowDialogueFragment(DialogueFragment dialogueFrag, IFlowObject flowObj, IList<Branch> dialogueOptions)
    {
        StaticStuff.PrintUI("going to set up a dialogue fragment with speaker: " + dialogueFrag.Speaker + " with text: " + dialogueFrag.Text + ", tech name: " + dialogueFrag.TechnicalName);
        StaticStuff.PrintUI("this dialogue fragment has: " + dialogueOptions.Count + " options");
        DialogueFragment d = dialogueOptions[0].Target as DialogueFragment;
        if(d!=null) StaticStuff.PrintUI(d.MenuText + ", " + d.Text + ", " + d.TechnicalName);
        this.gameObject.SetActive(true);
        
        SpeakerName.text = ((Entity)dialogueFrag.Speaker).DisplayName;
        SpeakerText.text = dialogueFrag.Text;
        foreach (GameObject go in DialogueOptions) go.SetActive(false);
        for (int i = 0; i < dialogueOptions.Count; i++)
        {
            DialogueOptions[i].SetActive(true);
            DialogueFragment df = dialogueOptions[i].Target as DialogueFragment;
            if (df != null)
            {                
                DialogueOptions[i].GetComponentInChildren<Text>().text = df.MenuText;
            }
            else
            {
                DialogueOptions[i].GetComponentInChildren<Text>().text = "Continue";                
            }
        }

        Player.ToggleMovementBlocked(true);
    }

    public void PauseConversation()
    {
        // conversation isn't over, but we want to temporarily shut it off while a character moves somewhere.
       // Debug.Log("----------------------- PauseConversation()");
        this.gameObject.SetActive(false);
        Player.ToggleMovementBlocked(true);
    }
    public void EndConversation()
    { 
        //Debug.Log("----------------------- EndConversation()");
        this.gameObject.SetActive(false);
        Player.ToggleMovementBlocked(false);        
    }
    // Start is called before the first frame update
    void Start()
    {
        CapChair = GameObject.FindObjectOfType<TheCaptainsChair>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
