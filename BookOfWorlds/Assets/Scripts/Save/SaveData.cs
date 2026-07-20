using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int coins;
    public List<ResourceEntry> resources;     // Wood, Stone, Milk, Wool
    public List<BuildingProgressEntry> buildingProgress;  // Bridge, House, Mill
    public List<string> restoredBuildings;          // Названия восстановленных зданий
    public List<string> openedLevels;               // Открытые локации
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
    public string buildingName;
    public string resourceName;
    public int investedAmount;
}