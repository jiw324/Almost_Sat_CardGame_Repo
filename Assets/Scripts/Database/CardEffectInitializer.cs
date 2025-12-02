using UnityEngine;

public class CardEffectInitializer : MonoBehaviour
{
    [Header("Assign Effect ScriptableObjects")]
    [SerializeField] private DamageEnemyEffect damageEnemyEffect;
    [SerializeField] private AOEDamageEffect aoeDamageEffect;
    [SerializeField] private GainShieldEffect gainShieldEffect;

    private void Awake()
    {
        // Register all effect assets with their string IDs
        CardEffectLibrary.RegisterEffect("damage_enemy", damageEnemyEffect);
        CardEffectLibrary.RegisterEffect("aoe_damage", aoeDamageEffect);
        CardEffectLibrary.RegisterEffect("gain_shield", gainShieldEffect);
    }
}
