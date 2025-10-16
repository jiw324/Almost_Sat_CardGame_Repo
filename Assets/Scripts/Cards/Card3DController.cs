using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Card3DController : MonoBehaviour
{
    [Header("Canvas References (inside the 3D card prefab)")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Image borderImage;  // optional visual highlight

    [Header("Behavior")]
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private float lookSpeed = 5f;

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

        // Optional: change border color for ranged vs melee
        if (borderImage)
            borderImage.color = Instance.Data.isRanged ? Color.cyan : Color.red;
    }

    private void Update()
    {
        /* Optional billboard effect: make the card face the camera
        if (faceCamera && Camera.main != null)
        {
            Quaternion targetRot = Quaternion.LookRotation(
                transform.position - Camera.main.transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, Time.deltaTime * lookSpeed);
        }
        */
    }
}
