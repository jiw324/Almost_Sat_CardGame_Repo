using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvatarUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private Image portraitImage;

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
}
