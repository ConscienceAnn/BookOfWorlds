using UnityEngine;
using Zenject;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class LevelGenerator : MonoBehaviour
{
    public static event System.Action OnLevelCleared;

    [Header("Parents")]
    [SerializeField] private Transform buildingsParent;
    [SerializeField] private Transform animalsParent;
    [SerializeField] private Transform collectableParent;
    [SerializeField] private Transform sellZoneParent;

    [Inject] private DiContainer container;
    [Inject] private GameSaveController gameSaveController;
    [Inject] private LevelProgress levelProgress;
    [Inject] private PlayerController player;

    private LevelDataSO currentLevelData;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<BuildingController> spawnedBuildings = new List<BuildingController>();

    public List<BuildingController> GetBuildings() => spawnedBuildings;

    // ===== LIFECYCLE =====

    private void OnApplicationQuit()
    {
        Debug.Log("[LevelGenerator] OnApplicationQuit — принудительная очистка");
        ClearLevelImmediate();
    }

    // ===== PUBLIC METHODS =====

    public async UniTask GenerateLevelAsync(LevelDataSO data)
    {
        await ClearLevelAsync();

        currentLevelData = data;

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

    // ===== GENERATION METHODS =====

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
        if (player != null)
        {
            player.transform.position = position;
        }
        else
        {
            Debug.LogError("Player не найден в LevelGenerator!");
        }
    }

    // ===== CLEAR METHODS =====

    private async UniTask ClearLevelAsync()
    {
        if (levelProgress != null)
        {
            levelProgress.ReturnToGameCamera();
            levelProgress.HideComplete();
        }

        ResourcePool[] allPools = FindObjectsOfType<ResourcePool>(true);
        foreach (var pool in allPools)
        {
            if (pool != null)
            {
                pool.ClearPool();
            }
        }

        ResourceSpawner[] spawners = FindObjectsOfType<ResourceSpawner>();
        foreach (var spawner in spawners)
        {
            if (spawner != null)
            {
                spawner.ClearAllResources();
                Destroy(spawner.gameObject);
            }
        }

        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
        spawnedBuildings.Clear();

        ClearParentChildren(collectableParent);
        ClearParentChildren(buildingsParent);
        ClearParentChildren(animalsParent);
        ClearParentChildren(sellZoneParent);

        OnLevelCleared?.Invoke();

        await UniTask.DelayFrame(2);
    }

    private void ClearLevelImmediate()
    {
        // Очищаем родительские папки
        ClearParentChildrenImmediate(collectableParent);
        ClearParentChildrenImmediate(buildingsParent);
        ClearParentChildrenImmediate(animalsParent);
        ClearParentChildrenImmediate(sellZoneParent);

        // Удаляем все созданные объекты
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        spawnedObjects.Clear();
        spawnedBuildings.Clear();

        // Очищаем пулы
        ResourcePool[] pools = FindObjectsOfType<ResourcePool>(true);
        foreach (var pool in pools)
        {
            if (pool != null)
            {
                pool.ClearPool();
            }
        }

        Debug.Log("[LevelGenerator] Принудительная очистка завершена");
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

    private void ClearParentChildrenImmediate(Transform parent)
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child != null)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}