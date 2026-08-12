using UnityEngine;
using Zenject;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Collections;

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

    public async UniTask GenerateLevelAsync(LevelDataSO data)
    {
        // 1. Асинхронно очищаем уровень и ЖДЁМ завершения
        await ClearLevelAsync();

        currentLevelData = data;

        // 2. Создаём объекты (уже после очистки)
        GenerateCollectableObjects(data.collectableObjectsPrefab);
        GenerateBuildings(data.buildingsPrefab);

        if (gameSaveController != null)
        {
            gameSaveController.RefreshBuildingsList();
        }

        SetPlayerStartPosition(data.playerStartPosition);

        if (gameSaveController != null)
        {
            gameSaveController.LoadGame();
        }

        GenerateAnimals(data.animalsPrefab);
        GenerateSellZone(data.sellZoneData);
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
        }
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
    }

    private void SetPlayerStartPosition(Vector3 position)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = position;
        }
        else
        {
            Debug.LogError($"Игрок НЕ НАЙДЕН по тегу 'Player'!");
        }
    }

    /// <summary>
    /// АСИНХРОННАЯ ОЧИСТКА — ждёт фактического удаления объектов
    /// </summary>
    private async UniTask ClearLevelAsync()
    {
        // УНИЧТОЖАЕМ ВИЗУАЛ ЗАВЕРШЕНИЯ
        LevelProgress levelProgress = FindObjectOfType<LevelProgress>();
        if (levelProgress != null)
        {
            levelProgress.ReturnToGameCamera();
            levelProgress.HideComplete();
        }

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

        // 3. Удаляем точки спавна
        ResourceSpawner[] spawners = FindObjectsOfType<ResourceSpawner>();
        foreach (var spawner in spawners)
        {
            if (spawner != null)
            {
                Destroy(spawner.gameObject);
            }
        }

        // 4. Очищаем родительские объекты
        ClearParentChildren(collectableParent);
        ClearParentChildren(buildingsParent);
        ClearParentChildren(animalsParent);
        ClearParentChildren(sellZoneParent);

        // 5. Оповещаем подписчиков
        OnLevelCleared?.Invoke();

        // ===== ВАЖНО: ЖДЁМ 2 КАДРА, ЧТОБЫ UNITY УСПЕЛ УНИЧТОЖИТЬ ОБЪЕКТЫ =====
        await UniTask.DelayFrame(2);
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