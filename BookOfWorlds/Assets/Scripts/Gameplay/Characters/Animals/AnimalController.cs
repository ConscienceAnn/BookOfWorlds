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

    [Header("Movement")]
    [SerializeField] private AnimalMover animalMover;

    [Inject] private IPlayerInventory inventory;
    [Inject] private PlayerUIMediator playerUIMediator;  
    [Inject] private ProgressBarFactory progressBarFactory;

    private bool isAvailable = true;
    private float cooldownTimer = 0f;
    private IResourceBehaviour behaviour;

    public bool IsAvailable => isAvailable;

    public string GetResourceName() => animalData?.resourceData?.resourceName ?? "Unknown";
    public int GetAmount() => animalData?.resourceAmount ?? 1;
    public Transform GetTransform() => transform;
    public bool TryCollect() { Interact(); return true; }

    private void Awake()
    {
        if (visualState == null)
            visualState = GetComponent<VisualState>();
        if (visualState == null)
            visualState = GetComponentInChildren<VisualState>();

        if (animalMover == null)
            animalMover = GetComponent<AnimalMover>();
    }

    private void Start()
    {
        // Убираем дублирование создания прогресс-бара (было два раза)
        if (progressBar == null && progressBarFactory != null)
        {
            progressBar = progressBarFactory.CreateProgressBar(transform, GetProgressBarOffset());
            Debug.Log($"ProgressBar создан для {animalData?.animalName}");
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        if (visualState != null)
            visualState.SetColored();

        if (animalData != null && progressBar != null)
        {
            behaviour = AnimalBehaviourFactory.Create(animalData, progressBar);
            Debug.Log($"Behaviour создан для {animalData.animalName} типа {animalData.animalType}");
        }

        if (animalMover != null)
        {
            animalMover.SetStartPosition(transform.position);
            bool canMove = animalData != null && animalData.canMove;
            animalMover.IsEnabled = canMove;
            Debug.Log($"AnimalMover для {animalData?.animalName}: enabled={canMove}");
        }

        // Подписываемся на событие респавна
        if (behaviour is AnimalBehaviourBase animalBehaviour)
        {
            animalBehaviour.OnAnimalRespawned += OnRespawned;
        }
    }

    private void Update()
    {
        // Движение только если животное ДОСТУПНО и движение включено
        if (animalMover != null && animalMover.IsEnabled && isAvailable)
        {
            animalMover.Tick();
        }

        if (!isAvailable)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0)
            {
                isAvailable = true;

                if (visualState != null)
                    visualState.SetColored();

                // Возобновляем движение, когда животное снова доступно
                if (animalMover != null && animalMover.IsEnabled)
                {
                    animalMover.Resume();
                }

                Debug.Log($"{animalData.animalName} готова дать {GetResourceName()}!");
            }
        }
    }

    private Vector3 GetProgressBarOffset()
    {
        return new Vector3(0, 2.5f, 0);
    }

    private void OnDestroy()
    {
        if (behaviour is AnimalBehaviourBase animalBehaviour)
        {
            animalBehaviour.OnAnimalRespawned -= OnRespawned;
            animalBehaviour.Dispose();
        }
    }

    public void Interact()
    {
        if (!isAvailable)
        {
            playerUIMediator?.ShowNotification($"{animalData.animalName} ещё не готова!", 2f);  
            return;
        }

        if (!inventory.CanAdd(GetResourceName(), GetAmount()))
        {
            playerUIMediator?.ShowNotification($"Нет места для {GetResourceName()}!", 2f); 
            return;
        }

        // Останавливаем движение при сборе
        if (animalMover != null && animalMover.IsEnabled)
        {
            animalMover.Pause();
            Debug.Log($"{animalData.animalName}: движение остановлено для сбора");
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

        // Возобновляем движение, когда респавн завершён
        if (animalMover != null && animalMover.IsEnabled)
        {
            animalMover.Resume();
        }

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