using UnityEngine;

public class ObstacleSpeedPenalty : MonoBehaviour
{
    [SerializeField] private float speedPenalty = 1.5f;
    [SerializeField] private bool destroySelfOnHit = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        SpeedManager.Instance.ApplySpeedPenalty(speedPenalty);

        if (destroySelfOnHit) Destroy(gameObject);
    }
}
