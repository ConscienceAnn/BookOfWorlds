using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ResourceSource : MonoBehaviour, ICollectable
{
    [Header("Настройки ресурса")]
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

    // ===== РЕАЛИЗАЦИЯ ICollectable =====
    public bool IsAvailable => isAvailable;
    public string GetResourceName() => data?.resourceName ?? "Unknown";
    public int GetAmount() => amountPerCollect;
    public Transform GetTransform() => transform;

    public bool TryCollect()
    {
        return Interact();
    }
    // ===== КОНЕЦ РЕАЛИЗАЦИИ =====

    public string ResourceName => data?.resourceName ?? "Unknown";
    public ResourceDataSO ResourceData => data;
    public int AmountPerCollect => amountPerCollect;

    public void SetBehaviour(IResourceBehaviour newBehaviour)
    {
        behaviour = newBehaviour;
        Debug.Log($"ResourceSource: поведение установлено для {gameObject.name}");
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
        if (visualState != null)
        {
            visualState.SetColored();
        }
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    public void SetData(ResourceDataSO newData)
    {
        data = newData;
        Debug.Log($"ResourceSource.SetData: data теперь = {data?.resourceName ?? "NULL"}");
    }

    public bool Interact()
    {
        if (data == null)
        {
            Debug.LogError($"ResourceSource: data is NULL on {gameObject.name}!");
            return false;
        }

        if (!isAvailable)
        {
            playerUI?.ShowNotification($"Ресурс {data.resourceName} ещё не восстановился!");
            return false;
        }

        if (!inventory.CanAdd(data.resourceName, amountPerCollect))
        {
            playerUI?.ShowNotification($"Инвентарь для {data.resourceName} полон!");
            return false;
        }

        inventory.TryAdd(data.resourceName, amountPerCollect);

        isAvailable = false;

        if (visualState != null)
        {
            visualState.SetGray();
        }

        if (behaviour != null)
        {
            behaviour.OnCollect(this);
        }
        else
        {
            Hide();
            RespawnAsync(cts.Token).Forget();
        }

        OnCollected?.Invoke(this);

        Debug.Log($"Собран {data.resourceName} (+{amountPerCollect})");
        return true;
    }

    public event System.Action<ResourceSource> OnCollected;

    public void Hide()
    {
        isAvailable = false;
        if (visualState != null)
        {
            visualState.SetGray();
        }
        gameObject.SetActive(false);
    }

    public void Show()
    {
        isAvailable = true;
        if (visualState != null)
        {
            visualState.SetColored();
        }
        gameObject.SetActive(true);
    }

    public void SetGray()
    {
        if (visualState != null)
        {
            visualState.SetGray();
        }
    }

    public void SetColored()
    {
        if (visualState != null)
        {
            visualState.SetColored();
        }
    }

    private async UniTaskVoid RespawnAsync(CancellationToken token)
    {
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
        Debug.Log($"Ресурс {data.resourceName} восстановился!");
    }

    public void ResetState()
    {
        isAvailable = true;
        if (visualState != null)
        {
            visualState.SetColored();
        }
        gameObject.SetActive(true);
    }

    public bool HasInventory()
    {
        return inventory != null;
    }
}