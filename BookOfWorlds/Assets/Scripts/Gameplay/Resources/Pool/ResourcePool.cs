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

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject();
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    private void OnEnable()
    {
        LevelGenerator.OnLevelCleared += ClearPool;
    }

    private void OnDisable()
    {
        LevelGenerator.OnLevelCleared -= ClearPool;
        // При отключении пула очищаем
        ClearPool();
    }

    private void OnApplicationQuit()
    {
        // При выходе из игры принудительно очищаем пул
        ClearPool();
    }

    // ===== POOL METHODS =====

    private GameObject CreateNewObject()
    {
        GameObject selectedPrefab = prefabs[Random.Range(0, prefabs.Length)];

        if (selectedPrefab == null)
        {
            Debug.LogError($"ResourcePool: selectedPrefab is NULL!");
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

        VisualState vs = obj.GetComponent<VisualState>();
        if (vs != null)
        {
            vs.ForceRefresh();
        }

        ResourceSource source = obj.GetComponent<ResourceSource>();
        if (source != null)
        {
            source.SetData(resourceData);
            source.SetColored();
            source.Show();
            source.ApplyCurrentMultiplier();
        }

        return obj;
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = null;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
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
                source.ApplyCurrentMultiplier();
                Debug.Log($"[ResourcePool] {resourceData?.resourceName ?? "Resource"} получен из пула с множителем {RespawnSettings.Multiplier}x");
            }
        }

        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;

        ResourceSource source = obj.GetComponent<ResourceSource>();
        if (source != null)
        {
            source.ReturnToPool();
        }

        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    public void ClearPool()
    {
        // При очистке пула мы УНИЧТОЖАЕМ объекты, а не возвращаем в пул
        // Поэтому НЕ нужно вызывать ReturnToPool() — объекты всё равно удаляются
        foreach (var obj in pool)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        pool.Clear();

        // Также удаляем все активные объекты
        ResourceSource[] activeSources = FindObjectsOfType<ResourceSource>();
        foreach (var source in activeSources)
        {
            if (source != null && source.gameObject != null)
            {
                Destroy(source.gameObject);
            }
        }

        Debug.Log($"[ResourcePool] Пул очищен для {resourceData?.resourceName ?? "Unknown"}");
    }

    public int GetPoolSize()
    {
        return pool.Count;
    }
}