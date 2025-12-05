using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreenManager : MonoBehaviour
{
    private static VictoryScreenManager _instance;

    private GameObject victoryUI;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void ShowVictory()
    {
        if (_instance == null)
        {
            GameObject go = new GameObject("VictoryScreenManager");
            _instance = go.AddComponent<VictoryScreenManager>();
        }

        _instance.InternalShowVictory();
    }

    private void InternalShowVictory()
    {
        if (victoryUI != null)
        {
            victoryUI.SetActive(true);
            return;
        }

        GameObject prefab = Resources.Load<GameObject>("UI/VictoryScreen");
        if (prefab == null)
        {
            Debug.LogError("VictoryScreen prefab not found in Resources/UI/");
            return;
        }

        victoryUI = Instantiate(prefab);
        victoryUI.SetActive(true);

        var button = victoryUI.GetComponentInChildren<UnityEngine.UI.Button>();
        if (button != null)
            button.onClick.AddListener(ReturnToMainMenu);
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
