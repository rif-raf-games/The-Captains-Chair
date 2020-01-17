using Articy.Unity;
using Articy.The_Captain_s_Chair.Features;
using Articy.The_Captain_s_Chair;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientEntity : NPC
{
    Character_Action_List_FeatureFeature MyActionList;
    CharacterActionList MyActionListPlayer;
    TextMesh BarkText;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        ArticyReference ar = GetComponent<ArticyReference>();
        if(ar == null) { Debug.LogError("There's no ArticyReference component on this AmbientEntity: " + this.name); return; }
        if(ar.reference == null) { Debug.LogError("There's no action list assigned to the ArticyReference on this AmbientEntity: " + this.name); return; }
        ArticyObject ao = ar.reference.GetObject();
        Character_Action_List_Template cat = ar.reference.GetObject() as Character_Action_List_Template;
        if(cat == null) { Debug.LogError("The ArticyReference object is not a Parent Character Action List Template type: " + this.name); return; }
        MyActionList = cat.Template.Character_Action_List_Feature;
        if(MyActionList == null ) { Debug.LogError("Something wacky is going on with this AmbientEntity's action lsit: " + this.name); return; }
        MyActionListPlayer = GetComponent<CharacterActionList>();
        if(MyActionListPlayer == null) { Debug.LogError("No CharacterActionList component on this AmbientEntity: " + this.name); return; }
        MyActionListPlayer.BeginCAL(MyActionList, this.gameObject);

        BarkText = GetComponentInChildren<TextMesh>();
        if(BarkText == null) { Debug.LogError("There's no TextMesh on this AmbientEntity: " + this.gameObject.name); return;  }
        ToggleBarkText(false);
    }

    public void SetBarkText(string text)
    {
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
    public void CALDone()
    {
        if (this.gameObject == null) Debug.LogError("why is my GameObject null");
        MyActionListPlayer.BeginCAL(MyActionList, this.gameObject);
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
