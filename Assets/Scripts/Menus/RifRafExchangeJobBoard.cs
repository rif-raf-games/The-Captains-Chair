using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.Features;
using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.The_Captain_s_Chair.Templates;
using Articy.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RifRafExchangeJobBoard : MonoBehaviour
{    
    [Header("Listings")]
    public Button[] JobButtons;

    [Header("Parameters")]
    public Text JobName;
    public Text JobLocation;
    public Text PointOfContact;
    public Text Description;

    [Header("Popups")]
    public GameObject QuitPopup;
    public GameObject AcceptPopup;

    [Header("Articy")]
    public ArticyRef JobBoardContainer;

    [Header("Misc")]
    public RifRafInGamePopUp InGamePopUp;

    [Header("Buttons")]
    public Button[] MainButtons;

    int JobIndex;
    MCP MCP;
    
    private void Awake()
    {

    }
    public void SetMCP(MCP mcp)
    {
        this.MCP = mcp;
    }

    public void ToggleMainButtons(bool isActive)
    {
       // Debug.Log("++++++++++ToggleMainButtons(): " + isActive);
        foreach (Button b in MainButtons) b.interactable = isActive;
    }

    List<ArticyObject> ActiveJobs = new List<ArticyObject>();
    public void FillBoard()
    {
        ShutOffQuitAcceptPopups();

        FlowFragment jobContainer = JobBoardContainer.GetObject() as FlowFragment;

        List<ArticyObject> jobs = jobContainer.Children;

        Exchange_Missions em;
        Exchange_MissionsTemplate emt;
        Exchange_MissionFeature emf;
        int buttonIndex = 0;
        ActiveJobs.Clear();
        foreach (Button b in JobButtons) b.gameObject.SetActive(false);
        foreach (ArticyObject job in jobs)
        {           
            em = job as Exchange_Missions;           
            string varName = "Activity.Finished_" + em.Template.Exchange_Mission.ProgressVariable;            
            string var = ArticyGlobalVariables.Default.GetVariableByString<string>(varName);
            //Debug.Log("varName: " + varName + " has a value of: " + var);
            if (var.Equals("True"))
            {          
                continue;
            }
            ActiveJobs.Add(job);
            string s = em.DisplayName + "\n";
            emt = em.Template;
            emf = em.Template.Exchange_Mission;
           // Debug.Log("button index: " + buttonIndex);
            Button b = JobButtons[buttonIndex++];
            b.gameObject.SetActive(true);
            Transform trans = b.gameObject.transform.GetChild(2);
            Text text = trans.GetComponent<Text>();
            text.text = emf.Job_Name;
           // b.gameObject.transform.GetChild(1).GetComponent<Text>().text = emf.Job_Name;
        }

        JobIndex = 0;
        SetupDescription();        
    }

    void SetupDescription()
    {
        Exchange_Missions em;
        //Exchange_MissionsTemplate emt;
        Exchange_MissionFeature emf;
        //FlowFragment jobContainer = JobBoardContainer.GetObject() as FlowFragment;
        //List<ArticyObject> jobs = jobContainer.Children;
        //ArticyObject job = jobs[JobIndex];
        ArticyObject job = ActiveJobs[JobIndex];
        em = job as Exchange_Missions;
       //emt = em.Template;
        emf = em.Template.Exchange_Mission;
        JobName.text = emf.Job_Name;
        JobLocation.text = emf.Job_Location;
        PointOfContact.text = emf.Point_Of_Contact;
        Description.text = emf.Job_Description;
    }
    
    public void OnClickAcceptJob()
    {
         Debug.Log("RifRafExchangeJobBoard.OnClickAccpetJob().");

        //  Debug.LogWarning("OK we now have a job set up so lets rock it");
        InGamePopUp.ShutOffExchangeBoard();

        /* Exchange_Missions em;
         Exchange_MissionsTemplate emt;
         Exchange_MissionFeature emf;

         FlowFragment jobContainer = JobBoardContainer.GetObject() as FlowFragment;
         List<ArticyObject> jobs = jobContainer.Children;
         //ArticyObject job = jobs[JobIndex];
         ArticyObject job = ActiveJobs[JobIndex];
         em = job as Exchange_Missions;
         emt = em.Template;
         emf = em.Template.Exchange_Mission;*/

        MenuButton menuButton = InGamePopUp.GetCurJobButton();
        if(menuButton == null) { Debug.LogError("Null menu button"); return; }
        if(menuButton.LoadingScreen.SceneToLoad == "")
        //if(emf != null)
        {   // Exchange mission
            Exchange_MissionFeature emf = menuButton.ExchangeMission;
            string s = emf.Job_Name + "\n";
            s += emf.Job_Location + "\n"; ;
            s += emf.Point_Of_Contact + "\n"; ;
            s += emf.Job_Description + "\n"; ;
            s += emf.ToGoSceneFirstTime + "\n";
            s += emf.ToGoSceneAfterFirstTime + "\n";
            s += emf.ProgressVariable + "\n";
            string varName = /*"Activity.Progress_" +*/ emf.ProgressVariable;
            //ArticyGlobalVariables.Default.Mission.cur
            s += varName + "\n";
            string var = ArticyGlobalVariables.Default.GetVariableByString<string>(varName);
            ArticyGlobalVariables.Default.Mission.Current_Progress_Variable = varName;
            s += var + "\n";

            Debug.Log("var: " + var);
            int progress = int.Parse(var);
            s += "int prog: " + progress + "\n";

            if (progress == 0)
            {
                this.MCP.LoadNextScene(emf.ToGoSceneFirstTime); // SJ: OnClickAcceptJob() progress == 0
            }
            else
            {
                this.MCP.LoadNextScene(emf.ToGoSceneAfterFirstTime); // SJ: OnClickAcceptJob() progress != 0
            }
        }
        else
        {   // Task
            // we're going to a mini game, so fill up the mini game info container with the current pause object's information, then start the mini game                               
            Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");
            //Mini_Game_Jump curJump = CurPauseObject as Mini_Game_Jump;
           // Debug.Log("Set Up Mini game: " + curJump.TechnicalName + ", is SaveFragment null: " + (curJump.Template.Success_Save_Fragment.SaveFragment == null));
            //jumpSave.Template.Mini_Game_Scene.Scene_Name = curJump.Template.Mini_Game_Scene.Scene_Name;
            jumpSave.Template.LoadingScreen.SceneToLoad = menuButton.LoadingScreen.SceneToLoad;
            jumpSave.Template.LoadingScreen.LoadingImages = menuButton.LoadingScreen.LoadingImages;
            jumpSave.Template.LoadingScreen.DisplayTime = menuButton.LoadingScreen.DisplayTime;
            jumpSave.Template.LoadingScreen.FadeTime = menuButton.LoadingScreen.FadeTime;

            jumpSave.Template.Mini_Game_Puzzles_To_Play.Puzzle_Numbers = menuButton.PuzzlesToPlay.Puzzle_Numbers;

            jumpSave.Template.Dialogue_List.DialoguesToPlay = menuButton.DialogueList.DialoguesToPlay;

            jumpSave.Template.Success_Mini_Game_Result.SceneName = menuButton.SuccessResult.SceneName;
            jumpSave.Template.Success_Mini_Game_Result.LoadingImages = menuButton.SuccessResult.LoadingImages;
            jumpSave.Template.Success_Mini_Game_Result.DisplayTime = menuButton.SuccessResult.DisplayTime;
            jumpSave.Template.Success_Mini_Game_Result.FadeTime = menuButton.SuccessResult.FadeTime;
            jumpSave.Template.Success_Mini_Game_Result.Dialogue = menuButton.SuccessResult.Dialogue;

            jumpSave.Template.Quit_Mini_Game_Result.SceneName = menuButton.QuitResult.SceneName;
            jumpSave.Template.Quit_Mini_Game_Result.LoadingImages = menuButton.QuitResult.LoadingImages;
            jumpSave.Template.Quit_Mini_Game_Result.DisplayTime = menuButton.QuitResult.DisplayTime;
            jumpSave.Template.Quit_Mini_Game_Result.FadeTime = menuButton.QuitResult.FadeTime;
            jumpSave.Template.Quit_Mini_Game_Result.Dialogue = menuButton.QuitResult.Dialogue;

            jumpSave.Template.Success_Save_Fragment.SaveFragment = menuButton.SuccessSaveFragment.SaveFragment;

            ArticyGlobalVariables.Default.Mini_Games.Coming_From_Main_Game = true;
            ArticyGlobalVariables.Default.Mini_Games.Returning_From_Mini_Game = false;
            ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success = false;
            ArticyGlobalVariables.Default.Mission.Current_Progress_Variable = menuButton.ExchangeMission.ProgressVariable; ;

            FindObjectOfType<MCP>().LoadNextScene(menuButton.LoadingScreen.SceneToLoad, null, null, "", "", menuButton); // MGJ: ArticyFlow.OnBranchesUpdate() CurPauseObject = Mini_Game_Jump
        }

    }
    public void OnClickAccept()
    {
          Debug.Log("RifRafExchangeJobBoard.OnClickAcceptJob().  JobIndex: " + JobIndex);
        if (QuitPopup.activeSelf == true) return;
        
        // AcceptPopup.SetActive(true);
        ToggleAcceptPopup(true);
    }

    public void ShutOffQuitAcceptPopups()
    {
      //  Debug.Log("ShutOffQuitAcceptPopups()");
        //QuitPopup.SetActive(false);
        ToggleQuitPopUp(false);
        AcceptPopup.SetActive(false);
    }

    public void OnClickJob(int index)
    {
      //  Debug.Log("RifRafExchangeJobBoard.OnClickJob() index: " + index);

        if (QuitPopup.activeSelf == true || AcceptPopup.activeSelf == true) return;
        JobIndex = index;
        SetupDescription();
    }    

    void ToggleAcceptPopup(bool isActive)
    {
       // Debug.Log("ToggleAcceptPopup(): " + isActive);
        AcceptPopup.SetActive(isActive);
        ToggleMainButtons(!isActive);
    }

    public void OnClickCloseJobBoard()
    {
      //  Debug.Log("RifRafExchangeJobBoard.OnClickCloseJobBoard().");
        if (AcceptPopup.activeSelf == true) return;

        //QuitPopup.SetActive(true);        
        ToggleQuitPopUp(true);
    }

    void ToggleQuitPopUp(bool isActive)
    {
        QuitPopup.SetActive(isActive);
        ToggleMainButtons(!isActive);
    }

    public void OnClickAcceptClose()
    {
        //Debug.Log("RifRafExchangeJobBoard.OnClickAcceptClose().");
        InGamePopUp.ShutOffExchangeBoard();
    }

    public void OnClickClosePopup()
    {
      //  Debug.Log("OnClickClosePopup()");
        // QuitPopup.SetActive(false);
        ToggleQuitPopUp(false);
        AcceptPopup.SetActive(false);
    }

    
}
