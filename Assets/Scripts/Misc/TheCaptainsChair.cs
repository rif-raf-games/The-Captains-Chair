using Articy.The_Captain_s_Chair;
using Articy.Unity;
//using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using Articy.The_Captain_s_Chair.GlobalVariables;

public class TheCaptainsChair : MonoBehaviour
{
    CCPlayer Player;
    public ArticyRef DialogueToStartOn;
   
    [Header("Debug")]
    public ArticyFlow ArticyFlowToPrint;
    Dictionary<string, NPC> ArticyRefNPCs = new Dictionary<string, NPC>();
    // Start is called before the first frame update
    void Start()
    {

        ArticyDatabase.DefaultGlobalVariables.Notifications.AddListener("*.*", MyGameStateVariablesChanged);
        //Debug.Log("Welcome to The Captain's Chair!!");
        StaticStuff.SetCaptainsChair(this.ArticyFlowToPrint);
        // get a list of all the NPC's so that we can search for them quickly via an articy reference
        List<NPC> npcs = GameObject.FindObjectsOfType<NPC>().ToList();
        foreach(NPC npc in npcs)
        {
            if (npc.ArticyEntityReference == null || npc.ArticyEntityReference.GetObject() == null)
            {
                Debug.LogWarning("This NPC has a missing or broken ArticyRef so make sure all is well: " + npc.name);
                continue;                
            }
            ArticyRefNPCs.Add(npc.name, npc);
        }
        Player = FindObjectOfType<CCPlayer>();
        if(ArticyGlobalVariables.Default.Mini_Games.Returning_From_Mini_Game == true)
        {            
            if(ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success == true)
            {
                Debug.Log("set another start thing for the articy flow player");
                Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");
                ArticyObject flowStartAO = jumpSave.Template.Flow_Start_Success.ReferenceSlot;
                Debug.Log("flow start: " + flowStartAO.TechnicalName);
                Player.GetComponent<ArticyFlow>().CheckDialogue(flowStartAO as Dialogue, Player.gameObject);
            }
            else
            {
                Debug.LogWarning("We're coming back from a mini game failure and we haven't supported this yet");
            }
            ArticyGlobalVariables.Default.Mini_Games.Returning_From_Mini_Game = false;
            ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success = false;
            ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Score = 0;
        }
        else
        {
            if(DialogueToStartOn == null)
            {
                Debug.LogError("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ WTF 1");
            }
            if(DialogueToStartOn.GetObject() == null)
            {
                Debug.LogError("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ WTF 2");
            }
            if(DialogueToStartOn.GetObject() as Dialogue == null)
            {
                Debug.LogError("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ WTF 3");
            }
            Player.GetComponent<ArticyFlow>().CheckDialogue(DialogueToStartOn.GetObject() as Dialogue, Player.gameObject);
        }
        SoundFX soundFX = FindObjectOfType<SoundFX>();
        SoundFXPlayer.Init(soundFX);
        //DeleteSaveData();
       // LoadSaveData();
    }

    bool ShouldCheckAIs = false;
    private void LateUpdate()
    {
        if(ShouldCheckAIs == true)
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
        Debug.Log("aVariableName: " + aVariableName + " changed to: " + aValue.ToString());
        ShouldCheckAIs = true;
       // CaptainsChair.SaveSaveData();
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(Screen.width-100, Screen.height-100, 100, 100), "Delete Data"))
        {
            Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");            
        }
    }
    public void ToggleNavMeshes(bool val)
    {        
        foreach(KeyValuePair<string,NPC> entry in ArticyRefNPCs)
        {            
            entry.Value.ToggleNavMeshAgent(val);
        }
        
        //Player.ToggleNavMeshAgent(val);
    }
    public NPC GetNPCFromActorName(string name)
    {
//Debug.Log("get npc: " + name);
        NPC npc = ArticyRefNPCs[name];
        return npc;
    }
    /*public ArticyRef DebugAmbientTrigger;
    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 100), "fel"))
        {
            Ambient_Trigger at = DebugAmbientTrigger.GetObject() as Ambient_Trigger;
            Debug.Log("Want to start an ambient flow: " + at.name);
            List<ArticyObject> ambientEntities = at.Template.Ambient_Actors.Ambient_Actors_Strip;
            foreach (ArticyObject ao in ambientEntities)
            {
                NPC npc = GetNPCFromArticyObj(ao);
                if (npc == null) { Debug.LogError("There's no NPC associated with the provided ArticyObject. " + ao.name); return; }
                Debug.Log("found this NPC from the AmbientFlow list: " + npc.name);
                ArticyFlowPlayer afp = npc.GetComponent<ArticyFlowPlayer>();
                ArticyFlow af = npc.GetComponent<ArticyFlow>();
                CharacterActionList cal = npc.GetComponent<CharacterActionList>();
                af.StopForPuppetShow();
                afp.StartOn = FlowPauseTarget.GetObject();
                afp.Play();
                cal.SetStopped(true);
            }
        }
    }*/
    // public CharacterActionList debugCAL;
    /*void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 100), "stop"))
        {
            debugCAL.SetStopped(true);
        }
        if (GUI.Button(new Rect(0, 100, 100, 100), "resume"))
        {
            debugCAL.SetStopped(false);
        }
    }*/


    // Update is called once per frame
    void Update()
    {

    }

    #region SAVE_DATA
    [System.Serializable]
    public class SaveDataDic
    {
        public Dictionary<string, object> saveData;

        public SaveDataDic()
        {
            saveData = new Dictionary<string, object>();
        }
    }

    public void SaveSaveData()
    {
        SaveDataDic saveData = new SaveDataDic();
        saveData.saveData = ArticyDatabase.DefaultGlobalVariables.Variables;
        
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (File.Exists(Application.persistentDataPath + "/globalVars.dat"))
        {
            file = File.Open(Application.persistentDataPath + "/globalVars.dat", FileMode.Open);
        }
        else
        {
            file = File.Create(Application.persistentDataPath + "/globalVars.dat");
        }
        bf.Serialize(file, saveData);
        file.Close();
    }

    void LoadSaveData()
    {
        if (File.Exists(Application.persistentDataPath + "/globalVars.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/globalVars.dat", FileMode.Open);
            SaveDataDic saveData = (SaveDataDic)bf.Deserialize(file);
            ArticyDatabase.DefaultGlobalVariables.Variables = saveData.saveData;
            file.Close();
        }
    }

    public void DeleteSaveData()
    { 
        if (File.Exists(Application.persistentDataPath + "/globalVars.dat"))
        {
            File.Delete(Application.persistentDataPath + "/globalVars.dat");
        }
        ArticyDatabase.DefaultGlobalVariables.ResetVariables();
    }
    #endregion

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
}