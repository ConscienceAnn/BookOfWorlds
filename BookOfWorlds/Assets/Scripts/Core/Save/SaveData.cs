using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // ===== ÃËÎÁÀËÜÍÛÅ ÄÀÍÍÛÅ =====
    public int currentLevel = 0;
    public List<string> openedLevels = new List<string>();

    // ===== ÄÀÍÍÛÅ ÒÅÊÓÙÅÃÎ ÓĞÎÂÍß =====
    public int coins = 0;
    public List<ResourceEntry> resources = new List<ResourceEntry>();

    // ===== ÄÀÍÍÛÅ ÇÄÀÍÈÉ (ïî óğîâíÿì) =====
    public List<LevelProgressData> levelProgress = new List<LevelProgressData>();
}

[System.Serializable]
public class ResourceEntry
{
    public string resourceName;
    public int amount;
}

[System.Serializable]
public class LevelProgressData
{
    public string levelName;
    public bool isCompleted;
    public List<BuildingProgressEntry> buildingProgress = new List<BuildingProgressEntry>();
    public List<string> restoredBuildings = new List<string>();
}

[System.Serializable]
public class BuildingProgressEntry
{
    public string buildingId;
    public string resourceName;
    public int investedAmount;
}