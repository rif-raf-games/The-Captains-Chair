//using Articy.Captainschairdemo;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CargoMCP : MonoBehaviour //CaptainsChairSceneRoot
{
    public enum eGameState { FADE_IN, PLAYING, FADE_OUT, NONE };
    public eGameState GameState;

    public Image FadeImage;
    public int[] PuzzlesToLoad;
    public CargoPuzzle[] Puzzles;
    public int CurPuzzle;

    public void Start()
    {
      //  base.Start();

        GameState = eGameState.NONE;
        FadeImage.gameObject.SetActive(true);
        StartCoroutine(LoadPuzzleScenes());
    }
    IEnumerator LoadPuzzleScenes()
    {
        float startTime = Time.time;
        Puzzles = new CargoPuzzle[PuzzlesToLoad.Length];
        for (int i = 0; i < PuzzlesToLoad.Length; i++)
        {
            //Debug.Log("LoadScene: " + PuzzlesToLoad[i]);
            string puzzleName = "Puzzle" + PuzzlesToLoad[i].ToString("D2");
            //Debug.Log("load puzzle: " + puzzleName);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(puzzleName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            //Debug.Log("Load Done: " + PuzzlesToLoad[i]);            
            Scene puzzleScene = SceneManager.GetSceneAt(1);            
            CargoPuzzle newPuzzle = null;
            GameObject[] newPuzzleObjs = puzzleScene.GetRootGameObjects();
            foreach(GameObject go in newPuzzleObjs)
            {
                newPuzzle = go.GetComponent<CargoPuzzle>();
                if (newPuzzle != null) break;
            }
            newPuzzle.Init(this);
            newPuzzle.transform.parent = this.transform;
            Puzzles[i] = newPuzzle;
            asyncLoad = SceneManager.UnloadSceneAsync(puzzleScene);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            //Debug.Log("Unload Done: " + PuzzlesToLoad[i]);            
        }    
        
        for(int i=1; i<Puzzles.Length; i++)
        {
            Puzzles[i].gameObject.SetActive(false);
        }
        CurPuzzle = 0;

        float endTime = Time.time;
        float deltaTime = endTime - startTime;
        //Debug.Log("load time: " + deltaTime);
        if(deltaTime < 1f )
        {
            yield return new WaitForSeconds(1f - deltaTime);
        }
        //Debug.Log("start fade in");
        SetupLerpFade(1f, 0f, 1.5f);
        GameState = eGameState.FADE_IN;
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

    IEnumerator FadePause()
    {       
        GameState = eGameState.NONE;
        yield return new WaitForSeconds(1f);        
        EndCurrentPuzzle();
    }
    public void PuzzleFinished()
    {
        GameState = eGameState.FADE_OUT;
        SetupLerpFade(0f, 1f, 1.5f);
    }
    void EndCurrentPuzzle()
    {        
        /*if(CurPuzzle == Puzzles.Length-1)
        {
            base.PlayFirstBranch();
            Slot_Container sc = ArticyDatabase.GetObject<Slot_Container>("Start_On_Object");
            ArticyObject curStartOn = sc.Template.Slot_Feature.Slot_Feature_Slot;
            var a = curStartOn as IObjectWithFeatureSlot_Feature;
            if (a != null)
            {
                sc.Template.Slot_Feature.Slot_Feature_Slot = a.GetFeatureSlot_Feature().Slot_Feature_Slot;
            }
            else Debug.LogError("no slot feature in EndCurrentPuzzle");

            SceneManager.LoadScene(1);
           // SceneManager.LoadScene(1);
        }
        else
        {
            Puzzles[CurPuzzle].gameObject.SetActive(false);
            CurPuzzle++;
            Puzzles[CurPuzzle].gameObject.SetActive(true);
            SetupLerpFade(1f, 0f, 1.5f);
            GameState = eGameState.FADE_IN;            
        }*/
    }
    void StartCurrentPuzzle()
    {
        GameState = eGameState.PLAYING;
        Puzzles[CurPuzzle].BeginPlaying();
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
    
   /* private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 50, 50), "Hub"))
        {
            SceneManager.LoadScene(0);
        }       
    }*/

}
