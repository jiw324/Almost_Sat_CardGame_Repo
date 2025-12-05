using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the contents of the Relic Bag popup.
/// When the panel is enabled, it builds a 3 x N grid of the player's current relics.
/// </summary>
public class RelicBagMenu : MonoBehaviour
{
    private const string GridObjectName = "RelicGrid";

    private RectTransform _gridRoot;

    private void Awake()
    {
        EnsureGridExists();
        gameObject.SetActive(false); // start hidden; opened via bag trigger
    }

    private void OnEnable()
    {
        Refresh();
    }

    /// <summary>
    /// Rebuild the grid to reflect the current relics in the GameSession.
    /// </summary>
    public void Refresh()
    {
        if (_gridRoot == null)
            EnsureGridExists();

        // Clear old slots
        for (int i = _gridRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(_gridRoot.GetChild(i).gameObject);
        }

        var session = SessionGrabber.getGameSession();
        if (session == null)
        {
            Debug.LogWarning("[RelicBagMenu] No GameSession found; cannot display relics.");
            return;
        }

        IReadOnlyList<RelicData> relics = session.GetPlayerRelics();
        if (relics == null || relics.Count == 0)
        {
            // Nothing to show; you can add a "No relics" label here later if you like.
            Debug.Log("[RelicBagMenu] Relic bag is empty.");
            return;
        }

        foreach (var relic in relics)
        {
            CreateRelicSlot(relic);
        }

        LogRelicsToConsole(relics);
    }

    private void EnsureGridExists()
    {
        if (_gridRoot != null)
            return;

        // Look for an existing grid child first.
        var existing = transform.Find(GridObjectName);
        if (existing != null)
        {
            _gridRoot = existing as RectTransform;
        }
        else
        {
            GameObject gridGO = new GameObject(GridObjectName, typeof(RectTransform));
            gridGO.transform.SetParent(transform, false);
            _gridRoot = gridGO.GetComponent<RectTransform>();

            _gridRoot.anchorMin = new Vector2(0f, 0f);
            _gridRoot.anchorMax = new Vector2(1f, 1f);
            _gridRoot.offsetMin = new Vector2(20f, 20f);
            _gridRoot.offsetMax = new Vector2(-20f, -20f);

            var layout = gridGO.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(150f, 150f);
            layout.spacing = new Vector2(10f, 10f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3; // 3 x N grid
        }
    }

    private void CreateRelicSlot(RelicData relic)
    {
        GameObject slot = new GameObject(relic != null ? $"RelicSlot_{relic.relicId}" : "RelicSlot", typeof(RectTransform));
        slot.transform.SetParent(_gridRoot, false);

        RectTransform slotRect = slot.GetComponent<RectTransform>();
        slotRect.localScale = Vector3.one;

        Image bg = slot.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        // Icon
        GameObject iconGO = new GameObject("Icon", typeof(RectTransform));
        iconGO.transform.SetParent(slot.transform, false);
        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.3f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        Image iconImage = iconGO.AddComponent<Image>();
        if (relic != null && relic.icon != null)
        {
            iconImage.sprite = relic.icon;
            iconImage.preserveAspect = true;
        }

        // Name text
        GameObject textGO = new GameObject("Name", typeof(RectTransform));
        textGO.transform.SetParent(slot.transform, false);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0f);
        textRect.anchorMax = new Vector2(0.9f, 0.3f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text nameText = textGO.AddComponent<Text>();
        nameText.text = relic != null ? relic.relicName : "Unknown Relic";
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.color = Color.white;
        nameText.fontSize = 18;
        nameText.raycastTarget = false;
        if (nameText.font == null)
        {
            nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }

    /// <summary>
    /// Helper to print all relics currently shown in the bag to the Console.
    /// </summary>
    private void LogRelicsToConsole(IReadOnlyList<RelicData> relics)
    {
        if (relics == null || relics.Count == 0)
        {
            Debug.Log("[RelicBagMenu] Relic bag is empty.");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("[RelicBagMenu] Relics in bag:");
        for (int i = 0; i < relics.Count; i++)
        {
            RelicData r = relics[i];
            if (r == null)
            {
                sb.AppendLine($"  {i + 1}. (null relic)");
            }
            else
            {
                sb.AppendLine($"  {i + 1}. id={r.relicId}, name={r.relicName}, rarity={r.rarity}, effectType={r.effectType}");
            }
        }

        Debug.Log(sb.ToString());
    }
}


