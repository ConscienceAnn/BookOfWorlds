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

        while (remainingTime > 0)
        {
            if (token.IsCancellationRequested)
            {
                isRespawning = false;
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
            return;
        }

        // Ресурс на респавне - корректируем оставшееся время
        float baseRespawnTime = data.respawnTime;
        float baseElapsed = baseRespawnTime - currentRespawnDuration;
        float newTotalTime = baseRespawnTime / multiplier;
        float newRemaining = newTotalTime - baseElapsed;
        currentRespawnDuration = Mathf.Max(0.05f, newRemaining);
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