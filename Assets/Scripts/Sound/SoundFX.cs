using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFX : MonoBehaviour
{     
    [System.Serializable]
    public class FXInfo
    {
        string Name { get; set; }
        public AudioClip Clip;

        public void SetName() { this.Name = Clip.name; }
        public string GetName() { return this.Name; }
    }

    public List<FXInfo> SoundFXList;    

    AudioSource AudioSource;
    public List<string> SoundFXNames;

    private void Awake()
    {
        this.AudioSource = GetComponent<AudioSource>();
        SoundFXList = new List<FXInfo>();
        SoundFXNames = new List<string>();
    }

    public void SetupFXList(List<SoundFX.FXInfo> soundFXUsedInScene)
    {
        SoundFXList.Clear();
        SoundFXNames.Clear();
        foreach (FXInfo info in soundFXUsedInScene)
        {
            info.SetName();
            SoundFXList.Add(info);            
            SoundFXNames.Add(info.GetName());
        }
    }

    /*private void Start()
    {        
        foreach (FXInfo info in SoundFXList)
        {
            info.SetName();
            SoundFXNames.Add(info.GetName());
        }
    }*/

    public void PlayVO(AudioClip voClip)
    {
        this.AudioSource.PlayOneShot(voClip);
    }

    public void PlayFX(string fxID )
    {
        int fxIndex = SoundFXNames.IndexOf(fxID);
        if(fxIndex == -1) { /*Debug.LogWarning("Trying to play a sound effect that doesn't exist: " + fxID); */ return; }

        this.AudioSource.PlayOneShot(SoundFXList[fxIndex].Clip);      
    }

    public void SetVolume(int vol)
    {
        this.AudioSource.volume = vol / 100f;
    }
}

public static class SoundFXPlayer
{
    static SoundFX SoundFX;
    public static void Init(SoundFX soundFX, int vol)
    {
        if (soundFX == null) { Debug.LogError("Trying to set up a null sound fx Player."); return; }
        SoundFX = soundFX;
        if(vol != -1f) SoundFX.SetVolume(vol); // monote - update this volume thing with the mCP
    }
    public static void Play(string fxID)
    {
        if(SoundFX == null ) { Debug.LogError("Trying to play sound fx with no sound fx player"); return; }
        SoundFX.PlayFX(fxID);
    }

    public static void PlayVO(AudioClip voClip)
    {
        if (SoundFX == null) { Debug.LogError("Trying to play VO with no sound fx player"); return; }
        SoundFX.PlayVO(voClip);
    }
}
