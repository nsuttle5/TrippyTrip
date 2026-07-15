using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseScreenUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text milesTraveledText;
    [SerializeField] private string unitSuffix = " mi";
    [SerializeField] private int decimalPlaces = 1;
    [SerializeField] private Button mainMenuButton;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (milesTraveledText != null)
        {
            milesTraveledText.text = RunResults.MilesTraveled.ToString($"F{decimalPlaces}") + unitSuffix;
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}