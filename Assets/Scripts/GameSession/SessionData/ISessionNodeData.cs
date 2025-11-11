using System.Collections.Generic;

public class ISessionNodeData { }

[System.Serializable]
public class CombatNodeData : ISessionNodeData
{
    public int enemyHealth = 10;
    public int enemyMana = 1;
    public List<CardInstance> enemyDeck = new List<CardInstance>();
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
    public int testEventVal = 0;
}

[System.Serializable]
public class LootNodeData : ISessionNodeData
{
    public int testLootVal = 0;
}
