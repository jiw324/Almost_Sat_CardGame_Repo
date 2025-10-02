public abstract class GameStateBase
{
    protected GameManager gameManager;

    public GameStateBase(GameManager manager)
    {
        gameManager = manager;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}