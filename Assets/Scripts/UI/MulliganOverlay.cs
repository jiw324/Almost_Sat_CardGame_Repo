using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MulliganOverlay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject root;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private MulliganCardOption cardOptionPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private TextMeshProUGUI mulliganText;

    private readonly List<MulliganCardOption> activeOptions = new();
    private Action<List<CardInstance>, List<CardInstance>> onComplete;
    private int requiredSelectionCount;
    private int currentSelectionCount;

    private void Awake()
    {
        HideImmediate();
    }

    public void Show(IReadOnlyList<CardInstance> cards, int requiredSelection, Action<List<CardInstance>, List<CardInstance>> onCompleteCallback)
    {
        if (cards == null || cards.Count == 0)
        {
            Debug.LogWarning("[MulliganOverlay] No cards provided for mulligan.");
            onCompleteCallback?.Invoke(new List<CardInstance>(), new List<CardInstance>());
            return;
        }

        requiredSelectionCount = Mathf.Clamp(requiredSelection, 0, cards.Count);
        currentSelectionCount = 0;
        onComplete = onCompleteCallback;
        ClearOptions();

        foreach (var card in cards)
        {
            var option = Instantiate(cardOptionPrefab, cardContainer);
            option.Initialize(card);
            option.SelectionChanged += OnOptionSelectionChanged;
            activeOptions.Add(option);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(OnConfirm);
            confirmButton.onClick.AddListener(OnConfirm);
            UpdateConfirmButtonState();
        }

        if (mulliganText != null)
            mulliganText.text = $"Choose {requiredSelection} extra cards to start with!";

        SetRootActive(true);
    }

    private void OnConfirm()
    {
        if (requiredSelectionCount > 0 && currentSelectionCount != requiredSelectionCount)
        {
            Debug.LogWarning($"[MulliganOverlay] Cannot confirm mulligan until exactly {requiredSelectionCount} cards are selected.");
            return;
        }

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnConfirm);

        var keep = new List<CardInstance>();
        var discard = new List<CardInstance>();

        foreach (var option in activeOptions)
        {
            if (option == null || option.Card == null)
                continue;

            if (option.IsSelected)
                keep.Add(option.Card);
            else
                discard.Add(option.Card);
        }

        onComplete?.Invoke(keep, discard);
        onComplete = null;

        SetRootActive(false);
        ClearOptions();
        turnManager.OnMulliganFinished();
    }

    public void HideImmediate()
    {
        SetRootActive(false);
        ClearOptions();
    }

    private void SetRootActive(bool isActive)
    {
        if (root != null)
            root.SetActive(isActive);
        else
            gameObject.SetActive(isActive);
    }

    private void ClearOptions()
    {
        foreach (var option in activeOptions)
        {
            if (option != null)
            {
                option.SelectionChanged -= OnOptionSelectionChanged;
                Destroy(option.gameObject);
            }
        }
        activeOptions.Clear();
    }

    private void OnOptionSelectionChanged(MulliganCardOption option, bool isSelected)
    {
        if (option == null) return;

        if (isSelected)
        {
            if (currentSelectionCount >= requiredSelectionCount && requiredSelectionCount > 0)
            {
                option.SetSelectedWithoutNotify(false);
                Debug.LogWarning($"[MulliganOverlay] You can only select {requiredSelectionCount} cards.");
                return;
            }
            currentSelectionCount++;
        }
        else
        {
            currentSelectionCount = Mathf.Max(0, currentSelectionCount - 1);
        }

        UpdateConfirmButtonState();
    }

    private void UpdateConfirmButtonState()
    {
        if (confirmButton == null)
            return;

        confirmButton.interactable = requiredSelectionCount == 0 || currentSelectionCount == requiredSelectionCount;
    }
}
