using System.Collections;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [Header("What to spawn")]
    [SerializeField] private GameObject[] objectsToSpawn;
    [SerializeField] private Transform spawnPoint;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveDuration = 3f;

    [Header("Continuous Spawning")]
    [Tooltip("Random gap (seconds) between spawns is picked fresh each cycle within this range.")]
    [SerializeField] private float minSpawnInterval = 1.5f;
    [SerializeField] private float maxSpawnInterval = 2.5f;
    [SerializeField] private bool startSpawningOnAwake = true;
    [SerializeField] private bool destroyAfterMoveDuration = true;

    private Coroutine _spawnRoutine;

    private void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;

        if (startSpawningOnAwake)
            StartSpawning();
    }


    public void StartSpawning()
    {
        StopSpawning();
        _spawnRoutine = StartCoroutine(ContinuousSpawnRoutine());
    }

    public void StopSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    private IEnumerator ContinuousSpawnRoutine()
    {
        while (true)
        {
            SpawnOne();

            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }


    public void SpawnOne()
    {
        if (objectsToSpawn == null || objectsToSpawn.Length == 0)
        {
            Debug.LogWarning("CarSpawner: no objects assigned to spawn.");
            return;
        }

        int randomIndex = Random.Range(0, objectsToSpawn.Length);
        GameObject prefab = objectsToSpawn[randomIndex];

        GameObject instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        StartCoroutine(MoveForward(instance));
    }

    private IEnumerator MoveForward(GameObject obj)
    {
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            if (obj == null) yield break;

            obj.transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (destroyAfterMoveDuration && obj != null)
            Destroy(obj);
    }
}