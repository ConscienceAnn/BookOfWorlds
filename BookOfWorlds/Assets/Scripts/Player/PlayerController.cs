using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour
{
    [Inject] private PlayerInputHandler inputHandler;

    private PlayerMovement movement;
    private PlayerCollector collector;
    private PlayerStateManager stateManager;

    // Публичные события для внешних систем
    public event System.Action<ResourceSource> OnCollectStart;   //  Изменено
    public event System.Action<ResourceSource> OnCollectComplete; //  Изменено
    public event System.Action<PlayerState> OnStateChanged;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        collector = GetComponent<PlayerCollector>();
        stateManager = GetComponent<PlayerStateManager>();

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

    //  Исправлено: теперь принимает ResourceSource
    private void HandleCollectStart(ResourceSource target)
    {
        OnCollectStart?.Invoke(target);
    }

    //  Исправлено: теперь принимает ResourceSource
    private void HandleCollectComplete(ResourceSource target)
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
        collector?.TryInteract();
    }

    private void FixedUpdate()
    {
        movement?.FixedUpdateMovement();
    }
}