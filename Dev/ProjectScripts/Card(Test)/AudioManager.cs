using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip[] sfxClips = new AudioClip[10];

    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
            }
            return instance;
        }
    }

    public enum effect
    {
        cardShuffleSound = 0,
        cardPickSound = 1,
        cardDealSound = 2,
        cardDiscardSound = 3,
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudioOnce(AudioClip audioClip, float volume)
    {
        audioSource.PlayOneShot(audioClip, volume);
    }

    public void PlayAudioById(int id, float volume)
    {
        AudioClip audioClip = Resources.Load<AudioClip>($"Audio/Battle/{id}");
        if (audioClip == null) return;
        audioSource.PlayOneShot(audioClip, volume);
    }
}
