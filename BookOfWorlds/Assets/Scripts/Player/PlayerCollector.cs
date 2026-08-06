using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;

public class PlayerCollector : MonoBehaviour
{
    [Header("Collect Settings")]
    [SerializeField] private float collectDuration = 1.5f;
    [SerializeField] private float interactRange = 2f;

    private ICollectable currentTarget;
    private bool isCollecting = false;

    public event System.Action<ICollectable> OnCollectStart;
    public event System.Action<ICollectable> OnCollectComplete;

    [Inject] private IPlayerInventory inventory;
    [Inject] private PlayerUI playerUI;

    public bool IsCollecting => isCollecting;

    public void TryInteract()
    {
        if (isCollecting)
        {
            playerUI?.ShowNotification("Уже собираем ресурс...", 1.5f);
            return;
        }

        // 1. Сначала проверяем, есть ли рядом НЕДОСТУПНАЯ корова
        AnimalController unavailableAnimal = FindUnavailableAnimal();
        if (unavailableAnimal != null)
        {
            string message = $"{unavailableAnimal.GetAnimalName()} ещё не готова дать {unavailableAnimal.GetResourceName()}!";
            playerUI?.ShowNotification(message, 2f);
            return;
        }

        // 2. Находим ДОСТУПНЫЙ собираемый объект
        ICollectable collectable = FindCollectable();
        if (collectable != null)
        {
            StartCollect(collectable);
            return;
        }

        // 3. Зона продажи
        SellZone sellZone = FindSellZone();
        if (sellZone != null)
        {
            sellZone.Sell();
            return;
        }

        // 4. Здание
        BuildingController building = FindBuilding();
        if (building != null)
        {
            building.Interact();
            return;
        }
    }

    private AnimalController FindUnavailableAnimal()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRange);
        foreach (var hit in hitColliders)
        {
            AnimalController animal = hit.GetComponentInParent<AnimalController>();
            if (animal != null && !animal.IsAvailable)
                return animal;
        }
        return null;
    }

    private ICollectable FindCollectable()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRange);
        foreach (var hit in hitColliders)
        {
            ResourceSource resource = hit.GetComponent<ResourceSource>();
            if (resource != null && resource.IsAvailable && resource.gameObject.activeSelf)
                return resource;

            AnimalController animal = hit.GetComponentInParent<AnimalController>();
            if (animal != null && animal.IsAvailable && animal.gameObject.activeSelf)
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

    public void StartCollect(ICollectable collectable)
    {
        if (isCollecting) return;

        if (!inventory.CanAdd(collectable.GetResourceName(), collectable.GetAmount()))
        {
            playerUI?.ShowNotification($"Инвентарь для {collectable.GetResourceName()} полон!");
            return;
        }

        isCollecting = true;
        currentTarget = collectable;

        RotateToTarget(collectable.GetTransform());

        OnCollectStart?.Invoke(collectable);
        Debug.Log($"Начинаем сбор: {collectable.GetResourceName()}");

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

    private async UniTaskVoid CollectAsync(ICollectable collectable)
    {
        float timer = 0f;

        while (timer < collectDuration)
        {
            timer += Time.deltaTime;

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
                Debug.Log($"Собран {collectable.GetResourceName()}");
            }
            else
            {
                Debug.Log($"Не удалось собрать {collectable.GetResourceName()}");
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