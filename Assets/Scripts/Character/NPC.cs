using Articy.Unity;
using Articy.The_Captain_s_Chair.Features;
using Articy.The_Captain_s_Chair;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : CharacterEntity
{    
    Character_Action_List_Template CurBehavior;
    List<Character_Action_List_Template> Behaviors = new List<Character_Action_List_Template>();
    TextMesh BarkText;
    
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        
        BarkText = GetComponentInChildren<TextMesh>();
        if (BarkText == null) { Debug.LogError("There's no TextMesh on this NPC: " + this.gameObject.name); return; }
        ToggleBarkText(false);
       
        AI_Template t = ArticyAIReference.GetObject() as AI_Template;
        if (t == null) { Debug.LogError("This NPC: " + this.name + " does not have an AI_Template on it's ArticyAIReference"); return; }
        List<ArticyObject> children = t.Children;
        foreach(ArticyObject child in children)
        {
            Behaviors.Add(child as Character_Action_List_Template);
        }

        RestartBehavior();
    }    

    public void RestartBehavior()
    {
        CurBehavior = GetValidBehavior();
        StartCurrentBehavior();
    }

    Character_Action_List_Template GetValidBehavior()
    {
        Character_Action_List_Template validBehavior = null;

        foreach (Character_Action_List_Template cat in Behaviors)
        {
            List<InputPin> inPins = cat.InputPins;
            foreach (InputPin pin in inPins)
            {
                ArticyScriptCondition conditionScript = pin.Text;
                bool b = conditionScript.CallScript();
                //Debug.Log(pin.Text.RawScript + ": " + b);
                if(b == true)
                {
                    if (validBehavior != null) Debug.LogWarning("You have more than one valid Behavior on this NPC's AI: " + this.name);
                    validBehavior = cat;
                    //validList = cat.Template.Character_Action_List_Feature;
                }
            }            
        }

        return validBehavior;
    }

    void StartCurrentBehavior()
    {
        if(CurBehavior == null) { Debug.LogError("trying to start a null behavior on this npc: " + this.name); return; }

        GetComponent<BehaviorFlowPlayer>().StartBehaviorFlow(CurBehavior, this.gameObject);
        //Character_Action_List_FeatureFeature actionListToPlay = CurBehavior.Template.Character_Action_List_Feature;
        //MyActionListPlayer.BeginCAL(actionListToPlay, this.gameObject);       
    }

    public void EndCAL()
    {
       // Debug.Log(this.name + ": NPC EndCAL()");
        if (CurBehavior == null) { Debug.LogError("trying to get output pins on a null behavior on this npc: " + this.name); return; }
        List<OutputPin> outputPins = CurBehavior.OutputPins;
        foreach(OutputPin pin in outputPins)
        {
            ArticyScriptInstruction instructionScrip = pin.Text;
            instructionScrip.CallScript();
            //Debug.Log(pin.Text.RawScript);
        }
        RestartBehavior();
    }

    public void SetBarkText(string text)
    {
        //Debug.LogWarning("Setting text: " + text + " on object: " + this.name);
        BarkText.text = text;
    }
    public void ToggleBarkText(bool val)
    {
        BarkText.GetComponent<MeshRenderer>().enabled = val;
        if(val == true)
        {
            BarkText.transform.LookAt(Camera.main.transform);
            BarkText.transform.Rotate(0f, 180, 0f);
        }
    }
  

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
    public override void LateUpdate()
    {
        base.LateUpdate();
    }
}
