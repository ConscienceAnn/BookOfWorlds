using UnityEngine;
using Zenject;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Parents")]
    [SerializeField] private Transform buildingsParent;
    [SerializeField] private Transform resourcesParent;
    [SerializeField] private Transform animalsParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject sellZonePrefab;
    [SerializeField] private GameObject cowPrefab;
    [SerializeField] private GameObject rabbitPrefab;

    [Inject] private DiContainer container;
    [Inject] private ResourcePool woodPool;
    [Inject] private ResourcePool stonePool;
    [Inject] private GameSaveController gameSaveController;

    private LevelDataSO currentLevelData;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<BuildingController> spawnedBuildings = new List<BuildingController>();

    public List<BuildingController> GetBuildings() => spawnedBuildings;

    public void GenerateLevel(LevelDataSO data)
    {
        ClearLevel();

        currentLevelData = data;

        Debug.Log($" Генерируем уровень: {data.levelName}");

        GenerateBuildings(data.buildings);
        GenerateResources(data.resources);
        GenerateAnimals(data.animals);
        GenerateSellZone(data.sellZonePosition);
        SetPlayerStartPosition(data.playerStartPosition);
        ApplySavedProgress();

        Debug.Log($" Уровень {data.levelName} сгенерирован!");
    }

    private void GenerateBuildings(List<BuildingSpawnData> buildingsData)
    {
        foreach (var data in buildingsData)
        {
            if (data.buildingPrefab == null)
            {
                Debug.LogWarning($"Префаб для здания {data.buildingId} не назначен!");
                continue;
            }

            GameObject buildingObj = container.InstantiatePrefab(
                data.buildingPrefab,
                data.position,
                Quaternion.Euler(data.rotation),
                buildingsParent
            );

            spawnedObjects.Add(buildingObj);

            BuildingController building = buildingObj.GetComponent<BuildingController>();
            if (building != null)
            {
                spawnedBuildings.Add(building);
                Debug.Log($"- Создано здание: {building.GetBuildingName()}");
            }
        }
    }

    private void GenerateResources(List<ResourceSpawnData> resourcesData)
    {
        foreach (var data in resourcesData)
        {
            ResourcePool pool = data.resourceType == "Wood" ? woodPool : stonePool;

            if (pool == null)
            {
                Debug.LogWarning($"Пул для {data.resourceType} не найден!");
                continue;
            }

            for (int i = 0; i < data.poolSize; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    0,
                    Random.Range(-0.5f, 0.5f)
                );

                GameObject obj = pool.Get(data.position + offset, Quaternion.identity);
                if (obj != null)
                {
                    spawnedObjects.Add(obj);
                }
            }

            Debug.Log($"- Создан ресурс: {data.resourceType} (x{data.poolSize}) в {data.position}");
        }
    }

    private void GenerateAnimals(List<AnimalSpawnData> animalsData)
    {
        foreach (var data in animalsData)
        {
            GameObject prefab = data.animalType == "Cow" ? cowPrefab : rabbitPrefab;

            if (prefab == null)
            {
                Debug.LogWarning($"Префаб для {data.animalType} не найден!");
                continue;
            }

            GameObject animalObj = container.InstantiatePrefab(
                prefab,
                data.position,
                Quaternion.identity,
                animalsParent
            );

            spawnedObjects.Add(animalObj);

            Debug.Log($" - Создано животное: {data.animalType}");
        }
    }

    private void GenerateSellZone(Vector3 position)
    {
        if (sellZonePrefab == null)
        {
            Debug.LogWarning("SellZone префаб не назначен!");
            return;
        }

        GameObject sellZone = container.InstantiatePrefab(sellZonePrefab, position, Quaternion.identity, null);
        spawnedObjects.Add(sellZone);

        Debug.Log($" - SellZone создана в {position}");
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
                ResourceSource source = obj.GetComponent<ResourceSource>();
                if (source != null)
                {
                    source.Hide();
                    continue;
                }

                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
        spawnedBuildings.Clear();

        Debug.Log(" Уровень очищен");
    }
}