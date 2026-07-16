using UnityEngine;
using UnityEngine.Events;

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

    [Header("Events")]
    [SerializeField] private UnityEvent onStationDeparted;


    private float _currentGapMiles;
    private float _nextSpawnMiles;
    private bool _stationSpawned;
    private GameObject _activeStation;

    private void Start()
    {
        _currentGapMiles = initialGapMiles;
        _nextSpawnMiles = _currentGapMiles;
    }

    private void Update()
    {
        if (!_stationSpawned && odometer.TotalMiles >= _nextSpawnMiles)
        {
            SpawnStation();
        }
    }

    private void SpawnStation()
    {
        _activeStation = Instantiate(
            gasStationPrefab,
            spawnPoint.position,
            spawnPoint.rotation * gasStationPrefab.transform.rotation);

        SideObjectScroll scroll = _activeStation.GetComponent<SideObjectScroll>();
        if (scroll == null) scroll = _activeStation.AddComponent<SideObjectScroll>();
        scroll.m_destroyZ = float.NegativeInfinity;

        GasStationTrigger trigger = _activeStation.GetComponentInChildren<GasStationTrigger>();
        if (trigger == null) trigger = _activeStation.AddComponent<GasStationTrigger>();
        trigger.Init(this);

        _stationSpawned = true;
    }

    public void OnPlayerEnteredStation()
    {
        carMovement.SetPaused(true);
        shopUI.Show(OnContinuePressed);
    }

    private void OnContinuePressed()
    {
        shopUI.Hide();
        carMovement.SetPaused(false);

        if (_activeStation != null) Destroy(_activeStation, stationDespawnDelay);
        _activeStation = null;
        _stationSpawned = false;

        _currentGapMiles *= gapGrowthMultiplier;
        _nextSpawnMiles = odometer.TotalMiles + _currentGapMiles;

        onStationDeparted?.Invoke();
    }
}