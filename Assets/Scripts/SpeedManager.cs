using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SpeedManager : MonoBehaviour
{
    public static SpeedManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CarMovement carMovement;
    [SerializeField] private SpeedOdometer odometer;

    [Header("Speed Curve")]
    [SerializeField] private float startingSpeed = 0.05f;
    [SerializeField] private float speedRampPerSecond = 0.15f;
    [SerializeField] private float maxBaseSpeed = 8f;

    [Header("Lose Condition")]
    [SerializeField] private float stalledGraceDuration = 1.5f;

    [SerializeField] private float loseSpeedThreshold = 0.15f;

    [SerializeField] private float openingGraceDuration = 4f;

    [SerializeField] private string loseSceneName = "LoseScene";
    [SerializeField] private UnityEvent onLose;

    [SerializeField, Range(0f, 1f)] private float basePenaltyMultiplier = 1f;

    private float _roundTime;
    private float _timeBelowThreshold;
    private bool _hasLost;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        carMovement.BaseScrollSpeed = startingSpeed;
    }

    private void Update()
    {
        if (_hasLost) return;

        if (carMovement.IsPaused)
        {
            // don't ramp speed or evaluate the lose condition while parked at a gas station
            _timeBelowThreshold = 0f;
            return;
        }

        _roundTime += Time.deltaTime;

        carMovement.BaseScrollSpeed = Mathf.Min(
            maxBaseSpeed,
            carMovement.BaseScrollSpeed + speedRampPerSecond * Time.deltaTime);

        CheckLoseCondition();
    }

    private void CheckLoseCondition()
    {
        if (_roundTime < openingGraceDuration)
        {
            _timeBelowThreshold = 0f;
            return;
        }

        if (CarMovement.scrollSpeed <= loseSpeedThreshold)
        {
            _timeBelowThreshold += Time.deltaTime;
            if (_timeBelowThreshold >= stalledGraceDuration)
            {
                TriggerLose();
            }
        }
        else
        {
            _timeBelowThreshold = 0f;
        }
    }

    public void ApplySpeedPenalty(float amount)
    {
        CarMovement.scrollSpeed = Mathf.Max(0f, CarMovement.scrollSpeed - amount);
        carMovement.BaseScrollSpeed = Mathf.Max(0f, carMovement.BaseScrollSpeed - amount * basePenaltyMultiplier);
    }

    private void TriggerLose()
    {
        _hasLost = true;
        RunResults.MilesTraveled = odometer.TotalMiles;
        onLose?.Invoke();
        SceneManager.LoadScene(loseSceneName);
    }
}