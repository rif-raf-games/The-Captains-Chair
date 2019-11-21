using Articy.The_Captain_s_Chair;
using Articy.Unity;
//using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class TheCaptainsChair : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Welcome to The Captain's Chair!!");
        SoundFX soundFX = FindObjectOfType<SoundFX>();
        SoundFXPlayer.Init(soundFX);
        LoadSaveData();
    }

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

    private void OnGUI()
    {
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

    }

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