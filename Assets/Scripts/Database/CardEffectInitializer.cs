using UnityEngine;

public class CardEffectInitializer : MonoBehaviour
{
    [Header("Assign Effect ScriptableObjects")]
    [SerializeField] private DamageEnemyEffect damageEnemyEffect;
    [SerializeField] private AOEDamageEffect aoeDamageEffect;
    [SerializeField] private GainShieldEffect gainShieldEffect;
    [SerializeField] private RunicBlastEffect runicBlastEffect;
    [SerializeField] private StrengthEffect strengthEffect;
    [SerializeField] private WeaknessEffect weaknessEffect;
    [SerializeField] private PoisonEffect poisonEffect;

    private void Awake()
    {
        CardEffectLibrary.RegisterEffect("damage_enemy", damageEnemyEffect);
        CardEffectLibrary.RegisterEffect("aoe_damage", aoeDamageEffect);
        CardEffectLibrary.RegisterEffect("gain_shield", gainShieldEffect);
        CardEffectLibrary.RegisterEffect("runic_blast", runicBlastEffect);
        CardEffectLibrary.RegisterEffect("strength", strengthEffect);
        CardEffectLibrary.RegisterEffect("weakness", weaknessEffect);
        CardEffectLibrary.RegisterEffect("poison", poisonEffect);
    }
}
