using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Play: FindObjectOfType<AudioManager>().Play("");
    //Stop: FindObjectOfType<AudioManager>().Pause("");

    [Header("Scripts")]
    public TileMap scriptTileMap;

    [Header("Audio")]
    public Sound[] sounds;

    void Awake()
    {
        foreach(Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    void Start()
    {
        Play("Gameplay Music");
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return;
        }

        s.source.Play();
    }

    public void Pause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return;
        }

        s.source.Stop();
    }

    public void PlayFootstepAudio()
    {
        string unitClass = scriptTileMap.selectedUnit.GetComponent<UnitStats>().unitClass;

        if (unitClass == "Fighter")
        {
            string footstepAudio = "Male Footsteps";
            Sound s = Array.Find(sounds, sound => sound.name == footstepAudio);
            s.source.Play();
        }
        else if (unitClass == "Druid")
        {
            string footstepAudio = "Floating Footsteps";
            Sound s = Array.Find(sounds, sound => sound.name == footstepAudio);
            s.source.Play();
        }
        else if (unitClass == "Rogue")
        {
            string footstepAudio = "Soft Footsteps";
            Sound s = Array.Find(sounds, sound => sound.name == footstepAudio);
            s.source.Play();
        }
        else if (unitClass == "Bandit" || unitClass == "Bandit Archer" || unitClass == "Bandit Captain")
        {
            string footstepAudio = "Bandit Footsteps";
            Sound s = Array.Find(sounds, sound => sound.name == footstepAudio);
            s.source.Play();
        }
    }

    public void StopFootstepAudio()
    {
        string unitClass = scriptTileMap.selectedUnit.GetComponent<UnitStats>().unitClass;
        if (unitClass == "Fighter")
        {
            string footstepAudio = "Male Footsteps";
            Sound s = Array.Find(sounds, sound => sound.name == footstepAudio);
            s.source.Stop();
        }
        else if (unitClass == "Druid")
        {
            string footstepAudio = "Floating Footsteps";
            Sound s = Array.Find(sounds, sound => sound.name == footstepAudio);
            s.source.Stop();
        }
        else if (unitClass == "Rogue")
        {
            string footstepAudio = "Soft Footsteps";
            Sound s = Array.Find(sounds, sound => sound.name == footstepAudio);
            s.source.Stop();
        }
        else if (unitClass == "Bandit" || unitClass == "Bandit Archer" || unitClass == "Bandit Captain")
        {
            string footstepAudio = "Bandit Footsteps";
            Sound s = Array.Find(sounds, sound => sound.name == footstepAudio);
            s.source.Stop();
        }

    }
}
