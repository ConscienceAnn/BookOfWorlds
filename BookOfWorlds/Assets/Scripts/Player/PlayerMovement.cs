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

    [Inject] private Camera mainCamera;

    private Rigidbody rb;
    private PlayerStateManager stateManager;
    private Vector2 moveInput;
    private Vector3 moveDirection;
    private float currentSpeed;

    public Vector3 MoveDirection => moveDirection;
    public bool IsMoving => moveInput.magnitude > minMoveThreshold;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stateManager = GetComponent<PlayerStateManager>();

        
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

        if (!stateManager.IsCollecting())
        {
            UpdateMovementState();
        }
    }

    private void UpdateMovementState()
    {
        float inputMagnitude = moveInput.magnitude;
        PlayerState newState;

        if (inputMagnitude < minMoveThreshold)
        {
            newState = PlayerState.Idle;
            currentSpeed = 0f;
        }
        else if (inputMagnitude > runThreshold)
        {
            newState = PlayerState.Run;
            currentSpeed = runSpeed;
        }
        else
        {
            newState = PlayerState.Walk;
            currentSpeed = walkSpeed;
        }

        stateManager.ChangeState(newState);
    }

    public void FixedUpdateMovement()
    {
        if (stateManager.IsCollecting()) return;

        Move();
        Rotate();
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
}