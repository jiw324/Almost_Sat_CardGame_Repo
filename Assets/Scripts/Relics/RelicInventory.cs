using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable inventory for storing relic names (for save/load system)
/// </summary>
[System.Serializable]
public class RelicInventory
{
    public List<string> relicNames = new List<string>();

    public void AddRelic(string relicName)
    {
        if (!string.IsNullOrEmpty(relicName))
        {
            relicNames.Add(relicName);
            Debug.Log($"[RelicInventory] Added {relicName} to inventory");
        }
    }

    public void RemoveRelic(string relicName)
    {
        relicNames.Remove(relicName);
    }

    public bool HasRelic(string relicName)
    {
        return relicNames.Contains(relicName);
    }

    public void Clear()
    {
        relicNames.Clear();
    }

    public int Count => relicNames.Count;
}

