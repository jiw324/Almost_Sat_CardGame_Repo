using TMPro;
using UnityEngine;

public class AvatarUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;

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
}
