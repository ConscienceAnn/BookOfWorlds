using UnityEngine;
using Zenject;

public class ResourceSource : MonoBehaviour
{
    [Header("Настройки ресурса")]
    [SerializeField] private ResourceDataSO data;
    [SerializeField] private int amountPerCollect = 1;

    [Inject] private IPlayerInventory inventory;

    private bool isAvailable = true;
    private float respawnTimer = 0f;

    public string ResourceName => data.resourceName;
    public bool IsAvailable => isAvailable;

    private void Update()
    {
        if (!isAvailable)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0)
            {
                isAvailable = true;
                gameObject.SetActive(true);
                Debug.Log($" Ресурс {data.resourceName} восстановился!");
            }
        }
    }

    public void Interact()
    {
        if (!isAvailable)
        {
            Debug.Log($" Ресурс {data.resourceName} ещё не восстановился!");
            return;
        }

        // Проверяем, есть ли место в инвентаре
        if (!inventory.CanAdd(data.resourceName, amountPerCollect))
        {
            Debug.Log($" Нет места для {data.resourceName}! Инвентарь полон.");
            return;
        }

        // Добавляем ресурс в инвентарь
        inventory.TryAdd(data.resourceName, amountPerCollect);

        // Ресурс исчезает
        isAvailable = false;
        respawnTimer = data.respawnTime;
        gameObject.SetActive(false);

        Debug.Log($" Собран {data.resourceName} (+{amountPerCollect})");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isAvailable ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}