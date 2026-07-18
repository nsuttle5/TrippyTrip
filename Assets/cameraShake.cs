using UnityEngine;

public class cameraShake : MonoBehaviour
{
    [Header("Shake")]
    [SerializeField] private bool shakeOnStart = true;
    [SerializeField, Min(0f)] private float positionAmplitude = 0.03f;
    [SerializeField, Min(0f)] private float rotationAmplitude = 0.5f;
    [SerializeField, Min(0.01f)] private float shakeFrequency = 0.35f;
    [SerializeField, Min(0f)] private float intensityChangeSpeed = 2f;

    private Vector3 _baseLocalPosition;
    private Quaternion _baseLocalRotation;
    private float _seedX;
    private float _seedY;
    private float _seedZ;
    private float _targetIntensity;
    private float _currentIntensity;

    private void Awake()
    {
        _baseLocalPosition = transform.localPosition;
        _baseLocalRotation = transform.localRotation;
        _seedX = Random.Range(0f, 1000f);
        _seedY = Random.Range(0f, 1000f);
        _seedZ = Random.Range(0f, 1000f);
    }

    private void OnEnable()
    {
        if (shakeOnStart)
        {
            StartShake();
        }
    }

    private void OnDisable()
    {
        transform.localPosition = _baseLocalPosition;
        transform.localRotation = _baseLocalRotation;
        _currentIntensity = 0f;
        _targetIntensity = 0f;
    }

    private void LateUpdate()
    {
        _currentIntensity = Mathf.MoveTowards(_currentIntensity, _targetIntensity, intensityChangeSpeed * Time.deltaTime);

        if (_currentIntensity <= 0f)
        {
            transform.localPosition = _baseLocalPosition;
            transform.localRotation = _baseLocalRotation;
            return;
        }

        float time = Time.unscaledTime * shakeFrequency;
        float posX = (Mathf.PerlinNoise(_seedX, time) - 0.5f) * 2f * positionAmplitude * _currentIntensity;
        float posY = (Mathf.PerlinNoise(_seedY, time) - 0.5f) * 2f * positionAmplitude * _currentIntensity;
        float posZ = (Mathf.PerlinNoise(_seedZ, time) - 0.5f) * 2f * positionAmplitude * _currentIntensity;

        float rotX = (Mathf.PerlinNoise(_seedX + 10f, time) - 0.5f) * 2f * rotationAmplitude * _currentIntensity;
        float rotY = (Mathf.PerlinNoise(_seedY + 10f, time) - 0.5f) * 2f * rotationAmplitude * _currentIntensity;
        float rotZ = (Mathf.PerlinNoise(_seedZ + 10f, time) - 0.5f) * 2f * rotationAmplitude * _currentIntensity;

        transform.localPosition = _baseLocalPosition + new Vector3(posX, posY, posZ);
        transform.localRotation = _baseLocalRotation * Quaternion.Euler(rotX, rotY, rotZ);
    }

    public void StartShake()
    {
        _targetIntensity = 1f;
    }

    public void StopShake()
    {
        _targetIntensity = 0f;
    }
}
