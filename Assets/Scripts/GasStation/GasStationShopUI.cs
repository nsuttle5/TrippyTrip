using System;
using UnityEngine;
using UnityEngine.UI;

public class GasStationShopUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button continueButton;

    private Action _onContinue;
    private bool _initialized;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        if (panelRoot == null) panelRoot = gameObject;

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(HandleContinueClicked);
        }
        else
        {
            Debug.LogWarning("GasStationShopUI: continueButton is not assigned in the Inspector.", this);
        }
    }

    public void Show(Action onContinue)
    {
        EnsureInitialized();
        _onContinue = onContinue;

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GasStationShopUI: panelRoot is missing, cannot show shop UI.", this);
        }
    }

    public void Hide()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void HandleContinueClicked()
    {
        Action callback = _onContinue;
        _onContinue = null;
        callback?.Invoke();
    }
}