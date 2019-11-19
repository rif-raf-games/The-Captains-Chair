using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Articy.Unity;
using Articy.The_Captain_s_Chair;

public class ArticyFlow : MonoBehaviour, IArticyFlowPlayerCallbacks, IScriptMethodProvider
{
    public enum ArticyState { FREE_ROAM, CUT_SCENE, CONVERSATION, NUM_ARTICY_STATES };
    public ArticyState CurArticyState;

    // objet references
    TheCaptainsChair CaptainsChair;
    CCPlayer Player;
    ArticyFlowPlayer FlowPlayer;

    // flow stuff
    IFlowObject CurPauseObject = null;
    List<Branch> CurBranches = new List<Branch>();
    Branch NextBranch = null;

    // temp UI's for cutscenes and conversations
    public CutSceneUI CutSceneUI;
    public ConvoUI ConvoUI;

    public bool IsCalledInForecast { get; set; }
    
    void Start()
    {
        Player = GameObject.FindObjectOfType<CCPlayer>();
        CaptainsChair = GameObject.FindObjectOfType<TheCaptainsChair>();
        FlowPlayer = this.GetComponent<ArticyFlowPlayer>();       

        CurArticyState = ArticyState.NUM_ARTICY_STATES;

        ArticyDatabase.DefaultGlobalVariables.Notifications.AddListener("*.*", MyGameStateVariablesChanged);


    }

    void MyGameStateVariablesChanged(string aVariableName, object aValue)
    {
        Debug.Log("aVariableName: " + aVariableName + " changed to: " + aValue.ToString());
        CaptainsChair.SaveSaveData();
    }
 

    public void DeleteSaveData()
    {
        if (IsCalledInForecast == false)
        {
            CaptainsChair.DeleteSaveData();
        }        
    }
    public void OpenCaptainsDoor()
    {
        if (IsCalledInForecast == false)
        {
            Debug.Log("-------------------------------------------------------------- called OpenCaptainsDoor() but we're changing functionality");        
        }
        else
        {
            Debug.Log("-------------------------------------------------------------- OpenCaptainDoor(): Do NOT open door, we're just forecasting");
        }
            
    }

    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() START *************");
        if(aObject == null)
        {
            Debug.LogWarning("We have a null iFlowObject in OnFlowPlayerPaused(), so we're at a dangling end point somewhere that needs to get sorted out.");
            return;
        }
        StaticStuff.PrintFlowPaused("OnFlowPlayerPaused() IFlowObject Type: " + aObject.GetType() + ", with TechnicalName: " + ((ArticyObject)aObject).TechnicalName);

        CurPauseObject = aObject;

        StaticStuff.PrintFlowPaused("************** OnFlowPlayerPaused() END ***************");
    }
    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
        StaticStuff.PrintFlowBranchesUpdate("************** OnBranchesUpdated() START *************");
        StaticStuff.PrintFlowBranchesUpdate("Num branches: " + aBranches.Count);

        CurBranches.Clear();
        int i = 0;
        foreach (Branch b in aBranches)
        {
            StaticStuff.PrintFlowBranchesUpdate("branch: " + i + " is type: " + b.Target.GetType());
            if (b.IsValid == false) Debug.LogWarning("Invalid branch in OnBranchesUpdate(): " + b.DefaultDescription);
            CurBranches.Add(b);
        }
        DialogueFragment df = CurPauseObject as DialogueFragment;        
        if(df != null )
        {
            StaticStuff.PrintFlowBranchesUpdate("We're on a dialogue fragment, so set the text based on current flow state.");
            switch (CurArticyState)
            {
                case ArticyState.CUT_SCENE:
                    CutSceneUI.SetCutsceneNode(CurPauseObject as DialogueFragment);
                    break;
                case ArticyState.CONVERSATION:
                    Player.StopNavMeshMovement();
                    ConvoUI.ShowDialogueFragment(CurPauseObject as DialogueFragment, CurPauseObject, aBranches);
                    break;
                default:
                    Debug.LogError("In invalid state for DialogueFragment: " + CurArticyState);
                    break;
            }
        }
        else if (aBranches.Count == 1)
        {   // We're paused and there's only one valid branch available. This is common so have it's own section
            if(CurBranches[0].Target.GetType().Equals(typeof(Scene)))
            {
                StaticStuff.PrintFlowBranchesUpdate("The target is a Scene, so Play() it.");
                NextBranch = CurBranches[0];
            }
            else if (CurPauseObject.GetType().Equals(typeof(Cut_Scene)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're starting a cut scene.");
                StartCutScene(CurPauseObject as Cut_Scene);
            }
            else if(CurPauseObject.GetType().Equals(typeof(Dialogue)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're about to start a Dialogue.");
                NextBranch = CurBranches[0];
            }            
            else if(CurBranches[0].Target.GetType().Equals(typeof(Hub)))
            {
                StaticStuff.PrintFlowBranchesUpdate("The target is a Hub, so Play() it.");
                NextBranch = CurBranches[0];
            }
            else if (CurBranches[0].Target.GetType().Equals(typeof(Cut_Scene)))
            {
                StaticStuff.PrintFlowBranchesUpdate("The target is a Cut_Scene, so Play() it.");
                NextBranch = CurBranches[0];
            }
            else if(CurPauseObject.GetType().Equals(typeof(Hub)) && CurBranches[0].Target.GetType().Equals(typeof(OutputPin)))
            {
                StaticStuff.PrintFlowBranchesUpdate("We're paused on a Hub with no Target so we're in Free Roam.  Don't Play() anything.");
                CurArticyState = ArticyState.FREE_ROAM;                
            }
            
            else
            {
                Debug.LogWarning("We haven't supported this single branch situation yet. CurPauseObject: " + CurPauseObject.GetType() + ", branch: " + CurBranches[0].Target.GetType());
            }
        }
        else
        {
            Debug.LogWarning("We haven't supported this case yet.");
        }

        StaticStuff.PrintFlowBranchesUpdate("************** OnBranchesUpdated() END ***************");
    }

    public void StartCutScene(Cut_Scene cutScene)
    {
        CurArticyState = ArticyState.CUT_SCENE;        
        CutSceneUI.StartCutScene(cutScene);
        NextBranch = CurBranches[0];
    }

    public void StartConvo(Dialogue convoStart)
    {        
        CurArticyState = ArticyState.CONVERSATION;
        FlowPlayer.StartOn = convoStart;
    }

    public void UIButtonCallback(int buttonIndex)
    {
        StaticStuff.PrintUI("UIButtonCallback() buttonIndex: " + buttonIndex);
        NextBranch = CurBranches[buttonIndex];
        if (CurBranches[buttonIndex].Target.GetType().Equals(typeof(DialogueFragment)) == false)
        {
            StaticStuff.PrintUI("Chosen branch isn't a dialogue fragment, so for now assume we're done talking and shut off the UI");
            CutSceneUI.EndCutScene();
            ConvoUI.EndConversation();
        }
        else
        {
            StaticStuff.PrintUI("Chosen branch is a dialogue fragment, so just let the engine handle the next phase.");
        }
    }
    // Start is called before the first frame update
    

    // Update is called once per frame
    void Update()
    {
        if (NextBranch != null)
        {
            Branch b = NextBranch;
            NextBranch = null;
            FlowPlayer.Play(b);
        }
    }
}

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
//s = JsonConvert.SerializeObject(ArticyDatabase.DefaultGlobalVariables.Variables);
//Debug.Log(s);
//JsonConvert.SerializeObject(ArticyDatabase.DefaultGlobalVariables.Variables;

//default variables, just use ArticyDatabase.DefaultGlobalVariables.

// How you save is up to you, for example if you are using JSON.NET, it could look something like this
// save the variables
// File.WriteAllText(@"d:\SaveFile.json", JsonConvert.SerializeObject(ArticyDatabase.DefaultGlobalVariables.Variables));
// load the variables
//ArticyDatabase.DefaultGlobalVariables.Variables = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(@"D:\SaveFile.json"));




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
