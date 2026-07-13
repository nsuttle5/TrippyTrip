using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RadioManager : MonoBehaviour
{
    [System.Serializable]
    public class RadioTrack
    {
        public Button button;
        public AudioClip audioClip;
    }

    [SerializeField] private List<RadioTrack> tracks = new List<RadioTrack>();
    [SerializeField] private Button stopAllButton;
    [SerializeField] private AudioSource playbackSource;

    private int activeTrackIndex = -1;
    private readonly List<UnityAction> trackListeners = new List<UnityAction>();
    private UnityAction stopAllListener;
    private readonly List<int> pausedSamples = new List<int>();
    private readonly List<bool> isPaused = new List<bool>();

    private void Awake()
    {
        if (playbackSource == null)
        {
            playbackSource = GetComponent<AudioSource>();
        }

        if (playbackSource == null)
        {
            playbackSource = gameObject.AddComponent<AudioSource>();
        }

        playbackSource.loop = true;

        pausedSamples.Clear();
        isPaused.Clear();

        for (int index = 0; index < tracks.Count; index++)
        {
            int capturedIndex = index;
            UnityAction trackListener = () => SelectTrack(capturedIndex);
            trackListeners.Add(trackListener);
            pausedSamples.Add(0);
            isPaused.Add(false);

            if (tracks[capturedIndex].button != null)
            {
                tracks[capturedIndex].button.onClick.AddListener(trackListener);
            }
        }

        if (stopAllButton != null)
        {
            stopAllListener = StopAllAudio;
            stopAllButton.onClick.AddListener(stopAllListener);
        }
    }

    private void OnDestroy()
    {
        for (int index = 0; index < tracks.Count; index++)
        {
            if (tracks[index].button != null && index < trackListeners.Count)
            {
                tracks[index].button.onClick.RemoveListener(trackListeners[index]);
            }
        }

        if (stopAllButton != null && stopAllListener != null)
        {
            stopAllButton.onClick.RemoveListener(stopAllListener);
        }
    }

    private void SelectTrack(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= tracks.Count)
        {
            return;
        }

        AudioClip selectedClip = tracks[trackIndex].audioClip;

        if (selectedClip == null || playbackSource == null)
        {
            return;
        }

        if (activeTrackIndex == trackIndex && playbackSource.isPlaying)
        {
            return;
        }

        if (activeTrackIndex >= 0 && activeTrackIndex < tracks.Count)
        {
            if (playbackSource.isPlaying)
            {
                pausedSamples[activeTrackIndex] = playbackSource.timeSamples;
                isPaused[activeTrackIndex] = true;
                playbackSource.Pause();
            }
        }

        playbackSource.clip = selectedClip;
        playbackSource.timeSamples = isPaused[trackIndex] ? pausedSamples[trackIndex] : 0;

        if (!playbackSource.isPlaying)
        {
            playbackSource.Play();
        }

        activeTrackIndex = trackIndex;
        isPaused[trackIndex] = false;
    }

    private void StopAllAudio()
    {
        if (playbackSource != null)
        {
            playbackSource.Stop();
        }

        for (int index = 0; index < tracks.Count; index++)
        {
            pausedSamples[index] = 0;
            isPaused[index] = false;
        }

        activeTrackIndex = -1;
    }
}
