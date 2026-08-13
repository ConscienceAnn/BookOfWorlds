using UnityEngine;
using System.Collections.Generic;
using Zenject;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private ResourceFactory resourceFactory;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] public ResourceType resourceType = ResourceType.Wood;

    [Header("Rotation")]
    [SerializeField] private Vector3 fixedRotation = new Vector3(-90f, 0f, 0f);
    [SerializeField] private bool useFixedRotation = true;
    [SerializeField] private bool randomYRotation = true;

    [Inject] private ParticleFactory particleFactory;
    [Inject] private ResourceFlyAnimation flyAnimation;
    [Inject] private ResourceBehaviourFactory behaviourFactory;

    private List<ResourceSource> activeResources = new List<ResourceSource>();
    private Dictionary<Transform, ResourceSource> occupiedPoints = new Dictionary<Transform, ResourceSource>();
    private Transform pendingRespawnPoint = null;
    private List<Transform> freePointsCache = new List<Transform>();

    public ResourceType ResourceType => resourceType;

    private void Start()
    {
        var allSpawners = FindObjectsOfType<ResourceSpawner>();
        int sameTypeCount = 0;
        foreach (var s in allSpawners)
        {
            if (s != null && s.resourceType == resourceType)
            {
                sameTypeCount++;
            }
        }

        if (sameTypeCount > 1)
        {
            Debug.LogWarning($"[ResourceSpawner] ВНИМАНИЕ! Найдено {sameTypeCount} спавнеров с resourceType={resourceType}! Это может вызывать дублирование!");
        }

        if (resourceFactory == null)
        {
            Debug.LogError("ResourceSpawner: resourceFactory is NULL!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError($"ResourceSpawner: spawnPoints is empty! (resourceType: {resourceType})");
            return;
        }

        SpawnAllResources();
    }

    public void SpawnAllResources()
    {
        ClearAllResources();
        occupiedPoints.Clear();

        int spawnedCount = 0;
        foreach (var point in spawnPoints)
        {
            if (point != null)
            {
                SpawnResource(point.position, point.rotation, point);
                spawnedCount++;
            }
        }

        Debug.Log($"[ResourceSpawner] Создано {spawnedCount} ресурсов типа {resourceType}");
    }

    public void ClearAllResources()
    {
        foreach (var source in activeResources)
        {
            if (source != null)
            {
                source.OnCollected -= OnResourceCollected;
                ReturnResourceToPool(source.gameObject);
            }
        }
        activeResources.Clear();
        occupiedPoints.Clear();
        pendingRespawnPoint = null;

        CancelInvoke(nameof(RespawnResource));
    }

    private void ReturnResourceToPool(GameObject obj)
    {
        if (obj == null || resourceFactory == null) return;

        switch (resourceType)
        {
            case ResourceType.Wood:
                resourceFactory.ReturnWood(obj);
                break;
            case ResourceType.Stone:
                resourceFactory.ReturnStone(obj);
                break;
            default:
                Debug.LogWarning($"Неизвестный тип ресурса: {resourceType}, уничтожаем объект");
                Destroy(obj);
                break;
        }
    }

    private void SpawnResource(Vector3 position, Quaternion rotation, Transform spawnPoint = null)
    {
        if (resourceFactory == null) return;

        GameObject obj = CreateResourceByType(position, rotation);
        if (obj == null)
        {
            Debug.LogError($"ResourceSpawner: failed to create resource of type {resourceType}!");
            return;
        }

        if (useFixedRotation)
        {
            float randomY = randomYRotation ? Random.Range(0f, 360f) : 0f;
            obj.transform.rotation = Quaternion.Euler(
                fixedRotation.x,
                randomY,
                fixedRotation.z
            );
        }
        else
        {
            obj.transform.rotation = rotation;
        }

        ResourceSource source = obj.GetComponent<ResourceSource>();
        if (source != null)
        {
            // Применяем текущий множитель респавна
            source.ApplyCurrentMultiplier();

            IResourceBehaviour behaviour = behaviourFactory.Create(resourceType, particleFactory, flyAnimation);
            if (behaviour != null)
            {
                source.SetBehaviour(behaviour);
            }

            source.OnCollected += OnResourceCollected;
            activeResources.Add(source);

            if (spawnPoint != null && !occupiedPoints.ContainsKey(spawnPoint))
            {
                occupiedPoints.Add(spawnPoint, source);
            }

            float multiplier = RespawnSettings.Multiplier;
            float respawnTime = source.ResourceData?.respawnTime ?? 7f;
            Debug.Log($"[ResourceSpawner] Создан {resourceType} с множителем {multiplier}x (время респавна: {respawnTime / multiplier:F2} сек)");
        }
        else
        {
            Debug.LogWarning($"ResourceSpawner: ResourceSource not found on {obj.name}");
        }
    }

    private GameObject CreateResourceByType(Vector3 position, Quaternion rotation)
    {
        switch (resourceType)
        {
            case ResourceType.Wood:
                return resourceFactory.CreateWood(position, rotation);
            case ResourceType.Stone:
                return resourceFactory.CreateStone(position, rotation);
            default:
                Debug.LogWarning($"Неизвестный тип ресурса: {resourceType}");
                return null;
        }
    }

    private void OnResourceCollected(ResourceSource source)
    {
        if (source == null) return;

        source.OnCollected -= OnResourceCollected;
        activeResources.Remove(source);

        Transform occupiedPoint = null;
        foreach (var kvp in occupiedPoints)
        {
            if (kvp.Value == source)
            {
                occupiedPoint = kvp.Key;
                break;
            }
        }

        if (occupiedPoint != null)
        {
            occupiedPoints.Remove(occupiedPoint);
        }

        ReturnResourceToPool(source.gameObject);

        if (occupiedPoint != null)
        {
            pendingRespawnPoint = occupiedPoint;
            // Используем актуальное время респавна с учетом множителя
            float baseRespawnTime = source.ResourceData?.respawnTime ?? 7f;
            float multiplier = RespawnSettings.Multiplier;
            float respawnDelay = baseRespawnTime / multiplier;

            Debug.Log($"[ResourceSpawner] Ресурс собран, респавн через {respawnDelay:F2} сек (база: {baseRespawnTime:F2} сек, множитель: {multiplier}x)");

            CancelInvoke(nameof(RespawnResource));
            Invoke(nameof(RespawnResource), respawnDelay);
        }
    }

    private void RespawnResource()
    {
        freePointsCache.Clear();

        if (pendingRespawnPoint != null)
        {
            if (IsPositionOccupied(pendingRespawnPoint.position))
            {
                foreach (var spawnPoint in spawnPoints)
                {
                    if (spawnPoint != null && !occupiedPoints.ContainsKey(spawnPoint))
                    {
                        freePointsCache.Add(spawnPoint);
                    }
                }

                if (freePointsCache.Count > 0)
                {
                    var selectedPoint = freePointsCache[Random.Range(0, freePointsCache.Count)];
                    if (selectedPoint != null)
                    {
                        SpawnResource(selectedPoint.position, selectedPoint.rotation, selectedPoint);
                        pendingRespawnPoint = null;
                        return;
                    }
                }

                Invoke(nameof(RespawnResource), 0.5f);
                return;
            }

            SpawnResource(
                pendingRespawnPoint.position,
                pendingRespawnPoint.rotation,
                pendingRespawnPoint
            );
            pendingRespawnPoint = null;
        }
        else
        {
            foreach (var freePoint in spawnPoints)
            {
                if (freePoint != null && !occupiedPoints.ContainsKey(freePoint))
                {
                    freePointsCache.Add(freePoint);
                }
            }

            if (freePointsCache.Count == 0)
            {
                return;
            }

            var chosenPoint = freePointsCache[Random.Range(0, freePointsCache.Count)];
            if (chosenPoint != null)
            {
                SpawnResource(chosenPoint.position, chosenPoint.rotation, chosenPoint);
            }
        }
    }

    private bool IsPositionOccupied(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 4f);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    public int GetActiveResourceCount()
    {
        return activeResources.Count;
    }
}