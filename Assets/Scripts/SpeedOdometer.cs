using TMPro;
using UnityEngine;

public class SpeedOdometer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text odometerText;
    [SerializeField] private string unitSuffix = " mi";
    [SerializeField] private int decimalPlaces = 1;

    [Header("Miles Conversion")]
    [SerializeField] private float milesPerSpeedUnit = 0.08f;
    [SerializeField] private float milesPerSecondAtRest = 0f;
    [SerializeField] private float speedThreshold = 0.05f;

    [Header("Progressive Gain")]
    [SerializeField] private float gainRampPerSecond = 0.015f;
    [SerializeField] private float maxGainMultiplier = 6f;

    private float _displayMiles;
    private float _gainMultiplier = 1f;

    private void Awake()
    {
        if (odometerText == null)
        {
            odometerText = GetComponent<TMP_Text>();
        }
    }

    private void Start()
    {
        RefreshDisplay();
    }

    private void Update()
    {
        float speed = GetCurrentSpeed();
        bool isMoving = speed > speedThreshold;

        if (isMoving)
        {
            _gainMultiplier = Mathf.MoveTowards(_gainMultiplier, maxGainMultiplier, gainRampPerSecond * Time.deltaTime);
        }

        float milesThisFrame = milesPerSecondAtRest;

        if (isMoving)
        {
            milesThisFrame += speed * milesPerSpeedUnit * _gainMultiplier;
        }

        _displayMiles += milesThisFrame * Time.deltaTime;
        RefreshDisplay();
    }

    public void ResetOdometer()
    {
        _displayMiles = 0f;
        _gainMultiplier = 1f;
        RefreshDisplay();
    }

    private float GetCurrentSpeed()
    {
        return Mathf.Max(0f, CarMovement.scrollSpeed);
    }

    private void RefreshDisplay()
    {
        if (odometerText == null)
        {
            return;
        }

        odometerText.text = _displayMiles.ToString($"F{decimalPlaces}") + unitSuffix;
    }
}
