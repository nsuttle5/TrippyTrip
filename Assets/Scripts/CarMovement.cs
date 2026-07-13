using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarMovement : MonoBehaviour
{
    [Header("Core Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float bound;
    [SerializeField] private float boundForce;
    [SerializeField] private float boundStart;
    [SerializeField] private float rotationScale;
    [SerializeField] private float rotationSpeed;

    [Header("Shrink Mechanic")]
    [SerializeField] private bool enableShrinkMechanic = true;
    [SerializeField] private Key shrinkKey = Key.LeftShift;
    [SerializeField] private float shrinkScaleMultiplier = 0.35f;
    [SerializeField] private float shrinkDuration = 2f;
    [SerializeField] private float shrinkTransitionSpeed = 8f;

    [Header("Launch Mechanic")]
    [SerializeField] private bool enableLaunchMechanic = true;
    [SerializeField] private Key launchKey = Key.Space;
    [SerializeField] private float launchVelocity = 15f;
    [SerializeField] private float fallAcceleration = 25f;

    [Header("Speed Boost Mechanic")]
    [SerializeField] private bool enableBoostMechanic = true;
    [SerializeField] private Key boostKey = Key.LeftCtrl;
    [SerializeField] private float boostSpeedMultiplier = 2f;
    [SerializeField] private float boostDuration = 1.5f;

    [Header("Road Scroll")]
    [SerializeField] private float baseScrollSpeed = 1f;
    [SerializeField] private float scrollSpeedTransitionSpeed = 5f;
    private const string scrollSpeedPropertyName = "_speed";

    private Vector3 _originalScale;
    private float _groundY;
    private bool _isShrinking;
    private bool _isLaunched;
    private bool _isBoosting;
    private float _currentMoveSpeedMultiplier = 1f;
    private float _currentScrollSpeed;
    private int _scrollSpeedPropertyId;

    void Start()
    {
        _originalScale = transform.localScale;
        _groundY = rb.position.y;

        _currentScrollSpeed = baseScrollSpeed;
        _scrollSpeedPropertyId = Shader.PropertyToID(scrollSpeedPropertyName);
        Shader.SetGlobalFloat(_scrollSpeedPropertyId, _currentScrollSpeed);
    }

    void Update()
    {
        HandleMechanicInput();

        float horizontalInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed)
            {
                horizontalInput = -1f;
            }
            else if (Keyboard.current.dKey.isPressed)
            {
                horizontalInput = 1f;
            }
        }

        Vector3 position = rb.position;
        position.x = Mathf.Clamp(position.x, -bound, bound);
        rb.position = position;

        float closeness = Mathf.Clamp01((Mathf.Abs(position.x) - (bound - boundStart)) / boundStart);

        float effectiveMoveSpeed = moveSpeed * _currentMoveSpeedMultiplier;
        float desiredSpeed = horizontalInput * effectiveMoveSpeed - closeness * boundForce * effectiveMoveSpeed * Mathf.Sign(position.x);
        float currentSpeed = rb.linearVelocity.x;
        float newSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * Time.deltaTime);

        float verticalVelocity = rb.linearVelocity.y;
        if (_isLaunched)
        {
            verticalVelocity -= fallAcceleration * Time.deltaTime;

            if (rb.position.y <= _groundY && verticalVelocity <= 0f)
            {
                Vector3 landedPosition = rb.position;
                landedPosition.y = _groundY;
                rb.position = landedPosition;
                verticalVelocity = 0f;
                _isLaunched = false;
            }
        }

        rb.linearVelocity = new Vector3(newSpeed, verticalVelocity, rb.linearVelocity.z);

        float targetScrollSpeed = baseScrollSpeed * _currentMoveSpeedMultiplier;
        _currentScrollSpeed = Mathf.MoveTowards(_currentScrollSpeed, targetScrollSpeed, scrollSpeedTransitionSpeed * Time.deltaTime);
        Shader.SetGlobalFloat(_scrollSpeedPropertyId, _currentScrollSpeed);

        float rotationAngle = currentSpeed * rotationScale;
        float smoothedY = Mathf.LerpAngle(transform.localEulerAngles.y, rotationAngle, Time.deltaTime * rotationSpeed);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, smoothedY, transform.localEulerAngles.z);
    }

    private void HandleMechanicInput()
    {
        if (Keyboard.current == null) return;

        if (enableShrinkMechanic && !_isShrinking && Keyboard.current[shrinkKey].wasPressedThisFrame)
        {
            StartCoroutine(ShrinkRoutine());
        }

        if (enableLaunchMechanic && !_isLaunched && Keyboard.current[launchKey].wasPressedThisFrame)
        {
            Launch();
        }

        if (enableBoostMechanic && !_isBoosting && Keyboard.current[boostKey].wasPressedThisFrame)
        {
            StartCoroutine(BoostRoutine());
        }
    }

    private void Launch()
    {
        _isLaunched = true;
        Vector3 velocity = rb.linearVelocity;
        velocity.y = launchVelocity;
        rb.linearVelocity = velocity;
    }

    private IEnumerator ShrinkRoutine()
    {
        _isShrinking = true;
        Vector3 targetScale = _originalScale * shrinkScaleMultiplier;

        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * shrinkTransitionSpeed);
            yield return null;
        }
        transform.localScale = targetScale;

        yield return new WaitForSeconds(shrinkDuration);

        while (Vector3.Distance(transform.localScale, _originalScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _originalScale, Time.deltaTime * shrinkTransitionSpeed);
            yield return null;
        }
        transform.localScale = _originalScale;

        _isShrinking = false;
    }

    private IEnumerator BoostRoutine()
    {
        _isBoosting = true;
        _currentMoveSpeedMultiplier = boostSpeedMultiplier;

        yield return new WaitForSeconds(boostDuration);

        _currentMoveSpeedMultiplier = 1f;
        _isBoosting = false;
    }
}