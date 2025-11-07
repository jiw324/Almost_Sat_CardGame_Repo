using UnityEngine;
using System;

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

    public void StartMinigame(GameObject minigamePrefab, Action<float> onComplete)
    {
        if (isActive) return;
        isActive = true;
        overlayPanel.SetActive(true);

        GameObject instance = Instantiate(minigamePrefab, minigameContainer);
        var controller = instance.GetComponent<IMinigame>();

        controller.Initialize(result =>
        {
            onComplete?.Invoke(result);
            EndMinigame(instance);
        });
    }

    private void EndMinigame(GameObject instance)
    {
        Destroy(instance);
        overlayPanel.SetActive(false);
        isActive = false;
    }
}
