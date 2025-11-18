
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
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private RectTransform visualRoot;

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
        originalScale = visualRoot.localScale;
        originalPosition = visualRoot.localPosition;
        Instance = instance;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (Instance == null || Instance.Data == null) return;
        
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
        if (Instance.Data.type == "spell")
        {
            visualRoot.Find("Type").Find("Spell").gameObject.SetActive(true);
        }
        else if (Instance.Data.isRanged)
        {
            visualRoot.Find("Type").Find("Ranged").gameObject.SetActive(true);
        }
        else
        {
            visualRoot.Find("Type").Find("Melee").gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        // Smooth scaling when hovered
        Vector3 targetScale = isHovered ? originalScale * hoverScale : originalScale;
        visualRoot.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);

        Vector3 currentPos = visualRoot.localPosition;
        float targetY = isHovered
            ? originalPosition.y + hoverHeight
            : originalPosition.y;
        float newY = Mathf.Lerp(currentPos.y, targetY, Time.deltaTime * animationSpeed);
        visualRoot.localPosition = new Vector3(currentPos.x, newY, currentPos.z);

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
