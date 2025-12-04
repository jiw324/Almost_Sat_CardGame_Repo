using System;
using System.Threading.Tasks;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    [SerializeField] private GameObject overlayPanel;   // MinigameOverlay
    [SerializeField] private Transform minigameContainer;

    private bool isActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        overlayPanel.SetActive(false);
    }

    public async Task<float> StartMinigameAsync(GameObject minigamePrefab)
    {
        if (isActive) return 0f;
        isActive = true;
        overlayPanel.SetActive(true);

        GameObject instance = Instantiate(minigamePrefab, minigameContainer);
        var controller = instance.GetComponent<IMinigame>();

        float result = await controller.PlayAsync();

        EndMinigame(instance);
        return result;
    }

    /// <summary>
    /// Starts a minigame with a configuration callback before playing.
    /// </summary>
    public async Task<float> StartMinigameAsync(GameObject minigamePrefab, System.Action<GameObject> configureCallback)
    {
        if (isActive) return 0f;
        isActive = true;
        overlayPanel.SetActive(true);

        GameObject instance = Instantiate(minigamePrefab, minigameContainer);
        
        // Allow configuration before playing
        configureCallback?.Invoke(instance);
        
        var controller = instance.GetComponent<IMinigame>();

        float result = await controller.PlayAsync();

        EndMinigame(instance);
        return result;
    }

    private void EndMinigame(GameObject instance)
    {
        Destroy(instance);
        overlayPanel.SetActive(false);
        isActive = false;
    }
}
