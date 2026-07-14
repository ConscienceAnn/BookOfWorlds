using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour
{
    [Inject] private PlayerInputHandler inputHandler;

    private PlayerMovement movement;
    private PlayerCollector collector;
    private PlayerStateManager stateManager;

    // Публичные события для внешних систем
    public event System.Action<GameObject> OnCollectStart;
    public event System.Action<GameObject> OnCollectComplete;
    public event System.Action<PlayerState> OnStateChanged;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        collector = GetComponent<PlayerCollector>();
        stateManager = GetComponent<PlayerStateManager>();

        // Подписываемся на события внутренних компонентов
        if (collector != null)
        {
            collector.OnCollectStart += HandleCollectStart;
            collector.OnCollectComplete += HandleCollectComplete;
        }

        if (stateManager != null)
        {
            stateManager.OnStateChanged += HandleStateChanged;
        }
    }

    private void OnDestroy()
    {
        // Отписываемся
        if (collector != null)
        {
            collector.OnCollectStart -= HandleCollectStart;
            collector.OnCollectComplete -= HandleCollectComplete;
        }

        if (stateManager != null)
        {
            stateManager.OnStateChanged -= HandleStateChanged;
        }
    }

    // Обработчики-обертки для проброса событий наружу
    private void HandleCollectStart(GameObject target)
    {
        OnCollectStart?.Invoke(target);
    }

    private void HandleCollectComplete(GameObject target)
    {
        OnCollectComplete?.Invoke(target);
    }

    private void HandleStateChanged(PlayerState state)
    {
        OnStateChanged?.Invoke(state);
    }

    private void OnEnable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnMovementInput += HandleMovementInput;
            inputHandler.OnCollectInput += HandleCollectInput;
        }
    }

    private void OnDisable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnMovementInput -= HandleMovementInput;
            inputHandler.OnCollectInput -= HandleCollectInput;
        }
    }

    private void HandleMovementInput(Vector2 input)
    {
        movement?.SetMoveInput(input);
    }

    private void HandleCollectInput()
    {
        collector?.TryCollect();
    }

    private void FixedUpdate()
    {
        movement?.FixedUpdateMovement();
    }
}