using UnityEngine;
using Zenject;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Parents")]
    [SerializeField] private Transform buildingsParent;
    [SerializeField] private Transform animalsParent;
    [SerializeField] private Transform collectableParent;
    [SerializeField] private Transform sellZoneParent;


    [Inject] private DiContainer container;
    [Inject] private GameSaveController gameSaveController;

    private LevelDataSO currentLevelData;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<BuildingController> spawnedBuildings = new List<BuildingController>();

    public List<BuildingController> GetBuildings() => spawnedBuildings;

    public void GenerateLevel(LevelDataSO data)
    {
        ClearLevel();

        currentLevelData = data;

        Debug.Log($"Генерируем уровень: {data.levelName}");

        // 1. СОЗДАЁМ ВСЕ РЕСУРСЫ ИЗ ПРЕФАБА!
        GenerateCollectableObjects(data.collectableObjectsPrefab);

        // 2. Создаём здания
        GenerateBuildings(data.buildingsPrefab);

        // 3. Создаём животных
        GenerateAnimals(data.animalsPrefab);

        // 4. Создаём зону продажи
        GenerateSellZone(data.sellZoneData);

        // 5. Устанавливаем позицию игрока
        SetPlayerStartPosition(data.playerStartPosition);

        // 6. Применяем сохранённый прогресс
        ApplySavedProgress();

        Debug.Log($" Уровень {data.levelName} сгенерирован!");
    }

    //  НОВЫЙ МЕТОД — СОЗДАЁТ ВСЕ РЕСУРСЫ ИЗ ПРЕФАБА!
    private void GenerateCollectableObjects(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("CollectableObjects префаб не назначен в LevelData!");
            return;
        }

        GameObject collectableObj = container.InstantiatePrefab(
            prefab,
            Vector3.zero,
            Quaternion.identity,
            collectableParent
        );

        spawnedObjects.Add(collectableObj);

        Debug.Log($"- Созданы все ресурсы из префаба: {prefab.name}");
    }

    private void GenerateBuildings(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Buildings префаб не назначен в LevelData!");
            return;
        }

        GameObject buildingsObj = container.InstantiatePrefab(prefab, buildingsParent);
        spawnedObjects.Add(buildingsObj);

        // Находим все BuildingController в созданном префабе
        var buildings = buildingsObj.GetComponentsInChildren<BuildingController>();
        foreach (var building in buildings)
        {
            spawnedBuildings.Add(building);
            Debug.Log($"  - Найдено здание: {building.GetBuildingName()}");
        }

        Debug.Log($"  - Созданы все здания из префаба: {prefab.name}");
    }

    private void GenerateAnimals(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Animals префаб не назначен в LevelData!");
            return;
        }

        GameObject animalsObj = container.InstantiatePrefab(
            prefab,
            Vector3.zero,
            Quaternion.identity,
            animalsParent
        );

        spawnedObjects.Add(animalsObj);

        Debug.Log($"  - Созданы все животные из префаба: {prefab.name}");
    }

    private void GenerateSellZone(GameObject sellZonePrefab)
    {
        if (sellZonePrefab == null)
        {
            Debug.LogWarning("SellZone префаб не назначен в LevelData!");
            return;
        }

        GameObject sellZone = container.InstantiatePrefab(
            sellZonePrefab,
            sellZoneParent
        );

        spawnedObjects.Add(sellZone);
        Debug.Log($"  - SellZone создана из префаба: {sellZonePrefab.name}");
    }

    private void SetPlayerStartPosition(Vector3 position)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = position;
            Debug.Log($"- Игрок перемещён в {position}");
        }
    }

    private void ApplySavedProgress()
    {
        SaveData saveData = SaveSystem.Load();
        if (saveData == null) return;

        if (gameSaveController != null)
        {
            foreach (var building in spawnedBuildings)
            {
                gameSaveController.RegisterBuilding(building);
            }
            gameSaveController.RefreshAllSystems();
        }
    }

    private void ClearLevel()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
        spawnedBuildings.Clear();

        Debug.Log(" Уровень очищен");
    }
}