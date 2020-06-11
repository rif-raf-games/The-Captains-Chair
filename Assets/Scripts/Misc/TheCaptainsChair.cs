﻿using Articy.The_Captain_s_Chair;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using Articy.The_Captain_s_Chair.GlobalVariables;

public class TheCaptainsChair : MonoBehaviour
{
    // DEBUG - set the Activty.ID  to EXCHANGE
    // Start on scene that allows you to begin having jobs.  - Ep1.Exchange
    // have the bool unlock the jobs are available bit
    CCPlayer Player;
    MCP MCP;
    public ArticyRef DialogueToStartOn;

    [Header("Room And Floor Settings")]
    public float FadeTime = 1f;
    public float RoomFadeOpacity = .2f;
    public float FloorFadeOpacity = 0f;

    [Header("Sound")]
    public List<SoundFX.FXInfo> SoundFXUsedInScene;

    [Header("Debug")]
    public ArticyFlow ArticyFlowToPrint;
    Dictionary<string, NPC> ArticyRefNPCs = new Dictionary<string, NPC>();


    private void Awake()
    {
        StaticStuff.SetOrientation(StaticStuff.eOrientation.LANDSCAPE, this.name);
        this.MCP = FindObjectOfType<MCP>();
        if (this.MCP == null)
        {
            Debug.LogWarning("no MCP yet so load it up");
            StaticStuff.CreateMCPScene();            
        }        
    }
   
    void Start()
    {
        bool forceDialogueStart = false;
        if(this.MCP == null)
        {
            Debug.LogWarning("TheCaptainsChair.Start() getting MCP");
            this.MCP = FindObjectOfType<MCP>();
            this.MCP.TMP_ShutOffUI();
            forceDialogueStart = true;
        }        

        ArticyDatabase.DefaultGlobalVariables.Notifications.AddListener("*.*", MyGameStateVariablesChanged);
        //Debug.Log("Welcome to The Captain's Chair!!");
        StaticStuff.SetCaptainsChair(this.ArticyFlowToPrint);
        Player = FindObjectOfType<CCPlayer>();
        // get a list of all the NPC's so that we can search for them quickly via an articy reference
        List<NPC> npcs = GameObject.FindObjectsOfType<NPC>().ToList();
        string s = "TheCaptainsChair.Start() ArticyRefNPCs:\n";
        foreach (KeyValuePair<string, NPC> entry in ArticyRefNPCs)
        {
            s += entry.Key + ", " + entry.Value.name + "\n";
        }
       // Debug.Log(s);
        foreach (NPC npc in npcs)
        {
            if (npc.ArticyEntityReference == null || npc.ArticyEntityReference.GetObject() == null)
            {
                Debug.LogWarning("This NPC has a missing or broken ArticyRef so make sure all is well: " + npc.name);
                continue;
            }
           // Debug.Log("about to add npc: " + npc.name);
            ArticyRefNPCs.Add(npc.name, npc);
        }

        Dialogue dialogueToStartOn = null;
        if (ArticyGlobalVariables.Default.Mini_Games.Returning_From_Mini_Game == true)
        {
            ArticyObject flowStartAO;
            Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");
            if (ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success == true)
            {
                Debug.Log("We were a success so do the success thing");
                flowStartAO = jumpSave.Template.Success_Mini_Game_Result.Dialogue; //jumpSave.Template.Flow_Start_Success.ReferenceSlot;
                                                                                   // Debug.Log("flow start: " + flowStartAO.TechnicalName);
                                                                                   // Player.GetComponent<ArticyFlow>().CheckIfDialogueShouldStart(flowStartAO as Dialogue, Player.gameObject);
            }
            else
            {
                Debug.Log("We quit the mini game so deal with that");
                flowStartAO = jumpSave.Template.Quit_Mini_Game_Result.Dialogue; //jumpSave.Template.Flow_Start_Success.ReferenceSlot;
                                                                                // Debug.Log("flow start: " + flowStartAO.TechnicalName);
                                                                                //Player.GetComponent<ArticyFlow>().CheckIfDialogueShouldStart(flowStartAO as Dialogue, Player.gameObject);
            }
            Debug.Log("flow start: " + flowStartAO.TechnicalName);
            //Player.GetComponent<ArticyFlow>().CheckIfDialogueShouldStart(flowStartAO as Dialogue, Player.gameObject);
            dialogueToStartOn = flowStartAO as Dialogue;

            ArticyGlobalVariables.Default.Mini_Games.Returning_From_Mini_Game = false;
            ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success = false;
            ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Score = 0;
            StaticStuff.SaveSaveData("TheCaptainsChair.Start() after finishing the \"return from mini game == true\" bit");
        }
        else
        {
            /*if(DialogueToStartOn == null)
            {
                Debug.LogError("There's no dialogue defined to start on in this scene");
            }
            if(DialogueToStartOn.GetObject() == null)
            {
                Debug.LogError("There's no dialogue defined to start on in this scene");
            }
            if(DialogueToStartOn.GetObject() as Dialogue == null)
            {
                Debug.LogError("The starting Articy element is not a Dialogue");
            }*/
            //Player.GetComponent<ArticyFlow>().CheckIfDialogueShouldStart(DialogueToStartOn.GetObject() as Dialogue, Player.gameObject);
            dialogueToStartOn = DialogueToStartOn.GetObject() as Dialogue;
        }

        if (dialogueToStartOn == null) { Debug.LogError("We've got no Dialogue to start on in this scene"); return; }
        if(forceDialogueStart == true)
        {
            Debug.LogWarning("We're forcing the start of the dialogue since we had no MCP when this started");
            Player.GetComponent<ArticyFlow>().CheckIfDialogueShouldStart(dialogueToStartOn, Player.gameObject);
            //Joystick.gameObject.transform.parent.gameObject.SetActive(true);
            this.MCP.TMP_GetJoystick().gameObject.transform.parent.gameObject.SetActive(true);            
        }
        else
        {
            this.MCP.SetDialogueToStartSceneOn(dialogueToStartOn);
        }

        string playerLoc = ArticyGlobalVariables.Default.Save_Info.Last_Player_Position;
        if (!(playerLoc.Equals("null") || playerLoc.Equals("")))
        {
            string[] loc = playerLoc.Split(',');
            Vector3 pos = new Vector3(float.Parse(loc[0]), float.Parse(loc[1]), float.Parse(loc[2]));
            Player.transform.position = pos;
        }

       // SoundFX soundFX = FindObjectOfType<SoundFX>();
       // SoundFXPlayer.Init(soundFX, this.MCP.GetAudioVolume());
        VisualFX visualFX = FindObjectOfType<VisualFX>();
        VisualFXPlayer.Init(visualFX);
       // BackgroundMusic bgMusic = FindObjectOfType<BackgroundMusic>();
      //  BackgroundMusicPlayer.Init(bgMusic, this.MCP.GetAudioVolume());

        this.MCP.SetupSceneSound(SoundFXUsedInScene);
    }

    public void CheckStartDialogue(Dialogue startDialogue)
    {
        Player.GetComponent<ArticyFlow>().CheckIfDialogueShouldStart(startDialogue, Player.gameObject);
    }

    bool ShouldCheckAIs = false;
    private void LateUpdate()
    {
        if (ShouldCheckAIs == true)
        {
            ShouldCheckAIs = false;
            foreach (KeyValuePair<string, NPC> entry in ArticyRefNPCs)
            {
                if (entry.Value.name.Equals("Carver") || entry.Value.name.Equals("Grunfeld")) continue;
                Debug.Log("checking if npc: " + entry.Value.name + " has it's AI changed");
                bool changed = entry.Value.CheckForAIChange();
            }
        }
    }
    /// <summary>
    /// Called from Articy when a variable changes
    /// </summary>    
    void MyGameStateVariablesChanged(string aVariableName, object aValue)
    {
        //Debug.Log("aVariableName: " + aVariableName + " changed to: " + aValue.ToString());
        return;
        //ShouldCheckAIs = true;
        // CaptainsChair.SaveSaveData();
    }

    public NPC GetNPCFromActorName(string name)
    {
        //Debug.Log("get npc: " + name);
        if (ArticyRefNPCs.ContainsKey(name) == false)
        {
            Debug.LogError("Trying to get an NPC of name: " + name + " but it is not in the scene.");
            return null;
        }
        NPC npc = ArticyRefNPCs[name];
        return npc;
    }
}


    /*private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 200, 100, 100), "Show Data Path"))
        {
            StaticStuff.ShowDataPath();
        }
        if (GUI.Button(new Rect(Screen.width-100, 0, 100, 100), "Sim Startup\nData Load"))
        {
            StaticStuff.CheckSceneLoadSave();
        }
        if (GUI.Button(new Rect(Screen.width - 100, Screen.height / 2 - 100, 100, 100), "Menu On"))
        {
            FindObjectOfType<MCP>().ToggleMenuUI(true);
        }
    }*/
    //private void OnGUI()
    // {
    /*if (GUI.Button(new Rect(0, 0, 100, 100), "SaveData"))
    {
        SaveSaveData();
    }
    if (GUI.Button(new Rect(0, 100, 100, 100), "LoadData"))
    {
        LoadSaveData();
    }*/
    /*(if (GUI.Button(new Rect(0, 0, 100, 50), "Delete\nSave Data"))
    {
        DeleteSaveData();
    }*/

    // }

    /* NOT USING THE BELOW STUFF YET BUT I'M KEEPING IT FOR REFERENCE
    [System.Serializable]
    public class SaveDataObj
    {
        public List<string> keys;
        public List<object> values;

        public SaveDataObj()
        {
            keys = new List<string>();
            values = new List<object>();
        }
    }
    
    void SaveBinData()
    {
        Debug.Log("SaveBinData");
        SaveDataObj saveData = new SaveDataObj();
        foreach(KeyValuePair<string, object> pair in ArticyDatabase.DefaultGlobalVariables.Variables)
        {
            saveData.keys.Add(pair.Key);
            saveData.values.Add(pair.Value);
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/globalVars.dat");
        bf.Serialize(file, saveData);
        file.Close();
    }

    void LoadBinData()
    {
        if(File.Exists(Application.persistentDataPath + "/globalVars.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/globalVars.dat", FileMode.Open);
            SaveDataObj saveData = (SaveDataObj)bf.Deserialize(file);
            file.Close();
        }
    }    */
