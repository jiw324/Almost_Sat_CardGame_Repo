using System.Collections.Generic;

public class ISessionNodeData { }

[System.Serializable]
public class CombatNodeData : ISessionNodeData
{
    public string enemyDefinitionName; // Name/path to EnemyDefinition asset
    public int enemyHealth = 10;
    public int enemyMana = 1;
    public DeckInstance enemyDeck = new DeckInstance();
}

[System.Serializable]
public class RestNodeData : ISessionNodeData
{
    public int testRestVal = 0;
}

[System.Serializable]
public class ShopNodeData : ISessionNodeData
{
    public int testShopVal = 0;
}

[System.Serializable]
public class EventNodeData : ISessionNodeData
{
    //public int testEventVal = 0;
    // ID of the relic granted when this loot node is resolved.
    public string grantedRelicId;
    // True if this loot was triggered from an Event node instead of a Loot node
    public bool fromEvent;
}

[System.Serializable]
public class RelicNodeData : ISessionNodeData
{
    // ID of the relic granted when this loot node is resolved.
    public string grantedRelicId;
    // True if this loot was triggered from an Event node instead of a Loot node
    public bool fromEvent;
}

public class LootNodeData : ISessionNodeData
{
    public int testRelicVal = 0;
}
