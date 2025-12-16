using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvatarUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private Image portraitImage;

    [Header("Status Effect Icons")]
    [SerializeField] private GameObject strengthIcon;
    [SerializeField] private GameObject weaknessIcon;
    [SerializeField] private GameObject poisonIcon;

    public void SetHealth(int value)
    {
        if (healthText != null)
            healthText.text = value.ToString();
    }

    public void SetMana(int value)
    {
        if (manaText != null)
            manaText.text = value.ToString();
    }

    public void SetPortrait(Sprite portrait)
    {
        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
        }
    }

    /// <summary>
    /// Update status effect icons for the avatar (player or enemy)
    /// </summary>
    public void UpdateStatusIcons(EntityBase entity)
    {
        if (entity == null)
        {
            if (strengthIcon != null) strengthIcon.SetActive(false);
            if (weaknessIcon != null) weaknessIcon.SetActive(false);
            if (poisonIcon != null) poisonIcon.SetActive(false);
            return;
        }

        if (strengthIcon != null)
            strengthIcon.SetActive(entity.HasStrength);

        if (weaknessIcon != null)
            weaknessIcon.SetActive(entity.HasWeakness);

        if (poisonIcon != null)
            poisonIcon.SetActive(entity.PoisonStacks > 0);
    }
}