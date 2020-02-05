using Articy.The_Captain_s_Chair;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConvoUI : MonoBehaviour
{
    public GameObject SpeakerPanel;
    public Text SpeakerName;
    public Text SpeakerText;
    public GameObject[] DialogueOptions;

    TheCaptainsChair CapChair;
    public bool TextTyping = false;    

    public CCPlayer Player;
    Coroutine TypewriterCoroutine;
    string CurDialogueText;

    // Start is called before the first frame update
    void Start()
    {
        CapChair = GameObject.FindObjectOfType<TheCaptainsChair>();
    }
    public void ShowDialogueFragment(DialogueFragment dialogueFrag, IFlowObject flowObj, IList<Branch> dialogueOptions)
    {
        StaticStuff.PrintUI("going to set up a dialogue fragment with speaker: " + dialogueFrag.Speaker + " with text: " + dialogueFrag.Text + ", tech name: " + dialogueFrag.TechnicalName);
        StaticStuff.PrintUI("this dialogue fragment has: " + dialogueOptions.Count + " options");
        DialogueFragment d = dialogueOptions[0].Target as DialogueFragment;
        if(d!=null) StaticStuff.PrintUI(d.MenuText + ", " + d.Text + ", " + d.TechnicalName);
        this.gameObject.SetActive(true);
        Entity speaker = ((Entity)dialogueFrag.Speaker);

        if(speaker.DisplayName.Equals("Dialogue Pause")) SpeakerPanel.SetActive(false);        
        else SpeakerPanel.SetActive(true);        
        SpeakerName.text = speaker.DisplayName;
       
        SpeakerText.text = "";
        CurDialogueText = dialogueFrag.Text;
        if (TypewriterCoroutine != null ) StopCoroutine(TypewriterCoroutine);
        TypewriterCoroutine = StartCoroutine(TypewriterEffect());
        
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
    }

    IEnumerator TypewriterEffect()
    {
        //Debug.Log("Start effect");        
        foreach (GameObject go in DialogueOptions) go.GetComponent<Button>().enabled = false;
        foreach (char character in CurDialogueText.ToCharArray())
        {
            SpeakerText.text += character;
            yield return new WaitForSeconds(0.05f);
            TextTyping = true; // wait a sec so that the click off via any press on screen 
        }
        TextTyping = false;
        foreach (GameObject go in DialogueOptions) go.GetComponent<Button>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(TextTyping == true && Input.GetMouseButtonUp(0))
        {
            //Debug.Log("shut off via button");
            StopCoroutine(TypewriterCoroutine);
            SpeakerText.text = CurDialogueText;
            foreach (GameObject go in DialogueOptions) go.GetComponent<Button>().enabled = true;
            TextTyping = false;
        }
    }

    public void PauseConversation()
    {
        // conversation isn't over, but we want to temporarily shut it off while a character moves somewhere.
        Debug.Log("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ PauseConversation()");
        this.gameObject.SetActive(false);
       // Player.ToggleMovementBlocked(true);
    }
    public void EndConversation()
    { 
        //Debug.Log("----------------------- EndConversation()");
        this.gameObject.SetActive(false);
        //Player.ToggleMovementBlocked(false);        
    }
    

    

}

