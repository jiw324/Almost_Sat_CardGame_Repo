using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MulliganCardOption : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private RectTransform visualRoot;
    [SerializeField] private Toggle selectToggle;
    [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f); // Gold color

    private Image border;
    private Image portraitBorder;
    private Image manaBackground;
    private Image descriptionBackground;
    private Image damageBackground;
    private Image healthBackground;
    private Image typeBackground;
    private Image nameBg;
    private Image nameBgInner;
    private Image nameLight;

    public CardInstance Card { get; private set; }
    public bool IsSelected => selectToggle != null && selectToggle.isOn;

    public event Action<MulliganCardOption, bool> SelectionChanged;

    public void Initialize(CardInstance instance)
    {
        Card = instance;

        CacheBackgroundReferences();

        if (Card?.Data == null)
        {
            Debug.LogWarning("[MulliganCardOption] Tried to initialize with null card instance.");
            return;
        }

        if (nameText != null)
            nameText.text = Card.Data.cardName;

        if (costText != null)
            costText.text = Card.Data.cost.ToString();

        if (damageText != null)
        {
            damageText.text = Card.Data.damage > 0 ? Card.Data.damage.ToString() : "";
            damageText.gameObject.SetActive(Card.Data.damage > 0);
        }

        if (descriptionText != null)
            descriptionText.text = Card.Data.description;

        // Set artwork image
        if (artworkImage != null)
        {
            artworkImage.sprite = Card.Data.artwork;
            artworkImage.enabled = Card.Data.artwork != null;
        }
        if (Card.Data.type == "spell")
        {
            visualRoot.Find("Type").Find("Spell").gameObject.SetActive(true);
        }
        else if (Card.Data.isRanged)
        {
            visualRoot.Find("Type").Find("Ranged").gameObject.SetActive(true);
        }
        else
        {
            visualRoot.Find("Type").Find("Melee").gameObject.SetActive(true);
        }

        if (Card.Data.hasMinigame)
            ApplyGoldColor();

        if (selectToggle != null)
        {
            selectToggle.onValueChanged.RemoveListener(OnToggleChanged);
            selectToggle.SetIsOnWithoutNotify(false);
            selectToggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    private void CacheBackgroundReferences()
    {
        // Find and cache all the background images
        Transform borderTransform = visualRoot.Find("Card Border");
            if (borderTransform != null)
                border = borderTransform.GetComponent<Image>();

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

        // Cache Name elements
        Transform name = visualRoot.Find("Name");
        if (name != null)
        {
            Transform bgTransform = name.Find("Bg");
            if (bgTransform != null)
                nameBg = bgTransform.GetComponent<Image>();

            Transform bgInnerTransform = name.Find("BgInner");
            if (bgInnerTransform != null)
                nameBgInner = bgInnerTransform.GetComponent<Image>();

            Transform lightTransform = name.Find("Light");
            if (lightTransform != null)
                nameLight = lightTransform.GetComponent<Image>();
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

        if (nameBg != null)
            nameBg.color = goldColor;

        if (nameBgInner != null)
            nameBgInner.color = goldColor;

        if (nameLight != null)
            nameLight.color = goldColor;
    }


    public void SetSelectedWithoutNotify(bool selected)
    {
        if (selectToggle == null) return;
        selectToggle.SetIsOnWithoutNotify(selected);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (Card != null && Card.Data != null)
            Debug.Log($"[MulliganCardOption] {(isOn ? "Selected" : "Deselected")} {Card.Data.cardName}");
        SelectionChanged?.Invoke(this, isOn);
    }

    private void OnDestroy()
    {
        if (selectToggle != null)
            selectToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}

