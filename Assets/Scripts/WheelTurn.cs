using UnityEngine;
using UnityEngine.InputSystem;

public class WheelTurn : MonoBehaviour
{
    [Header("Turning")]
    [SerializeField] private Transform wheelModel;
    [SerializeField] private Vector3 steeringAxis = Vector3.up;
    [SerializeField] private float maxTurnAngle = 18f;
    [SerializeField] private float turnResponseSpeed = 55f;
    [SerializeField] private float returnSpeed = 75f;

    [Header("Optional Dynamics")]
    [SerializeField] private Rigidbody carBody;
    [SerializeField] private float speedInfluence = 0.15f;

    private Quaternion _baseLocalRotation;
    private float _currentTurnAngle;

    private void Awake()
    {
        if (wheelModel == null)
        {
            wheelModel = transform;
        }

        _baseLocalRotation = wheelModel.localRotation;
    }

    private void Update()
    {
        float steeringInput = GetSteeringInput();
        float speedFactor = 1f;

        if (carBody != null)
        {
            speedFactor += Mathf.Abs(carBody.linearVelocity.x) * speedInfluence;
        }

        float targetAngle = steeringInput * maxTurnAngle;
        float response = steeringInput != 0f ? turnResponseSpeed * speedFactor : returnSpeed;

        _currentTurnAngle = Mathf.MoveTowards(_currentTurnAngle, targetAngle, response * Time.deltaTime);
        wheelModel.localRotation = _baseLocalRotation * Quaternion.AngleAxis(_currentTurnAngle, steeringAxis.normalized);
    }

    private static float GetSteeringInput()
    {
        if (Keyboard.current == null)
        {
            return 0f;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            return -1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            return 1f;
        }

        return 0f;
    }
}
