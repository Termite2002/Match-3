using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundEffects
{
    land, 
    swap,
    resolve,
    upgrade,
    powerup,
    score
}

[RequireComponent(typeof(AudioSource))]
public class AudioMixer : Singleton<AudioMixer>
{
    [SerializeField]
    private AudioSource music,
                        soundEffects;

    [SerializeField]
    private AudioClip[] sounds;

    protected override void Init()
    {
        soundEffects = GetComponent<AudioSource>();
    }

    // Play background music
    public void PlayMusic()
    {
        music.Play();
    }

    // pause/unpause bg music
    public void PauseMusic(bool pause)
    {
        if (pause)
            music.Pause();
        else
            music.UnPause();
    }

    // play a sound effect
    public void PlaySound(SoundEffects effect)
    {
        soundEffects.PlayOneShot(sounds[(int)effect]);
    }

    // play a sound effect after a time delay
    public IEnumerator PlayDelayedSound(SoundEffects effect, float t)
    {
        yield return new WaitForSeconds(t);
        PlaySound(effect);
    }
}
