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
        // Debug.Log("RifRafExchangeJobBoard.OnClickAccpetJob().");

        //  Debug.LogWarning("OK we now have a job set up so lets rock it");
        InGamePopUp.ShutOffExchangeBoard();

        Exchange_Missions em;
        Exchange_MissionsTemplate emt;
        Exchange_MissionFeature emf;

        FlowFragment jobContainer = JobBoardContainer.GetObject() as FlowFragment;
        List<ArticyObject> jobs = jobContainer.Children;
        //ArticyObject job = jobs[JobIndex];
        ArticyObject job = ActiveJobs[JobIndex];
        em = job as Exchange_Missions;
        emt = em.Template;
        emf = em.Template.Exchange_Mission;

        string s = emf.Job_Name + "\n";
        s += emf.Job_Location + "\n"; ;
        s += emf.Point_Of_Contact + "\n"; ;
        s += emf.Job_Description + "\n"; ;
        s += emf.ToGoSceneFirstTime + "\n";
        s += emf.ToGoSceneAfterFirstTime + "\n";
        s += emf.ProgressVariable + "\n";
        string varName = "Activity.Progress_" + emf.ProgressVariable;
        s += varName + "\n";
        string var = ArticyGlobalVariables.Default.GetVariableByString<string>(varName);
        s += var + "\n";


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
       // Debug.LogError("**********************LOOK HERE FOR SCENE LOADING ISSUES!!!!!!!!!! " + s);
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

    public void OnClickAccept()
    {
      //  Debug.Log("RifRafExchangeJobBoard.OnClickAcceptJob().  JobIndex: " + JobIndex);
        if (QuitPopup.activeSelf == true) return;

        // AcceptPopup.SetActive(true);
        ToggleAcceptPopup(true);
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
