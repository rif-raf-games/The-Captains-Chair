using Articy.The_Captain_s_Chair;
using Articy.Unity;
//using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheCaptainsChair : MonoBehaviour
{           
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Welcome to The Captain's Chair!!");
       // LoadTheData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   /* private void OnGUI()
    {
        if(GUI.Button(new Rect(0,0,100,100), "SaveData"))
        {
            SaveTheData();
        }
        if (GUI.Button(new Rect(0, 100, 100, 100), "LoadData"))
        {
            LoadTheData();
        }
        if (GUI.Button(new Rect(0, 200, 100, 100), "Delete"))
        {
            PlayerPrefs.DeleteKey("SaveData");
        }

    }

    [System.Serializable]
    public class SaveData
    {
        public List<string> keys;
        public List<bool> values;

        public SaveData()
        {
            keys = new List<string>();
            values = new List<bool>();
        }
    }

    public void SaveTheData()
    {
        string s = JsonUtility.ToJson(ArticyDatabase.DefaultGlobalVariables.Variables);
        Debug.Log(s);
        SaveData saveData = new SaveData();
        foreach (var pair in ArticyDatabase.DefaultGlobalVariables.Variables)
        {
            Debug.Log(pair.Key + ", " + pair.Value + ", Value type: " + pair.Value.GetType());
            saveData.keys.Add(pair.Key);
            saveData.values.Add((bool)pair.Value);            
        }        

        string saveInfo = JsonUtility.ToJson(saveData);
        Debug.Log("saveInfo: " + saveInfo);
        PlayerPrefs.SetString("SaveData", saveInfo);

        string loadInfo = PlayerPrefs.GetString("SaveData");
        SaveData loadData = JsonUtility.FromJson<SaveData>(loadInfo);        
    }
    void LoadTheData()
    {
        if (PlayerPrefs.HasKey("SaveData") == false)
        {
            Debug.LogWarning("You do not have any save data to load");
            return;
        }
        string loadInfo = PlayerPrefs.GetString("SaveData");
        SaveData loadData = JsonUtility.FromJson<SaveData>(loadInfo);

        int i = 0;
        foreach(string key in loadData.keys)
        {
            object o = (object)loadData.values[i];
            ArticyDatabase.DefaultGlobalVariables.SetVariableByString(key, o);
            i++;
        }
    }*/
//    saveInfo: { "keys":["Episode_01.s01_Visited_Sick_Bay","Episode_01.s01_Visited_Bridge","Episode_01.s01_Visited_Engine_Room","Episode_01.s01_Visited_Common","Episode_01.s01_Visited_Storage","Episode_01.s01_Visited_Lab","Captains_Chair.Peace_Keeper","Captains_Chair.Promethius","Captains_Chair.Sea_Exporation"]




        //s = JsonConvert.SerializeObject(ArticyDatabase.DefaultGlobalVariables.Variables);
        //Debug.Log(s);
        //JsonConvert.SerializeObject(ArticyDatabase.DefaultGlobalVariables.Variables;

        //default variables, just use ArticyDatabase.DefaultGlobalVariables.

        // How you save is up to you, for example if you are using JSON.NET, it could look something like this
        // save the variables
        // File.WriteAllText(@"d:\SaveFile.json", JsonConvert.SerializeObject(ArticyDatabase.DefaultGlobalVariables.Variables));
        // load the variables
        //ArticyDatabase.DefaultGlobalVariables.Variables = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(@"D:\SaveFile.json"));

        //If you don't have JSON.NET you could also write to a text file like this. This is how you would save
        /*using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\SaveFile.txt"))
        {
            foreach (var pair in ArticyDatabase.DefaultGlobalVariables.Variables)
            {
                file.WriteLine(string.Format("{0} = {1}", pair.Key, pair.Value));
            }
        }*/

        //and this how you read the data
        /*using (System.IO.StreamReader file = new System.IO.StreamReader(@"D:\SaveFile.txt"))
        {
            Dictionary<string, object> savedVars = new Dictionary<string, object>();
            string line;

            while ((line = file.ReadLine()) != null)
            {
                var split = line.Split('=');
                var name = split[0];
                var value = split[1];

                savedVars[name] = name;

                UnityEngine.Debug.LogFormat("Read saved var: {0} Value: {1}", name, value);
            }

            ArticyDatabase.DefaultGlobalVariables.Variables = savedVars;
        }*/


        //There are methods to set variables directly, but not on the database.You will find those on the global variables. 
        // For example if you want to change a variable on the default set of global variables, you could write something like this:
        // ArticyDatabase.DefaultGlobalVariables.SetVariableByString("GameState.Health", 100);
        // var health = ArticyDatabase.DefaultGlobalVariables.GetVariableByString<int>("GameState.Health");


        // But you can of course build your own save / load functionality, especially making use of the setProp() and getProp() methods on each object.
        // While this can be a bit cumbersome, especially if you can't make sure which objects/types/property you need to save, it should work.
        // Here is a very basic solution to get you started, this uses json, but using BinaryWriter/ Reader should be very similar.

        // As you see you have to write down which objects and which property you want to save / load.While not perfect, it might be a start for your solution.
        /*public class SaveFileHandler
        {
        public void Save()
        {
            using (StreamWriter file = File.CreateText(@"D:\save.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(file))
                {
                    writer.Formatting = Formatting.Indented;

                    var playerCharacter = ArticyDatabase.GetObject<Character>("PlayerCharacter");

                    writer.WriteStartObject();
                    WriteObjectProperty(writer, playerCharacter, "DisplayName");
                    // read all the other important properties

                    writer.WriteEndObject();
                }
            }
        }

        public void Load()
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(@"D:\save.json"));

            var playerCharacter = ArticyDatabase.GetObject<Character>("PlayerCharacter");
            playerCharacter.setProp("DisplayName", values["DisplayName"]);
            // read all the other important properties
        }


        public void WriteObjectProperty(JsonWriter aJsonWriter, IPropertyProvider aObject, string aName)
        {
            aJsonWriter.WritePropertyName(aName);
            aJsonWriter.WriteValue((string)aObject.getProp(aName));
        }
        
         public void OnApplicationPause(bool pauseStatus)
    {
        Save();
    }

    public void OnApplicationQuit()
    {
        Save();
    }



    /// <summary>
    ///  Sauvegarde - à refaire
    /// </summary>
    public void Save()
    {
        //Manque save des isNew des feature DisplayCondition

        if (isLoaded)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Application.persistentDataPath  + "\\SaveFileDatabase.txt"))
            {
                foreach (var pair in ArticyDatabase.DefaultGlobalVariables.Variables)
                {
                    UnityEngine.Debug.LogFormat("Save var: {0} Value: {1}.", pair.Key, pair.Value);
         
                    file.WriteLine(string.Format("{0}={1}", pair.Key, pair.Value));
                }



            }


            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Application.persistentDataPath + "\\SaveFileTemplate.txt"))
            {

                List<Message> listMessage = ArticyDatabase.GetAllOfType<Message>();
                file.WriteLine("Message");
                foreach (Message msg in listMessage)
                {

                    file.WriteLine(string.Format("{0}={1}={2}", msg.TechnicalName, "Read",msg.Template.Path.isPath));

                }

                List<CallEvent> listCall = ArticyDatabase.GetAllOfType<CallEvent>();
                file.WriteLine("CallEvent");
                foreach (CallEvent call in listCall)
                {

                    file.WriteLine(string.Format("{0}={1}={2}", call.TechnicalName, "Read", call.Template.Call.Listen));
                }
            }
        }
    }
         
         */
    }





