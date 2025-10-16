
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUIController : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private Image background;
    [SerializeField] private Image border;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text descText;

    [Header("Visuals")]
    [SerializeField] private Color selectedBorder = Color.yellow;
    [SerializeField] private Color normalBorder;

    public CardInstance Instance { get; private set; }

    private bool isSelected = false;

    public void Initialize(CardInstance instance)
    {
        Instance = instance;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (Instance == null || Instance.Data == null) return;
        nameText.text = Instance.Data.cardName;
        //costText.text = Instance.Data.cost.ToString();
        descText.text = Instance.Data.description;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (BoardManager.Instance == null) return;
        BoardManager.Instance.SelectCard(this);
    }

    public void SetSelectedVisual(bool selected)
    {
        isSelected = selected;
        if (border != null)
            border.color = isSelected ? selectedBorder : normalBorder;
    }
}
