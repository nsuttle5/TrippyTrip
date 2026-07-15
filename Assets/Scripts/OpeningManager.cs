using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpeningManager : MonoBehaviour
{
    [System.Serializable]
    public class IntroStep
    {
        public CanvasGroup canvasGroup;
    }

    [SerializeField] private List<IntroStep> introSteps = new List<IntroStep>();
    [SerializeField] private Button beginButton;
    [SerializeField] private CanvasGroup beginButtonGroup;
    [SerializeField] private string nextSceneName = "";
    [SerializeField] private float delayBetweenSteps = 1f;
    [SerializeField] private float fadeInDuration = 0.75f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private readonly List<CanvasGroup> _allGroups = new List<CanvasGroup>();
    private Coroutine _sequenceRoutine;
    private bool _beginClicked;

    private void Awake()
    {
        CacheCanvasGroups();
        HideAllGroups();

        if (beginButton != null)
        {
            beginButton.onClick.AddListener(HandleBeginClicked);
        }
    }

    private void Start()
    {
        _sequenceRoutine = StartCoroutine(PlayOpeningSequence());
    }

    private void OnDestroy()
    {
        if (beginButton != null)
        {
            beginButton.onClick.RemoveListener(HandleBeginClicked);
        }
    }

    private void CacheCanvasGroups()
    {
        _allGroups.Clear();

        for (int index = 0; index < introSteps.Count; index++)
        {
            CanvasGroup group = introSteps[index] != null ? introSteps[index].canvasGroup : null;
            if (group != null && !_allGroups.Contains(group))
            {
                _allGroups.Add(group);
            }
        }

        if (beginButtonGroup != null && !_allGroups.Contains(beginButtonGroup))
        {
            _allGroups.Add(beginButtonGroup);
        }
    }

    private void HideAllGroups()
    {
        for (int index = 0; index < _allGroups.Count; index++)
        {
            SetGroupVisible(_allGroups[index], false);
        }

        if (beginButton != null)
        {
            beginButton.interactable = false;
        }
    }

    private IEnumerator PlayOpeningSequence()
    {
        for (int index = 0; index < introSteps.Count; index++)
        {
            CanvasGroup group = introSteps[index] != null ? introSteps[index].canvasGroup : null;
            if (group == null)
            {
                continue;
            }

            yield return new WaitForSecondsRealtime(delayBetweenSteps);
            yield return FadeGroup(group, 0f, 1f, fadeInDuration);
        }

        if (beginButtonGroup != null)
        {
            yield return new WaitForSecondsRealtime(delayBetweenSteps);
            yield return FadeGroup(beginButtonGroup, 0f, 1f, fadeInDuration);
        }

        if (beginButton != null)
        {
            beginButton.interactable = true;
        }
    }

    private void HandleBeginClicked()
    {
        if (_beginClicked)
        {
            return;
        }

        _beginClicked = true;
        if (beginButton != null)
        {
            beginButton.interactable = false;
        }

        StartCoroutine(FadeOutAndLoadScene());
    }

    private IEnumerator FadeOutAndLoadScene()
    {
        if (_sequenceRoutine != null)
        {
            StopCoroutine(_sequenceRoutine);
            _sequenceRoutine = null;
        }

        for (int index = 0; index < _allGroups.Count; index++)
        {
            CanvasGroup group = _allGroups[index];
            if (group != null)
            {
                yield return FadeGroup(group, group.alpha, 0f, fadeOutDuration);
            }
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("OpeningManager: nextSceneName is not set, so no scene will be loaded.", this);
            yield break;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeGroup(CanvasGroup group, float startAlpha, float targetAlpha, float duration)
    {
        if (group == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            SetGroupAlpha(group, targetAlpha);
            yield break;
        }

        float elapsed = 0f;
        SetGroupInteractable(group, targetAlpha > 0.99f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            SetGroupAlpha(group, Mathf.Lerp(startAlpha, targetAlpha, normalizedTime));
            yield return null;
        }

        SetGroupAlpha(group, targetAlpha);
    }

    private void SetGroupVisible(CanvasGroup group, bool isVisible)
    {
        if (group == null)
        {
            return;
        }

        group.alpha = isVisible ? 1f : 0f;
        SetGroupInteractable(group, isVisible);
    }

    private void SetGroupAlpha(CanvasGroup group, float alpha)
    {
        if (group == null)
        {
            return;
        }

        group.alpha = alpha;
        SetGroupInteractable(group, alpha > 0.99f);
    }

    private void SetGroupInteractable(CanvasGroup group, bool isInteractable)
    {
        group.interactable = isInteractable;
        group.blocksRaycasts = isInteractable;
    }
}
