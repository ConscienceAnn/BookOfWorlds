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


    private void OnApplicationQuit()
    {
        ClearLevelImmediate();
    }


    public async UniTask GenerateLevelAsync(LevelDataSO data)
    {
        await ClearLevelAsync();

        currentLevelData = data;

        if (levelProgress != null && data != null)
        {
            levelProgress.SetLevelCompletePrefab(data.levelCompleteEffectPrefab);
            Debug.Log($"[LevelGenerator] Установлен префаб завершения для {data.levelName}: {data.levelCompleteEffectPrefab?.name ?? "NULL"}");
        }

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

    /// <summary>
    /// Асинхронная очистка уровня для нормального игрового процесса.
    /// Использует Destroy() для безопасного удаления в конце кадра.
    /// </summary>
    private async UniTask ClearLevelAsync()
    {
        await ClearLevelInternal(false);
    }

    /// <summary>
    /// Мгновенная очистка уровня для выхода из игры.
    /// Использует DestroyImmediate() для гарантированного удаления.
    /// </summary>
    private void ClearLevelImmediate()
    {
        ClearLevelInternal(true).Forget();
    }

    /// <summary>
    /// Внутренний метод очистки уровня.
    /// </summary>

    private async UniTask ClearLevelInternal(bool immediate)
    {
        // 1. Скрываем визуал завершения уровня
        if (levelProgress != null)
        {
            levelProgress.ReturnToGameCamera();
            levelProgress.HideComplete();
        }

        // 2. Очищаем все пулы ресурсов
        ResourcePool[] allPools = FindObjectsOfType<ResourcePool>(true);
        foreach (var pool in allPools)
        {
            if (pool != null)
            {
                pool.ClearPool();
            }
        }

        // 3. Удаляем спавнеры ресурсов
        ResourceSpawner[] spawners = FindObjectsOfType<ResourceSpawner>();
        foreach (var spawner in spawners)
        {
            if (spawner != null)
            {
                spawner.ClearAllResources();

                if (immediate)
                    DestroyImmediate(spawner.gameObject);
                else
                    Destroy(spawner.gameObject);
            }
        }

        // 4. Удаляем все созданные LevelGenerator объекты
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                if (immediate)
                    DestroyImmediate(obj);
                else
                    Destroy(obj);
            }
        }
        spawnedObjects.Clear();
        spawnedBuildings.Clear();

        // 5. Очищаем родительские папки
        if (immediate)
        {
            ClearParentChildrenImmediate(collectableParent);
            ClearParentChildrenImmediate(buildingsParent);
            ClearParentChildrenImmediate(animalsParent);
            ClearParentChildrenImmediate(sellZoneParent);
        }
        else
        {
            ClearParentChildren(collectableParent);
            ClearParentChildren(buildingsParent);
            ClearParentChildren(animalsParent);
            ClearParentChildren(sellZoneParent);
        }

        // 6. Оповещаем подписчиков об очистке
        OnLevelCleared?.Invoke();

        // 7. Ждём 2 кадра для завершения удаления (только в обычном режиме)
        if (!immediate)
        {
            await UniTask.DelayFrame(2);
        }
    }

    // ===== HELPER METHODS =====

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