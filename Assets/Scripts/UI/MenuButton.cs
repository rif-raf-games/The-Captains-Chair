using Articy.The_Captain_s_Chair.Features;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    public Image Icon;
    public Text JobNumText;
    public Text JobNameText;

    public string JobLocation;
    public string PointOfContact;
    public string JobDescription;

    public Exchange_MissionFeature ExchangeMission = null;
    public LoadingScreenFeature LoadingScreen = null;
    public Mini_Game_Puzzles_To_PlayFeature PuzzlesToPlay = null;
    public Dialogue_ListFeature DialogueList = null;
    public Success_Mini_Game_ResultFeature SuccessResult = null;
    public Quit_Mini_Game_ResultFeature QuitResult = null;
    public Success_Save_FragmentFeature SuccessSaveFragment = null;
}
