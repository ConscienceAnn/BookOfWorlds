using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Zenject;

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

    [Header("Behaviour")]
    [SerializeField] private ParticleFactory particleFactory;
    [SerializeField] private ResourceFlyAnimation flyAnimation;

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
            if (isStone)
            {
                var behaviour = new StoneBehaviour(particleFactory, flyAnimation);
                source.SetBehaviour(behaviour);
                Debug.Log($"Created StoneBehaviour for {obj.name}");
            }
            else
            {
                var behaviour = new TreeBehaviour(particleFactory, flyAnimation);
                source.SetBehaviour(behaviour);
                Debug.Log($"Created TreeBehaviour for {obj.name}");
            }

            source.OnCollected += OnResourceCollected;
            activeResources.Add(source);

            if (spawnPoint != null && !occupiedPoints.ContainsKey(spawnPoint))
            {
                occupiedPoints.Add(spawnPoint, source);
                Debug.Log($"Point {spawnPoint.name} occupied");
            }
        }
        else
        {
            Debug.LogWarning($"ResourceSpawner: ResourceSource not found on {obj.name}");
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
            Debug.Log($"Point {occupiedPoint.name} freed");
        }

        if (isStone)
        {
            resourceFactory?.ReturnStone(source.gameObject);
        }
        else
        {
            resourceFactory?.ReturnWood(source.gameObject);
        }

        if (occupiedPoint != null)
        {
            pendingRespawnPoint = occupiedPoint;
            float respawnDelay = source.ResourceData?.respawnTime ?? 5f;
            CancelInvoke(nameof(RespawnResource));
            Invoke(nameof(RespawnResource), respawnDelay);
            Debug.Log($"Respawn scheduled in {respawnDelay} sec at point {occupiedPoint.name}");
        }
    }

    private void RespawnResource()
    {
        if (pendingRespawnPoint != null)
        {
            if (IsPositionOccupied(pendingRespawnPoint.position))
            {
                List<Transform> freePoints = spawnPoints
                    .Where(p => p != null && !occupiedPoints.ContainsKey(p))
                    .ToList();

                if (freePoints.Count > 0)
                {
                    var point = freePoints[Random.Range(0, freePoints.Count)];
                    if (point != null)
                    {
                        SpawnResource(point.position, point.rotation, point);
                        Debug.Log($"Resource respawned at free point {point.name}");
                        pendingRespawnPoint = null;
                        return;
                    }
                }

                Debug.LogWarning("No free points, retrying in 0.5 sec");
                Invoke(nameof(RespawnResource), 0.5f);
                return;
            }

            SpawnResource(
                pendingRespawnPoint.position,
                pendingRespawnPoint.rotation,
                pendingRespawnPoint
            );
            Debug.Log($"Resource respawned at point {pendingRespawnPoint.name}");
            pendingRespawnPoint = null;
        }
        else
        {
            List<Transform> freePoints = spawnPoints
                .Where(p => p != null && !occupiedPoints.ContainsKey(p))
                .ToList();

            if (freePoints.Count == 0)
            {
                Debug.LogWarning("ResourceSpawner: no free points for respawn!");
                return;
            }

            var point = freePoints[Random.Range(0, freePoints.Count)];
            if (point != null)
            {
                SpawnResource(point.position, point.rotation, point);
                Debug.Log($"Resource respawned at free point {point.name}");
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