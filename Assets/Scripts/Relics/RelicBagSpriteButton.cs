using UnityEngine;

/// <summary>
/// World-space sprite button that always sits near the bottom-right of the main camera.
/// Clicking it toggles a linked popup panel (typically a UI panel on a Canvas).
/// </summary>
public class RelicBagSpriteButton : MonoBehaviour
{
    [Tooltip("Panel GameObject that will be shown/hidden when this sprite is clicked.")]
    public GameObject panelToToggle;

    [Header("Screen Position (Viewport)")]
    [Range(0f, 1f)] public float viewportX = 0.9f;
    [Range(0f, 1f)] public float viewportY = 0.1f;

    [Header("Depth From Camera")]
    [Tooltip("How far in front of the camera to place the sprite (world units).")]
    public float distanceFromCamera = 5f;

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;

        // Ensure there is a collider so OnMouseDown works
        var collider = GetComponent<Collider>();
        if (collider == null)
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                var box = gameObject.AddComponent<BoxCollider>();
                // Match collider size roughly to sprite bounds
                box.size = spriteRenderer.bounds.size;
            }
            else
            {
                gameObject.AddComponent<BoxCollider>();
            }
        }
    }

    private void LateUpdate()
    {
        if (_cam == null)
        {
            _cam = Camera.main;
            if (_cam == null) return;
        }

        // Position this sprite relative to the camera's viewport (bottom-right-ish)
        var viewportPos = new Vector3(viewportX, viewportY, distanceFromCamera);
        var worldPos = _cam.ViewportToWorldPoint(viewportPos);
        transform.position = worldPos;
    }

    private void OnMouseDown()
    {
        if (panelToToggle == null)
            return;

        bool isActive = panelToToggle.activeSelf;
        panelToToggle.SetActive(!isActive);
    }
}


