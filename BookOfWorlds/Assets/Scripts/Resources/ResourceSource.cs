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
    [SerializeField] private VisualState visualState; //  НОВОЕ

    [Inject] private IPlayerInventory inventory;
    [Inject] private PauseService pauseService;

    private bool isAvailable = true;
    private CancellationTokenSource cts;

    public string ResourceName => data.resourceName;
    public bool IsAvailable => isAvailable;

    private void Awake()
    {
        // Если VisualState не назначен — пытаемся найти
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
        // Изначально цветной
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

    public void Interact()
    {
        if (!isAvailable)
        {
            Debug.Log($" Ресурс {data.resourceName} ещё не восстановился!");
            return;
        }

        if (!inventory.CanAdd(data.resourceName, amountPerCollect))
        {
            Debug.Log($" Нет места для {data.resourceName}!");
            return;
        }

        inventory.TryAdd(data.resourceName, amountPerCollect);

        isAvailable = false;

        //  Становимся серым
        if (visualState != null)
        {
            visualState.SetGray();
        }

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();

        RespawnAsync(cts.Token).Forget();

        Debug.Log($" Собран {data.resourceName} (+{amountPerCollect})");
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

        isAvailable = true;

        // Становимся цветным
        if (visualState != null)
        {
            visualState.SetColored();
        }

        Debug.Log($" Ресурс {data.resourceName} восстановился!");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isAvailable ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}