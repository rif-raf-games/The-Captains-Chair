using Articy.The_Captain_s_Chair;
using Articy.Unity;
//using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

public class TheCaptainsChair : MonoBehaviour
{
    public CCPlayer Player;
    public ArticyRef FlowPauseTarget;
    [Header("Debug")]
    public ArticyFlow ArticyFlowToPrint;
    Dictionary<string, NPC> ArticyRefNPCs = new Dictionary<string, NPC>();
    // Start is called before the first frame update
    void Start()
    {
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
        SoundFX soundFX = FindObjectOfType<SoundFX>();
        SoundFXPlayer.Init(soundFX);
        DeleteSaveData();
       // LoadSaveData();
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