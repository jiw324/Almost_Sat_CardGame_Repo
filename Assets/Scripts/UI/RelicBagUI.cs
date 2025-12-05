using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the Relic Bag UI - displays relics when opened
/// </summary>
public class RelicBagUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject bagPanel;
    [SerializeField] private Button bagIconButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform relicContentArea;
    [SerializeField] private GameObject relicDisplayPrefab;
    
    [Header("Empty State")]
    [SerializeField] private GameObject emptyStatePanel;
    [SerializeField] private TextMeshProUGUI emptyText;

    private RelicBag playerRelicBag;
    private bool isOpen = false;

    private void Start()
    {
        // Initially hide the bag panel
        if (bagPanel != null)
        {
            bagPanel.SetActive(false);
        }

        // Set up button listeners
        if (bagIconButton != null)
        {
            bagIconButton.onClick.AddListener(ToggleBagPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBagPanel);
        }

        // Find or create player's relic bag
        playerRelicBag = FindOrCreateRelicBag();
    }

    private RelicBag FindOrCreateRelicBag()
    {
        RelicBag bag = FindObjectOfType<RelicBag>();
        
        if (bag == null)
        {
            var session = GameSession.Instance;
            if (session != null)
            {
                bag = session.GetComponent<RelicBag>();
                if (bag == null)
                {
                    bag = session.gameObject.AddComponent<RelicBag>();
                    Debug.Log("[RelicBagUI] Created RelicBag on GameSession");
                }
            }
        }
        
        return bag;
    }

    public void ToggleBagPanel()
    {
        if (isOpen)
        {
            CloseBagPanel();
        }
        else
        {
            OpenBagPanel();
        }
    }

    public void OpenBagPanel()
    {
        if (bagPanel == null) return;

        bagPanel.SetActive(true);
        isOpen = true;

        // Refresh the display
        RefreshRelicDisplay();
    }

    public void CloseBagPanel()
    {
        if (bagPanel == null) return;

        bagPanel.SetActive(false);
        isOpen = false;
    }

    private void RefreshRelicDisplay()
    {
        // Clear existing relic displays
        ClearRelicDisplay();

        if (playerRelicBag == null)
        {
            ShowEmptyState("Relic bag not found!");
            return;
        }

        List<RelicData> relics = playerRelicBag.GetAllRelics();

        if (relics == null || relics.Count == 0)
        {
            ShowEmptyState("Your bag is empty.\nCollect relics from Relic nodes!");
            return;
        }

        // Hide empty state
        if (emptyStatePanel != null)
        {
            emptyStatePanel.SetActive(false);
        }

        // Display each relic
        foreach (RelicData relic in relics)
        {
            if (relic == null) continue;

            GameObject relicObj = Instantiate(relicDisplayPrefab, relicContentArea);
            RelicDisplayItem displayItem = relicObj.GetComponent<RelicDisplayItem>();
            
            if (displayItem != null)
            {
                displayItem.SetRelic(relic);
            }
        }

        Debug.Log($"[RelicBagUI] Displayed {relics.Count} relics");
    }

    private void ClearRelicDisplay()
    {
        if (relicContentArea == null) return;

        foreach (Transform child in relicContentArea)
        {
            Destroy(child.gameObject);
        }
    }

    private void ShowEmptyState(string message)
    {
        if (emptyStatePanel != null)
        {
            emptyStatePanel.SetActive(true);
        }

        if (emptyText != null)
        {
            emptyText.text = message;
        }
    }

    private void OnDestroy()
    {
        if (bagIconButton != null)
        {
            bagIconButton.onClick.RemoveListener(ToggleBagPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseBagPanel);
        }
    }

    // Public method to refresh display (call when relics change)
    public void UpdateDisplay()
    {
        if (isOpen)
        {
            RefreshRelicDisplay();
        }
    }
}

