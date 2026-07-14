
using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField]
    private int _maxCurrency = 999;

    [SerializeField]
    private TextMeshProUGUI _currencyDisplay;

    private int m_currencyCount = 0;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Currency"))
        {
            Object.Destroy(other.gameObject);
            m_currencyCount++;

            if (m_currencyCount > _maxCurrency)
                m_currencyCount = _maxCurrency;

            _currencyDisplay.text = m_currencyCount.ToString();
        }
    }

    public int GetCurrencyCount()
    {
        return m_currencyCount;
    }
}