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

    // ===== ЧЕРЕЗ DI =====
    [Inject] private LevelProgress levelProgress;
    [Inject] private PlayerController player;

    private LevelDataSO currentLevelData;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<BuildingController> spawnedBuildings = new List<BuildingController>();

    public List<BuildingController> GetBuildings() => spawnedBuildings;

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
        // ===== ИСПОЛЬЗУЕМ INJECTED PLAYER =====
        if (player != null)
        {
            player.transform.position = position;
        }
        else
        {
            Debug.LogError("Player не найден в LevelGenerator!");
        }
    }

    private async UniTask ClearLevelAsync()
    {
        // ===== ИСПОЛЬЗУЕМ INJECTED LEVELPROGRESS =====
        if (levelProgress != null)
        {
            levelProgress.ReturnToGameCamera();
            levelProgress.HideComplete();
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

        GameObject[] resources = GameObject.FindGameObjectsWithTag("Collectable");
        foreach (var resource in resources)
        {
            if (resource != null)
            {
                Destroy(resource);
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

        ClearParentChildren(collectableParent);
        ClearParentChildren(buildingsParent);
        ClearParentChildren(animalsParent);
        ClearParentChildren(sellZoneParent);

        OnLevelCleared?.Invoke();

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