using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private ResourceFactory resourceFactory;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private bool isStone = false;

    [Header("Rotation")]
    [SerializeField] private Vector3 fixedRotation = new Vector3(-90f, 0f, 0f);
    [SerializeField] private bool useFixedRotation = true;
    [SerializeField] private bool randomYRotation = true;

    private List<ResourceSource> activeResources = new List<ResourceSource>();
    private Dictionary<Transform, ResourceSource> occupiedPoints = new Dictionary<Transform, ResourceSource>();
    private Transform pendingRespawnPoint = null;

    private void Start()
    {
        if (resourceFactory == null)
        {
            Debug.LogError("ResourceSpawner: resourceFactory is NULL!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError($"ResourceSpawner: spawnPoints is empty! (isStone: {isStone})");
            return;
        }

        SpawnAllResources();
    }

    public void SpawnAllResources()
    {
        ClearAllResources();
        occupiedPoints.Clear();

        foreach (var point in spawnPoints)
        {
            if (point != null)
            {
                SpawnResource(point.position, point.rotation, point);
            }
        }
    }

    private void ClearAllResources()
    {
        foreach (var source in activeResources)
        {
            if (source != null)
            {
                source.OnCollected -= OnResourceCollected;
                if (isStone)
                    resourceFactory?.ReturnStone(source.gameObject);
                else
                    resourceFactory?.ReturnWood(source.gameObject);
            }
        }
        activeResources.Clear();
    }

    private void SpawnResource(Vector3 position, Quaternion rotation, Transform spawnPoint = null)
    {
        if (resourceFactory == null) return;

        GameObject obj;
        if (isStone)
        {
            obj = resourceFactory.CreateStone(position, rotation);
        }
        else
        {
            obj = resourceFactory.CreateWood(position, rotation);
        }

        if (obj == null)
        {
            Debug.LogError("ResourceSpawner: failed to create resource!");
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
            source.OnCollected += OnResourceCollected;
            activeResources.Add(source);

            if (spawnPoint != null && !occupiedPoints.ContainsKey(spawnPoint))
            {
                occupiedPoints.Add(spawnPoint, source);
                Debug.Log($"“очка {spawnPoint.name} зан€та");
            }
        }
    }

    private void OnResourceCollected(ResourceSource source)
    {
        if (source == null) return;

        source.OnCollected -= OnResourceCollected;
        activeResources.Remove(source);

        // Ќаходим точку, которой принадлежал этот ресурс
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
            Debug.Log($"“очка {occupiedPoint.name} освободилась");
        }

        if (isStone)
        {
            resourceFactory?.ReturnStone(source.gameObject);
        }
        else
        {
            resourceFactory?.ReturnWood(source.gameObject);
        }

        // –еспавн в ту же точку через врем€ из данных ресурса
        if (occupiedPoint != null)
        {
            pendingRespawnPoint = occupiedPoint;
            float respawnDelay = source.ResourceData?.respawnTime ?? 5f; // из ResourceDataSO
            CancelInvoke(nameof(RespawnResource));
            Invoke(nameof(RespawnResource), respawnDelay);
            Debug.Log($"«апланирован респавн через {respawnDelay} сек в точку {occupiedPoint.name}");
        }
    }

    private void RespawnResource()
    {
        if (pendingRespawnPoint != null)
        {
            SpawnResource(
                pendingRespawnPoint.position,
                pendingRespawnPoint.rotation,
                pendingRespawnPoint
            );
            Debug.Log($"–есурс респавнулс€ в точку {pendingRespawnPoint.name}");
            pendingRespawnPoint = null;
        }
        else
        {
            // ≈сли по какой-то причине точка не сохранена Ч ищем свободную
            List<Transform> freePoints = spawnPoints
                .Where(p => p != null && !occupiedPoints.ContainsKey(p))
                .ToList();

            if (freePoints.Count == 0)
            {
                Debug.LogWarning("ResourceSpawner: нет свободных точек дл€ респавна!");
                return;
            }

            var point = freePoints[Random.Range(0, freePoints.Count)];
            if (point != null)
            {
                SpawnResource(point.position, point.rotation, point);
                Debug.Log($"–есурс респавнулс€ в свободную точку {point.name}");
            }
        }
    }

    public int GetActiveResourceCount()
    {
        return activeResources.Count;
    }
}