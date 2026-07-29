using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;

public class PlayerCollector : MonoBehaviour
{
    [Header("Collect Settings")]
    [SerializeField] private float collectDuration = 1.5f;
    [SerializeField] private float interactRange = 2f;

    private PlayerController playerController;
    private ICollectable currentTarget;
    private bool isCollecting = false;

    public event System.Action<ICollectable> OnCollectStart;
    public event System.Action<ICollectable> OnCollectComplete;

    [Inject] private IPlayerInventory inventory;
    [Inject] private PlayerUI playerUI;

    public bool IsCollecting => isCollecting;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void TryInteract()
    {
        if (isCollecting)
        {
            playerUI?.ShowNotification("Уже собираем ресурс...", 1.5f);
            return;
        }

        // 1. Находим любой собираемый объект
        ICollectable collectable = FindCollectable();
        if (collectable != null)
        {
            // Проверяем инвентарь
            if (!inventory.CanAdd(collectable.GetResourceName(), collectable.GetAmount()))
            {
                playerUI?.ShowNotification($"Инвентарь для {collectable.GetResourceName()} полон!");
                return;
            }

            // Запускаем сбор (универсально!)
            StartCollect(collectable);
            return;
        }

        // 2. Зона продажи
        SellZone sellZone = FindSellZone();
        if (sellZone != null)
        {
            sellZone.Sell();
            return;
        }

        // 3. Здание
        BuildingController building = FindBuilding();
        if (building != null)
        {
            Debug.Log($"Найдено здание! Вызываем Interact()");
            building.Interact();
            return;
        }

        Debug.Log("Рядом нет ресурсов, зоны продажи, зданий или животных");
    }

    /// <summary>
    /// Поиск собираемого объекта в радиусе
    /// </summary>
    private ICollectable FindCollectable()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRange);
        foreach (var hit in hitColliders)
        {
            // Проверяем ResourceSource (деревья, камни)
            ResourceSource resource = hit.GetComponent<ResourceSource>();
            if (resource != null && resource.IsAvailable)
                return resource;

            // Проверяем AnimalController (корова)
            AnimalController animal = hit.GetComponentInParent<AnimalController>();
            if (animal != null && animal.IsAvailable)
                return animal;
        }
        return null;
    }

    private SellZone FindSellZone()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRange);
        foreach (var hit in hitColliders)
        {
            SellZone sellZone = hit.GetComponent<SellZone>();
            if (sellZone != null)
                return sellZone;
        }
        return null;
    }

    private BuildingController FindBuilding()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRange);
        foreach (var hit in hitColliders)
        {
            BuildingTrigger trigger = hit.GetComponent<BuildingTrigger>();
            if (trigger != null)
            {
                BuildingController building = trigger.GetComponentInParent<BuildingController>();
                if (building != null && !building.IsRestored())
                    return building;
            }
        }
        return null;
    }

    /// <summary>
    /// Универсальный старт сбора для любого объекта, реализующего ICollectable
    /// </summary>
    private void StartCollect(ICollectable collectable)
    {
        isCollecting = true;
        currentTarget = collectable;

        // Поворачиваемся к объекту
        RotateToTarget(collectable.GetTransform());

        // Вызываем событие
        OnCollectStart?.Invoke(collectable);
        Debug.Log($" Начинаем сбор: {collectable.GetResourceName()}");

        // Запускаем асинхронный сбор
        CollectAsync(collectable).Forget();
    }

    private void RotateToTarget(Transform target)
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }

    /// <summary>
    /// Асинхронный процесс сбора
    /// </summary>
    private async UniTaskVoid CollectAsync(ICollectable collectable)
    {
        float timer = 0f;

        while (timer < collectDuration)
        {
            timer += Time.deltaTime;

            // Проверяем, доступен ли объект
            if (collectable == null || !collectable.IsAvailable)
            {
                Debug.Log("Ресурс пропал во время сбора");
                FinishCollect();
                return;
            }

            await UniTask.Yield(this.GetCancellationTokenOnDestroy());
        }

        CompleteCollect(collectable);
    }

    private void CompleteCollect(ICollectable collectable)
    {
        if (collectable != null && collectable.IsAvailable)
        {
            bool success = collectable.TryCollect();

            if (success)
            {
                OnCollectComplete?.Invoke(collectable);
                Debug.Log($" Собран {collectable.GetResourceName()}");
            }
            else
            {
                Debug.Log($" Не удалось собрать {collectable.GetResourceName()}");
            }
        }
        else
        {
            Debug.Log("Ресурс недоступен для сбора");
        }

        FinishCollect();
    }

    private void FinishCollect()
    {
        isCollecting = false;
        currentTarget = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}