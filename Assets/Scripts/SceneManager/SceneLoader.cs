using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    [SerializeField] private RouteConfig routeConfig;
    private string currentModulePath;
    private bool isTransitioning;
    [SerializeField] private FadeOverlay fadeOverlay;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Go(GameRoute.MainMenu);
    }

    public void Go(GameRoute gameRoute)
    {
        if (isTransitioning)
        {
            return;
        }

        bool isValidRoute = ValidateGameRoute(gameRoute);
        if(!isValidRoute)
        {
            return;
        }

        if(!routeConfig.GetScenePath(gameRoute, out var nextScenePath)) {
            Debug.LogError($"No scene path found for route {gameRoute}");
            return;
        }

        if (!string.IsNullOrEmpty(currentModulePath) && currentModulePath == nextScenePath)
        {
            return;
        }

        StartCoroutine(SwitchSceneRoutine(nextScenePath));
    }

    private bool ValidateGameRoute(GameRoute gameRoute)
    {
        return true;
    }

    IEnumerator SwitchSceneRoutine(string nextPath)
    {
        isTransitioning = true;

        yield return fadeOverlay.FadeIn();

        var loadOp = UnitySceneManager.LoadSceneAsync(nextPath, LoadSceneMode.Additive);
        if (loadOp == null) { 
            isTransitioning = false; 
            yield break; 
        }
        yield return loadOp;

        var nextScene = UnitySceneManager.GetSceneByPath(nextPath);
        if (nextScene.IsValid())
        {
            UnitySceneManager.SetActiveScene(nextScene); 
        }

        if (!string.IsNullOrEmpty(currentModulePath))
        {
            var unloadOp = UnitySceneManager.UnloadSceneAsync(currentModulePath);
            if (unloadOp != null) yield return unloadOp;
        }
        currentModulePath = nextPath;
        yield return fadeOverlay.FadeOut();
        isTransitioning = false;
    }
}
