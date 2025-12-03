using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple registry to map minigame IDs to their prefabs.
/// Assign minigame prefabs in the Unity Inspector.
/// </summary>
public class MinigameRegistry : MonoBehaviour
{
    public static MinigameRegistry Instance { get; private set; }

    [System.Serializable]
    public class MinigameEntry
    {
        public string minigameId;
        public GameObject prefab;
    }

    [SerializeField] private List<MinigameEntry> minigames = new List<MinigameEntry>();
    private Dictionary<string, GameObject> minigameDict;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        BuildDictionary();
    }

    private void BuildDictionary()
    {
        minigameDict = new Dictionary<string, GameObject>();
        foreach (var entry in minigames)
        {
            if (!string.IsNullOrEmpty(entry.minigameId) && entry.prefab != null)
            {
                minigameDict[entry.minigameId] = entry.prefab;
            }
        }
    }

    public GameObject GetMinigamePrefab(string minigameId)
    {
        if (string.IsNullOrEmpty(minigameId) || minigameDict == null)
            return null;

        minigameDict.TryGetValue(minigameId, out GameObject prefab);
        return prefab;
    }
}

