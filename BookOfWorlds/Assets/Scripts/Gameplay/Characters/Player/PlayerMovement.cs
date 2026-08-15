using UnityEngine;
using Zenject;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float runThreshold = 0.5f;
    [SerializeField] private float minMoveThreshold = 0.1f;

    [Header("Footstep Settings")]
    [SerializeField] private float stepInterval = 0.5f;

    [Inject] private Camera mainCamera;
    [Inject] private AudioHelper _audioHelper;
    [Inject] private PauseService _pauseService;

    private Rigidbody rb;
    private PlayerController playerController;
    private Vector2 moveInput;
    private Vector3 moveDirection;
    private float currentSpeed;
    private float stepTimer = 0f;

    public Vector3 MoveDirection => moveDirection;
    public bool IsMoving => moveInput.magnitude > minMoveThreshold;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationY |
                        RigidbodyConstraints.FreezeRotationZ;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.maxAngularVelocity = 0.01f;
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void FixedUpdateMovement()
    {
        // Проверяем, не собирает ли игрок
        if (playerController != null && playerController.IsCollecting)
        {
            // Если собирает — останавливаемся
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        Move();
        Rotate();

        // Обновляем состояние в зависимости от движения
        UpdateMovementState();

        // ===== ШАГИ =====
        UpdateFootsteps();
    }

    private void UpdateMovementState()
    {
        if (playerController == null || playerController.StateMachine == null) return;

        float inputMagnitude = moveInput.magnitude;

        if (inputMagnitude < minMoveThreshold)
        {
            currentSpeed = 0f;
        }
        else if (inputMagnitude > runThreshold)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
    }

    private void Move()
    {
        if (mainCamera == null) return;

        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * moveInput.y + right * moveInput.x);

        if (moveDirection.magnitude > minMoveThreshold)
        {
            moveDirection.Normalize();
            Vector3 velocity = moveDirection * currentSpeed;
            velocity.y = rb.velocity.y;
            rb.velocity = velocity;
        }
        else
        {
            Vector3 velocity = rb.velocity;
            velocity.x = 0;
            velocity.z = 0;
            rb.velocity = velocity;
            moveDirection = Vector3.zero;
        }
    }

    private void Rotate()
    {
        if (moveDirection.magnitude > minMoveThreshold && currentSpeed > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    // ===== НОВЫЙ МЕТОД: ШАГИ =====
    private void UpdateFootsteps()
    {
        // Проверяем, движется ли игрок и не на паузе
        bool isMoving = moveInput.magnitude > minMoveThreshold;
        bool isPaused = _pauseService != null && _pauseService.IsPaused;

        if (isMoving && !isPaused && playerController != null && !playerController.IsCollecting)
        {
            stepTimer += Time.fixedDeltaTime;
            if (stepTimer >= stepInterval)
            {
                stepTimer = 0f;
                _audioHelper?.PlaySound("footstep");
            }
        }
        else
        {
            stepTimer = stepInterval; // Сброс таймера при остановке
        }
    }
}