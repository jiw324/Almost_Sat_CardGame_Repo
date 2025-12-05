using System;

[Serializable]
public class RelicJSON
{
    public string id;
    public string name;
    public string description;
    public string rarity;
    public string effectType;
    public string iconResourcePath; // e.g. "Relics/RelicIcon"
    public int effectValue;
}

[Serializable]
public class RelicJSONWrapper
{
    public RelicJSON[] relics;
}


