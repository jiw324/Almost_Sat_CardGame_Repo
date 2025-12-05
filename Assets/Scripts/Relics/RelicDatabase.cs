using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Loads all relic definitions from a JSON file in StreamingAssets and
/// provides random selection for loot nodes.
/// </summary>
public class RelicDatabase : MonoBehaviour
{
    public static RelicDatabase Instance { get; private set; }

    private readonly List<RelicJSON> _relics = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;

        var go = new GameObject("RelicDatabase");
        Instance = go.AddComponent<RelicDatabase>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadRelics();
    }

    private void LoadRelics()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "relics.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[RelicDatabase] No relics.json found at {path}");
            return;
        }

        string json = File.ReadAllText(path);
        RelicJSONWrapper wrapper = JsonUtility.FromJson<RelicJSONWrapper>(json);
        if (wrapper?.relics == null)
        {
            Debug.LogWarning("[RelicDatabase] relics.json had no relics array.");
            return;
        }

        _relics.Clear();
        _relics.AddRange(wrapper.relics);

        Debug.Log($"[RelicDatabase] Loaded {_relics.Count} relics from JSON.");
    }

    public bool TryGetRelicById(string id, out RelicJSON relic)
    {
        relic = null;
        if (string.IsNullOrWhiteSpace(id))
            return false;

        for (int i = 0; i < _relics.Count; i++)
        {
            if (string.Equals(_relics[i].id, id, System.StringComparison.OrdinalIgnoreCase))
            {
                relic = _relics[i];
                return true;
            }
        }

        return false;
    }

    public bool TryGetRandomRelic(out RelicJSON relic)
    {
        relic = null;
        if (_relics.Count == 0)
            return false;

        int index = Random.Range(0, _relics.Count);
        relic = _relics[index];
        return true;
    }

    /// <summary>
    /// Convert RelicJSON definition into a RelicData ScriptableObject instance at runtime.
    /// Icon is loaded via Resources using iconResourcePath.
    /// </summary>
    public RelicData CreateRuntimeRelicData(RelicJSON json)
    {
        if (json == null)
            return null;

        RelicData data = ScriptableObject.CreateInstance<RelicData>();
        data.relicId = json.id;
        data.relicName = json.name;
        data.description = json.description;
        data.rarity = json.rarity;
        data.effectType = json.effectType;
        data.effectValue = json.effectValue;

        if (!string.IsNullOrWhiteSpace(json.iconResourcePath))
        {
            data.icon = Resources.Load<Sprite>(json.iconResourcePath);
        }

        return data;
    }
}


