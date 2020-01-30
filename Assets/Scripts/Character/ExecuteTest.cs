using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Articy.Unity;
using Articy.The_Captain_s_Chair;

public class ExecuteTest : MonoBehaviour
{
    public ArticyRef MyAIRef;
    List<Character_Action_List_Template> Behaviors = new List<Character_Action_List_Template>();
    // Start is called before the first frame update
    void Start()
    {
        AI_Template t = MyAIRef.GetObject() as AI_Template;                               
        List<ArticyObject> children = t.Children;
        foreach(ArticyObject child in children)
        {
            Behaviors.Add(child as Character_Action_List_Template);            
        }
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(0,0,100,100), "feh"))
        {
            foreach(Character_Action_List_Template cat in Behaviors)
            {
                List<InputPin> inPins = cat.InputPins;
                foreach (InputPin pin in inPins)
                {                    
                    ArticyScriptCondition conditionScript = pin.Text;
                    bool b = conditionScript.CallScript();
                    Debug.Log(pin.Text.RawScript + ": " + b);                    
                }
                List<OutputPin> putPins = cat.OutputPins;
                foreach (OutputPin pin in putPins)
                {
                    ArticyScriptInstruction instructionScrip = pin.Text;
                    instructionScrip.CallScript();
                    Debug.Log(pin.Text.RawScript);
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
