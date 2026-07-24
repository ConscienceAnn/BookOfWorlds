using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private ResourceFactory resourceFactory;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private bool isStone = false;

    [Header("Rotation")]
    [SerializeField] private Vector3 fixedRotation = new Vector3(-90f, 0f, 0f);
    [SerializeField] private bool useFixedRotation = true;

    private List<ResourceSource> activeResources = new List<ResourceSource>();
    private List<Transform> occupiedPoints = new List<Transform>(); 

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
            obj.transform.rotation = Quaternion.Euler(fixedRotation);
        }

        ResourceSource source = obj.GetComponent<ResourceSource>();
        if (source != null)
        {
            source.OnCollected += OnResourceCollected;
            activeResources.Add(source);

            //  «апоминаем, что точка зан€та
            if (spawnPoint != null && !occupiedPoints.Contains(spawnPoint))
            {
                occupiedPoints.Add(spawnPoint);
            }
        }
    }

    private void OnResourceCollected(ResourceSource source)
    {
        if (source == null) return;

        source.OnCollected -= OnResourceCollected;
        activeResources.Remove(source);

        //  ”дал€ем точку из зан€тых (если она там есть)
        Transform occupiedPoint = occupiedPoints.FirstOrDefault(p =>
            Vector3.Distance(p.position, source.transform.position) < 0.1f
        );
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

        CancelInvoke(nameof(RespawnResource));
        Invoke(nameof(RespawnResource), respawnTime);
    }

    private void RespawnResource()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return;

        //  Ќаходим свободные точки
        List<Transform> freePoints = spawnPoints
            .Where(p => p != null && !occupiedPoints.Contains(p))
            .ToList();

        if (freePoints.Count == 0)
        {
            Debug.LogWarning("ResourceSpawner: нет свободных точек дл€ респавна!");
            return;
        }

        // ¬ыбираем случайную свободную точку
        var point = freePoints[Random.Range(0, freePoints.Count)];

        if (point != null)
        {
            SpawnResource(point.position, point.rotation, point);
            Debug.Log($"–есурс респавнулс€ в свободной точке {point.name}");
        }
    }

    public int GetActiveResourceCount()
    {
        return activeResources.Count;
    }
}