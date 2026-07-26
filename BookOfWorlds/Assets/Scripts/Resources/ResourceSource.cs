using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ResourceSource : MonoBehaviour
{
    [Header("Настройки ресурса")]
    [SerializeField] private ResourceDataSO data;
    [SerializeField] private int amountPerCollect = 1;

    [Header("Visual")]
    [SerializeField] private VisualState visualState;

    [Inject] private IPlayerInventory inventory;
    [Inject] private PauseService pauseService;

    private bool isAvailable = true;
    private CancellationTokenSource cts;

    public string ResourceName => data?.resourceName ?? "Unknown";

    public ResourceDataSO ResourceData => data;
    public bool IsAvailable => isAvailable;

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
        Debug.Log($"ResourceSource.SetData: вызыван на {gameObject.name}, newData = {(newData != null ? newData.resourceName : "NULL")}");
        data = newData;
        Debug.Log($"ResourceSource.SetData: data теперь = {(data != null ? data.resourceName : "NULL")}");
    }

    public void Interact()
    {
         Debug.Log($"ResourceSource.Interact: вызван на {gameObject.name}, data = {(data != null ? data.resourceName : "NULL")}");
    
    if (data == null)
    {
        Debug.LogError($"ResourceSource: data is NULL on {gameObject.name}!");
        return;
    }

        if (!isAvailable)
        {
            Debug.Log($"Ресурс {data.resourceName} ещё не восстановился!");
            return;
        }

        if (!inventory.CanAdd(data.resourceName, amountPerCollect))
        {
            Debug.Log($"Нет места для {data.resourceName}!");
            return;
        }

        inventory.TryAdd(data.resourceName, amountPerCollect);

        isAvailable = false;

        if (visualState != null)
        {
            visualState.SetGray();
        }

        OnCollected?.Invoke(this);

        Debug.Log($"Собран {data.resourceName} (+{amountPerCollect})");
    }

    public event System.Action<ResourceSource> OnCollected;

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

        isAvailable = true;

        if (visualState != null)
        {
            visualState.SetColored();
        }

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