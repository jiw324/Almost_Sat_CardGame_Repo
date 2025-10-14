using UnityEngine;

public abstract class CardEffectSO : ScriptableObject
{
    [Header("Targeting")]
    public bool isAOE = false;
    public TargetGroup targetGroup = TargetGroup.Enemies;

    // single-target execution
    public virtual void Execute(Actor target) { }

    // multi-target execution (default = call single on each)
    public virtual void ExecuteMany(Actor[] targets)
    {
        if (targets == null) return;
        foreach (var t in targets) Execute(t);
    }
}
