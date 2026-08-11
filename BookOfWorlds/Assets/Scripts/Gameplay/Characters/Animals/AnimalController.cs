using UnityEngine;
using Zenject;

public class AnimalController : MonoBehaviour, ICollectable
{
    [Header("Animal Data")]
    [SerializeField] private AnimalDataSO animalData;

    [Header("Visuals")]
    [SerializeField] private Animator animator;
    [SerializeField] private VisualState visualState;

    [Header("Progress Bar")]
    [SerializeField] private ProgressBarUI progressBar;

    [Header("Movement (только для зайца)")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float moveRadius = 3f;

    [Inject] private IPlayerInventory inventory;
    [Inject] private PlayerUI playerUI;
    [Inject] private ProgressBarFactory progressBarFactory;

    private bool isAvailable = true;
    private float cooldownTimer = 0f;
    private IResourceBehaviour behaviour;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float moveTimer = 0f;
    private bool isMoving = false;
    private bool isRabbit = false;

    public bool IsAvailable => isAvailable;

    public string GetResourceName() => animalData?.resourceData?.resourceName ?? "Unknown";
    public int GetAmount() => animalData?.resourceAmount ?? 1;
    public Transform GetTransform() => transform;
    public bool TryCollect() { Interact(); return true; }

    private void Awake()
    {
        Debug.Log($"AnimalController.Awake() на {gameObject.name}");

        if (visualState == null)
            visualState = GetComponent<VisualState>();
        if (visualState == null)
            visualState = GetComponentInChildren<VisualState>();

        isRabbit = animalData != null && animalData.animalType == AnimalDataSO.AnimalType.Rabbit;

        if (isRabbit)
        {
            startPosition = transform.position;
            ChooseNewTarget();
            Debug.Log($"Заяц инициализирован, будет двигаться");
        }
        else
        {
            Debug.Log($"Корова инициализирована, будет стоять на месте");
        }
    }

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (visualState != null)
            visualState.SetColored();

        if (progressBar == null && progressBarFactory != null)
        {
            progressBar = progressBarFactory.CreateProgressBar(transform, GetProgressBarOffset());
            Debug.Log($"ProgressBar создан для {animalData?.animalName}");
        }

        if (animalData != null && progressBar != null)
        {
            behaviour = AnimalBehaviourFactory.Create(animalData, progressBar);
            Debug.Log($"Behaviour создан для {animalData.animalName} типа {animalData.animalType}");
        }
    }

    private void Update()
    {
        if (isRabbit && isAvailable)
        {
            UpdateMovement();
        }

        if (!isAvailable)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0)
            {
                isAvailable = true;

                if (visualState != null)
                    visualState.SetColored();

                Debug.Log($"{animalData.animalName} готова дать {GetResourceName()}!");
            }
        }
    }

    private void UpdateMovement()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > 0.1f)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
            isMoving = true;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        else
        {
            isMoving = false;
            moveTimer += Time.deltaTime;
            if (moveTimer > 1f)
            {
                ChooseNewTarget();
                moveTimer = 0f;
            }
        }

        if (animator != null)
        {
            animator.SetBool("IsRunning", isMoving);
        }
    }

    private void ChooseNewTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * moveRadius;
        targetPosition = startPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
        isMoving = true;
    }

    private Vector3 GetProgressBarOffset()
    {
        return new Vector3(0, 2.5f, 0);
    }

    private void OnDestroy()
    {
        if (behaviour is CowBehaviour cow)
            cow.OnCowRespawned -= OnRespawned;
        else if (behaviour is RabbitBehaviour rabbit)
            rabbit.OnRabbitRespawned -= OnRespawned;
    }

    public void Interact()
    {
        if (!isAvailable)
        {
            playerUI?.ShowNotification($"{animalData.animalName} ещё не готова!", 2f);
            return;
        }

        if (!inventory.CanAdd(GetResourceName(), GetAmount()))
        {
            playerUI?.ShowNotification($"Нет места для {GetResourceName()}!", 2f);
            return;
        }

        inventory.TryAdd(GetResourceName(), GetAmount());

        isAvailable = false;
        cooldownTimer = animalData?.cooldownTime ?? 8f;

        if (visualState != null)
            visualState.SetGray();

        if (behaviour != null)
        {
            behaviour.OnCollect(transform);
            Debug.Log($"Behaviour.OnCollect() вызван для {animalData?.animalName}");
        }

        PlayCollectAnimation();

        Debug.Log($"Собрано {GetResourceName()} (+{GetAmount()}) от {animalData.animalName}");
    }

    private void OnRespawned()
    {
        isAvailable = true;

        if (visualState != null)
            visualState.SetColored();

        Debug.Log($"{animalData.animalName} готова дать {GetResourceName()}!");
    }

    private void PlayCollectAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Collect");
    }

    public string GetAnimalName()
    {
        return animalData?.animalName ?? "Животное";
    }

    public string GetResourceNamePublic()
    {
        return GetResourceName();
    }
}