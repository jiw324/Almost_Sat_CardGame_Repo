using System.Collections;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float duration = 1f;
    [SerializeField] private float floatDistance = 1f; // World units
    [SerializeField] private AnimationCurve floatCurve;
    [SerializeField] private AnimationCurve fadeCurve;

    private TMP_Text damageText;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float startTime;
    private Camera mainCamera;
    private bool shouldFloat = true;

    private void Awake()
    {
        // Initialize animation curves if not set in Inspector
        if (floatCurve == null || floatCurve.keys.Length == 0)
        {
            floatCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            floatCurve.preWrapMode = WrapMode.Clamp;
            floatCurve.postWrapMode = WrapMode.Clamp;
        }

        if (fadeCurve == null || fadeCurve.keys.Length == 0)
        {
            fadeCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
            fadeCurve.preWrapMode = WrapMode.Clamp;
            fadeCurve.postWrapMode = WrapMode.Clamp;
        }

        // Get or find camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }

        // Try to find existing TMP_Text
        damageText = GetComponentInChildren<TMP_Text>();
        
        // If not found, create one
        if (damageText == null)
        {
            GameObject textObj = new GameObject("DamageText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = Vector3.zero;
            textObj.transform.localScale = Vector3.one;
            
            damageText = textObj.AddComponent<TextMeshProUGUI>();
            damageText.alignment = TextAlignmentOptions.Center;
            damageText.fontSize = 36;
            damageText.color = Color.red;
        }
    }

    public void Initialize(int damage, Vector3 worldPosition, bool floatUpward = true)
    {
        if (damageText == null)
        {
            Debug.LogError("[DamagePopup] Cannot initialize: damageText is null!");
            return;
        }

        // Set text
        damageText.text = $"-{damage}";
        damageText.color = Color.red;

        // Ensure we have a Canvas component set to World Space
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100; // High sorting order to appear on top

        // Preserve the prefab's rotation before modifying anything
        Quaternion preservedRotation = transform.rotation;

        // Set up the canvas scale (World Space canvases need a scale to be visible)
        // Typical scale for world space UI is around 0.001 to 0.01
        transform.localScale = Vector3.one * 0.01f;

        // Position at world position
        transform.position = worldPosition;
        
        // Restore the preserved rotation (set in prefab to face camera correctly)
        transform.rotation = preservedRotation;
        
        // Offset upward slightly so it appears above the entity
        transform.position += Vector3.up * 0.5f;
        
        // Add random Z rotation (-15 to 15 degrees) for visual variety
        float randomZRotation = Random.Range(-15f, 15f);
        transform.Rotate(0, 0, randomZRotation, Space.Self);
        
        // Preserve the prefab's rotation (don't reset it)
        // The rotation should be set in the prefab to face the camera correctly

        // Store whether to float or stay in place
        shouldFloat = floatUpward;
        
        // Store start position
        startPosition = transform.position;
        targetPosition = startPosition + Vector3.up * floatDistance;

        // Start animation
        startTime = Time.time;
        StartCoroutine(AnimatePopup());
    }

    private IEnumerator AnimatePopup()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed = Time.time - startTime;
            float t = elapsed / duration;

            // Float upward (only if enabled)
            if (shouldFloat)
            {
                float floatT = floatCurve.Evaluate(t);
                transform.position = Vector3.Lerp(startPosition, targetPosition, floatT);
            }
            else
            {
                // Stay in place
                transform.position = startPosition;
            }

            // Fade out
            float alpha = fadeCurve.Evaluate(t);
            Color color = damageText.color;
            color.a = alpha;
            damageText.color = color;

            yield return null;
        }

        // Destroy after animation
        Destroy(gameObject);
    }
}

