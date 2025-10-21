
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUIController : MonoBehaviour, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler
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
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverHeight = 50;

    public CardInstance Instance { get; private set; }

    private bool isSelected = false;
    private bool isHovered = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private float animationSpeed = 8f;

    public void Initialize(CardInstance instance)
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
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

    private void Update()
    {
        // Smooth scaling when hovered
        Vector3 targetScale = isHovered ? originalScale * hoverScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);

        Vector3 currentPos = transform.localPosition;
        float targetY = isHovered
            ? originalPosition.y + hoverHeight
            : originalPosition.y;
        float newY = Mathf.Lerp(currentPos.y, targetY, Time.deltaTime * animationSpeed);
        transform.localPosition = new Vector3(currentPos.x, newY, currentPos.z);

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        // Only deselect if this card is currently selected
        //if (BoardManager.Instance != null && BoardManager.Instance.IsSelectedCard(this))
          //  BoardManager.Instance.DeselectCard();
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
