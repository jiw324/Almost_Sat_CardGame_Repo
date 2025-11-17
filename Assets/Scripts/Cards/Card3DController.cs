using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Card3DController : MonoBehaviour
{
    [Header("Canvas References (inside the 3D card prefab)")]
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text descText;

    public CardInstance Instance { get; private set; }

    // Called by BoardSlot.PlaceCard()
    public void Initialize(CardInstance instance)
    {
        Instance = instance;
        name = instance.Data.name;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (Instance == null || Instance.Data == null)
        {
            Debug.LogWarning("[Card3DController] Missing card data.");
            return;
        }

        if (nameText != null)
            nameText.text = Instance.Data.cardName;
        
        if (costText != null)
            costText.text = Instance.Data.cost.ToString();
        
        if (damageText != null)
        {
            damageText.text = Instance.Data.damage > 0 ? Instance.Data.damage.ToString() : "";
            damageText.gameObject.SetActive(Instance.Data.damage > 0);
        }
        
        if (descText != null)
            descText.text = Instance.Data.description;
        
        // Set artwork image
        if (artworkImage != null)
        {
            artworkImage.sprite = Instance.Data.artwork;
            artworkImage.enabled = Instance.Data.artwork != null;
        }
    }
}
