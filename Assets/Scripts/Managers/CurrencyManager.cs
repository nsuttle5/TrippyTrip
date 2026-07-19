
using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField]
    private int _maxCurrency = 999;

    [SerializeField]
    private TextMeshProUGUI _currencyDisplay;

    [SerializeField]
    private AudioClip _pickupClip;

    [SerializeField]
    private Vector2 _pickupPitchRange = new Vector2(0.9f, 1.1f);

    private int m_currencyCount = 0;
    public Transform coinstack;
    public float coinToScale = .02f;
    public static CurrencyManager Instance { get; private set; }
    private AudioSource _audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Currency"))
        {
            Object.Destroy(other.gameObject);
            m_currencyCount++;

            PlayPickupSound();

            if (m_currencyCount > _maxCurrency)
                m_currencyCount = _maxCurrency;

            _currencyDisplay.text = m_currencyCount.ToString();
        }
    }

    public int GetCurrencyCount()
    {
        return m_currencyCount;
    }

    public bool Buy(Shop.item item)
    {
        if (m_currencyCount >= item.price)
        {
            m_currencyCount -= item.price;
            _currencyDisplay.text = m_currencyCount.ToString();
            Debug.Log($"Bought {item.name} for ${item.price}");
            return true;
        }
        else
        {
            Debug.Log($"Not enough currency to buy {item.name}. Current currency: ${m_currencyCount}, Item price: ${item.price}");
            return false;
        }
    }

    void Update()
    {
        if (coinstack != null)
        {
            float targetScale = coinToScale * m_currencyCount;
            coinstack.localScale = Vector3.Lerp(coinstack.localScale, new Vector3(1, targetScale, 1), Time.deltaTime * 5f);
        }
    }

    private void PlayPickupSound()
    {
        if (_pickupClip == null || _audioSource == null)
        {
            return;
        }

        _audioSource.pitch = Random.Range(_pickupPitchRange.x, _pickupPitchRange.y);
        _audioSource.PlayOneShot(_pickupClip);
        _audioSource.pitch = 1f;
    }
}