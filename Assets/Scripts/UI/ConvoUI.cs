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
    MCP MCP;
    bool TextTyping = false;
    bool IsInteractive = true;
    List<ArticyObject> CurValidAOTargets = null;
    IList<Branch> CurDialogueOptions = null;

    public CCPlayer Player;
    Coroutine TypewriterCoroutine;
    string CurDialogueText;

    int NumValidButtons = 0;

    private void Awake()
    {
        TypewriterSpeed = DefaultTypewriterSpeed;       
    }    

    public void SetSceneArticyFlowObject()
    {
        ArticyFlow = GameObject.FindObjectOfType<ArticyFlow>();        
    }
    public void SetMCP(MCP mcp)
    {
        this.MCP = mcp;       
    }

    public void ShowDialogueFragment(DialogueFragment dialogueFrag, IFlowObject flowObj, IList<Branch> dialogueOptions, bool isInteractive, float typewriterSpeed, List<ArticyObject> validAOTargets = null)
    {
        StaticStuff.PrintUI("going to set up a dialogue fragment with speaker: " + dialogueFrag.Speaker + " with text: " + dialogueFrag.Text + ", tech name: " + dialogueFrag.TechnicalName);

        this.MCP.StartDialogueConversation();
        if (dialogueOptions != null)
        {   // if dialogeOptions is null then we're calling this from a debug spot
            StaticStuff.PrintUI("this dialogue fragment has: " + dialogueOptions.Count + " options");
            DialogueFragment d = dialogueOptions[0].Target as DialogueFragment;
            if (d != null) StaticStuff.PrintUI(d.MenuText + ", " + d.Text + ", " + d.TechnicalName);
        }
        

        Entity speakerEntity = ((Entity)dialogueFrag.Speaker);
        if (speakerEntity == null) Debug.LogError("THIS DIALOGUE HAS NO ENTITY");
        else
        {
            if (speakerEntity.DisplayName.Equals("Dialogue Pause")) SpeakerPanel.SetActive(false);
            else SpeakerPanel.SetActive(true);
            if (speakerEntity.PreviewImage.Asset != null) SpeakerImage.sprite = speakerEntity.PreviewImage.Asset.LoadAssetAsSprite();
            else SpeakerImage.sprite = null;
            SpeakerName.text = speakerEntity.DisplayName;
            if(SpeakerName.text.Contains("Captain"))
            {
                SpeakerImage.sprite = this.MCP.CaptainAvatar;
            }
        }
        NumValidButtons = 0;
        IsInteractive = isInteractive;
        TypewriterSpeed = typewriterSpeed;
        SpeakerText.text = "";
        CurDialogueText = dialogueFrag.Text;
        CurDialogueOptions = dialogueOptions;
        CurValidAOTargets = validAOTargets;
        
        SetupValidButtons();
        ShutOffButtons();
        if (TypewriterCoroutine != null) StopCoroutine(TypewriterCoroutine);
        TypewriterCoroutine = StartCoroutine(TypewriterEffect());

        // VO stuff
        VO_Dialogue_Fragment vod = flowObj as VO_Dialogue_Fragment;
        if (vod != null)
        {   // we've got a voice over dialogue                        
            Asset a = vod.Template.VO_File.VOFile as Asset;
            if(a != null)
            {
                AudioClip ac = a.LoadAsset<AudioClip>();
                SoundFXPlayer.PlayVO(ac);
            }            
            string color = vod.Template.VO_File.Color;
            if (color != "")
            {
                string[] colors = color.Split(',');
                SpeakerText.color = new Color(int.Parse(colors[0]), int.Parse(colors[1]), int.Parse(colors[2]));
            }
            SpeakerText.fontStyle = FontStyle.Italic;
        }
        else
        {
            SpeakerText.color = Color.white;
            SpeakerText.fontStyle = FontStyle.Normal;
        }
    }

    public void ShutOffButtons()
    {
        foreach (GameObject go in DialogueOptions) go.SetActive(false);
    }
    public void TurnOnValidButtons()
    {
        for (int i = 0; i < NumValidButtons; i++) DialogueOptions[i].SetActive(true);
    }
    public void SetupValidButtons()
    {
        NumValidButtons = (CurDialogueOptions != null ? CurDialogueOptions.Count : CurValidAOTargets.Count);
        for (int i = 0; i < NumValidButtons; i++)
        {
            DialogueOptions[i].SetActive(true);
            DialogueFragment df = (CurDialogueOptions != null ? CurDialogueOptions[i].Target as DialogueFragment : CurValidAOTargets[i] as DialogueFragment);
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
        //Debug.Log("Start effect: " + TypewriterSpeed);                
        foreach (GameObject go in DialogueOptions) go.SetActive(false);
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
            for (int i = 0; i < NumValidButtons; i++) DialogueOptions[i].SetActive(true);
        }        
    }

    IEnumerator NextDialogueFragmentDelay()
    {
       // Debug.Log("NextDialogueFragmentDelay() a");
        yield return new WaitForSeconds(2);        
        for (int i = 0; i < NumValidButtons; i++) DialogueOptions[i].SetActive(true);        
        ArticyObject target = null;
        if (CurValidAOTargets != null) target = CurValidAOTargets[0];
        ArticyFlow.ConvoButtonClicked(0, target);
    }    
   
    public void EndConversation()
    { 
        //Debug.LogWarning("----------------------- EndConversation()");
        this.MCP.StartFreeRoam();        
    }
    
    public void OnClickDialogueButton(int buttonIndex)
    {
        //Debug.LogError("OnClickDialogueButton() buttonIndex: " + buttonIndex + ", ArticyFlow hash: " + ArticyFlow.GetHashCode());        
        ArticyObject target = null;
        if (CurValidAOTargets != null) target = CurValidAOTargets[buttonIndex];
        ArticyFlow.ConvoButtonClicked(buttonIndex, target);
    }
    

}