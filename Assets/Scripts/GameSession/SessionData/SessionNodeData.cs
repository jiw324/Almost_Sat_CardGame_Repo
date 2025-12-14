using System;

[Serializable]
public class SessionNodeData
{
}

[Serializable]
public class CombatNodeData : SessionNodeData
{
    public string enemyDefinitionName; // Name/path to EnemyDefinition asset
    public int enemyHealth = 10;
    public int enemyMana = 1;
    public DeckInstance enemyDeck = new DeckInstance();
}

[Serializable]
public class RestNodeData : SessionNodeData
{
    public int testRestVal = 0;
}

[Serializable]
public class ShopNodeData : SessionNodeData
{
    public int testShopVal = 0;
}

[Serializable]
public class EventNodeData : SessionNodeData
{
    public string grantedRelicId;
    public bool fromEvent;
}

[Serializable]
public class RelicNodeData : SessionNodeData
{
    public string grantedRelicId;
    public bool fromEvent;
}

[Serializable]
public class LootNodeData : SessionNodeData
{
    public int testRelicVal = 0;
}
