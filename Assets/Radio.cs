using System;
using UnityEngine;
using System.Collections.Generic;

public class Radio : MonoBehaviour
{
    [Serializable]
    public class Song
    {
        public AudioClip audio;
        public int channel;
        public float tune;
    }

    public AudioSource staticAudio;
    public AudioSource musicAudio;
    public Knob tuneKnob;
    public float staticStart = 15f;
    public float staticVolumeMultiplier = 1;
    public float musicVolumeMultiplier = 1;
    public List<Song> songs = new List<Song>();

    Song currentSong;

    void Awake()
    {
        if (staticAudio != null)
        {
            staticAudio.loop = true;
            if (!staticAudio.isPlaying && staticAudio.clip != null)
            {
                staticAudio.Play();
            }
        }

        if (musicAudio != null)
        {
            musicAudio.loop = true;
        }
    }

    void Update()
    {
        if (tuneKnob == null || musicAudio == null || staticAudio == null)
        {
            return;
        }

        float tuneValue = tuneKnob.value;
        float closestDistance = Mathf.Infinity;
        Song closestSong = null;

        foreach (var song in songs)
        {
            if (song == null || song.audio == null)
            {
                continue;
            }

            float songDistance = Mathf.Abs(song.tune - tuneValue);
            if (songDistance < closestDistance)
            {
                closestDistance = songDistance;
                closestSong = song;
            }
        }

        if (closestSong == null)
        {
            currentSong = null;
            musicAudio.Stop();
            musicAudio.clip = null;
            musicAudio.volume = 0f;
            staticAudio.volume = Mathf.Max(0f, staticVolumeMultiplier);
            if (!staticAudio.isPlaying && staticAudio.clip != null)
            {
                staticAudio.Play();
            }
            return;
        }

        if (currentSong != closestSong)
        {
            currentSong = closestSong;
            musicAudio.clip = currentSong.audio;
            musicAudio.Play();
        }

        float normalizedSignal = 1f - Mathf.Clamp01(closestDistance / Mathf.Max(staticStart, 0.001f));
        float signalStrength = normalizedSignal * normalizedSignal * (3f - 2f * normalizedSignal);
        float clampedMusicMultiplier = Mathf.Max(0f, musicVolumeMultiplier);
        float clampedStaticMultiplier = Mathf.Max(0f, staticVolumeMultiplier);

        musicAudio.volume = signalStrength * clampedMusicMultiplier;
        staticAudio.volume = (1f - signalStrength) * clampedStaticMultiplier;

        if (signalStrength > 0f)
        {
            if (!musicAudio.isPlaying)
            {
                musicAudio.Play();
            }
        }
        else if (musicAudio.isPlaying)
        {
            musicAudio.Pause();
        }

        if (!staticAudio.isPlaying && staticAudio.clip != null)
        {
            staticAudio.Play();
        }
    }
}
