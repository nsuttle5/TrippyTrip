using System.Collections;
using UnityEngine;

public class ScenerySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spawnPoint;

    [Header("Pool")]
    [SerializeField] private SceneryData[] allScenery;

    [Header("Road Sides")]
    [SerializeField] private float leftSideX = -18f;
    [SerializeField] private float rightSideX = 18f;

    [Header("Spawn Timing")]
    [SerializeField] private float minSpawnInterval = 2f;
    [SerializeField] private float maxSpawnInterval = 4f;
    [SerializeField] private bool startOnPlay = true;

    [Header("Anti-Repetition")]
    [SerializeField] private bool avoidImmediateRepeats = true;

    private Coroutine _spawnRoutine;
    private SceneryData _lastSpawned;

    private void Start()
    {
        if (minSpawnInterval > maxSpawnInterval)
        {
            (minSpawnInterval, maxSpawnInterval) = (maxSpawnInterval, minSpawnInterval);
        }

        if (startOnPlay)
        {
            _spawnRoutine = StartCoroutine(SpawnLoop());
        }
    }

    public void StartSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
        }

        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
            SpawnOne();
        }
    }

    private void SpawnOne()
    {
        if (spawnPoint == null || allScenery == null || allScenery.Length == 0)
        {
            return;
        }

        SceneryData data = PickScenery();
        if (data == null || data.modelPrefab == null)
        {
            return;
        }

        ScenerySide side = data.spawnSide == ScenerySide.Either
            ? (Random.value < 0.5f ? ScenerySide.Left : ScenerySide.Right)
            : data.spawnSide;

        float sideX = side == ScenerySide.Left ? leftSideX : rightSideX;
        Vector3 spawnPosition = spawnPoint.position + data.positionOffset;
        spawnPosition.x = sideX + data.positionOffset.x;

        Quaternion sideRotation = Quaternion.identity;
        if (data.faceRoad)
        {
            Vector3 lookDirection = new Vector3(-spawnPosition.x, 0f, 0f);
            if (lookDirection.sqrMagnitude > 0.0001f)
            {
                sideRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            }

            Vector3 sideOffset = side == ScenerySide.Left ? data.leftFacingOffset : data.rightFacingOffset;
            sideRotation *= Quaternion.Euler(sideOffset);
        }

        Quaternion spawnRotation = spawnPoint.rotation * sideRotation * Quaternion.Euler(data.rotationOffset);
        Vector3 spawnScale = data.scaleMultiplier;

        GameObject instance = Instantiate(data.modelPrefab, spawnPosition, spawnRotation);
        instance.transform.localScale = Vector3.Scale(instance.transform.localScale, spawnScale);

        if (ObjectMover.worldMove != null)
        {
            instance.transform.SetParent(ObjectMover.worldMove);
        }

        _lastSpawned = data;
    }

    private SceneryData PickScenery()
    {
        if (allScenery.Length == 1)
        {
            return allScenery[0];
        }

        SceneryData chosen = null;
        int attempts = 0;

        while (attempts < 8)
        {
            SceneryData candidate = allScenery[Random.Range(0, allScenery.Length)];
            if (!avoidImmediateRepeats || candidate != _lastSpawned)
            {
                chosen = candidate;
                break;
            }

            attempts++;
        }

        if (chosen == null)
        {
            chosen = allScenery[Random.Range(0, allScenery.Length)];
        }

        return chosen;
    }
}