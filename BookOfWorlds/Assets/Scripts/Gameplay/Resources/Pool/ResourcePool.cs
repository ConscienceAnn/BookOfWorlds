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
        // Подписываемся на событие очистки уровня
        LevelGenerator.OnLevelCleared += ClearPool;
    }

    private void OnDisable()
    {
        // Отписываемся от события
        LevelGenerator.OnLevelCleared -= ClearPool;
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
            }
        }

        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    /// <summary>
    /// Очищает пул — вызывается автоматически при очистке уровня
    /// </summary>
    public void ClearPool()
    {
        foreach (var obj in pool)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        pool.Clear();
    }

    public int GetPoolSize()
    {
        return pool.Count;
    }
}