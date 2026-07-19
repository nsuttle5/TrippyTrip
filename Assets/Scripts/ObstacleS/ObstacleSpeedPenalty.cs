using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObstacleSpeedPenalty : MonoBehaviour
{
    [SerializeField] private float speedPenalty = 1.5f;
    [SerializeField] private bool destroySelfOnHit = true;
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private AudioClip passByClip;
    [SerializeField] private Vector2 passByPitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] private bool isCarObstacle;
    [SerializeField] private float passByZThreshold = 0f;
    [SerializeField] private GameObject destroyParticle;

    private bool _hasHit;
    private bool _hasPlayedPassBySound;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1f;
    }

    private void Update()
    {
        if (_hasHit || _hasPlayedPassBySound || !isCarObstacle || passByClip == null)
        {
            return;
        }

        if (transform.position.z <= passByZThreshold)
        {
            _hasPlayedPassBySound = true;
            PlayPassBySound();
        }
    }

    public void Configure(float penalty, bool destroyOnHit, AudioClip[] sounds, AudioClip passClip, Vector2 pitchRange, bool carObstacle, GameObject particle)
    {
        speedPenalty = penalty;
        destroySelfOnHit = destroyOnHit;
        hitSounds = sounds;
        passByClip = passClip;
        passByPitchRange = pitchRange;
        isCarObstacle = carObstacle;
        destroyParticle = particle;
    }

    private void PlayPassBySound()
    {
        if (_audioSource == null)
        {
            return;
        }

        _audioSource.pitch = Random.Range(passByPitchRange.x, passByPitchRange.y);
        _audioSource.PlayOneShot(passByClip);
        _audioSource.pitch = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit)
        {
            if (!other.CompareTag("ground")) return;
            Instantiate(destroyParticle, transform.position, Quaternion.identity);
            Destroy(gameObject);
            return;
        }
        ;
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

        if (destroySelfOnHit)
        {
            Vector3 direction = (transform.position - other.transform.position).normalized + new Vector3(0, 1, 0);
            GetComponent<Rigidbody>().useGravity = true;
            GetComponent<Rigidbody>().AddForce(direction * 30, ForceMode.Impulse);
            GetComponent<Rigidbody>().AddTorque(direction * 10, ForceMode.Impulse);
            Destroy(gameObject, 10);
        }
    }
}