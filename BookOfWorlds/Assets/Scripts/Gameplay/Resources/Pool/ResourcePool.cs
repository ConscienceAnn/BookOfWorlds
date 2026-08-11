using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ResourcePool : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private int initialSize = 15;
    [SerializeField] private ResourceDataSO resourceData;

    [Inject] private DiContainer container;

    private Queue<GameObject> pool = new Queue<GameObject>();

    // ===== UNITY LIFECYCLE =====

    private void Awake()
    {
        if (resourceData == null)
        {
            Debug.LogError($"ResourcePool: resourceData is NULL on {gameObject.name}!");
            return;
        }

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError($"ResourcePool: prefabs array is empty on {gameObject.name}!");
            return;
        }

        Debug.Log($"[ResourcePool] Awake() for {gameObject.name}, resource: {resourceData.resourceName}, префабов: {prefabs.Length}");

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject();
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    private void OnEnable()
    {
        //  Подписываемся на событие очистки уровня
        LevelGenerator.OnLevelCleared += ClearPool;
    }

    private void OnDisable()
    {
        //  Отписываемся от события
        LevelGenerator.OnLevelCleared -= ClearPool;
    }

    // ===== POOL METHODS =====

    private GameObject CreateNewObject()
    {
        GameObject selectedPrefab = prefabs[Random.Range(0, prefabs.Length)];
        Debug.Log($"[ResourcePool] CreateNewObject() for {selectedPrefab?.name ?? "NULL"}");

        if (selectedPrefab == null)
        {
            Debug.LogError($"[ResourcePool] selectedPrefab is NULL!");
            return null;
        }

        GameObject obj;
        if (container != null)
        {
            obj = container.InstantiatePrefab(selectedPrefab);
        }
        else
        {
            obj = Instantiate(selectedPrefab);
        }

        obj.SetActive(false);

        Debug.Log($"[ResourcePool] Created object: {obj.name}, active: {obj.activeSelf}");

        VisualState vs = obj.GetComponent<VisualState>();
        if (vs != null)
        {
            vs.ForceRefresh();
            Debug.Log($"[ResourcePool] VisualState.ForceRefresh() вызван для {obj.name}");
        }
        else
        {
            Debug.LogWarning($"[ResourcePool] VisualState NOT FOUND on {obj.name}!");
        }

        ResourceSource source = obj.GetComponent<ResourceSource>();
        if (source != null)
        {
            source.SetData(resourceData);
            source.SetColored();
            source.Show();
            Debug.Log($"[ResourcePool] ResourceSource настроен для {obj.name}");
        }

        return obj;
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        Debug.Log($"[ResourcePool] Get() called, position: {position}");

        GameObject obj = null;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            Debug.Log($"[ResourcePool] Object taken from pool: {obj?.name ?? "NULL"}");
        }
        else
        {
            Debug.Log($"[ResourcePool] Pool is empty, creating new object");
            obj = CreateNewObject();
        }

        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            obj.SetActive(true);

            ResourceSource source = obj.GetComponent<ResourceSource>();
            if (source != null)
            {
                source.Show();
                Debug.Log($"[ResourcePool] ResourceSource.Show() вызван для {obj.name}");
            }
            else
            {
                Debug.LogWarning($"[ResourcePool] ResourceSource NOT on {obj.name}!");
            }
        }
        else
        {
            Debug.LogWarning($"[ResourcePool] Get() returned NULL!");
        }

        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;

        Debug.Log($"[ResourcePool] Return() called for {obj.name}");

        obj.SetActive(false);
        pool.Enqueue(obj);

        Debug.Log($"[ResourcePool] Object returned to pool, pool size: {pool.Count}");
    }

    /// <summary>
    /// Очищает пул — вызывается автоматически при очистке уровня
    /// </summary>
    public void ClearPool()
    {
        Debug.Log($"[ResourcePool] ClearPool() called for {name}");

        foreach (var obj in pool)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        pool.Clear();
        Debug.Log($"[ResourcePool] Пул {name} очищен, размер: {pool.Count}");
    }

    public int GetPoolSize()
    {
        return pool.Count;
    }
}