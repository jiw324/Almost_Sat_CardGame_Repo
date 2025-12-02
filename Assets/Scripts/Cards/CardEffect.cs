using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
    public abstract void Execute(EntityBase caster, EntityBase target, int value);
}
