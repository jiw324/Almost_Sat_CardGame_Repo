using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays a single relic in the bag UI
/// </summary>
public class RelicDisplayItem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image relicIcon;
    [SerializeField] private TextMeshProUGUI relicNameText;
    [SerializeField] private TextMeshProUGUI relicDescriptionText;
    [SerializeField] private TextMeshProUGUI effectValueText;

    private RelicData relicData;

    public void SetRelic(RelicData relic)
    {
        if (relic == null)
        {
            Debug.LogWarning("[RelicDisplayItem] Attempted to set null relic");
            return;
        }

        relicData = relic;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (relicData == null) return;

        // Set relic icon
        if (relicIcon != null)
        {
            if (relicData.icon != null)
            {
                relicIcon.sprite = relicData.icon;
                relicIcon.enabled = true;
            }
            else
            {
                relicIcon.enabled = false;
            }
        }

        // Set relic name
        if (relicNameText != null)
        {
            relicNameText.text = relicData.relicName;
        }

        // Set description
        if (relicDescriptionText != null)
        {
            relicDescriptionText.text = relicData.description;
        }

        // Set effect value (if needed)
        if (effectValueText != null)
        {
            if (relicData.effectValue > 0)
            {
                effectValueText.text = $"+{relicData.effectValue}";
            }
            else
            {
                effectValueText.text = "";
            }
        }
    }
}

