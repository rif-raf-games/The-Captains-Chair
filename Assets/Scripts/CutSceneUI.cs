using Articy.The_Captain_s_Chair;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutSceneUI : MonoBehaviour
{    
    public Text DescriptionText; 
    public Text NodeText;

    public CCPlayer Player;

    public void StartCutScene(Cut_Scene cutScene)
    {
        this.gameObject.SetActive(true);        
        Player.ToggleMovementBlocked(true);
        DescriptionText.text = cutScene.Text;        
    }
    public void SetCutsceneNode(DialogueFragment node)
    {        
        NodeText.text = node.Text;
    }
    public void EndCutScene()
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
