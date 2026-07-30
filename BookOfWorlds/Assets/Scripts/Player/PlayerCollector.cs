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

        // 1. Находим любой объект для взаимодействия
        IInteractable interactable = FindInteractable();
        if (interactable != null)
        {
            //  Объект сам решает, что делать (показать уведомление или собрать)
            interactable.Interact();
            return;
        }

        Debug.Log("Рядом нет объектов для взаимодействия");
    }

    /// <summary>
    /// Поиск объекта для взаимодействия в радиусе
    /// </summary>
    private IInteractable FindInteractable()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactRange);
        foreach (var hit in hitColliders)
        {
            // Проверяем IInteractable (универсальный интерфейс)
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
                return interactable;

            // Проверяем в родителе (для животных)
            interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable != null)
                return interactable;
        }
        return null;
    }

    /// <summary>
    /// Универсальный старт сбора для ICollectable
    /// </summary>
    public void StartCollect(ICollectable collectable)
    {
        if (isCollecting) return;

        // Проверяем инвентарь
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

}