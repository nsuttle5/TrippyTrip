using UnityEngine;
using UnityEngine.Playables;


[RequireComponent(typeof(PlayableDirector))]
public class CutsceneDialogueBridge : MonoBehaviour
{
    private PlayableDirector _director;

    [Header("Example wiring for the intro scene")]
    [SerializeField] private DialogueData goblinIntroDialogue;
    [SerializeField] private Transform goblinSpeakerAnchor; 

    [Header("What happens after dialogue ends")]
    [Tooltip("If true, calls director.Resume() to continue the same Timeline. " +
             "If false, you can hook OnDialogueComplete elsewhere")]
    [SerializeField] private bool resumeTimelineOnComplete = true;

    public System.Action OnDialogueComplete;

    void Awake()
    {
        _director = GetComponent<PlayableDirector>();
    }

    public void PlayDialogue(DialogueData data, Transform speaker)
    {
        _director.Pause();
        DialogueManager.Instance.StartDialogue(data, speaker, OnDialogueFinished);
    }


    public void PlayGoblinIntroDialogue()
    {
        PlayDialogue(goblinIntroDialogue, goblinSpeakerAnchor);
    }

    // Add one of these per dialogue trigger point in your cutscene,
    // public void PlayGoblinFollowUpDialogue() => PlayDialogue(goblinFollowUpDialogue, goblinSpeakerAnchor);

    private void OnDialogueFinished()
    {
        if (resumeTimelineOnComplete)
            _director.Resume();

        OnDialogueComplete?.Invoke();
    }
}
