using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticlePulseController : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float playDuration = 1f;
    [SerializeField] private float stopDuration = 1f;
    [SerializeField] private bool useRandomIntervals;
    [SerializeField] private Vector2 playDurationRange = new Vector2(0.5f, 1.5f);
    [SerializeField] private Vector2 stopDurationRange = new Vector2(0.5f, 1.5f);

    [Header("Options")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool useUnscaledTime;

    private ParticleSystem _particleSystem;
    private Coroutine _pulseRoutine;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        if (playOnStart)
        {
            _pulseRoutine = StartCoroutine(PulseLoop());
        }
    }

    private void OnDisable()
    {
        if (_pulseRoutine != null)
        {
            StopCoroutine(_pulseRoutine);
            _pulseRoutine = null;
        }

        if (_particleSystem != null)
        {
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void StartPulsing()
    {
        if (_pulseRoutine != null)
        {
            StopCoroutine(_pulseRoutine);
        }

        _pulseRoutine = StartCoroutine(PulseLoop());
    }

    public void StopPulsing(bool clearParticles = true)
    {
        if (_pulseRoutine != null)
        {
            StopCoroutine(_pulseRoutine);
            _pulseRoutine = null;
        }

        if (_particleSystem != null)
        {
            _particleSystem.Stop(true, clearParticles ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            _particleSystem.Play();
            yield return Wait(playDuration, playDurationRange);

            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            yield return Wait(stopDuration, stopDurationRange);
        }
    }

    private IEnumerator Wait(float fixedDuration, Vector2 range)
    {
        float duration = useRandomIntervals
            ? Random.Range(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y))
            : Mathf.Max(0f, fixedDuration);

        if (duration <= 0f)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
    }
}
