using UnityEngine;
using Zenject;

public class ResourceSource : MonoBehaviour
{
    [Header("Настройки ресурса")]
    [SerializeField] private ResourceDataSO data;
    [SerializeField] private int amountPerCollect = 1;

    [Inject] private IPlayerInventory inventory;

    private bool isAvailable = true;
    private ResourceRespawner respawner;

    public string ResourceName => data.resourceName;
    public bool IsAvailable => isAvailable;

    private void Awake()
    {
        // Создаём респавнер с колбэком на восстановление
        respawner = new ResourceRespawner(data.respawnTime, OnRespawnComplete);
    }

    private void OnDestroy()
    {
        respawner?.Cancel();
    }

    public void Interact()
    {
        if (!isAvailable)
        {
            Debug.Log($" Ресурс {data.resourceName} ещё не восстановился!");
            return;
        }

        if (!inventory.CanAdd(data.resourceName, amountPerCollect))
        {
            Debug.Log($" Нет места для {data.resourceName}!");
            return;
        }

        inventory.TryAdd(data.resourceName, amountPerCollect);

        // Ресурс собран
        isAvailable = false;
        gameObject.SetActive(false);

        // Запускаем респавн
        respawner.StartRespawn();

        Debug.Log($" Собран {data.resourceName} (+{amountPerCollect})");
    }

    private void OnRespawnComplete()
    {
        isAvailable = true;
        gameObject.SetActive(true);
        Debug.Log($" Ресурс {data.resourceName} восстановился!");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isAvailable ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}