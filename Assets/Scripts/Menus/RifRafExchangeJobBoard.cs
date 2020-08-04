using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.Features;
using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.The_Captain_s_Chair.Templates;
using Articy.Unity;
using System.Collections;
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

    int JobIndex;
    MCP MCP;
    
    public void Init(MCP mcp)
    {
        this.MCP = mcp;
    }

    List<ArticyObject> ActiveJobs = new List<ArticyObject>();
    public void FillBoard()
    {
        ShutOffPopups();

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
            Button b = JobButtons[buttonIndex++];
            b.gameObject.SetActive(true);
            b.gameObject.transform.GetChild(2).GetComponent<Text>().text = emf.Job_Name;
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
        this.MCP.TMP_ShutOffExchangeBoard();

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

    public void ShutOffPopups()
    {
        QuitPopup.SetActive(false);
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
     //   Debug.Log("RifRafExchangeJobBoard.OnClickAcceptJob().  JobIndex: " + JobIndex);
        if (QuitPopup.activeSelf == true) return;

        AcceptPopup.SetActive(true);
    }
    public void OnClickCloseJobBoard()
    {
      //  Debug.Log("RifRafExchangeJobBoard.OnClickCloseJobBoard().");
        if (AcceptPopup.activeSelf == true) return;

        QuitPopup.SetActive(true);        
    }

    

    public void OnClickAcceptClose()
    {
        Debug.Log("RifRafExchangeJobBoard.OnClickAcceptClose().");
        this.MCP.TMP_ShutOffExchangeBoard();
    }

    public void OnClickClosePopup()
    {
        QuitPopup.SetActive(false);
        AcceptPopup.SetActive(false);
    }

    
}
