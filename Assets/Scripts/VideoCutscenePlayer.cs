using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoCutscenePlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Scene Flow")]
    [SerializeField] private string nextSceneName = "Gameplay";
    [SerializeField] private bool playOnStart = true;

    private bool _hasEnded;

    private void Awake()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    private void Start()
    {
        if (playOnStart) videoPlayer.Play();
    }

    public void SkipCutscene()
    {
        if (_hasEnded) return;
        videoPlayer.Stop();
        OnVideoFinished(videoPlayer);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (_hasEnded) return;
        _hasEnded = true;

        SceneManager.LoadScene(nextSceneName);
    }
}

