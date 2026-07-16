using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObstacleSpeedPenalty : MonoBehaviour
{
    [SerializeField] private float speedPenalty = 1.5f;
    [SerializeField] private bool destroySelfOnHit = true;
    [SerializeField] private AudioClip[] hitSounds;

    private bool _hasHit;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    public void Configure(float penalty, bool destroyOnHit, AudioClip[] sounds)
    {
        speedPenalty = penalty;
        destroySelfOnHit = destroyOnHit;
        hitSounds = sounds;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit) return;
        if (!other.CompareTag("Player")) return;

        _hasHit = true;

        if (SpeedManager.Instance != null)
        {
            SpeedManager.Instance.ApplySpeedPenalty(speedPenalty);
        }

        if (hitSounds != null && hitSounds.Length > 0)
        {
            AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
            if (clip != null) AudioSource.PlayClipAtPoint(clip, transform.position);
        }

        if (destroySelfOnHit) Destroy(gameObject);
    }
}