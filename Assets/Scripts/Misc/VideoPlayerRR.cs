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
        videoName = "Maj_Intro_Cinematic__Compressed";
        VideoClip clip = Resources.Load<VideoClip>("Movies/" + videoName);
        VideoPlayer.clip = clip;
        OnVideoEnd = callback;        
        BackgroundMusicPlayer.Play("Off");
        VideoPlayer.Play();        
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
       // Debug.Log("VideoPlayerRR.EndReached()");            
        VideoPlayer.Stop();
        if (OnVideoEnd != null) OnVideoEnd.Invoke();
        OnVideoEnd = null;
        ToggleVideoPlayerChild(false);
        ConvoUI.SetSkipMovieButtonActive(false);
        FindObjectOfType<CCPlayer>().GetComponent<ArticyFlow>().SkipDialogue();
        // check if we need to hold off to handle IAP
        if(SceneManager.GetActiveScene().name.Contains("E1.Hanger_Intro"))
        {
            Debug.Log("We want to do the IAP after this dialogue so set the flag");
            this.MCP.SaveNextObjectForIAP = true;
        }
    }
}
