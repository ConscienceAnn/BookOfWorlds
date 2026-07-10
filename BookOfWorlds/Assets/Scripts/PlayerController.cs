using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))] // Добавляем Animator
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Animator animator; // Ссылка на Animator

    private Vector2 moveInput;
    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;

        // Замораживаем вращение по осям X и Z
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;

        // Если Animator не назначен в Inspector, пытаемся найти
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        // Проверяем, на земле ли персонаж
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);

        // Движение
        MovePlayer();

        // Поворот
        RotatePlayer();
    }

    private void MovePlayer()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 velocity = moveDirection * moveSpeed;
            velocity.y = rb.velocity.y;
            rb.velocity = velocity;
        }
        else
        {
            Vector3 velocity = rb.velocity;
            velocity.x = Mathf.Lerp(velocity.x, 0, Time.fixedDeltaTime * 10);
            velocity.z = Mathf.Lerp(velocity.z, 0, Time.fixedDeltaTime * 10);
            rb.velocity = velocity;
        }
    }

    private void RotatePlayer()
    {
        // Если есть движение — поворачиваем персонажа
        if (moveDirection.magnitude > 0.1f)
        {
            // Целевой поворот в сторону движения
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            // Плавный поворот
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    private void Update()
    {
        // Обновляем анимацию
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        // Получаем скорость движения (горизонтальную)
        float speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

        // Передаём скорость в Animator
        animator.SetFloat("Speed", speed);

        // Передаём, на земле ли персонаж
        animator.SetBool("IsGrounded", isGrounded);

        // Если есть прыжок — передаём
        // animator.SetBool("IsJumping", !isGrounded && rb.velocity.y > 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * 1.1f);
    }
}