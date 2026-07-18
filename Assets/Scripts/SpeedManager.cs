using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SpeedManager : MonoBehaviour
{
    public static SpeedManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CarMovement carMovement;
    [SerializeField] private SpeedOdometer odometer;
    [SerializeField] private TMP_Text gasText;

    [Header("Initial Ramp-Up (one-time)")]
    [SerializeField] private float startingSpeed = 0.05f;
    [SerializeField] private float initialRampPerSecond = 0.15f;
    [Tooltip("The 'good speed' the car ramps up to once at round start. After reaching this, the car never ramps up again on its own -- only the passive decline and gas-recovery ramps apply from here on.")]
    [SerializeField] private float cruisingSpeedTarget = 4f;

    [Header("Passive Decline (permanent, after reaching cruising speed)")]
    [Tooltip("Very slow constant decline to the speed target, active for the rest of the round regardless of gas level.")]
    [SerializeField] private float passiveDecayPerSecond = 0.02f;
    [Tooltip("Much steeper decline rate used instead of the passive one whenever gas is empty.")]
    [SerializeField] private float emptyGasDecayPerSecond = 1.5f;

    [Header("Gas")]
    [SerializeField] private float maxGas = 100f;
    [SerializeField] private float passiveGasDrainPerSecond = 1f;
    [Tooltip("Extra gas drained per second on top of the passive drain, while scrollSpeed is below the target and catching back up (e.g. recovering from an obstacle hit).")]
    [SerializeField] private float recoveryExtraGasDrainPerSecond = 3f;
    [SerializeField] private string gasSuffix = " gas";
    [SerializeField] private int gasDecimalPlaces = 0;

    [Header("Lose Condition")]
    [SerializeField] private float stalledGraceDuration = 1.5f;
    [SerializeField] private float loseSpeedThreshold = 0.15f;
    [SerializeField] private float openingGraceDuration = 4f;
    [SerializeField] private string loseSceneName = "LoseScene";
    [SerializeField] private UnityEvent onLose;

    private float _roundTime;
    private float _timeBelowThreshold;
    private bool _hasLost;
    private bool _hasReachedCruisingSpeed;
    private float _currentGas;

    public float CurrentGas => _currentGas;
    public float MaxGas => maxGas;
    public float GasPercent => maxGas > 0f ? _currentGas / maxGas : 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        carMovement.BaseScrollSpeed = startingSpeed;
        _currentGas = maxGas;
        RefreshGasDisplay();
    }

    private void Update()
    {
        if (_hasLost) return;

        if (carMovement.IsPaused)
        {
            // don't ramp/decay speed, drain gas, or evaluate the lose condition while parked at a gas station
            _timeBelowThreshold = 0f;
            return;
        }

        _roundTime += Time.deltaTime;

        UpdateSpeedCurve();
        UpdateGas();
        CheckLoseCondition();
    }

    private void UpdateSpeedCurve()
    {
        if (!_hasReachedCruisingSpeed)
        {
            carMovement.BaseScrollSpeed = Mathf.Min(
                cruisingSpeedTarget,
                carMovement.BaseScrollSpeed + initialRampPerSecond * Time.deltaTime);

            if (carMovement.BaseScrollSpeed >= cruisingSpeedTarget)
            {
                _hasReachedCruisingSpeed = true;
            }
        }
        else
        {
            bool isEmpty = _currentGas <= 0f;
            float decayRate = isEmpty ? emptyGasDecayPerSecond : passiveDecayPerSecond;
            carMovement.BaseScrollSpeed = Mathf.Max(0f, carMovement.BaseScrollSpeed - decayRate * Time.deltaTime);
        }
    }

    private void UpdateGas()
    {
        bool isRecovering = CarMovement.scrollSpeed < carMovement.BaseScrollSpeed - 0.01f;

        float drain = passiveGasDrainPerSecond * Time.deltaTime;
        if (isRecovering) drain += recoveryExtraGasDrainPerSecond * Time.deltaTime;

        _currentGas = Mathf.Max(0f, _currentGas - drain);
        RefreshGasDisplay();
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
    }

    public void RefillGas()
    {
        _currentGas = maxGas;
        RefreshGasDisplay();
    }

    private void RefreshGasDisplay()
    {
        if (gasText == null)
        {
            return;
        }

        gasText.text = _currentGas.ToString($"F{gasDecimalPlaces}") + gasSuffix;
    }

    private void TriggerLose()
    {
        _hasLost = true;
        RunResults.MilesTraveled = odometer.TotalMiles;
        onLose?.Invoke();
        SceneManager.LoadScene(loseSceneName);
    }
}