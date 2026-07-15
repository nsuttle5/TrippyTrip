using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManagement : MonoBehaviour
{
    [SerializeField] private Button loadSceneButton;
    [SerializeField] private string sceneName;

    private void Awake()
    {
        if (loadSceneButton != null)
        {
            loadSceneButton.onClick.AddListener(LoadScene);
        }
    }

    private void OnDestroy()
    {
        if (loadSceneButton != null)
        {
            loadSceneButton.onClick.RemoveListener(LoadScene);
        }
    }

    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneManagement: No scene name assigned.", this);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
