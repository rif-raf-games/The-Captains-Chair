using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSound : MonoBehaviour
{
    public GameObject AmbientAudioSourceTemplate;

    List<GameObject> AmbientSounds = new List<GameObject>();

    public void StartAmbientSound(string ambientSound)
    {
       // Debug.Log("StartAmbientSound(): " + ambientSound);
        GameObject newAmbientSound = Instantiate(AmbientAudioSourceTemplate);
        AudioClip clip = Resources.Load<AudioClip>("SFX/" + ambientSound);
        if(clip == null) { Debug.LogError("This ambient sound isn't in the Resources folder: " + ambientSound); }
        newAmbientSound.name = ambientSound;
        newAmbientSound.GetComponent<AudioSource>().clip = clip;
        newAmbientSound.GetComponent<AudioSource>().Play();
        AmbientSounds.Add(newAmbientSound);
    }

    public void ShutOffAmbientSound(string ambientSound)
    {
        GameObject sound = null;
        foreach(GameObject go in AmbientSounds)
        {
            if(go.name.Equals(ambientSound))
            {
                sound = go;
                break;
            }
        }
        if(sound == null) { Debug.LogError("This ambient sound is not playing: " + ambientSound); return; }
        AmbientSounds.Remove(sound);
        Destroy(sound);
    }
}
