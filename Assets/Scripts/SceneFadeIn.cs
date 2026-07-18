using System.Collections;
using UnityEngine;

public class SceneFadeIn : MonoBehaviour
{
    [Header("References")]
    [Tooltip("CanvasGroup on a fullscreen black Image covering the screen. Its GameObject can be left disabled in the editor -- this script re-enables it at runtime.")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private void Awake()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
        }
    }

    private void Start()
    {
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        if (fadeCanvasGroup == null) yield break;

        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = fadeCurve.Evaluate(Mathf.Clamp01(t / fadeDuration));
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, normalized);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.gameObject.SetActive(false);
    }
}
