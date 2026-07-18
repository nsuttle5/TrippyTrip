using System.Collections;
using UnityEngine;

public class WindshieldBlob : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The body mesh's Renderer -- NOT the eye sphere. Color randomization applies here.")]
    [SerializeField] private Renderer bodyRenderer;
    [Tooltip("The disconnected eye sphere's Transform.")]
    [SerializeField] private Transform eyeTransform;
    [SerializeField] private AudioSource audioSource; // optional; falls back to PlayClipAtPoint if unassigned
    [SerializeField] private AudioClip thunkSound;

    [Header("Spawn-In Animation")]
    [SerializeField] private float scaleInDuration = 0.12f;
    [SerializeField] private AnimationCurve scaleInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Color Variation")]
    [Tooltip("Leave empty to use fully random HSV colors instead of a fixed palette.")]
    [SerializeField] private Color[] possibleColors;
    [Tooltip("Must match the body material's color property Reference name (e.g. _BaseColor for URP Lit, _Color for Built-in).")]
    [SerializeField] private string colorPropertyName = "_BaseColor";

    [Header("Eye Darting")]
    [SerializeField] private float eyeWanderRadius = 0.05f;
    [SerializeField] private float eyeMoveDuration = 0.08f;
    [SerializeField] private Vector2 eyePauseRange = new Vector2(0.3f, 1.2f);

    [Header("Fling (on wipe)")]
    [SerializeField] private float flingDuration = 0.35f;
    [Tooltip("Base fling direction in local space; X sign is randomized per-fling for left/right variety.")]
    [SerializeField] private Vector3 flingLocalDirection = new Vector3(1f, 0.4f, 0f);
    [SerializeField] private float flingDistance = 2f;
    [SerializeField] private float flingSpinDegrees = 720f;

    private Vector3 _targetScale;
    private Vector3 _eyeRestLocalPos;
    private MaterialPropertyBlock _propertyBlock;
    private int _colorPropertyId;
    private bool _isFlung;

    private void Awake()
    {
        if (eyeTransform != null) _eyeRestLocalPos = eyeTransform.localPosition;
        _propertyBlock = new MaterialPropertyBlock();
        _colorPropertyId = Shader.PropertyToID(colorPropertyName);
    }


    public void Spawn()
    {
        _targetScale = transform.localScale; // captured here, not Awake, so spawner-applied scale multipliers are respected
        RandomizeColor();
        PlayThunk();
        StartCoroutine(ScaleInRoutine());
        StartCoroutine(EyeDartRoutine());
    }

    private void RandomizeColor()
    {
        if (bodyRenderer == null) return;

        Color color = possibleColors != null && possibleColors.Length > 0
            ? possibleColors[Random.Range(0, possibleColors.Length)]
            : Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.7f, 1f);

        bodyRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(_colorPropertyId, color);
        bodyRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void PlayThunk()
    {
        if (thunkSound == null) return;

        if (audioSource != null) audioSource.PlayOneShot(thunkSound);
        else AudioSource.PlayClipAtPoint(thunkSound, transform.position);
    }

    private IEnumerator ScaleInRoutine()
    {
        transform.localScale = Vector3.zero;
        float t = 0f;

        while (t < scaleInDuration)
        {
            t += Time.deltaTime;
            float normalized = scaleInCurve.Evaluate(Mathf.Clamp01(t / scaleInDuration));
            transform.localScale = _targetScale * normalized;
            yield return null;
        }

        transform.localScale = _targetScale;
    }

    private IEnumerator EyeDartRoutine()
    {
        if (eyeTransform == null) yield break;

        while (!_isFlung)
        {
            Vector2 randomOffset2D = Random.insideUnitCircle * eyeWanderRadius;
            Vector3 targetLocalPos = _eyeRestLocalPos + new Vector3(randomOffset2D.x, randomOffset2D.y, 0f);
            Vector3 startPos = eyeTransform.localPosition;

            float t = 0f;
            while (t < eyeMoveDuration)
            {
                t += Time.deltaTime;
                eyeTransform.localPosition = Vector3.Lerp(startPos, targetLocalPos, t / eyeMoveDuration);
                yield return null;
            }
            eyeTransform.localPosition = targetLocalPos;

            yield return new WaitForSeconds(Random.Range(eyePauseRange.x, eyePauseRange.y));
        }
    }

    public void Fling()
    {
        if (_isFlung) return;
        _isFlung = true;
        StopAllCoroutines();
        StartCoroutine(FlingRoutine());
    }

    private IEnumerator FlingRoutine()
    {
        Vector3 startPos = transform.localPosition;

        Vector3 direction = flingLocalDirection;
        direction.x *= Random.value < 0.5f ? 1f : -1f; // random left/right variety
        Vector3 endPos = startPos + direction.normalized * flingDistance;

        Quaternion startRot = transform.localRotation;

        float t = 0f;
        while (t < flingDuration)
        {
            t += Time.deltaTime;
            float normalized = t / flingDuration;
            transform.localPosition = Vector3.Lerp(startPos, endPos, normalized);
            transform.localRotation = startRot * Quaternion.Euler(0f, 0f, flingSpinDegrees * normalized);
            transform.localScale = Vector3.Lerp(_targetScale, Vector3.zero, normalized);
            yield return null;
        }

        Destroy(gameObject);
    }
}