using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerRR : MonoBehaviour
{
    public VideoPlayer VideoPlayer;
    public GameObject VideoPlayerChild;
    Action OnVideoEnd = null;

    
    // Start is called before the first frame update
    void Start()
    {
        VideoPlayer.loopPointReached += EndReached;
        ToggleVideoPlayerChild(false);     
    }  

    public void ToggleVideoPlayerChild(bool isActive)
    {
       // Debug.Log("ToggleVideoPlayerChild(): isActive: " + isActive);
        VideoPlayerChild.SetActive(isActive);
    }
    public void PlayVideo(string videoName, Action callback)
    {
       // videoName = "Maj_Intro_Cinematic__Compressed";
       // Debug.Log("VideoPlayerRR.PlayVideo(): videoName: " + videoName);
        ToggleVideoPlayerChild(true);
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
    }
}
