using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindshieldBlobSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Prefab must have a WindshieldBlob component.")]
    [SerializeField] private GameObject blobPrefab;

    [Header("Windshield Area")]
    [Tooltip("A BoxCollider placed on/parented to the actual windshield (not the camera), defining where blobs can spawn. Select it in Scene view to see its bounds as a visual guide -- resize/reposition it there rather than typing coordinates.")]
    [SerializeField] private BoxCollider windshieldArea;
    [SerializeField] private float minSeparation = 0.18f;
    [SerializeField] private int maxPlacementAttempts = 12;

    [Header("Blob Transform")]
    [Tooltip("Extra rotation (degrees) applied on top of the windshield's own facing. Use this to fix a model showing its back instead of its front.")]
    [SerializeField] private Vector3 rotationOffset = new Vector3(0f, 180f, 0f);
    [Tooltip("Multiplies the prefab's own scale for every spawned blob.")]
    [SerializeField] private float scaleMultiplier = 1f;

    [Header("Random Spawn Rotation")]
    [Tooltip("Axis (local windshield space) each blob's random spin is applied around. Try (0,0,1) for Z, (0,1,0) for Y, (1,0,0) for X if Z isn't the right one.")]
    [SerializeField] private Vector3 randomRotationAxis = Vector3.forward;
    [Tooltip("Random angle range (degrees) picked per spawn and applied around randomRotationAxis, on top of rotationOffset. (0,0) disables the variation.")]
    [SerializeField] private Vector2 randomRotationRange = new Vector2(0f, 360f);

    [Header("Spawn Timing (shrinks each leg)")]
    [SerializeField] private float baseMinSpawnInterval = 3f;
    [SerializeField] private float baseMaxSpawnInterval = 6f;
    [SerializeField] private float intervalReductionPerLeg = 0.25f;
    [SerializeField] private float minPossibleInterval = 0.8f;

    [Header("Limits")]
    [SerializeField] private int maxActiveBlobs = 8;

    private readonly List<WindshieldBlob> _activeBlobs = new List<WindshieldBlob>();
    private int _legIndex = -1;
    private Coroutine _spawnRoutine;

    private void Awake()
    {
        if (windshieldArea != null) windshieldArea.isTrigger = true;
    }

    private void Start()
    {
        StartNewLeg(); // first leg begins immediately
    }


    public void StartNewLeg()
    {
        _legIndex++;

        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float minInterval = Mathf.Max(minPossibleInterval, baseMinSpawnInterval - intervalReductionPerLeg * _legIndex);
            float maxInterval = Mathf.Max(minInterval, baseMaxSpawnInterval - intervalReductionPerLeg * _legIndex);

            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

            TrySpawnBlob();
        }
    }

    private void TrySpawnBlob()
    {
        _activeBlobs.RemoveAll(b => b == null);

        if (_activeBlobs.Count >= maxActiveBlobs) return;
        if (blobPrefab == null || windshieldArea == null) return;
        if (!TryFindOpenSpot(out Vector3 worldPos)) return;

        float randomSpin = Random.Range(randomRotationRange.x, randomRotationRange.y);
        Quaternion spawnRot = windshieldArea.transform.rotation
            * Quaternion.Euler(rotationOffset)
            * Quaternion.AngleAxis(randomSpin, randomRotationAxis);

        GameObject instance = Instantiate(blobPrefab, worldPos, spawnRot, windshieldArea.transform);
        instance.transform.localScale *= scaleMultiplier;

        WindshieldBlob blob = instance.GetComponent<WindshieldBlob>();
        if (blob != null)
        {
            blob.Spawn();
            _activeBlobs.Add(blob);
        }
    }

    private bool TryFindOpenSpot(out Vector3 worldPosition)
    {
        Bounds bounds = windshieldArea.bounds;

        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                bounds.center.z);

            bool tooClose = false;
            foreach (WindshieldBlob blob in _activeBlobs)
            {
                if (blob == null) continue;
                if (Vector3.Distance(candidate, blob.transform.position) < minSeparation)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                worldPosition = candidate;
                return true;
            }
        }

        worldPosition = Vector3.zero;
        return false;
    }

    public void FlingAllBlobs()
    {
        foreach (WindshieldBlob blob in _activeBlobs)
        {
            if (blob != null) blob.Fling();
        }
        _activeBlobs.Clear();
    }

    private void OnDrawGizmos()
    {
        if (windshieldArea == null) return;

        Gizmos.matrix = windshieldArea.transform.localToWorldMatrix;
        Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
        Gizmos.DrawCube(windshieldArea.center, windshieldArea.size);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(windshieldArea.center, windshieldArea.size);
    }
}