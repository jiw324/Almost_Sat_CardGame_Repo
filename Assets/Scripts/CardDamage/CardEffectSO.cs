using UnityEngine;

public abstract class CardEffectSO : ScriptableObject
{
    // default = single target
    public virtual void Execute(Actor target) { }

    // default = fan out to each; override if you need custom AOE logic
    public virtual void ExecuteMany(Actor[] targets)
    {
        if (targets == null) return;
        foreach (var t in targets)
            Execute(t);
    }
}
