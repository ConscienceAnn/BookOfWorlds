using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerCollector : MonoBehaviour
{
    [Header("Collect Settings")]
    [SerializeField] private float collectDuration = 1.5f;
    [SerializeField] private float interactRange = 2f;

    private PlayerStateManager stateManager;
    private ResourceSource currentTarget;
    private bool isCollecting = false;

    public event System.Action<ResourceSource> OnCollectStart;
    public event System.Action<ResourceSource> OnCollectComplete;

    private void Awake()
    {
        stateManager = GetComponent<PlayerStateManager>();
    }

    public void TryInteract()
    {
        if (isCollecting)
        {
            Debug.Log("Уже собираем ресурс...");
            return;
        }

        ResourceSource target = FindCollectable();
        if (target != null)
        {
            StartCollect(target);
            return;
        }

        SellZone sellZone = FindSellZone();
        if (sellZone != null)
        {
            sellZone.Sell();
            return;
        }

        BuildingController building = FindBuilding();
        if (building != null)
        {
            Debug.Log($"Найдено здание! Вызываем Interact()");
            building.Interact();
            return;
        }

        AnimalController animal = FindAnimal();
        if (animal != null)
        {
            animal.Interact();
            return;
        }

        Debug.Log("Рядом нет ресурсов, зоны продажи, зданий или животных");
    }

    private ResourceSource FindCollectable()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRange);
        foreach (var hit in hitColliders)
        {
            ResourceSource resource = hit.GetComponent<ResourceSource>();
            if (resource != null && resource.IsAvailable)
                return resource;
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

    private AnimalController FindAnimal()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRange);
        foreach (var hit in hitColliders)
        {
            AnimalController animal = hit.GetComponent<AnimalController>();
            if (animal != null && animal.IsAvailable)
                return animal;
        }
        return null;
    }

    private void StartCollect(ResourceSource target)
    {
        isCollecting = true;
        currentTarget = target;

        stateManager.ChangeState(PlayerState.Collect);
        RotateToTarget(target.transform);

        OnCollectStart?.Invoke(target);
        Debug.Log($"Начинаем сбор: {target.ResourceName}");

        CollectAsync(target).Forget();
    }

    private void RotateToTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }

    private async UniTaskVoid CollectAsync(ResourceSource target)
    {
        float timer = 0f;

        while (timer < collectDuration)
        {
            timer += Time.deltaTime;

            if (target == null || !target.IsAvailable)
            {
                Debug.Log("Ресурс пропал во время сбора");
                FinishCollect();
                return;
            }

            await UniTask.Yield(this.GetCancellationTokenOnDestroy());
        }

        CompleteCollect(target);
    }

    private void CompleteCollect(ResourceSource target)
    {
        if (target != null && target.IsAvailable)
        {
            target.Interact();
            OnCollectComplete?.Invoke(target);
            Debug.Log($"Собран {target.ResourceName}");
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

        var movement = GetComponent<PlayerMovement>();
        if (movement != null && movement.IsMoving)
        {
            stateManager.ChangeState(PlayerState.Run);
        }
        else
        {
            stateManager.ChangeState(PlayerState.Idle);
        }
    }

    public bool IsCollecting() => isCollecting;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}