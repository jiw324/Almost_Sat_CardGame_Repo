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
    [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f); // Gold color

    public CardInstance Instance { get; private set; }

    private bool isSelected = false;
    private bool isHovered = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private float animationSpeed = 8f;

    // References to background images
    private Image portraitBorder;
    private Image manaBackground;
    private Image descriptionBackground;
    private Image damageBackground;
    private Image healthBackground;
    private Image typeBackground;

    public void Initialize(CardInstance instance)
    {
        originalScale = visualRoot.localScale;
        originalPosition = visualRoot.localPosition;
        Instance = instance;

        // Cache references to background images
        CacheBackgroundReferences();

        UpdateUI();
    }

    private void CacheBackgroundReferences()
    {
        // Find and cache all the background images
        Transform portrait = visualRoot.Find("Portrait");
        if (portrait != null)
        {
            Transform portraitBorderTransform = portrait.Find("Portrait Border");
            if (portraitBorderTransform != null)
                portraitBorder = portraitBorderTransform.GetComponent<Image>();
        }

        Transform mana = visualRoot.Find("Mana");
        if (mana != null)
        {
            Transform manaBackgroundTransform = mana.Find("Mana Background");
            if (manaBackgroundTransform != null)
                manaBackground = manaBackgroundTransform.GetComponent<Image>();
        }

        Transform description = visualRoot.Find("Description");
        if (description != null)
        {
            Transform descBackgroundTransform = description.Find("Description Background");
            if (descBackgroundTransform != null)
                descriptionBackground = descBackgroundTransform.GetComponent<Image>();
        }

        Transform damage = visualRoot.Find("Damage");
        if (damage != null)
        {
            Transform damageBackgroundTransform = damage.Find("Damage Background");
            if (damageBackgroundTransform != null)
                damageBackground = damageBackgroundTransform.GetComponent<Image>();
        }

        Transform health = visualRoot.Find("Health");
        if (health != null)
        {
            Transform healthBackgroundTransform = health.Find("Health Background");
            if (healthBackgroundTransform != null)
                healthBackground = healthBackgroundTransform.GetComponent<Image>();
        }

        Transform type = visualRoot.Find("Type");
        if (type != null)
        {
            Transform typeBackgroundTransform = type.Find("Type Background");
            if (typeBackgroundTransform != null)
                typeBackground = typeBackgroundTransform.GetComponent<Image>();
        }
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

        // Apply gold color if minigamePrefab is not null
        if (Instance.Data.minigamePrefab != null)
        {
            ApplyGoldColor();
        }
    }

    private void ApplyGoldColor()
    {
        if (border != null)
            border.color = goldColor;

        if (portraitBorder != null)
            portraitBorder.color = goldColor;

        if (manaBackground != null)
            manaBackground.color = goldColor;

        if (descriptionBackground != null)
            descriptionBackground.color = goldColor;

        if (damageBackground != null)
            damageBackground.color = goldColor;

        if (healthBackground != null)
            healthBackground.color = goldColor;

        if (typeBackground != null)
            typeBackground.color = goldColor;
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
        {
            // Don't override gold color when selected
            if (Instance != null && Instance.Data != null && Instance.Data.minigamePrefab != null)
            {
                border.color = goldColor;
            }
            else
            {
                border.color = isSelected ? selectedBorder : normalBorder;
            }
        }
    }
}