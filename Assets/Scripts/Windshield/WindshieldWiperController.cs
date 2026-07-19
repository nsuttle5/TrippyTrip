using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WindshieldWiperController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform wiperPivot;
    [SerializeField] private WindshieldBlobSpawner blobSpawner;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip wipeClip;

    [Header("Input")]
    [SerializeField] private Key wipeKey = Key.E;
    [SerializeField] private float cooldown = 0.6f;

    [Header("Audio")]
    [SerializeField] private Vector2 wipePitchRange = new Vector2(0.9f, 1.1f);

    [Header("Wipe Motion")]
    [Tooltip("Axis (in the wiper pivot's LOCAL space) it rotates around. Try (0,0,1) for Z, (0,1,0) for Y, or (1,0,0) for X depending on your model's orientation -- this is the fix if the wiper was spinning around the wrong axis.")]
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    [Tooltip("Degrees to rotate counterclockwise. Flip the sign if it goes the wrong way.")]
    [SerializeField] private float sweepAngle = 90f;
    [SerializeField] private float sweepUpDuration = 0.18f;
    [SerializeField] private float sweepDownDuration = 0.25f;
    [SerializeField] private AnimationCurve sweepCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Quaternion _restRotation;
    private bool _isWiping;
    private float _cooldownTimer;

    private void Awake()
    {
        if (wiperPivot != null) _restRotation = wiperPivot.localRotation;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;

        bool pressed = Keyboard.current != null && Keyboard.current[wipeKey].wasPressedThisFrame;

        if (!_isWiping && _cooldownTimer <= 0f && pressed)
        {
            StartCoroutine(WipeRoutine());
        }
    }

    private IEnumerator WipeRoutine()
    {
        _isWiping = true;
        _cooldownTimer = cooldown;

        PlayWipeSound();

        yield return RotateWiper(0f, sweepAngle, sweepUpDuration);

        if (blobSpawner != null) blobSpawner.FlingAllBlobs();

        yield return RotateWiper(sweepAngle, 0f, sweepDownDuration);

        _isWiping = false;
    }

    private void PlayWipeSound()
    {
        if (wipeClip == null || audioSource == null)
        {
            return;
        }

        audioSource.pitch = Random.Range(wipePitchRange.x, wipePitchRange.y);
        audioSource.PlayOneShot(wipeClip);
        audioSource.pitch = 1f;
    }

    private IEnumerator RotateWiper(float fromAngle, float toAngle, float duration)
    {
        if (wiperPivot == null) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = sweepCurve.Evaluate(Mathf.Clamp01(t / duration));
            float angle = Mathf.LerpAngle(fromAngle, toAngle, normalized);
            wiperPivot.localRotation = _restRotation * Quaternion.AngleAxis(angle, rotationAxis);
            yield return null;
        }

        wiperPivot.localRotation = _restRotation * Quaternion.AngleAxis(toAngle, rotationAxis);
    }
}