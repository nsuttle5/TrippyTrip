using System.Collections;
using UnityEngine;

public class PassengerSeatSpawner : MonoBehaviour
{
    [Header("Catalog")]
    [Tooltip("Every possible passenger item. One is picked at random each spawn, never the same as the immediately previous pick.")]
    [SerializeField] private PassengerItemData[] allItems;

    [Header("Seat Anchor")]
    [Tooltip("Where items are parented/positioned -- should be a child of the CAR (not the camera), at/near the passenger seat.")]
    [SerializeField] private Transform passengerSeatAnchor;

    [Header("Timing")]
    [SerializeField] private float minGapBetweenSpawns = 8f;
    [SerializeField] private float maxGapBetweenSpawns = 20f;
    [SerializeField] private float minDisplayDuration = 3f;
    [SerializeField] private float maxDisplayDuration = 8f;

    [Header("Scale Animation")]
    [SerializeField] private float scaleInDuration = 0.3f;
    [SerializeField] private float scaleOutDuration = 0.25f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private PassengerItemData _lastSpawnedItem;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float gap = Random.Range(minGapBetweenSpawns, maxGapBetweenSpawns);
            yield return new WaitForSeconds(gap);

            PassengerItemData item = PickRandomItem();
            if (item == null) continue;

            yield return SpawnAndDespawnRoutine(item);

            _lastSpawnedItem = item;
        }
    }

    private PassengerItemData PickRandomItem()
    {
        if (allItems == null || allItems.Length == 0) return null;
        if (allItems.Length == 1) return allItems[0];

        PassengerItemData candidate;
        int safety = 0;
        do
        {
            candidate = allItems[Random.Range(0, allItems.Length)];
            safety++;
        }
        while (candidate == _lastSpawnedItem && safety < 10);

        return candidate;
    }

    private IEnumerator SpawnAndDespawnRoutine(PassengerItemData item)
    {
        if (item.modelPrefab == null || passengerSeatAnchor == null) yield break;

        GameObject instance = Instantiate(item.modelPrefab, passengerSeatAnchor);
        instance.transform.localPosition = item.positionOffset;
        instance.transform.localRotation = Quaternion.Euler(item.rotationOffset);
        instance.transform.localScale = Vector3.Scale(instance.transform.localScale, item.scaleMultiplier);

        Vector3 targetScale = instance.transform.localScale;
        instance.transform.localScale = Vector3.zero;

        yield return ScaleRoutine(instance.transform, Vector3.zero, targetScale, scaleInDuration);

        float displayDuration = Random.Range(minDisplayDuration, maxDisplayDuration);
        yield return new WaitForSeconds(displayDuration);

        yield return ScaleRoutine(instance.transform, targetScale, Vector3.zero, scaleOutDuration);

        Destroy(instance);
    }

    private IEnumerator ScaleRoutine(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = scaleCurve.Evaluate(Mathf.Clamp01(t / duration));
            target.localScale = Vector3.Lerp(from, to, normalized);
            yield return null;
        }
        target.localScale = to;
    }
}
