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
    public Vector3[] CameraPositions;
    MiniGame[] Puzzles;
    int CurPuzzle;

    TheCaptainsChair CaptainsChair;
    // UI
    ArticyFlow MiniGameArticyFlow;

    [Header("Misc Stuff")]
    public Text ResultsText;
    public Text DebugText;

    public Text OrientationText;

    public virtual void Awake()
    {
        //Debug.Log("MiniGameMCP.Awake(): " + this.name);
        CaptainsChair = GameObject.FindObjectOfType<TheCaptainsChair>();
        PuzzleDialogues = null;
        if (this.name.Contains("LockPick")) StaticStuff.SetOrientation(StaticStuff.eOrientation.PORTRAIT, this.name);        
        else StaticStuff.SetOrientation(StaticStuff.eOrientation.LANDSCAPE, this.name);                     
    }
    // Start is called before the first frame update
    public virtual void Start()
    {
        SoundFX soundFX = FindObjectOfType<SoundFX>();
        SoundFXPlayer.Init(soundFX, -1);
        
        GameState = eGameState.NONE;
        FadeImage.gameObject.SetActive(true);
        MiniGameArticyFlow = GetComponent<ArticyFlow>();
        if (MiniGameArticyFlow == null) Debug.LogError("There's no ArticyFlow component on this mini game MCP: " + this.name);
        else Debug.LogWarning("This is related to the UI moving...should be a quick fix");//MiniGameArticyFlow.ConvoUI.gameObject.SetActive(false);
        StartCoroutine(LoadPuzzleScenes());
    }

    IEnumerator LoadPuzzleScenes()
    {
        float startTime = Time.time;        
        if(ArticyGlobalVariables.Default.Mini_Games.Coming_From_Main_Game == true)
        {            
            Debug.Log("coming from a main game so get the puzzles from the articy data");
            ArticyGlobalVariables.Default.Mini_Games.Coming_From_Main_Game = false;
            Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");
            string[] puzzleNums = jumpSave.Template.Mini_Game_Puzzles_To_Play.Puzzle_Numbers.Split(',');
            Debug.Log("we're gonna play " + puzzleNums.Length + " puzzles.");
            PuzzlesToLoad = new int[puzzleNums.Length];
            for(int i=0; i<puzzleNums.Length; i++)
            {
                PuzzlesToLoad[i] = int.Parse(puzzleNums[i]);
            }
            PuzzleDialogues = jumpSave.Template.Dialogue_List.DialoguesToPlay;            
        }
        Puzzles = new MiniGame[PuzzlesToLoad.Length];
        CameraPositions = new Vector3[PuzzlesToLoad.Length];
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
            Debug.Log("load puzzle: " + puzzleName);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(puzzleName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            //Debug.Log("Load Done: " + puzzleName);            
            UnityEngine.SceneManagement.Scene puzzleScene = SceneManager.GetSceneAt(1);
            MiniGame newPuzzle = null;
            Vector3 camPos = Vector3.zero;
            GameObject[] newPuzzleObjs = puzzleScene.GetRootGameObjects();
            foreach (GameObject go in newPuzzleObjs)
            {
                if(newPuzzle == null) newPuzzle = go.GetComponent<MiniGame>();
                if (go.GetComponent<Camera>() != null) camPos = go.transform.position;
            }
            Puzzles[i] = newPuzzle;
            Puzzles[i].transform.parent = this.transform;
            CameraPositions[i] = camPos;
            asyncLoad = SceneManager.UnloadSceneAsync(puzzleScene);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
           // Debug.Log("Unload Done: " + puzzleScene);
            Puzzles[i].Init(this, puzzleName);                        
        }

        for (int i = 0; i < Puzzles.Length; i++)
        {
            Puzzles[i].gameObject.SetActive(false);
        }

        CurPuzzle = GetStartingCurPuzzle();
        Puzzles[CurPuzzle].gameObject.SetActive(true);
        Camera.main.transform.position = CameraPositions[CurPuzzle];
        Debug.Log("MiniGameMCP.LoadPuzzleScenes() CurPuzzle: " + CurPuzzle);
        
        float endTime = Time.time;
        float deltaTime = endTime - startTime;
        //Debug.Log("load time: " + deltaTime);
        if (deltaTime < 1f)
        {
            yield return new WaitForSeconds(1f - deltaTime);
        }
        //Debug.Log("start fade in");
        SetupLerpFade(1f, 0f, 1.5f);
        GameState = eGameState.FADE_IN;        
    }

    int GetStartingCurPuzzle()
    {
        int curPuzzle = 0;
        if (PuzzleNameRoot.Contains("Parking")) curPuzzle = ArticyGlobalVariables.Default.Mini_Games.Parking_Demo_Progress;
        else if (PuzzleNameRoot.Contains("LockPicking")) curPuzzle = ArticyGlobalVariables.Default.Mini_Games.LockPicking_Demo_Progress;
        else if (PuzzleNameRoot.Contains("Repair")) curPuzzle = ArticyGlobalVariables.Default.Mini_Games.Repair_Demo_Progress;

        if (curPuzzle >= Puzzles.Length)
        {
            curPuzzle = 0;
            if (PuzzleNameRoot.Contains("Parking")) ArticyGlobalVariables.Default.Mini_Games.Parking_Demo_Progress = 0;
            else if (PuzzleNameRoot.Contains("LockPicking")) ArticyGlobalVariables.Default.Mini_Games.LockPicking_Demo_Progress = 0;
            else if (PuzzleNameRoot.Contains("Repair")) ArticyGlobalVariables.Default.Mini_Games.Repair_Demo_Progress = 0;
            StaticStuff.SaveSaveData("MiniGameMCP.GetStartingCurPuzzle() if we're done with all the puzzles and resetting all puzzle progress");
        }

        return curPuzzle;
    }

    IEnumerator FadePause()
    {
        GameState = eGameState.NONE;
        yield return new WaitForSeconds(1f);
        EndCurrentPuzzle();
    }
    void EndCurrentPuzzle()
    {
        if (CurPuzzle == Puzzles.Length - 1)
        {
            Debug.Log("we're done with all puzzles");
            ArticyGlobalVariables.Default.Mini_Games.Returning_From_Mini_Game = true;
            ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success = true;
            if(PuzzleDialogues != null)
            {   // if PuzzleDialogues isn't null then we're under articy control
                Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");                
                MiniGameArticyFlow.HandleSavePoint(jumpSave.Template.Success_Save_Fragment.SaveFragment as Save_Point);
                //SceneManager.Load Scene(jumpSave.Template.Success_Mini_Game_Result.SceneName);
                FindObjectOfType<MCP>().LoadNextScene(jumpSave.Template.Success_Mini_Game_Result.SceneName);
            }
            else
            {
                Debug.LogWarning("If we're here then you're expecting some kind of demo.");
                /*if (PuzzleNameRoot.Contains("Parking")) SceneManager.Load Scene("ParkingDemo");
                else if (PuzzleNameRoot.Contains("LockPicking")) SceneManager.Load Scene("LockPickingDemo");
                else if (PuzzleNameRoot.Contains("Repair")) SceneManager.Load Scene("RepairDemo");*/
            }                        
        }
        else
        {
            Puzzles[CurPuzzle].gameObject.SetActive(false);
            CurPuzzle++;
            Puzzles[CurPuzzle].gameObject.SetActive(true);
            Camera.main.transform.position = CameraPositions[CurPuzzle];
            SetupLerpFade(1f, 0f, 1.5f);
            GameState = eGameState.FADE_IN;
        }
    }

    /*public void QuitCurrentPuzzle()
    {
        ArticyGlobalVariables.Default.Mini_Games.Returning_From_Mini_Game = true;
        ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success = false;
        if (PuzzleDialogues != null)
        {   // if PuzzleDialogues isn't null then we're under articy control
            Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");
            SceneManager.Load Scene(jumpSave.Template.Quit_Mini_Game_Result.SceneName);
        }
        else
        {
            if (PuzzleNameRoot.Contains("Parking")) SceneManager.Load Scene("ParkingDemo");
            else if (PuzzleNameRoot.Contains("LockPicking")) SceneManager.Load Scene("LockPickingDemo");
            else if (PuzzleNameRoot.Contains("Repair")) SceneManager.Load Scene("RepairDemo");
        }
    }*/

    public void CurrentDiaogueEnded()
    {
        Puzzles[CurPuzzle].DialogueEnded();
    }
    void StartCurrentPuzzle()
    {
        if(PuzzleDialogues != null)
        {   // if we're here, then we've gotten a list of dialogues from an articy ref, so use that
            if (PuzzleDialogues.Count == 0 || PuzzleDialogues.Count - 1 < CurPuzzle)
            {
                Debug.LogError("You don't have the Mini_Game_Jump set up properly because there's no entry in the Dialogues To Play list for this puzzle");                
            }
            else
            {
                Dialogue d = PuzzleDialogues[CurPuzzle] as Dialogue;
                if (d != null)
                {                    
                    bool dialogueActive = MiniGameArticyFlow.CheckIfDialogueShouldStart(d, null);
                    Puzzles[CurPuzzle].SetDialogueActive(dialogueActive);
                }
                else Debug.LogError("The articy object for this Dialogue in the Mini_Game_Jump Dialogues To Play list isn't a Dialogue: " + PuzzleDialogues[CurPuzzle].GetArticyType());
            }
        }
        else
        {   // if we're here, then we're doing either a straight MCP run or a demo run, so use the lsit set up in Unity
            if (PuzzleDialogueRefs == null || PuzzleDialogueRefs.Length == 0 || PuzzleDialogueRefs.Length - 1 < CurPuzzle)
            {
                Debug.LogError("You don't have the Mini_Game_Jump set up properly because there's no entry in the Dialogues To Play list for this puzzle");
            }
            else
            {
                //Debug.Log("trying to start mini game dialogue");
                Dialogue d = PuzzleDialogueRefs[CurPuzzle].GetObject() as Dialogue;
                if (d != null)
                {
                    bool dialogueActive = MiniGameArticyFlow.CheckIfDialogueShouldStart(d, null);
                    Puzzles[CurPuzzle].SetDialogueActive(dialogueActive);
                }
                else Debug.LogError("No dialogue specified for this mini game level: " + CurPuzzle);
            }
        }
        

        Puzzles[CurPuzzle].BeginPuzzleStartTime();
        GameState = eGameState.PLAYING;        
    }

    public void SavePuzzlesProgress(bool success)
    {
        if (success == true)
        {
            ArticyGlobalVariables.Default.Mini_Games.Parking_Demo_Progress = CurPuzzle + 1;
            if (PuzzleNameRoot.Contains("Parking")) ArticyGlobalVariables.Default.Mini_Games.Parking_Demo_Progress = CurPuzzle + 1;
            else if (PuzzleNameRoot.Contains("LockPicking")) ArticyGlobalVariables.Default.Mini_Games.LockPicking_Demo_Progress = CurPuzzle + 1;
            else if (PuzzleNameRoot.Contains("Repair")) ArticyGlobalVariables.Default.Mini_Games.Repair_Demo_Progress = CurPuzzle + 1;
            StaticStuff.SaveSaveData("MiniGameMCP.SavePuzzleProgress() if success == true");
        }
    }
    public void PuzzleFinished()
    {                
        MiniGameArticyFlow.QuitCurDialogue();
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
