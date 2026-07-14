using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerCollector : MonoBehaviour
{
    [Header("Collect Settings")]
    [SerializeField] private float collectDuration = 1.5f;
    [SerializeField] private float collectRange = 2f;
    [SerializeField] private LayerMask collectableLayer;

    private PlayerStateManager stateManager;
    private GameObject currentTarget;
    private bool isCollecting = false;

    // События для оповещения других систем
    public event System.Action<GameObject> OnCollectStart;
    public event System.Action<GameObject> OnCollectComplete;
    public event System.Action OnCollectCancel;

    private void Awake()
    {
        stateManager = GetComponent<PlayerStateManager>();
    }

    public void TryCollect()
    {
        // Если уже собираем - прерываем
        if (isCollecting)
        {
            CancelCollect();
            return;
        }

        // Проверяем, есть ли что собирать рядом
        GameObject target = FindCollectable();
        if (target != null)
        {
            StartCollect(target);
        }
        else
        {
            Debug.Log("Nothing to collect nearby!");
        }
    }

    private GameObject FindCollectable()
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            collectRange,
            collectableLayer
        );

        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Collectable"))
            {
                return hit.gameObject;
            }
        }

        return null;
    }

    private void StartCollect(GameObject target)
    {
        isCollecting = true;
        currentTarget = target;

        // Меняем состояние
        stateManager.ChangeState(PlayerState.Collect);

        // Поворачиваемся к объекту
        RotateToTarget(target);

        // Оповещаем
        OnCollectStart?.Invoke(target);
        Debug.Log($"Started collecting: {target.name}");

        // Запускаем асинхронный сбор
        CollectAsync(target).Forget();
    }

    private void RotateToTarget(GameObject target)
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }

    private async UniTaskVoid CollectAsync(GameObject target)
    {
        float timer = 0f;

        while (timer < collectDuration)
        {
            timer += Time.deltaTime;

            // Проверяем, не исчез ли объект
            if (target == null || !target.activeSelf)
            {
                CancelCollect();
                return;
            }

            await UniTask.Yield(this.GetCancellationTokenOnDestroy());
        }

        // Сбор завершен
        CompleteCollect(target);
    }

    private void CompleteCollect(GameObject target)
    {
        if (target != null && target.activeSelf)
        {
            // Деактивируем объект
            target.SetActive(false);

            // Оповещаем
            OnCollectComplete?.Invoke(target);
            Debug.Log($"Collected: {target.name}");
        }

        FinishCollect();
    }

    private void CancelCollect()
    {
        OnCollectCancel?.Invoke();
        Debug.Log("Collect canceled");
        FinishCollect();
    }

    private void FinishCollect()
    {
        isCollecting = false;
        currentTarget = null;

        // Возвращаемся к движению
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

    // Визуализация
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRange);
    }
}