/// <summary>
/// Runtime instance of a relic in a game session.
/// Wraps a static RelicData asset so we can track state like whether it is active.
/// </summary>
public class RelicInstance
{
    /// <summary>
    /// Static data for this relic (authoring-time definition).
    /// </summary>
    public RelicData Data { get; }

    /// <summary>
    /// Whether this relic is currently active. Future systems can toggle this.
    /// </summary>
    public bool IsActive { get; private set; }

    public RelicInstance(RelicData data, bool isActive = true)
    {
        Data = data;
        IsActive = isActive;
    }

    public void SetActive(bool active)
    {
        IsActive = active;
    }

    public override string ToString()
    {
        return Data != null ? Data.ToString() : "Relic[null]";
    }
}


