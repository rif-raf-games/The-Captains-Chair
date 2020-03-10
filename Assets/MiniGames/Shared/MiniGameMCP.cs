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
    public MiniGame[] Puzzles;
    public int CurPuzzle;

    // UI
    ArticyFlow MiniGameArticyFlow;

    [Header("Misc Stuff")]
    public Text ResultsText;
    public Text DebugText;


    public virtual void Awake()
    {
        Debug.Log("MiniGameMCP.Awake(): " + this.name);
        if (this.name.Contains("Parking"))
        {
            //StaticStuff.SetOrientation(StaticStuff.eOrientation.PORTRAIT);
        }
        else
        {
            //StaticStuff.SetOrientation(StaticStuff.eOrientation.LANDSCAPE);
        }
    }
    // Start is called before the first frame update
    public virtual void Start()
    {
        //Debug.Log("MiniGameMCP.Start()");
        GameState = eGameState.NONE;
        FadeImage.gameObject.SetActive(true);
        MiniGameArticyFlow = GetComponent<ArticyFlow>();
        if (MiniGameArticyFlow == null) Debug.LogError("There's no ArticyFlow component on this mini game MCP: " + this.name);
        else MiniGameArticyFlow.ConvoUI.gameObject.SetActive(false);
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
        }
        Puzzles = new MiniGame[PuzzlesToLoad.Length];
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
            Debug.Log("Load Done: " + puzzleName);            
            UnityEngine.SceneManagement.Scene puzzleScene = SceneManager.GetSceneAt(1);
            MiniGame newPuzzle = null;
            GameObject[] newPuzzleObjs = puzzleScene.GetRootGameObjects();
            foreach (GameObject go in newPuzzleObjs)
            {
                newPuzzle = go.GetComponent<MiniGame>();
                if (newPuzzle != null) break;
            }
            Puzzles[i] = newPuzzle;
            Puzzles[i].transform.parent = this.transform;
            asyncLoad = SceneManager.UnloadSceneAsync(puzzleScene);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            Debug.Log("Unload Done: " + puzzleScene);
            Puzzles[i].Init(this);                        
        }

        for (int i = 1; i < Puzzles.Length; i++)
        {
            Puzzles[i].gameObject.SetActive(false);
        }


        CurPuzzle = 0;
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
            Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");
            SceneManager.LoadScene(jumpSave.Template.Next_Game_Scene.Scene_Name);
            
        }
        else
        {
            Puzzles[CurPuzzle].gameObject.SetActive(false);
            CurPuzzle++;
            Puzzles[CurPuzzle].gameObject.SetActive(true);
            SetupLerpFade(1f, 0f, 1.5f);
            GameState = eGameState.FADE_IN;
        }

    }
    void StartCurrentPuzzle()
    {
        Puzzles[CurPuzzle].BeginPuzzle();
        GameState = eGameState.PLAYING;
        // UI
        Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");
        List<ArticyObject> dialogues = jumpSave.Template.Dialogue_List.DialoguesToPlay;
        if(dialogues == null || dialogues.Count == 0 || dialogues.Count-1 < CurPuzzle)
        {
           // Debug.LogError("You don't have the Mini_Game_Jump set up properly because there's no entry in the Dialogues To Play list for this puzzle");
        }
        else
        {
            Dialogue d = jumpSave.Template.Dialogue_List.DialoguesToPlay[CurPuzzle] as Dialogue;
            if (d != null) MiniGameArticyFlow.CheckDialogue(d, null);
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
