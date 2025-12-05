using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A bag/inventory system for storing relics
/// </summary>
public class RelicBag : MonoBehaviour
{
    [SerializeField] private List<RelicData> relics = new List<RelicData>();
    [SerializeField] private int maxRelics = 10; // Optional capacity limit

    /// <summary>
    /// Adds a relic to the bag
    /// </summary>
    public bool AddRelic(RelicData relic)
    {
        if (relic == null)
        {
            Debug.LogWarning("[RelicBag] Cannot add null relic");
            return false;
        }

        if (relics.Count >= maxRelics)
        {
            Debug.LogWarning("[RelicBag] Bag is full!");
            return false;
        }

        relics.Add(relic);
        Debug.Log($"[RelicBag] Added {relic.relicName} to bag");
        
        // Save to session inventory
        SaveToSession(relic);
        
        return true;
    }
    
    private void SaveToSession(RelicData relic)
    {
        var session = GameSession.Instance;
        if (session != null && session.gameSessionData != null)
        {
            session.gameSessionData.sessionPlayerData.relicInventory.AddRelic(relic.relicName);
        }
    }

    /// <summary>
    /// Removes a relic from the bag
    /// </summary>
    public bool RemoveRelic(RelicData relic)
    {
        if (relics.Contains(relic))
        {
            relics.Remove(relic);
            Debug.Log($"[RelicBag] Removed {relic.relicName} from bag");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the bag contains a specific relic
    /// </summary>
    public bool HasRelic(RelicData relic)
    {
        return relics.Contains(relic);
    }

    /// <summary>
    /// Gets all relics in the bag
    /// </summary>
    public List<RelicData> GetAllRelics()
    {
        return new List<RelicData>(relics);
    }

    /// <summary>
    /// Gets a relic at a specific index
    /// </summary>
    public RelicData GetRelicAt(int index)
    {
        if (index >= 0 && index < relics.Count)
        {
            return relics[index];
        }
        return null;
    }

    /// <summary>
    /// Gets the number of relics in the bag
    /// </summary>
    public int GetRelicCount()
    {
        return relics.Count;
    }

    /// <summary>
    /// Checks if the bag is full
    /// </summary>
    public bool IsFull()
    {
        return relics.Count >= maxRelics;
    }

    /// <summary>
    /// Checks if the bag is empty
    /// </summary>
    public bool IsEmpty()
    {
        return relics.Count == 0;
    }

    /// <summary>
    /// Clears all relics from the bag
    /// </summary>
    public void ClearBag()
    {
        relics.Clear();
        Debug.Log("[RelicBag] Cleared all relics from bag");
    }

    /// <summary>
    /// Prints all relics in the bag to console
    /// </summary>
    public void PrintBag()
    {
        Debug.Log($"[RelicBag] Contains {relics.Count} relics:");
        foreach (var relic in relics)
        {
            Debug.Log($"  - {relic.relicName}: {relic.description}");
        }
    }
}

