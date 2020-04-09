using Articy.The_Captain_s_Chair;
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
    CCPlayer Player;
    public ArticyRef DialogueToStartOn;

    [Header("Room And Floor Settings")]
    public float FadeTime = 1f;
    public float RoomFadeOpacity = .2f;
    public float FloorFadeOpacity = 0f;

    [Header("Debug")]
    public ArticyFlow ArticyFlowToPrint;
    Dictionary<string, NPC> ArticyRefNPCs = new Dictionary<string, NPC>();


    private void Awake()
    {
        StaticStuff.SetOrientation(StaticStuff.eOrientation.LANDSCAPE, this.name);
    }
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
                Player.GetComponent<ArticyFlow>().CheckIfDialogueShouldStart(flowStartAO as Dialogue, Player.gameObject);
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
                Debug.LogError("There's no dialogue defined to start on in this scene");
            }
            if(DialogueToStartOn.GetObject() == null)
            {
                Debug.LogError("There's no dialogue defined to start on in this scene");
            }
            if(DialogueToStartOn.GetObject() as Dialogue == null)
            {
                Debug.LogError("The starting Articy element is not a Dialogue");
            }
            Player.GetComponent<ArticyFlow>().CheckIfDialogueShouldStart(DialogueToStartOn.GetObject() as Dialogue, Player.gameObject);
        }
        SoundFX soundFX = FindObjectOfType<SoundFX>();
        SoundFXPlayer.Init(soundFX);
        VisualFX visualFX = FindObjectOfType<VisualFX>();
        VisualFXPlayer.Init(visualFX);
        BackgroundMusic bgMusic = FindObjectOfType<BackgroundMusic>();
        BackgroundMusicPlayer.Init(bgMusic);          

        if(StaticStuff.USE_DEBUG_MENU == true)
        {
            Object debugObject = Resources.Load("DebugMenu");
            Instantiate(debugObject);
        }        
        
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