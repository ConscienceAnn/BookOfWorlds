using UnityEngine;

/// <summary>
/// Компонент для движения животных.
/// </summary>
public class AnimalMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float moveRadius = 5f;
    [SerializeField] private float idleTimeMin = 2f;
    [SerializeField] private float idleTimeMax = 5f;

    [Header("Stuck Prevention")]
    [SerializeField] private float stuckCheckInterval = 1.5f;
    [SerializeField] private float stuckDistanceThreshold = 0.3f;
    [SerializeField] private float maxMoveTime = 5f;

    [Header("References")]
    [SerializeField] private Animator animator;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float idleTimer = 0f;
    private float stuckTimer = 0f;
    private Vector3 lastPosition;
    private float moveTimer = 0f;

    private bool isEnabled = true;
    private bool isPaused = false;

    public bool IsMoving => isMoving;
    public bool IsEnabled { get => isEnabled; set => isEnabled = value; }
    public bool IsPaused => isPaused;

    public event System.Action OnStartedMoving;
    public event System.Action OnStoppedMoving;

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition;
        idleTimer = Random.Range(idleTimeMin, idleTimeMax);
        isMoving = false;
        SetAnimatorBool(false);
        lastPosition = transform.position;
    }

    /// <summary>
    /// Вызывается каждый кадр из AnimalController
    /// </summary>
    public void Tick()
    {
        if (!isEnabled || isPaused) return;

        if (!isMoving)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0)
            {
                isMoving = true;
                SetAnimatorBool(true);
                SetRandomTarget();
                moveTimer = 0f;
                OnStartedMoving?.Invoke();
            }
            return;
        }

        // Движение к цели
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Поворот
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }

        // Проверка на застревание
        stuckTimer += Time.deltaTime;
        if (stuckTimer > stuckCheckInterval)
        {
            float distance = Vector3.Distance(transform.position, lastPosition);
            if (distance < stuckDistanceThreshold)
            {
                SetRandomTarget();
                moveTimer = 0f;
            }
            lastPosition = transform.position;
            stuckTimer = 0f;
        }

        // Таймаут движения
        moveTimer += Time.deltaTime;
        if (moveTimer > maxMoveTime)
        {
            SetRandomTarget();
            moveTimer = 0f;
        }

        // Проверка достижения цели
        if (Vector3.Distance(transform.position, targetPosition) < 0.3f)
        {
            isMoving = false;
            SetAnimatorBool(false);
            idleTimer = Random.Range(idleTimeMin, idleTimeMax);
            OnStoppedMoving?.Invoke();
        }
    }

    /// <summary>
    /// ПРИНУДИТЕЛЬНО ОСТАНОВИТЬ движение (при сборе ресурса)
    /// </summary>
    public void Pause()
    {
        if (isPaused) return;

        isPaused = true;
        isMoving = false;
        SetAnimatorBool(false);
    }

    /// <summary>
    /// ВОЗОБНОВИТЬ движение (после сбора) — С ФОРСИРОВАННЫМ ЗАПУСКОМ!
    /// </summary>
    public void Resume()
    {
        if (!isPaused) return;

        isPaused = false;

        
        isMoving = true;
        SetAnimatorBool(true);
        SetRandomTarget();
        moveTimer = 0f;
        OnStartedMoving?.Invoke();
    }

    /// <summary>
    /// Полная остановка движения (при уничтожении или отключении)
    /// </summary>
    public void ForceStop()
    {
        isMoving = false;
        isPaused = false;
        SetAnimatorBool(false);
        idleTimer = Random.Range(idleTimeMin, idleTimeMax);
        OnStoppedMoving?.Invoke();
    }

    private void SetRandomTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * moveRadius;
        targetPosition = new Vector3(
            startPosition.x + randomCircle.x,
            startPosition.y,
            startPosition.z + randomCircle.y
        );

        stuckTimer = 0f;
        lastPosition = transform.position;
        moveTimer = 0f;

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            SetRandomTarget();
        }
    }

    private void SetAnimatorBool(bool value)
    {
        if (animator != null)
            animator.SetBool("IsRunning", value);
    }

    public void SetStartPosition(Vector3 position)
    {
        startPosition = position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startPosition, moveRadius);

        if (isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetPosition, 0.3f);
        }
    }
}