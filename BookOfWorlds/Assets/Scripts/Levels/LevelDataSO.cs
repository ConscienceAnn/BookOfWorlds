using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "Levels/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [Header("Основная информация")]
    public string levelName = "Новый уровень";
    public int levelIndex = 0;

    [Header("Стартовые условия")]
    public int startCoins = 0;
    public Vector3 playerStartPosition = Vector3.zero;

    [Header("Здания")]
    public List<BuildingSpawnData> buildings = new List<BuildingSpawnData>();

    [Header("Ресурсы (точки спавна)")]
    public List<ResourceSpawnData> resources = new List<ResourceSpawnData>();

    [Header("Животные")]
    public List<AnimalSpawnData> animals = new List<AnimalSpawnData>();

    [Header("Зона продажи")]
    public Vector3 sellZonePosition = new Vector3(0, 0, 10);

    [Header("Условия завершения")]
    public int coinsToUnlockNextLevel = 100;
    public bool showCompleteUI = true;
}

/// <summary>
/// Данные для спавна здания
/// </summary>
[System.Serializable]
public class BuildingSpawnData
{
    [Header("Идентификация")]
    public string buildingId;           // "Bridge", "House" и т.д.
    public string buildingName;         // "Мост", "Дом"

    [Header("Префаб и данные")]
    public GameObject buildingPrefab;   // ССЫЛКА НА ПРЕФАБ!
    public BuildingDataSO buildingData; // ССЫЛКА НА BuildingDataSO!

    [Header("Позиция на карте")]
    public Vector3 position;            //  ПОЗИЦИЯ НА КАРТЕ
    public Vector3 rotation;            //  ПОВОРОТ НА КАРТЕ
}

/// <summary>
/// Данные для спавна ресурса
/// </summary>
[System.Serializable]
public class ResourceSpawnData
{
    public string resourceType;         // "Wood" или "Stone"
    public Vector3 position;
    public int poolSize = 3;
    public float respawnTime = 3f;
}

/// <summary>
/// Данные для спавна животного
/// </summary>
[System.Serializable]
public class AnimalSpawnData
{
    public string animalType;           // "Cow" или "Rabbit"
    public Vector3 position;
    public float cooldownTime = 8f;
}