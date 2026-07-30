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
    private GameObject defaultPrefab;

    private void Awake()
    {
        if (resourceData == null)
        {
            Debug.LogError($"ResourcePool: resourceData is NULL on {gameObject.name}!");
            return;
        }

        if (prefabs != null && prefabs.Length > 0)
        {
            defaultPrefab = prefabs[0];
        }
        else
        {
            Debug.LogError($"ResourcePool: prefabs array is empty on {gameObject.name}!");
            return;
        }

        Debug.Log($"[ResourcePool] Awake() for {gameObject.name}, resource: {resourceData.resourceName}");

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject();
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    private GameObject CreateNewObject()
    {
        Debug.Log($"[ResourcePool] CreateNewObject() for {defaultPrefab?.name ?? "NULL"}");

        if (defaultPrefab == null)
        {
            Debug.LogError($"[ResourcePool] defaultPrefab is NULL!");
            return null;
        }

        GameObject obj;
        if (container != null)
        {
            obj = container.InstantiatePrefab(defaultPrefab);
        }
        else
        {
            obj = Instantiate(defaultPrefab);
        }

        //  Сразу делаем неактивным (как и положено в пуле)
        obj.SetActive(false);

        Debug.Log($"[ResourcePool] Created object: {obj.name}, active: {obj.activeSelf}");

        //  ПРИНУДИТЕЛЬНО ОБНОВЛЯЕМ VisualState
        VisualState vs = obj.GetComponent<VisualState>();
        if (vs != null)
        {
            vs.ForceRefresh(); // Принудительно находим рендеры и применяем цвет
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
            source.Show(); //  Делаем доступным
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

            //  Активируем объект
            obj.SetActive(true);

            // Убеждаемся, что ресурс доступен и цветной
            ResourceSource source = obj.GetComponent<ResourceSource>();
            if (source != null)
            {
                source.Show(); //  Гарантируем состояние
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

        //  Деактивируем объект
        obj.SetActive(false);
        pool.Enqueue(obj);

        Debug.Log($"[ResourcePool] Object returned to pool, pool size: {pool.Count}");
    }

    public int GetPoolSize()
    {
        return pool.Count;
    }
}