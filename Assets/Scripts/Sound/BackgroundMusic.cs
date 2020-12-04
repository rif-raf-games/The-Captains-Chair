using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioSource AudioSource;
    float SavedVol;
    Coroutine FadeCoroutine;

    private void Awake()
    {
        //Debug.Log("BackgroundMusic.Awake()");
        this.AudioSource = GetComponent<AudioSource>();
    }
    
    public void PlayMusic(string musicName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Background Music/" + musicName);
        if(clip == null) { Debug.LogWarning("trying to play background music that isn't in the Resources/Background Music folder: " + musicName); return; }
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
    public float GetVolume()
    {
        return this.AudioSource.volume;
    }

    public IEnumerator FadeVolume()
    {        
        float startVol = this.AudioSource.volume;
        float timer = 0f;
        while(timer <= 1f)
        {
            float percentage = timer;
            float vol = Mathf.Lerp(startVol, 0f, percentage);
            this.AudioSource.volume = vol;
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    
    public void StartFade()
    {
        SavedVol = this.AudioSource.volume;
        FadeCoroutine = StartCoroutine(FadeVolume());
    }
    public void ResetVolume()
    {
        if (FadeCoroutine != null) StopCoroutine(FadeCoroutine);
        FadeCoroutine = null;
        StopMusic();
        this.AudioSource.volume = SavedVol;
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
        //Debug.Log("BackgroundMusicPlayer.Play() musicName: " + musicName);
        if (BGMusic == null) { Debug.LogError("Trying to play music with no music player"); return; }
        if (musicName.Equals("Off")) BGMusic.StopMusic();
        else BGMusic.PlayMusic(musicName);
    }           
}
