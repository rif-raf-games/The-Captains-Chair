using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Articy.Unity;
using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.The_Captain_s_Chair;
using System.Collections.Generic;

public class MiniGameMCP : MonoBehaviour
{
    public enum eGameState { FADE_IN, PLAYING, FADE_OUT, NONE };
    public eGameState GameState;
    public Image FadeImage;
    public string PuzzleNameRoot;
    public int[] PuzzlesToLoad;
    public ArticyRef[] PuzzleDialogueRefs;
    public List<ArticyObject> PuzzleDialogues;
    Vector3[] CameraPositions;
    Quaternion[] CameraRotations;
    MiniGame[] Puzzles;
    string ProgressVarName;
    string FinishedVarName;
    int CurPuzzleIndex;

    TheCaptainsChair CaptainsChair;
    // UI
    ArticyFlow MiniGameArticyFlow;

    [Header("Sound")]
    public List<SoundFX.FXInfo> SoundFXUsedInScene;

    [Header("Misc Stuff")]
    public Text ResultsText;
    public Text DebugText;

    public Text OrientationText;
    MCP MCP;

    public virtual void Awake()
    {
        //Debug.Log("MiniGameMCP.Awake(): " + this.name);
        CaptainsChair = GameObject.FindObjectOfType<TheCaptainsChair>();
        PuzzleDialogues = null;
        //if (this.name.Contains("LockPick")) StaticStuff.SetOrientation(StaticStuff.eOrientation.PORTRAIT, this.name);        
       // else StaticStuff.SetOrientation(StaticStuff.eOrientation.LANDSCAPE, this.name);

        
    }
    // Start is called before the first frame update
    public virtual void Start()
    {
        SoundFX soundFX = FindObjectOfType<SoundFX>();
        SoundFXPlayer.Init(soundFX, -1);

        VisualFX visualFX = FindObjectOfType<VisualFX>();
        VisualFXPlayer.Init(visualFX);

        this.MCP = FindObjectOfType<MCP>();

        GameState = eGameState.NONE;
        FadeImage.gameObject.SetActive(true);
        MiniGameArticyFlow = GetComponent<ArticyFlow>();
        if (MiniGameArticyFlow == null) Debug.LogError("There's no ArticyFlow component on this mini game MCP: " + this.name);
        //else Debug.LogWarning("This is related to the UI moving...should be a quick fix");//MiniGameArticyFlow.ConvoUI.gameObject.SetActive(false);
        StartCoroutine(LoadPuzzleScenes());
    }

    IEnumerator LoadPuzzleScenes()
    {
       // Debug.Log("LoadPuzzleScenes()");
        float startTime = Time.time;        
        if(ArticyGlobalVariables.Default.Mini_Games.Coming_From_Main_Game == true)
        {            
            //Debug.Log("coming from a main game so get the puzzles from the articy data");
            ArticyGlobalVariables.Default.Mini_Games.Coming_From_Main_Game = false;
            Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");
            string[] puzzleNums = jumpSave.Template.Mini_Game_Puzzles_To_Play.Puzzle_Numbers.Split(',');
            //Debug.Log("we're gonna play " + puzzleNums.Length + " puzzles.");
            PuzzlesToLoad = new int[puzzleNums.Length];
            for(int i=0; i<puzzleNums.Length; i++)
            {
                PuzzlesToLoad[i] = int.Parse(puzzleNums[i]);
            }
            PuzzleDialogues = jumpSave.Template.Dialogue_List.DialoguesToPlay;            
        }
        Puzzles = new MiniGame[PuzzlesToLoad.Length];
        CameraPositions = new Vector3[PuzzlesToLoad.Length];
        CameraRotations = new Quaternion[PuzzlesToLoad.Length];
        for (int i = 0; i < PuzzlesToLoad.Length; i++)
        {
            //Debug.Log("LoadScene: " + PuzzlesToLoad[i]);
            string puzzleName;
            if(PuzzlesToLoad[i] == 0)
            {
                puzzleName = PuzzleNameRoot + "Tutorial";
            }
            else
            {
                puzzleName = PuzzleNameRoot + PuzzlesToLoad[i].ToString("D3");
            }            
            //Debug.Log("load puzzle: " + puzzleName);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(puzzleName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            //Debug.Log("Load Done: " + puzzleName);            
            UnityEngine.SceneManagement.Scene puzzleScene = SceneManager.GetSceneAt(2);
            MiniGame newPuzzle = null;
            Vector3 camPos = Vector3.zero;
            Quaternion camRot = Quaternion.identity;
            GameObject[] newPuzzleObjs = puzzleScene.GetRootGameObjects();
            foreach (GameObject go in newPuzzleObjs)
            {
                if(newPuzzle == null) newPuzzle = go.GetComponent<MiniGame>();
                if (go.GetComponent<Camera>() != null)
                {
                    camPos = go.transform.position;
                    camRot = go.transform.rotation;
                }
            }
            Puzzles[i] = newPuzzle;
            Puzzles[i].transform.parent = this.transform;
            CameraPositions[i] = camPos;
            CameraRotations[i] = camRot;
            asyncLoad = SceneManager.UnloadSceneAsync(puzzleScene);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            // Debug.Log("Unload Done: " + puzzleScene);
            Button resetButton = this.MCP.InGamePopUp.MissionHint.ResetMiniGameButton;
            Puzzles[i].Init(this, puzzleName, SoundFXUsedInScene, resetButton);                        
        }

        for (int i = 0; i < Puzzles.Length; i++)
        {
            Puzzles[i].gameObject.SetActive(false);
        }

        // 0 = Majestic Free Roam, 1 = Parking Game, 2 = Lockpick Game, 3 = Repair Game, 4 = Crossing Free Roam
        
        //string varName = "error";
        switch (ArticyGlobalVariables.Default.Activity.ID)
        {
            case 1: // parking
                ProgressVarName = "Activity.Progress_Parking";
                FinishedVarName = "Activity.Finished_Parking";
                break;
            case 2: // lockpick
                ProgressVarName = "Activity.Progress_Lockpick";
                FinishedVarName = "Activity.Finished_Lockpick";
                break;
            case 3: // repair
                ProgressVarName = "Activity.Progress_Repair";
                FinishedVarName = "Activity.Finished_Repair";
                break;
            default:
                Debug.LogError("ERROR, we don't have a valid Activity ID: " + ArticyGlobalVariables.Default.Activity.ID);
                break;
        }

        //Debug.LogError("DONT PAINIC I JUST WANT TO SEE WHAT'S GOING ON: " + ProgressVarName);
        string var = ArticyGlobalVariables.Default.GetVariableByString<string>(ProgressVarName);
        int progress = int.Parse(var);
        if (progress == 0)
        {
            progress = 1;
            ArticyGlobalVariables.Default.SetVariableByString(ProgressVarName, progress);
           // Debug.LogError("moprog01 ******************************** Setting the var (b): " + ProgressVarName + " to " + progress + " because the current progress for the variable is 0");
            StaticStuff.SaveCurrentProfile("We went from progress on variable: " + ProgressVarName + ", so save");
        }
        
        CurPuzzleIndex = progress - 1; 
        Puzzles[CurPuzzleIndex].gameObject.SetActive(true);
        Camera.main.transform.position = CameraPositions[CurPuzzleIndex];
        Camera.main.transform.rotation = CameraRotations[CurPuzzleIndex];

        
        float endTime = Time.time;
        float deltaTime = endTime - startTime;        
        if (deltaTime < 1f)
        {
            yield return new WaitForSeconds(1f - deltaTime);
        }        
        SetupLerpFade(1f, 0f, 1.5f);
        GameState = eGameState.FADE_IN;        
    }
    

    IEnumerator FadePause()
    {
        GameState = eGameState.NONE;
        yield return new WaitForSeconds(1f);
        EndCurrentPuzzle();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - 100, Screen.height / 2 - 100, 100, 100), "Win"))
        {
            Puzzles[CurPuzzleIndex].TMP_WinGame();
        }        
    }   

    void EndCurrentPuzzle()
    {
        if (CurPuzzleIndex == Puzzles.Length - 1)
        {
            // Debug.LogError("moprog03 ********************************  we're done with all puzzles and this code is only called if we finished them all but this is AFTER the code to save needs to be handled");            
            ArticyGlobalVariables.Default.Mini_Games.Coming_From_Main_Game = false;
            ArticyGlobalVariables.Default.Mini_Games.Returning_From_Mini_Game = true;
            ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success = true;
            if(PuzzleDialogues != null)
            {   // if PuzzleDialogues isn't null then we're under articy control
                Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");                
                MiniGameArticyFlow.HandleSavePoint(jumpSave.Template.Success_Save_Fragment.SaveFragment as Save_Point);
                string sceneName = jumpSave.Template.Success_Mini_Game_Result.SceneName;
                FindObjectOfType<MCP>().LoadNextScene(sceneName, null, jumpSave); // MGJ: MiniGameMCP.EndCurrentPuzzle() - done with all the puzzles for this set
            }
            else
            {
               // Debug.LogWarning("If we're here then you're expecting some kind of demo.");
                /*if (PuzzleNameRoot.Contains("Parking")) SceneManager.Load Scene("ParkingDemo");
                else if (PuzzleNameRoot.Contains("LockPicking")) SceneManager.Load Scene("LockPickingDemo");
                else if (PuzzleNameRoot.Contains("Repair")) SceneManager.Load Scene("RepairDemo");*/
            }                        
        }
        else
        {
            Puzzles[CurPuzzleIndex].gameObject.SetActive(false);
            CurPuzzleIndex++;
            Puzzles[CurPuzzleIndex].gameObject.SetActive(true);
            Camera.main.transform.position = CameraPositions[CurPuzzleIndex];
            Camera.main.transform.rotation = CameraRotations[CurPuzzleIndex];
            SetupLerpFade(1f, 0f, 1.5f);
            GameState = eGameState.FADE_IN;
        }
    }
    
    public void CurrentDiaogueEnded()
    {        
        Puzzles[CurPuzzleIndex].DialogueEnded();
    }
    public void SetDialogueActive(bool isActive)
    {        
        Puzzles[CurPuzzleIndex].SetDialogueActive(isActive);
    }
    void StartCurrentPuzzle()
    {
       // Debug.Log("StartCurrentPuzzle()");
        if(PuzzleDialogues != null)
        {   // if we're here, then we've gotten a list of dialogues from an articy ref, so use that
            if (PuzzleDialogues.Count == 0 || PuzzleDialogues.Count - 1 < CurPuzzleIndex)
            {
                // no dialogue but keep this in case we need this                
                Puzzles[CurPuzzleIndex].SetDialogueActive(false);
                Puzzles[CurPuzzleIndex].DialogueEnded();                
            }
            else
            {
                Dialogue d = PuzzleDialogues[CurPuzzleIndex] as Dialogue;
                if (d != null)
                {                    
                    bool dialogueActive = MiniGameArticyFlow.CheckIfDialogueShouldStart(d, null);
                    Puzzles[CurPuzzleIndex].SetDialogueActive(dialogueActive);
                }
                else Debug.LogError("The articy object for this Dialogue in the Mini_Game_Jump Dialogues To Play list isn't a Dialogue: " + PuzzleDialogues[CurPuzzleIndex].GetArticyType());
            }
        }
        else
        {   // if we're here, then we're doing either a straight MCP run or a demo run, so use the lsit set up in Unity
            if (PuzzleDialogueRefs == null || PuzzleDialogueRefs.Length == 0 || PuzzleDialogueRefs.Length - 1 < CurPuzzleIndex)
            {
                Debug.LogError("You don't have the Mini_Game_Jump set up properly because there's no entry in the Dialogues To Play list for this puzzle");
            }
            else
            {
                Debug.Log("trying to start mini game dialogue");
                Dialogue d = PuzzleDialogueRefs[CurPuzzleIndex].GetObject() as Dialogue;
                if (d != null)
                {
                    bool dialogueActive = MiniGameArticyFlow.CheckIfDialogueShouldStart(d, null);
                    Puzzles[CurPuzzleIndex].SetDialogueActive(dialogueActive);
                }
                else Debug.LogError("No dialogue specified for this mini game level: " + CurPuzzleIndex);
            }
        }
        

        Puzzles[CurPuzzleIndex].BeginPuzzleStartTime();
        GameState = eGameState.PLAYING;        
    }

    public void ShowResultsText(string result)
    {
        MCP mcp = FindObjectOfType<MCP>();
        if(mcp == null) { Debug.LogError("Trying to show results on the MCP UI that isn't here."); return; }
        mcp.ShowResultsText(result);
    }
    public void HideResultsText()
    {
        MCP mcp = FindObjectOfType<MCP>();
        if (mcp == null) { Debug.LogError("Trying to hide results on the MCP UI that isn't here."); return; }
        mcp.HideResultsText();
    }

    public void SavePuzzlesProgress(bool success, string cameFrom = "not set in call")
    {
        //Debug.LogError("SavePuzzleProgress() success: " + success + ", cameFrom: " + cameFrom);
        if(success == true)
        {
            string var = ArticyGlobalVariables.Default.GetVariableByString<string>(ProgressVarName);
            int progress = int.Parse(var);
            progress++;
            ArticyGlobalVariables.Default.SetVariableByString(ProgressVarName, progress);            
          //  Debug.LogError("moprog02 ******************************** Setting the var (a): " + ProgressVarName + " to " + progress);

           // string var = ArticyGlobalVariables.Default.GetVariableByString<string>(ProgressVarName);
          //  int progress = int.Parse(var);
            //progress++;
           // Debug.LogError("The current ProgressVarName: " + ProgressVarName + " has a value of: " + progress);
            if (progress > 2)
            {
             //   Debug.LogError("Progress is more than 2, so we're done with the mission so set var: " + FinishedVarName + " to true");
                ArticyGlobalVariables.Default.SetVariableByString(FinishedVarName, true);
            }
            if (CurPuzzleIndex == Puzzles.Length - 1)
            {
             //   Debug.LogError("We are done with all the puzzles so reset the value to zero done with all the puzzles set the progress of " + ProgressVarName + " back to zero");
                ArticyGlobalVariables.Default.SetVariableByString(ProgressVarName, 0);
            }
            
            StaticStuff.SaveCurrentProfile("MiniGameMCP.SavePuzzlesProgress()");
        }       
    }
    public void PuzzleFinished()
    {                
        MiniGameArticyFlow.EndMiniGameDialogues();
        GameState = eGameState.FADE_OUT;
        SetupLerpFade(0f, 1f, 1.5f);
    }

    private void FixedUpdate()
    {
        if (GameState == eGameState.FADE_IN || GameState == eGameState.FADE_OUT)
        {
            float lerpTime = Time.time - LerpStartTime;
            float lerpPercentage = lerpTime / LerpDurationTime;
            float alpha = Mathf.Lerp(LerpFadeStart, LerpFadeEnd, lerpPercentage);
            if (lerpPercentage >= 1f)
            {
                alpha = LerpFadeEnd;
                if (GameState == eGameState.FADE_IN)
                {
                    StartCurrentPuzzle();
                }
                else
                {
                    StartCoroutine(FadePause());
                }
            }
            FadeImage.color = new Color(0f, 0f, 0f, alpha);
        }
    }

    float LerpFadeStart, LerpFadeEnd;
    float LerpStartTime, LerpDurationTime;
    void SetupLerpFade(float start, float end, float time)
    {
        LerpFadeStart = start;
        LerpFadeEnd = end;
        LerpStartTime = Time.time;
        LerpDurationTime = time;
    }    

}
