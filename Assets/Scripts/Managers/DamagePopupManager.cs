using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [Header("Prefab Reference")]
    [SerializeField] private GameObject damagePopupPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Spawns a damage popup at the given world position.
    /// </summary>
    public void ShowDamagePopup(int damage, Vector3 worldPosition)
    {
        if (damagePopupPrefab == null)
        {
            Debug.LogWarning("[DamagePopupManager] Damage popup prefab is not assigned!");
            return;
        }

        // Instantiate the prefab - this should preserve the prefab's rotation
        GameObject popup = Instantiate(damagePopupPrefab);
        
        DamagePopup popupScript = popup.GetComponent<DamagePopup>();
        if (popupScript == null)
        {
            popupScript = popup.AddComponent<DamagePopup>();
        }

        popupScript.Initialize(damage, worldPosition);
    }

    /// <summary>
    /// Spawns a damage popup at the entity's position (uses attackTargetTransform if available).
    /// Entities don't float - they stay in place so the damage number is visible.
    /// </summary>
    public void ShowDamagePopup(int damage, EntityBase entity)
    {
        if (entity == null)
        {
            Debug.LogWarning("[DamagePopupManager] Cannot show damage popup: entity is null!");
            return;
        }

        Vector3 position;
        if (entity.attackTargetTransform != null)
        {
            position = entity.attackTargetTransform.position;
        }
        else
        {
            position = entity.transform.position;
        }

        // Entities don't float - pass false to disable floating
        ShowDamagePopup(damage, position, false);
    }
    
    /// <summary>
    /// Spawns a damage popup at the given world position with optional floating.
    /// </summary>
    public void ShowDamagePopup(int damage, Vector3 worldPosition, bool floatUpward = true)
    {
        if (damagePopupPrefab == null)
        {
            Debug.LogWarning("[DamagePopupManager] Damage popup prefab is not assigned!");
            return;
        }

        GameObject popup = Instantiate(damagePopupPrefab);
        DamagePopup popupScript = popup.GetComponent<DamagePopup>();
        if (popupScript == null)
        {
            popupScript = popup.AddComponent<DamagePopup>();
        }

        popupScript.Initialize(damage, worldPosition, floatUpward);
    }
}

