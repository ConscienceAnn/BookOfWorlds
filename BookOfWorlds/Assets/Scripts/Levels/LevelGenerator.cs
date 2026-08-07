using UnityEngine;
using Zenject;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    // СОБЫТИЕ — оповещает об очистке уровня
    public static event System.Action OnLevelCleared;

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

        // 1. Ресурсы
        GenerateCollectableObjects(data.collectableObjectsPrefab);

        // 2. Здания
        GenerateBuildings(data.buildingsPrefab);

        // 3. Регистрируем здания
        if (gameSaveController != null)
        {
            gameSaveController.RefreshBuildingsList();
        }

        // 4. Позиция игрока
        SetPlayerStartPosition(data.playerStartPosition);

        // 5. Загружаем сохранение
        if (gameSaveController != null)
        {
            gameSaveController.LoadGame();
        }

        // 6. Животные
        GenerateAnimals(data.animalsPrefab);

        // 7. Зона продажи
        GenerateSellZone(data.sellZoneData);

        Debug.Log($"Уровень {data.levelName} сгенерирован!");
    }

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
        Debug.Log($"  - Созданы ресурсы: {prefab.name}");
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

        var buildings = buildingsObj.GetComponentsInChildren<BuildingController>();
        foreach (var building in buildings)
        {
            spawnedBuildings.Add(building);

            if (gameSaveController != null)
            {
                gameSaveController.RegisterBuilding(building);
            }

            Debug.Log($"  - Найдено здание: {building.GetBuildingName()}");
        }

        Debug.Log($"  - Созданы здания: {prefab.name}");
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
        Debug.Log($"  - Созданы животные: {prefab.name}");
    }

    private void GenerateSellZone(GameObject sellZonePrefab)
    {
        if (sellZonePrefab == null)
        {
            Debug.LogWarning("SellZone префаб не назначен в LevelData!");
            return;
        }

        GameObject sellZone = container.InstantiatePrefab(sellZonePrefab, sellZoneParent);
        spawnedObjects.Add(sellZone);
        Debug.Log($"  - SellZone создана: {sellZonePrefab.name}");
    }

    private void SetPlayerStartPosition(Vector3 position)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log($"  - Найден игрок: {player.name}, текущая позиция: {player.transform.position}");
            player.transform.position = position;
            Debug.Log($"  - Игрок перемещён в {position}, новая позиция: {player.transform.position}");
        }
        else
        {
            Debug.LogError($"  - Игрок НЕ НАЙДЕН по тегу 'Player'!");
        }
    }

    private void ClearLevel()
    {
        Debug.Log($"Очистка уровня: {spawnedObjects.Count} объектов");

        // 1. Удаляем все созданные объекты LevelGenerator
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
        spawnedBuildings.Clear();

        // 2. Удаляем ресурсы по тегу
        GameObject[] resources = GameObject.FindGameObjectsWithTag("Collectable");
        foreach (var resource in resources)
        {
            if (resource != null)
            {
                Destroy(resource);
            }
        }
        if (resources.Length > 0)
        {
            Debug.Log($"  - Удалено ресурсов: {resources.Length}");
        }

        // 3. Удаляем точки спавна
        ResourceSpawner[] spawners = FindObjectsOfType<ResourceSpawner>();
        foreach (var spawner in spawners)
        {
            if (spawner != null)
            {
                Destroy(spawner.gameObject);
            }
        }
        if (spawners.Length > 0)
        {
            Debug.Log($"  - Удалено спавнеров: {spawners.Length}");
        }

        // 4. Очищаем родительские объекты
        ClearParentChildren(collectableParent);
        ClearParentChildren(buildingsParent);
        ClearParentChildren(animalsParent);
        ClearParentChildren(sellZoneParent);

        // 5. Оповещаем всех подписчиков, что уровень очищен
        OnLevelCleared?.Invoke();
        Debug.Log("  - Отправлено событие OnLevelCleared");

        Debug.Log("Уровень очищен");
    }

    private void ClearParentChildren(Transform parent)
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
}