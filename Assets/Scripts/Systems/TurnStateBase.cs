public abstract class TurnStateBase
{
    protected TurnManager turnManager;

    public TurnStateBase(TurnManager manager)
    {
        turnManager = manager;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}