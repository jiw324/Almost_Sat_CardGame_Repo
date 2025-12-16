using System.Collections;
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
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Image grayFilm;
    [SerializeField] private RectTransform visualRoot;

    [Header("Status Effect Icons")]
    [SerializeField] private GameObject strengthIcon;
    [SerializeField] private GameObject weaknessIcon;
    [SerializeField] private GameObject poisonIcon;

    [Header("Card Play Animation")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Vector3 targetScale;

    public CardInstance Instance { get; private set; }
    public Vector3 TargetScale => targetScale;

    // Called by BoardSlot.PlaceCard()
    public void Initialize(CardInstance instance)
    {
        Instance = instance;
        name = instance.Data.name;
        UpdateVisuals();

        // Start animation
        StartCoroutine(PlayCardAnimation());
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
            visualRoot.Find("Damage").gameObject.SetActive(false);
            visualRoot.Find("Health").gameObject.SetActive(false);
        }

        UpdateStats();
        UpdateStatusIcons();
    }

    public void UpdateStats()
    {
        if (Instance == null) return;

        if (healthText != null)
        {
            healthText.text = Instance.CurrentHP.ToString();
        }

        if (damageText != null)
        {
            damageText.text = Instance.Attack.ToString();
        }
    }

    /// <summary>
    /// Update status effect icons based on current status effects
    /// </summary>
    public void UpdateStatusIcons()
    {
        if (Instance == null)
        {
            if (strengthIcon != null) strengthIcon.SetActive(false);
            if (weaknessIcon != null) weaknessIcon.SetActive(false);
            if (poisonIcon != null) poisonIcon.SetActive(false);
            return;
        }

        EntityBase owner = Instance.Owner;

        // Update Strength icon
        if (strengthIcon != null)
        {
            strengthIcon.SetActive(owner.HasStrength);
        }

        // Update Weakness icon
        if (weaknessIcon != null)
        {
            weaknessIcon.SetActive(owner.HasWeakness);
        }

        // Update Poison icon and stack count
        if (poisonIcon != null)
        {
            bool hasPoison = owner.PoisonStacks > 0;
            poisonIcon.SetActive(hasPoison);
        }
    }

    public void SetCanActVisual(bool canAct)
    {
        if (grayFilm != null)
        {
            grayFilm.gameObject.SetActive(!canAct); // Film is visible when canAct is FALSE
        }
    }

    /// <summary>
    /// Simple scale-up animation when card is placed
    /// </summary>
    private IEnumerator PlayCardAnimation()
    {
        // Use targetScale if set, otherwise use the prefab's current scale
        Vector3 finalScale;
        if (targetScale.magnitude > 0.01f)
        {
            finalScale = targetScale;
        }
        else
        {
            // Capture the prefab's scale as the target
            finalScale = transform.localScale;
            targetScale = finalScale; // Store it for future reference
        }

        // Start from small scale (10% of target)
        Vector3 startScale = finalScale * 0.1f;
        transform.localScale = startScale;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            // Simple ease-out curve
            t = 1f - (1f - t) * (1f - t);

            transform.localScale = Vector3.Lerp(startScale, finalScale, t);

            yield return null;
        }

        // Ensure final scale
        transform.localScale = finalScale;
    }
}