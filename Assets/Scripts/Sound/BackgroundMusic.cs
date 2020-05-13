using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    AudioSource AudioSource;

    // Start is called before the first frame update
    void Start()
    {
        this.AudioSource = GetComponent<AudioSource>();
    }
    public void PlayMusic(string musicName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Bacground Music/" + musicName);
        if(clip == null) { Debug.LogError("trying to play background music that isn't in the Resources/Background Music folder: " + musicName); return; }
        this.AudioSource.Stop();
        this.AudioSource.clip = clip;
        this.AudioSource.Play();
    }
    public void StopMusic()
    {
        this.AudioSource.Stop();            
    }

    public void SetVolume(int vol)
    {
        this.AudioSource.volume = vol / 100f;
    }
}

public static class BackgroundMusicPlayer
{
    static BackgroundMusic BGMusic;
    public static void Init(BackgroundMusic bgMusic, int vol)
    {
        if (bgMusic == null) { Debug.LogError("Trying to set up a null BackgroundMusic Player."); return; }        
        BGMusic = bgMusic;
        BGMusic.SetVolume(vol);
    }

    public static void Play(string musicName)
    {
        if (BGMusic == null) { Debug.LogError("Trying to play music with no music player"); return; }
        if (musicName.Equals("Off")) BGMusic.StopMusic();
        else BGMusic.PlayMusic(musicName);
    }
}
