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

    // ===== ЧЕРЕЗ DI =====
    [Inject] private PlayerCollector playerCollector;

    private bool isAvailable = true;
    private CancellationTokenSource cts;
    private IResourceBehaviour behaviour;

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

        // ===== ИСПОЛЬЗУЕМ INJECTED COLLECTOR =====
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

        int currentAmount = inventory.GetAmount(data.resourceName);
        int maxCapacity = inventory.GetMax(data.resourceName);
        bool canAdd = inventory.CanAdd(data.resourceName, amountPerCollect);

        if (!canAdd)
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
    }

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