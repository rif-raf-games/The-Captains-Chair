using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerRR : MonoBehaviour
{
    public VideoPlayer VideoPlayer;
    public Camera VideoCamera;
    Action OnVideoEnd = null;

    // Start is called before the first frame update
    void Start()
    {
        VideoPlayer.loopPointReached += EndReached;
        VideoCamera.enabled = false;
    }

   /* void OnGUI()
    {
        if(GUI.Button(new Rect(0,0,100,100), "Play"))
        {
            VideoCamera.enabled = true;
            VideoPlayer.Play();
        }
    }*/
    public void PlayVideo(string videoName, Action callback)
    {
      //  Debug.Log("VideoPlayerRR.PlayVideo(): videoName: " + videoName);
        //this.transform.parent.gameObject.SetActive(true);
        VideoClip clip = Resources.Load<VideoClip>("Movies/" + videoName);
        VideoPlayer.clip = clip;
        OnVideoEnd = callback;
        VideoCamera.enabled = true;
        BackgroundMusicPlayer.Play("Off");
        VideoPlayer.Play();
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
       // Debug.Log("VideoPlayerRR.EndReached()");
        VideoCamera.enabled = false;
        VideoPlayer.Stop();
        if (OnVideoEnd != null) OnVideoEnd.Invoke();
        OnVideoEnd = null;
    }
}
