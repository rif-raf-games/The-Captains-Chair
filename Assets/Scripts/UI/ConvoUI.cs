using Articy.The_Captain_s_Chair;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConvoUI : MonoBehaviour
{
    public const float TYPE_SPEED = 30f;
    public GameObject SpeakerPanel;
    public Text SpeakerName;
    public Text SpeakerText;
    public Image SpeakerImage;
    public float DefaultTypewriterSpeed = 20f;
    float TypewriterSpeed;
    public GameObject[] DialogueOptions;

    ArticyFlow ArticyFlow;
    bool TextTyping = false;
    bool IsInteractive = true;

    public CCPlayer Player;
    Coroutine TypewriterCoroutine;
    string CurDialogueText;

    private void Awake()
    {
        TypewriterSpeed = DefaultTypewriterSpeed;
        //Debug.Log(TypewriterSpeed);
    }
    // Start is called before the first frame update
    void Start()
    {
        ArticyFlow = GameObject.FindObjectOfType<ArticyFlow>();                
    }
    public void ShowDialogueFragment(DialogueFragment dialogueFrag, IFlowObject flowObj, IList<Branch> dialogueOptions, bool isInteractive, float typewriterSpeed)
    {
        StaticStuff.PrintUI("going to set up a dialogue fragment with speaker: " + dialogueFrag.Speaker + " with text: " + dialogueFrag.Text + ", tech name: " + dialogueFrag.TechnicalName);
        StaticStuff.PrintUI("this dialogue fragment has: " + dialogueOptions.Count + " options");
        DialogueFragment d = dialogueOptions[0].Target as DialogueFragment;
        if(d!=null) StaticStuff.PrintUI(d.MenuText + ", " + d.Text + ", " + d.TechnicalName);
        this.gameObject.SetActive(true);

        Entity speakerEntity = ((Entity)dialogueFrag.Speaker);
        if (speakerEntity == null) Debug.LogError("THIS DIALOGUE HAS NO ENTITY");
        else
        {
            if (speakerEntity.DisplayName.Equals("Dialogue Pause")) SpeakerPanel.SetActive(false);
            else SpeakerPanel.SetActive(true);
            if (speakerEntity.PreviewImage.Asset != null) SpeakerImage.sprite = speakerEntity.PreviewImage.Asset.LoadAssetAsSprite();
            else SpeakerImage.sprite = null;
            SpeakerName.text = speakerEntity.DisplayName;
        }        
        IsInteractive = isInteractive;
        TypewriterSpeed = typewriterSpeed;
        SpeakerText.text = "";
        CurDialogueText = dialogueFrag.Text;
        if (TypewriterCoroutine != null ) StopCoroutine(TypewriterCoroutine);
        TypewriterCoroutine = StartCoroutine(TypewriterEffect());
        
        foreach (GameObject go in DialogueOptions) go.SetActive(false);
        if(IsInteractive == true)
        {
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

        // VO stuff
        VO_Dialogue_Fragment vod = flowObj as VO_Dialogue_Fragment;
        if (vod != null)
        {   // we've got a voice over dialogue                        
            Asset a = vod.Template.VO_File.VOFile as Asset;
            AudioClip ac = a.LoadAsset<AudioClip>();
            SoundFXPlayer.PlayVO(ac);
            string color = vod.Template.VO_File.Color;
            if (color != "")
            {
                string[] colors = color.Split(',');
                SpeakerText.color = new Color(int.Parse(colors[0]), int.Parse(colors[1]), int.Parse(colors[2]));
            }
        }
        else
        {
            SpeakerText.color = Color.white;
        }
    }

    IEnumerator TypewriterEffect()
    {
        //Debug.Log("Start effect: " + TypewriterSpeed);        
        foreach (GameObject go in DialogueOptions) go.GetComponent<Button>().enabled = false;
        foreach (char character in CurDialogueText.ToCharArray())
        {
            SpeakerText.text += character;
            yield return new WaitForSeconds(1f / TypewriterSpeed);
            TextTyping = true; // wait a sec so that the click off via any press on screen 
        }
        TextTyping = false;
       // Debug.Log("TypewriterEffect end: " + IsInteractive);
        if(IsInteractive == false)
        {
            StartCoroutine(NextDialogueFragmentDelay());
        }
        else
        {
            foreach (GameObject go in DialogueOptions) go.GetComponent<Button>().enabled = true;
        }        
    }

    IEnumerator NextDialogueFragmentDelay()
    {
       // Debug.Log("NextDialogueFragmentDelay() a");
        yield return new WaitForSeconds(2);
        foreach (GameObject go in DialogueOptions) go.GetComponent<Button>().enabled = true;
       // Debug.Log("NextDialogueFragmentDelay() b");
        ArticyFlow.UIButtonCallback(0);
    }

    // Update is called once per frame
    void Update()
    {
        if(TextTyping == true && IsInteractive == true && Input.GetMouseButtonUp(0))
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

