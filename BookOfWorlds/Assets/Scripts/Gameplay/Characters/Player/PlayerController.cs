using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour
{
    [Inject] private PlayerInputHandlerMy inputHandler;

    private PlayerMovement movement;
    private PlayerCollector collector;
    private Animator animator;

    // ===== ÕŒ¬¿ﬂ STATE MACHINE =====
    public PlayerStateMachine StateMachine { get; private set; }

    // ===== —¬Œ…—“¬¿ ƒÀﬂ —Œ—“ŒﬂÕ»… =====
    public bool IsMoving => movement != null && movement.IsMoving;
    public bool IsCollecting => collector != null && collector.IsCollecting;

    // ===== —Œ¡€“»ﬂ =====
    public event System.Action<ICollectable> OnCollectStart;
    public event System.Action<ICollectable> OnCollectComplete;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        collector = GetComponent<PlayerCollector>();
        animator = GetComponent<Animator>();

        // ƒÓ·‡‚ÎˇÂÏ ÌÓ‚Û˛ State Machine
        StateMachine = GetComponent<PlayerStateMachine>();
        if (StateMachine == null)
        {
            StateMachine = gameObject.AddComponent<PlayerStateMachine>();
        }

        // œÓ‰ÔËÒÍË Ì‡ ÒÓ·˚ÚËˇ Ò·Ó‡
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

    // ===== Ã≈“Œƒ ƒÀﬂ ¿Õ»Ã¿÷»… =====
    public void SetAnimation(string parameter, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(parameter, value);
        }
    }

    // ===== Œ¡–¿¡Œ“◊» » —Œ¡€“»… =====
    private void HandleCollectStart(ICollectable target)
    {
        OnCollectStart?.Invoke(target);
    }

    private void HandleCollectComplete(ICollectable target)
    {
        OnCollectComplete?.Invoke(target);
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