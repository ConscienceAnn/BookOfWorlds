using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ResourceSource : MonoBehaviour, ICollectable, IInteractable
{
    [Header("Resource Data")]
    [SerializeField] private ResourceDataSO data;
    [SerializeField] private int amountPerCollect = 1;

    [Header("Visual")]
    [SerializeField] private VisualState visualState;

    [Inject] private IPlayerInventory inventory;
    [Inject] private PauseService pauseService;
    [Inject] private PlayerUIMediator playerUIMediator;
    [Inject] private UpgradeManager upgradeManager;
    [Inject] private PlayerCollector playerCollector;

    private bool isAvailable = true;
    private CancellationTokenSource cts;
    private IResourceBehaviour behaviour;

    // ===== ДЛЯ РЕСПАВНА =====
    private float currentRespawnDuration;
    private bool isRespawning = false;

    public event System.Action<ResourceSource> OnCollected;

    public bool IsAvailable => isAvailable;
    public string ResourceName => data?.resourceName ?? "Unknown";
    public ResourceDataSO ResourceData => data;
    public int AmountPerCollect => amountPerCollect;

    public string GetResourceName() => data?.resourceName ?? "Unknown";
    public int GetAmount() => amountPerCollect;
    public Transform GetTransform() => transform;

    public bool TryCollect()
    {
        return PerformCollect();
    }

    public void Interact()
    {
        if (!isAvailable)
        {
            playerUIMediator?.ShowNotification($"Ресурс {GetResourceName()} ещё не восстановился!", 2f);
            return;
        }

        if (!inventory.CanAdd(GetResourceName(), GetAmount()))
        {
            playerUIMediator?.ShowNotification($"Инвентарь для {GetResourceName()} полон!", 2f);
            return;
        }

        if (playerCollector != null)
        {
            playerCollector.StartCollect(this);
        }
        else
        {
            PerformCollect();
        }
    }

    private bool PerformCollect()
    {
        if (data == null)
        {
            Debug.LogError($"ResourceSource: data is NULL on {gameObject.name}!");
            return false;
        }

        if (!isAvailable)
        {
            playerUIMediator?.ShowNotification($"Ресурс {data.resourceName} ещё не восстановился!", 2f);
            return false;
        }

        if (!inventory.CanAdd(data.resourceName, amountPerCollect))
        {
            playerUIMediator?.ShowNotification($"Инвентарь для {data.resourceName} полон!", 2f);
            return false;
        }

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

        return true;
    }

    public void SetGray()
    {
        visualState?.SetGray();
    }

    public void SetColored()
    {
        visualState?.SetColored();
    }

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
        Debug.Log($"[ResourceSource] {data?.resourceName ?? "Unknown"} восстановился!");
    }

    public void ResetState()
    {
        isAvailable = true;
        SetColored();
        gameObject.SetActive(true);
    }

    public void SetBehaviour(IResourceBehaviour newBehaviour)
    {
        behaviour = newBehaviour;
    }

    public void InvokeCollected()
    {
        OnCollected?.Invoke(this);
    }

    // ===== РЕСПАВН С УЧЁТОМ УЛУЧШЕНИЙ =====

    private async UniTaskVoid RespawnAsync(CancellationToken token)
    {
        if (data == null) return;

        isRespawning = true;

        float multiplier = RespawnSettings.Multiplier;
        float remainingTime = data.respawnTime / multiplier;
        currentRespawnDuration = remainingTime;

        Debug.Log($"[ResourceSource] === РЕСПАВН СТАРТ ===");
        Debug.Log($"[ResourceSource]   - Ресурс: {data.resourceName}");
        Debug.Log($"[ResourceSource]   - Базовое время: {data.respawnTime:F2} сек");
        Debug.Log($"[ResourceSource]   - Итоговое время: {remainingTime:F2} сек (x{multiplier})");
        Debug.Log($"[ResourceSource]   - Начало: {System.DateTime.Now:HH:mm:ss.fff}");

        while (remainingTime > 0)
        {
            if (token.IsCancellationRequested)
            {
                isRespawning = false;
                Debug.Log($"[ResourceSource] {data.resourceName}: респавн отменён");
                return;
            }

            if (pauseService != null && pauseService.IsPaused)
            {
                await UniTask.Yield(token);
                continue;
            }

            float delta = Time.unscaledDeltaTime;
            remainingTime -= delta;
            currentRespawnDuration = remainingTime;

            await UniTask.Yield(token);
        }

        Debug.Log($"[ResourceSource] === РЕСПАВН ЗАВЕРШЁН ===");
        Debug.Log($"[ResourceSource]   - Ресурс: {data.resourceName}");
        Debug.Log($"[ResourceSource]   - Завершение: {System.DateTime.Now:HH:mm:ss.fff}");

        isRespawning = false;
        Show();
    }

    // ===== ОБНОВЛЕНИЕ ВРЕМЕНИ РЕСПАВНА ПРИ УЛУЧШЕНИИ =====

    private void OnRespawnMultiplierChanged()
    {
        if (data == null) return;

        float multiplier = RespawnSettings.Multiplier;

        if (!isRespawning)
        {
            currentRespawnDuration = data.respawnTime / multiplier;
            Debug.Log($"[ResourceSource] {data.resourceName}: время обновлено для будущего -> {currentRespawnDuration:F2} сек (x{multiplier})");
            return;
        }

        // Ресурс на респавне - корректируем оставшееся время
        float baseRespawnTime = data.respawnTime;
        float baseElapsed = baseRespawnTime - currentRespawnDuration;
        float newTotalTime = baseRespawnTime / multiplier;
        float newRemaining = newTotalTime - baseElapsed;
        currentRespawnDuration = Mathf.Max(0.05f, newRemaining);

        Debug.Log($"[ResourceSource] {data.resourceName}: время скорректировано!");
        Debug.Log($"[ResourceSource]   - Базовое время: {baseRespawnTime:F2} сек");
        Debug.Log($"[ResourceSource]   - Прошло: {baseElapsed:F2} сек");
        Debug.Log($"[ResourceSource]   - Новое полное: {newTotalTime:F2} сек");
        Debug.Log($"[ResourceSource]   - Осталось: {currentRespawnDuration:F2} сек (x{multiplier})");
    }

    // ===== UNITY LIFECYCLE =====

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

        cts = new CancellationTokenSource();

        EventBus.OnRespawnMultiplierChanged += OnRespawnMultiplierChanged;
    }

    private void Start()
    {
        SetColored();
    }

    private void OnDestroy()
    {
        EventBus.OnRespawnMultiplierChanged -= OnRespawnMultiplierChanged;
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

    public void ApplyCurrentMultiplier()
    {
        float multiplier = RespawnSettings.Multiplier;
        if (data != null)
        {
            currentRespawnDuration = data.respawnTime / multiplier;
            Debug.Log($"[ResourceSource] {data.resourceName}: применён множитель {multiplier}x, время респавна: {currentRespawnDuration:F2} сек");
        }
    }

    public void ReturnToPool()
    {
        // Отписываемся от событий перед возвратом в пул
        EventBus.OnRespawnMultiplierChanged -= OnRespawnMultiplierChanged;

        // Сбрасываем состояние
        isAvailable = true;
        isRespawning = false;
        SetColored();
        gameObject.SetActive(false);
    }
}