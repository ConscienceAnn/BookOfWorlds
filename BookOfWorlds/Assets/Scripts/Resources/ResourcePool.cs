using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ResourcePool : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private int initialSize = 10;
    [SerializeField] private ResourceDataSO resourceData;

    [Inject] private DiContainer container; // Zenject контейнер

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        if (resourceData == null)
        {
            Debug.LogError($"ResourcePool: resourceData is NULL on {gameObject.name}!");
        }

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject();
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    private GameObject CreateNewObject()
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError("ResourcePool: prefabs array is empty!");
            return null;
        }

        GameObject selectedPrefab = prefabs[Random.Range(0, prefabs.Length)];

        // ВАЖНО: создаём через Zenject, чтобы зависимости внедрились
        GameObject obj = container.InstantiatePrefab(selectedPrefab, transform);
        obj.SetActive(false);

        ResourceSource source = obj.GetComponent<ResourceSource>();
        if (source != null && resourceData != null)
        {
            source.SetData(resourceData);
        }

        return obj;
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = CreateNewObject();
            Debug.Log("ResourcePool: expanded, created new object.");
        }

        if (obj == null) return null;

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        //  Проверяем, что инъекции внедрены
        ResourceSource source = obj.GetComponent<ResourceSource>();
        if (source != null)
        {
            // Если inventory всё ещё null — внедряем вручную
            if (source.HasInventory() == false)
            {
                container.Inject(source);
                Debug.Log($"ResourcePool: manually injected dependencies into {obj.name}");
            }

            if (resourceData != null)
            {
                source.SetData(resourceData);
            }
            source.ResetState();
        }

        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}