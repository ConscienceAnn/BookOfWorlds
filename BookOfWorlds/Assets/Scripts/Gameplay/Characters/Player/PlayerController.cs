using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour
{
    [Inject] private PlayerInputHandler inputHandler;

    private PlayerMovement movement;
    private PlayerCollector collector;
    private Animator animator;

    // ===== НОВАЯ STATE MACHINE =====
    public PlayerStateMachine StateMachine { get; private set; }

    // ===== СВОЙСТВА ДЛЯ СОСТОЯНИЙ =====
    public bool IsMoving => movement != null && movement.IsMoving;
    public bool IsCollecting => collector != null && collector.IsCollecting;

    // ===== СОБЫТИЯ =====
    public event System.Action<ICollectable> OnCollectStart;
    public event System.Action<ICollectable> OnCollectComplete;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        collector = GetComponent<PlayerCollector>();
        animator = GetComponent<Animator>();

        //  Добавляем новую State Machine
        StateMachine = GetComponent<PlayerStateMachine>();
        if (StateMachine == null)
        {
            StateMachine = gameObject.AddComponent<PlayerStateMachine>();
        }

        //  Подписки на события сбора
        if (collector != null)
        {
            collector.OnCollectStart += HandleCollectStart;
            collector.OnCollectComplete += HandleCollectComplete;
        }
    }

    private void OnDestroy()
    {
        if (collector != null)
        {
            collector.OnCollectStart -= HandleCollectStart;
            collector.OnCollectComplete -= HandleCollectComplete;
        }
    }

    // ===== МЕТОД ДЛЯ АНИМАЦИЙ =====
    public void SetAnimation(string parameter, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(parameter, value);
        }
    }

    // ===== ОБРАБОТЧИКИ СОБЫТИЙ =====
    private void HandleCollectStart(ICollectable target)
    {
        OnCollectStart?.Invoke(target);
        Debug.Log($" Событие: Начало сбора {target?.GetResourceName()}");
    }

    private void HandleCollectComplete(ICollectable target)
    {
        OnCollectComplete?.Invoke(target);
        Debug.Log($" Событие: Завершение сбора {target?.GetResourceName()}");
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