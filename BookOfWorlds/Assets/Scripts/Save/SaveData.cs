using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int coins;
    public List<ResourceEntry> resources = new List<ResourceEntry>(); // Wood, Stone, Milk, Wool
    public List<BuildingProgressEntry> buildingProgress = new List<BuildingProgressEntry>();  // Bridge, House, Mill
    public List<string> restoredBuildings = new List<string>();         // Названия восстановленных зданий
    public List<string> openedLevels = new List<string>();               // Открытые локации
    public int currentLevel;                        // Текущая локация
}

[Serializable]
public class ResourceEntry
{
    public string resourceName;
    public int amount;
}

[Serializable]
public class BuildingProgressEntry
{
    public string buildingId;
    public string resourceName;
    public int investedAmount;
}