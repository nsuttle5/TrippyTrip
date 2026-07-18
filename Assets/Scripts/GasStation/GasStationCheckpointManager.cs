using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class GasStationCheckpointManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarMovement carMovement;
    [SerializeField] private SpeedOdometer odometer;
    [SerializeField] private GameObject gasStationPrefab;

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GasStationShopUI shopUI;

    [Header("Checkpoint Spacing (miles)")]
    [SerializeField] private float initialGapMiles = 5f;
    [SerializeField] private float gapGrowthMultiplier = 1.2f;

    [Header("Despawn")]

    [SerializeField] private float stationDespawnDelay = 2f;

    [Header("Camera Recenter")]
    [SerializeField, Min(0.01f)] private float cameraRecenterSpeed = 10f;
    [SerializeField, Min(0f)] private float cameraRecenterDelay = 0f;

    [Header("Events")]
    [SerializeField] private UnityEvent onStationDeparted;


    private float _currentGapMiles;
    private float _nextSpawnMiles;
    private bool _stationSpawned;
    private GameObject _activeStation;

    private Transform _oldCameraParent;
    private float _cameraRecenterStartTime;
    private bool _cameraRecenteringActive;
    private Vector3 _cameraLocalPositionVelocity;
    private Vector3 _cameraLocalRotationVelocity;

    private void Start()
    {
        _currentGapMiles = initialGapMiles;
        _nextSpawnMiles = _currentGapMiles;
        _oldCameraParent = Camera.main.transform.parent;
    }

    private void Update()
    {
        Transform cameraTransform = Camera.main.transform;

        if (!_stationSpawned && odometer.TotalMiles >= _nextSpawnMiles)
        {
            SpawnStation();
        }

        if (_cameraRecenteringActive && Time.time >= _cameraRecenterStartTime)
        {
            if (cameraTransform.localPosition == Vector3.zero && cameraTransform.localRotation == Quaternion.identity)
            {
                _cameraLocalPositionVelocity = Vector3.zero;
                _cameraLocalRotationVelocity = Vector3.zero;
                _cameraRecenteringActive = false;
                return;
            }

            float smoothTime = 1f / cameraRecenterSpeed;

            Vector3 newPosition = Vector3.SmoothDamp(cameraTransform.localPosition, Vector3.zero, ref _cameraLocalPositionVelocity, smoothTime);
            cameraTransform.localPosition = newPosition;

            Vector3 currentEuler = cameraTransform.localEulerAngles;
            float newX = Mathf.SmoothDampAngle(currentEuler.x, 0f, ref _cameraLocalRotationVelocity.x, smoothTime);
            float newY = Mathf.SmoothDampAngle(currentEuler.y, 0f, ref _cameraLocalRotationVelocity.y, smoothTime);
            float newZ = Mathf.SmoothDampAngle(currentEuler.z, 0f, ref _cameraLocalRotationVelocity.z, smoothTime);
            cameraTransform.localRotation = Quaternion.Euler(newX, newY, newZ);

            if (cameraTransform.localPosition.sqrMagnitude <= 0.0001f && Quaternion.Angle(cameraTransform.localRotation, Quaternion.identity) <= 0.1f)
            {
                cameraTransform.localPosition = Vector3.zero;
                cameraTransform.localRotation = Quaternion.identity;
                _cameraLocalPositionVelocity = Vector3.zero;
                _cameraLocalRotationVelocity = Vector3.zero;
                _cameraRecenteringActive = false;
            }
        }
    }

    private void BeginCameraRecenter(Transform newParent)
    {
        _cameraRecenterStartTime = Time.time + cameraRecenterDelay;
        _cameraLocalPositionVelocity = Vector3.zero;
        _cameraLocalRotationVelocity = Vector3.zero;
        _cameraRecenteringActive = true;
        Camera.main.transform.SetParent(newParent);
    }

    private void SpawnStation()
    {
        _activeStation = Instantiate(
            gasStationPrefab,
            spawnPoint.position,
            spawnPoint.rotation * gasStationPrefab.transform.rotation);

        _activeStation.transform.SetParent(ObjectMover.worldMove);

        //SideObjectScroll scroll = _activeStation.GetComponent<SideObjectScroll>();
        //if (scroll == null) scroll = _activeStation.AddComponent<SideObjectScroll>();
        //scroll.m_destroyZ = float.NegativeInfinity;

        GasStationTrigger trigger = _activeStation.GetComponentInChildren<GasStationTrigger>();
        if (trigger == null) trigger = _activeStation.AddComponent<GasStationTrigger>();
        trigger.Init(this);

        _stationSpawned = true;
    }

    public void OnPlayerEnteredStation(GasStationTrigger trigger)
    {
        carMovement.SetPaused(true);
        _oldCameraParent = Camera.main.transform.parent;
        BeginCameraRecenter(trigger.newCameraParent);
        trigger.doorOpenScript.Open(cameraRecenterDelay+0.5f);
        trigger.shopUIRoot.GetComponent<Canvas>().worldCamera = Camera.main;
        trigger.continueButton.onClick.RemoveListener(OnContinuePressed);
        trigger.continueButton.onClick.AddListener(OnContinuePressed);
        trigger.moneyText.text = CurrencyManager.Instance.GetCurrencyCount().ToString();
    }



    private void OnContinuePressed()
    {
        StartCoroutine(OnContinuePressedRoutine());
    }

    private IEnumerator OnContinuePressedRoutine()
    {
        carMovement.SetPaused(false);

        if (_activeStation != null)
        {
            GasStationTrigger trigger = _activeStation.GetComponent<GasStationTrigger>();
            if (trigger != null)
            {
                trigger.continueButton.onClick.RemoveListener(OnContinuePressed);
                trigger.doorOpenScript.Close(.1f);
            }
        }

        if (_oldCameraParent != null)
        {
            BeginCameraRecenter(_oldCameraParent);

            Transform cameraTransform = Camera.main.transform;
            while (_cameraRecenteringActive ||
                   cameraTransform.parent != _oldCameraParent ||
                   cameraTransform.localPosition != Vector3.zero ||
                   cameraTransform.localRotation != Quaternion.identity)
            {
                yield return null;
            }
        }

        if (_activeStation != null) Destroy(_activeStation, stationDespawnDelay);
        _activeStation = null;
        _stationSpawned = false;

        _currentGapMiles *= gapGrowthMultiplier;
        _nextSpawnMiles = odometer.TotalMiles + _currentGapMiles;

        onStationDeparted?.Invoke();
    }
}