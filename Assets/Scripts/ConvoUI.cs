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

    public Player Player;

    public void ShowDialogueFragment(DialogueFragment dialogueFrag, IFlowObject flowObj, IList<Branch> dialogueOptions)
    {
        Debug.Log("going to set up a dialogue fragment with speaker: " + dialogueFrag.Speaker + " with text: " + dialogueFrag.Text);
        Debug.Log("this dialogue fragment has: " + dialogueOptions.Count + " options");
        this.gameObject.SetActive(true);


        //ArticyObject a = (ArticyObject)flowObj;
        // ArticyObject speaker = dialogueFrag.Speaker;
        //Entity speakerEntity = (Entity)dialogueFrag.Speaker;
        /* var a = dialogueFrag as IObjectWithFeaturenpcCrew;        
         if(a != null ) 
         {
             SpeakerName.text = ((npcCrew)a).DisplayName;
         }
         else
         {
             SpeakerName.text = ((PC)dialogueFrag).DisplayName;
         }*/
        // npcCrew crew = (npcCrew)dialogueFrag.Speaker;
        //var crew = dialogueFrag as IObjectWithFeaturenpcCrew;
        //IObjectWithFeaturenpcCrew
        //npcCrew crew = (npcCrew)dialogueFrag.Speaker;
        //if (crew != null) SpeakerName.text = crew.DisplayName;
        // else SpeakerName.text = "NO NAME!";
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

    public void EndConversation()
    {
        this.gameObject.SetActive(false);
        Player.ToggleMovementBlocked(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
