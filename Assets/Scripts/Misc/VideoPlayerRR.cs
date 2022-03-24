using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoPlayerRR : MonoBehaviour
{
    public VideoPlayer VideoPlayer;
    public GameObject VideoPlayerChild;
    public ConvoUI ConvoUI;
    public MCP MCP;
    Action OnVideoEnd = null;

    
    // Start is called before the first frame update
    void Start()
    {
        VideoPlayer.loopPointReached += EndReached;
        ToggleVideoPlayerChild(false);     
    }

    public void SkipVideo()
    {
        EndReached(VideoPlayer);
    }

    private void Update()
    {
        if(VideoPlayer.isPlaying)
        {         
            if(ConvoUI.IsSkipMovieButtonActive() == false) ConvoUI.SetSkipMovieButtonActive(true);            
        }
    }

    public bool IsVideoPlaying()
    {
        return VideoPlayer.isPlaying;
    }

    public void ToggleVideoPlayerChild(bool isActive)
    {
       // Debug.Log("ToggleVideoPlayerChild(): isActive: " + isActive);
        VideoPlayerChild.SetActive(isActive);
    }
    public void PlayVideo(string videoName, Action callback)
    {
       // videoName = "Maj_Intro_Cinematic__Compressed";
      //  Debug.Log("-----------------------------------VideoPlayerRR.PlayVideo(): videoName: " + videoName);
        ToggleVideoPlayerChild(true);        
        VideoClip clip = Resources.Load<VideoClip>("Movies/" + videoName);
        VideoPlayer.clip = clip;
        OnVideoEnd = callback;        
        BackgroundMusicPlayer.Play("Off");
        VideoPlayer.Play();
        // check if we need to hold off to handle IAP
        if (SceneManager.GetActiveScene().name.Contains("Hangar_Intro"))
        {
            Debug.Log("We're about to play the IAP video so hold the dialogue object until after the IAP popups --IAP--");
            this.MCP.SaveNextObjectForIAP = true; 
        }        
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
       // Debug.Log("VideoPlayerRR.EndReached()");            
        VideoPlayer.Stop();
        if (OnVideoEnd != null) OnVideoEnd.Invoke();
        OnVideoEnd = null;
        ToggleVideoPlayerChild(false);
        ConvoUI.SetSkipMovieButtonActive(false);
        //FindObjectOfType<CCPlayer>().GetComponent<ArticyFlow>().SkipDialogue();

#if UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE
        if (SceneManager.GetActiveScene().name.Contains("Hangar_Intro"))
        {
            Debug.Log("Finished IAP video so get the popups ready to go --IAP--");
            FindObjectOfType<IAPManager>().RRBeginIAPProcess();
            return;
        }
#endif

        ArticyFlow af = FindObjectOfType<ArticyFlow>();
        if (af == null) { Debug.LogError("ERROR: no ArticyFlow object in scene."); return; }
        if (af.VideoPlayerPauseDialogue != null)
        {
            Debug.Log("We have a non null VideoPlayerPauseObject so get the flow going again --IAP--");
            af.StartAfterVideoPlayer();
        }
    }
}
