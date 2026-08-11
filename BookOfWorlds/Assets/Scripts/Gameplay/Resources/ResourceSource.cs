using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// Управляет состоянием ресурса: доступность, инвентарь, визуал
/// </summary>
public class ResourceSource : MonoBehaviour, ICollectable, IInteractable
{
    [Header("Resource Data")]
    [SerializeField] private ResourceDataSO data;
    [SerializeField] private int amountPerCollect = 1;

    [Header("Visual")]
    [SerializeField] private VisualState visualState;

    [Inject] private IPlayerInventory inventory;
    [Inject] private PauseService pauseService;
    [Inject] private PlayerUI playerUI;

    private bool isAvailable = true;
    private CancellationTokenSource cts;
    private IResourceBehaviour behaviour;

    public event System.Action<ResourceSource> OnCollected;

    // ===== ПУБЛИЧНЫЕ СВОЙСТВА =====
    public bool IsAvailable => isAvailable;
    public string ResourceName => data?.resourceName ?? "Unknown";
    public ResourceDataSO ResourceData => data;
    public int AmountPerCollect => amountPerCollect;

    // ===== РЕАЛИЗАЦИЯ ICollectable =====
    public string GetResourceName() => data?.resourceName ?? "Unknown";
    public int GetAmount() => amountPerCollect;
    public Transform GetTransform() => transform;

    public bool TryCollect()
    {
        return PerformCollect();
    }

    // ===== РЕАЛИЗАЦИЯ IInteractable =====
    public void Interact()
    {
        if (!isAvailable)
        {
            playerUI?.ShowNotification($"Ресурс {GetResourceName()} ещё не восстановился!");
            return;
        }

        if (!inventory.CanAdd(GetResourceName(), GetAmount()))
        {
            playerUI?.ShowNotification($"Инвентарь для {GetResourceName()} полон!");
            return;
        }

        PlayerCollector collector = FindObjectOfType<PlayerCollector>();
        if (collector != null)
        {
            collector.StartCollect(this);
        }
        else
        {
            PerformCollect();
        }
    }

    // ===== ОСНОВНАЯ ЛОГИКА =====
    private bool PerformCollect()
    {
        if (data == null)
        {
            Debug.LogError($"[ResourceSource] data is NULL on {gameObject.name}!");
            return false;
        }

        if (!isAvailable)
        {
            Debug.Log($"[ResourceSource] PerformCollect: isAvailable = false, ресурс ещё не восстановился");
            playerUI?.ShowNotification($"Ресурс {data.resourceName} ещё не восстановился!", 2f);
            return false;
        }

        int currentAmount = inventory.GetAmount(data.resourceName);
        int maxCapacity = inventory.GetMax(data.resourceName);
        bool canAdd = inventory.CanAdd(data.resourceName, amountPerCollect);

        Debug.Log($"[ResourceSource] Проверка добавления {data.resourceName}: current={currentAmount}, max={maxCapacity}, need={amountPerCollect}, canAdd={canAdd}");

        if (!canAdd)
        {
            Debug.Log($"[ResourceSource] Нет места для {data.resourceName}! Текущее: {currentAmount}, Макс: {maxCapacity}");
            playerUI?.ShowNotification($"Инвентарь для {data.resourceName} полон!", 2f);
            return false;
        }

        Debug.Log($"[ResourceSource] Начинаем сбор {data.resourceName} (+{amountPerCollect})");

        inventory.TryAdd(data.resourceName, amountPerCollect);

        isAvailable = false;

        if (behaviour != null)
        {
            behaviour.OnCollect(this);
        }
        else
        {
            Hide();
            RespawnAsync(cts.Token).Forget();
        }

        Debug.Log($"[ResourceSource] Собран {data.resourceName} (+{amountPerCollect})");
        return true;
    }

    // ===== УПРАВЛЕНИЕ ВИЗУАЛОМ =====
    public void SetGray()
    {
        visualState?.SetGray();
    }

    public void SetColored()
    {
        visualState?.SetColored();
    }

    // ===== УПРАВЛЕНИЕ СОСТОЯНИЕМ =====
    public void Hide()
    {
        isAvailable = false;
        SetGray();
        gameObject.SetActive(false);
    }

    public void Show()
    {
        isAvailable = true;
        SetColored();
        gameObject.SetActive(true);
        Debug.Log($"[ResourceSource] Show() вызван для {gameObject.name}, isAvailable={isAvailable}");
    }

    public void ResetState()
    {
        isAvailable = true;
        SetColored();
        gameObject.SetActive(true);
    }

    // ===== ПОВЕДЕНИЕ =====
    public void SetBehaviour(IResourceBehaviour newBehaviour)
    {
        behaviour = newBehaviour;
    }

    // ===== ВЫЗОВ СОБЫТИЯ (из TreeBehaviour/StoneBehaviour) =====
    public void InvokeCollected()
    {
        OnCollected?.Invoke(this);
        Debug.Log($"[ResourceSource] InvokeCollected() вызван для {gameObject.name}");
    }

    // ===== РЕСПАВН =====
    private async UniTaskVoid RespawnAsync(CancellationToken token)
    {
        if (data == null) return;

        float elapsed = 0f;
        float duration = data.respawnTime;

        while (elapsed < duration)
        {
            if (token.IsCancellationRequested)
                return;

            if (pauseService != null && pauseService.IsPaused)
            {
                await UniTask.Yield(token);
                continue;
            }

            elapsed += Time.unscaledDeltaTime;
            await UniTask.Yield(token);
        }

        Show();
        Debug.Log($"[ResourceSource] Ресурс {data.resourceName} восстановился!");
    }

    // ===== UNITY EVENTS =====
    private void Awake()
    {
        if (visualState == null)
        {
            visualState = GetComponent<VisualState>();
            if (visualState == null)
            {
                visualState = GetComponentInChildren<VisualState>();
            }
        }
    }

    private void Start()
    {
        SetColored();
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====
    public void SetData(ResourceDataSO newData)
    {
        data = newData;
    }

    public bool HasInventory()
    {
        return inventory != null;
    }

    public void SetRespawnTime(float time)
    {
        if (data != null)
        {
            data.respawnTime = time;
        }
    }
}