using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MulliganCardOption : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private RectTransform visualRoot;
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

        if (descriptionText != null)
            descriptionText.text = Card.Data.description;

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

