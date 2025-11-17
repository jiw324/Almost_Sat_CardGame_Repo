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
    [SerializeField] private Toggle selectToggle;

    public CardInstance Card { get; private set; }
    public bool IsSelected => selectToggle != null && selectToggle.isOn;

    public event Action<MulliganCardOption, bool> SelectionChanged;

    public void Initialize(CardInstance instance)
    {
        Card = instance;

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

        if (selectToggle != null)
        {
            selectToggle.onValueChanged.RemoveListener(OnToggleChanged);
            selectToggle.SetIsOnWithoutNotify(false);
            selectToggle.onValueChanged.AddListener(OnToggleChanged);
        }
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

