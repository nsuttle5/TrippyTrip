using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns obstacles from a rotating random "pool" of ObstacleData types.
/// Call StartNewLeg() (wired to GasStationCheckpointManager's onStationDeparted
/// event) to re-roll the pool and escalate both pool size and spawn frequency.
/// Reuses SideObjectScroll for movement, same as your other roadside objects.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarMovement carMovement;

    [Header("Catalog")]
    [Tooltip("Every obstacle type that could possibly be chosen into a leg's pool.")]
    [SerializeField] private ObstacleData[] allObstacles;
    [SerializeField] private bool allowDuplicatesInPool = false;

    [Header("Pool Size (grows each leg)")]
    [SerializeField] private int basePoolSize = 2;
    [SerializeField] private int poolSizeGrowthPerLeg = 1;

    [Header("Spawn Timing (shrinks each leg)")]
    [SerializeField] private float baseMinSpawnInterval = 2.5f;
    [SerializeField] private float baseMaxSpawnInterval = 4.5f;
    [SerializeField] private float intervalReductionPerLeg = 0.2f;
    [Tooltip("Floor so escalation never produces an impossible-to-dodge spawn rate.")]
    [SerializeField] private float minPossibleInterval = 0.6f;

    [Header("Lane Positions")]
    [SerializeField] private float leftLaneX = -3f;
    [SerializeField] private float centerLaneX = 0f;
    [SerializeField] private float rightLaneX = 3f;

    [Header("Spawn Point")]
    [Tooltip("Somewhere far down the road, same idea as your SideSpawner's spawn Z.")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float destroyZ = -20f;

    private readonly List<ObstacleData> _currentPool = new List<ObstacleData>();
    private int _legIndex = -1;
    private Coroutine _spawnRoutine;

    private void Start()
    {
        StartNewLeg(); // first leg begins immediately, before any gas station
    }

    /// <summary>Re-rolls the obstacle pool and restarts spawning with escalated count/frequency.
    /// Wire this to GasStationCheckpointManager's onStationDeparted UnityEvent.</summary>
    public void StartNewLeg()
    {
        _legIndex++;
        SelectRandomPool();

        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void SelectRandomPool()
    {
        _currentPool.Clear();

        if (allObstacles == null || allObstacles.Length == 0) return;

        int poolSize = basePoolSize + poolSizeGrowthPerLeg * _legIndex;

        if (allowDuplicatesInPool)
        {
            for (int i = 0; i < poolSize; i++)
            {
                _currentPool.Add(allObstacles[Random.Range(0, allObstacles.Length)]);
            }
        }
        else
        {
            poolSize = Mathf.Min(poolSize, allObstacles.Length);

            List<ObstacleData> shuffled = new List<ObstacleData>(allObstacles);
            for (int i = 0; i < shuffled.Count; i++)
            {
                int swapIndex = Random.Range(i, shuffled.Count);
                (shuffled[i], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[i]);
            }
            _currentPool.AddRange(shuffled.GetRange(0, poolSize));
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float minInterval = Mathf.Max(minPossibleInterval, baseMinSpawnInterval - intervalReductionPerLeg * _legIndex);
            float maxInterval = Mathf.Max(minInterval, baseMaxSpawnInterval - intervalReductionPerLeg * _legIndex);

            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

            // don't queue obstacles while parked at a gas station -- they'd just pile up motionless
            if (carMovement != null && carMovement.IsPaused) continue;

            SpawnOne();
        }
    }

    private void SpawnOne()
    {
        if (_currentPool.Count == 0) return;

        ObstacleData data = _currentPool[Random.Range(0, _currentPool.Count)];
        if (data == null || data.modelPrefab == null) return;

        float xPos = data.lane switch
        {
            ObstacleLane.Left => leftLaneX,
            ObstacleLane.Right => rightLaneX,
            _ => centerLaneX,
        };

        Vector3 spawnPos = new Vector3(xPos, spawnPoint.position.y, spawnPoint.position.z) + data.positionOffset;
        Quaternion spawnRot = spawnPoint.rotation * data.modelPrefab.transform.rotation * Quaternion.Euler(data.rotationOffset);

        GameObject instance = Instantiate(data.modelPrefab, spawnPos, spawnRot);
        instance.transform.localScale = Vector3.Scale(instance.transform.localScale, data.scaleMultiplier);

        SideObjectScroll scroll = instance.GetComponent<SideObjectScroll>();
        if (scroll == null) scroll = instance.AddComponent<SideObjectScroll>();
        scroll.m_destroyZ = destroyZ;

        ObstacleSpeedPenalty penalty = instance.GetComponent<ObstacleSpeedPenalty>();
        if (penalty == null) penalty = instance.AddComponent<ObstacleSpeedPenalty>();
        penalty.Configure(data.speedPenalty, data.destroySelfOnHit, data.hitSounds, data.deathParticle);
    }
}
