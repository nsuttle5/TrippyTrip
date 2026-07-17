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
        if (!other.attachedRigidbody.CompareTag("Player")) return;

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

        if (destroySelfOnHit) 
        {
            Vector3 direction = (transform.position-other.transform.position).normalized+new Vector3(0, 1, 0);
            GetComponent<Rigidbody>().useGravity = true;
            GetComponent<Rigidbody>().AddForce(direction*30, ForceMode.Impulse);
            GetComponent<Rigidbody>().AddTorque(direction*10, ForceMode.Impulse);
            Destroy(gameObject, 5);
        }
    }
}