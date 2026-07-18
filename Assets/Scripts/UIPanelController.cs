using UnityEngine;

public class UIPanelController : MonoBehaviour
{
    [SerializeField] private GameObject rulesPanel;
    [SerializeField] private GameObject creditsPanel;

    private void Start()
    {
        // Optional: make sure the panel starts hidden.
        rulesPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void OpenRulesPanel()
    {
        rulesPanel.SetActive(true);
    }

    public void CloseRulesPanel()
    {
        rulesPanel.SetActive(false);
    }

    public void OpenCreditsPanel()
    {
        creditsPanel.SetActive(true);
    }

    public void CloseCreditsPanel()
    {
        creditsPanel.SetActive(false);
    }
}