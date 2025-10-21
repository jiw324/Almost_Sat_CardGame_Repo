using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Card3DController : MonoBehaviour
{
    [Header("Canvas References (inside the 3D card prefab)")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text descText;

    public CardInstance Instance { get; private set; }

    // Called by BoardSlot.PlaceCard()
    public void Initialize(CardInstance instance)
    {
        Instance = instance;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (Instance == null || Instance.Data == null)
        {
            Debug.LogWarning("[Card3DController] Missing card data.");
            return;
        }

        if (nameText) nameText.text = Instance.Data.cardName;
        if (costText) costText.text = Instance.Data.cost.ToString();
        if (descText) descText.text = Instance.Data.description;
    }
}
