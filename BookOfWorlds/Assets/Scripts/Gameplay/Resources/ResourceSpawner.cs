using UnityEngine;
using System.Collections.Generic;
using Zenject;
using Cysharp.Threading.Tasks;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private ResourceFactory resourceFactory;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private string resourceTypeName = "Wood";
    [SerializeField] private float checkRadius = 1f;

    [Header("Rotation")]
    [SerializeField] private Vector3 fixedRotation = new Vector3(-90f, 0f, 0f);
    [SerializeField] private bool useFixedRotation = true;
    [SerializeField] private bool randomYRotation = true;

    [Inject] private ParticleFactory particleFactory;
    [Inject] private ResourceFlyAnimation flyAnimation;
    [Inject] private ResourceBehaviourFactory behaviourFactory;

    private Dictionary<Transform, ResourceSource> spawnPointToResource = new Dictionary<Transform, ResourceSource>();
    private Queue<Transform> respawnQueue = new Queue<Transform>();
    private bool isRespawning = false;

    private void Start()
    {
        if (resourceFactory == null)
        {
            Debug.LogError("ResourceManager: resourceFactory is NULL!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError($"ResourceManager: spawnPoints is empty!");
            return;
        }

        SpawnAllResources();
    }

    public void SpawnAllResources()
    {
        ClearAllResources();
        spawnPointToResource.Clear();
        respawnQueue.Clear();

        foreach (var point in spawnPoints)
        {
            if (point != null)
            {
                SpawnResourceAtPoint(point);
            }
        }
    }

    public void ClearAllResources()
    {
        foreach (var kvp in spawnPointToResource)
        {
            if (kvp.Value != null)
            {
                kvp.Value.OnCollected -= OnResourceCollected;
                ReturnResourceToPool(kvp.Value.gameObject);
            }
        }
        spawnPointToResource.Clear();
        respawnQueue.Clear();
        isRespawning = false;
        CancelInvoke(nameof(ProcessRespawnQueue));
    }

    private void ReturnResourceToPool(GameObject obj)
    {
        if (obj == null || resourceFactory == null) return;

        switch (resourceTypeName)
        {
            case "Wood": resourceFactory.ReturnWood(obj); break;
            case "Stone": resourceFactory.ReturnStone(obj); break;
            default: Destroy(obj); break;
        }
    }

    private void SpawnResourceAtPoint(Transform spawnPoint)
    {
        if (spawnPoint == null) return;
        if (spawnPointToResource.ContainsKey(spawnPoint))
        {
            return;
        }

        GameObject obj = CreateResourceByType(spawnPoint.position, spawnPoint.rotation);
        if (obj == null)
        {
            Debug.LogError($"Íå óäàëîñü ñîçäàòü ðåñóðñ òèïà {resourceTypeName}!");
            return;
        }

        ApplyRotation(obj);

        ResourceSource source = obj.GetComponent<ResourceSource>();
        if (source != null)
        {
            source.ApplyCurrentMultiplier();

            ResourceType type = GetResourceType();
            IResourceBehaviour behaviour = behaviourFactory.Create(type, particleFactory, flyAnimation);
            if (behaviour != null) source.SetBehaviour(behaviour);

            source.OnCollected += OnResourceCollected;

            spawnPointToResource.Add(spawnPoint, source);
        }
    }

    private GameObject CreateResourceByType(Vector3 position, Quaternion rotation)
    {
        switch (resourceTypeName)
        {
            case "Wood": return resourceFactory.CreateWood(position, rotation);
            case "Stone": return resourceFactory.CreateStone(position, rotation);
            default: return null;
        }
    }

    private void ApplyRotation(GameObject obj)
    {
        if (useFixedRotation)
        {
            float randomY = randomYRotation ? Random.Range(0f, 360f) : 0f;
            obj.transform.rotation = Quaternion.Euler(fixedRotation.x, randomY, fixedRotation.z);
        }
    }

    private ResourceType GetResourceType()
    {
        switch (resourceTypeName)
        {
            case "Wood": return ResourceType.Wood;
            case "Stone": return ResourceType.Stone;
            default: return ResourceType.Wood;
        }
    }

    private void OnResourceCollected(ResourceSource source)
    {
        if (source == null) return;

        Transform spawnPoint = null;
        foreach (var kvp in spawnPointToResource)
        {
            if (kvp.Value == source)
            {
                spawnPoint = kvp.Key;
                break;
            }
        }

        if (spawnPoint == null)
        {
            return;
        }

        source.OnCollected -= OnResourceCollected;
        spawnPointToResource.Remove(spawnPoint);
        ReturnResourceToPool(source.gameObject);

        respawnQueue.Enqueue(spawnPoint);

        if (!isRespawning)
        {
            ProcessRespawnQueueAsync().Forget();
        }
    }

    private async UniTaskVoid ProcessRespawnQueueAsync()
    {
        if (isRespawning) return;
        isRespawning = true;

        while (respawnQueue.Count > 0)
        {
            Transform point = respawnQueue.Dequeue();

            if (point == null) continue;
            if (spawnPointToResource.ContainsKey(point))
            {
                continue;
            }

            // ===== ÏÐÎÂÅÐÊÀ Ñ ÌÀËÅÍÜÊÈÌ ÐÀÄÈÓÑÎÌ =====
            if (IsPositionOccupied(point.position))
            {
                respawnQueue.Enqueue(point);
                await UniTask.Delay(500);
                continue;
            }

            float respawnDelay = GetRespawnDelay();

            await UniTask.Delay((int)(respawnDelay * 1000));

            SpawnResourceAtPoint(point);
        }

        isRespawning = false;
    }

    private float GetRespawnDelay()
    {
        float baseTime = 7f;
        float multiplier = RespawnSettings.Multiplier;
        return Mathf.Max(0.1f, baseTime / multiplier);
    }

    // ===== ÓÌÅÍÜØÅÍÍÛÉ ÐÀÄÈÓÑ ÏÐÎÂÅÐÊÈ =====
    private bool IsPositionOccupied(Vector3 position)
    {
        // Ïðîâåðÿåì òîëüêî ïðÿìîå ïîïàäàíèå íà òî÷êó
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    private void ProcessRespawnQueue()
    {
        if (respawnQueue.Count == 0) return;

        Transform point = respawnQueue.Dequeue();
        if (point == null || spawnPointToResource.ContainsKey(point))
        {
            ProcessRespawnQueue();
            return;
        }

        if (IsPositionOccupied(point.position))
        {
            respawnQueue.Enqueue(point);
            Invoke(nameof(ProcessRespawnQueue), 0.5f);
            return;
        }

        SpawnResourceAtPoint(point);

        if (respawnQueue.Count > 0)
        {
            Invoke(nameof(ProcessRespawnQueue), 0.5f);
        }
    }

    public int GetActiveResourceCount() => spawnPointToResource.Count;
    public int GetQueueCount() => respawnQueue.Count;
}