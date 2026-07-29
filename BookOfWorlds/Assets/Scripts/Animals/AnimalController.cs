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

    [Inject] private IPlayerInventory inventory;
    [Inject] private PlayerUI playerUI;

    private bool isAvailable = true;
    private float cooldownTimer = 0f;
    private CowBehaviour behaviour;

    // ===== РЕАЛИЗАЦИЯ ICollectable =====
    public bool IsAvailable => isAvailable;
    public string GetResourceName() => animalData?.resourceData?.resourceName ?? "Unknown";
    public int GetAmount() => animalData?.resourceAmount ?? 1;
    public Transform GetTransform() => transform;

    public bool TryCollect()
    {
        if (!isAvailable) return false;

        // Логика сбора (вызываем Interact)
        Interact();
        return true;
    }
    // ===== КОНЕЦ РЕАЛИЗАЦИИ =====

    private void Awake()
    {
        if (visualState == null)
            visualState = GetComponent<VisualState>();

        if (visualState == null)
            visualState = GetComponentInChildren<VisualState>();

        behaviour = new CowBehaviour(progressBar, animalData?.cooldownTime ?? 8f);
        behaviour.OnCowRespawned += OnCowRespawned;
    }

    private void OnDestroy()
    {
        if (behaviour != null)
            behaviour.OnCowRespawned -= OnCowRespawned;
    }

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (visualState != null)
            visualState.SetColored();
    }

    private void Update()
    {
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

    public void Interact()
    {
        if (!isAvailable)
        {
            playerUI?.ShowNotification($" {animalData.animalName} ещё не готова дать {GetResourceName()}!", 2f);
            return;
        }

        if (!inventory.CanAdd(GetResourceName(), GetAmount()))
        {
            playerUI?.ShowNotification($" Нет места для {GetResourceName()}!", 2f);
            return;
        }

        // Добавляем ресурс
        inventory.TryAdd(GetResourceName(), GetAmount());

        // Делаем корову недоступной
        isAvailable = false;
        cooldownTimer = animalData?.cooldownTime ?? 8f;

        // Корова становится серой
        if (visualState != null)
            visualState.SetGray();

        // Запускаем поведение сбора (прогресс-бар)
        behaviour.OnCollect(transform);

        PlayCollectAnimation();

        Debug.Log($" Собрано {GetResourceName()} (+{GetAmount()}) от {animalData.animalName}");
    }

    private void OnCowRespawned()
    {
        isAvailable = true;

        if (visualState != null)
            visualState.SetColored();

        Debug.Log($" {animalData.animalName} готова дать {GetResourceName()}!");
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

    private void OnDrawGizmos()
    {
        Gizmos.color = isAvailable ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}