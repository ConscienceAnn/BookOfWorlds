using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;

public class AnimalController : MonoBehaviour, ICollectable
{
    [Header("Animal Data")]
    [SerializeField] private AnimalDataSO animalData;

    [Header("Visuals")]
    [SerializeField] private Animator animator;
    [SerializeField] private VisualState visualState;

    [Inject] private IPlayerInventory inventory;
    [Inject] private PlayerUI playerUI;
    [Inject] private ProgressBarFactory progressBarFactory;

    private bool isAvailable = true;
    private float cooldownTimer = 0f;
    private IResourceBehaviour behaviour;
    private ProgressBarUI progressBar;

    public bool IsAvailable => isAvailable;
    public string GetResourceName() => animalData?.resourceData?.resourceName ?? "Unknown";
    public int GetAmount() => animalData?.resourceAmount ?? 1;
    public Transform GetTransform() => transform;

    public bool TryCollect()
    {
        if (!isAvailable) return false;
        Interact();
        return true;
    }

    private void Awake()
    {

        Debug.Log($" AnimalController.Awake() на {gameObject.name}, IsAvailable = {isAvailable}");
        Debug.Log($" AnimalController реализует ICollectable: {(this is ICollectable ? "ДА" : "НЕТ")}");

        Debug.Log($"AnimalController.Awake() для {animalData?.animalName}");

        if (visualState == null)
            visualState = GetComponent<VisualState>();

        if (visualState == null)
            visualState = GetComponentInChildren<VisualState>();

        //  СОЗДАЁМ PROGRESS BAR ЧЕРЕЗ ФАБРИКУ
        if (progressBarFactory != null)
        {
            progressBar = progressBarFactory.CreateProgressBar(
                transform,
                new Vector3(0, 2.5f, 0)
            );
        }

        //  СОЗДАЁМ ПОВЕДЕНИЕ ЧЕРЕЗ ФАБРИКУ
        if (animalData != null && progressBar != null)
        {
            behaviour = AnimalBehaviourFactory.Create(animalData, progressBar);
            Debug.Log($"AnimalController: behaviour {(behaviour != null ? "СОЗДАН" : "NULL")} для {animalData.animalType}");
        }
        else
        {
            Debug.LogError($"AnimalController: animalData или progressBar = NULL для {gameObject.name}!");
        }

        // Подписываемся на события
        if (behaviour is CowBehaviour cowBehaviour)
        {
            cowBehaviour.OnCowRespawned += OnRespawned;
        }
        else if (behaviour is RabbitBehaviour rabbitBehaviour)
        {
            rabbitBehaviour.OnRabbitRespawned += OnRespawned;
        }
    }

    private void OnDestroy()
    {
        if (behaviour is CowBehaviour cowBehaviour)
        {
            cowBehaviour.OnCowRespawned -= OnRespawned;
        }
        else if (behaviour is RabbitBehaviour rabbitBehaviour)
        {
            rabbitBehaviour.OnRabbitRespawned -= OnRespawned;
        }

        if (progressBarFactory != null && progressBar != null)
        {
            progressBarFactory.DestroyProgressBar(progressBar);
        }
    }

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (visualState != null)
            visualState.SetColored();

        if (progressBar != null)
        {
            progressBar.Hide();
        }
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
        Debug.Log($"AnimalController.Interact() вызван для {animalData?.animalName}, behaviour = {(behaviour != null ? "ЕСТЬ" : "НЕТ")}, isAvailable = {isAvailable}");

        if (!isAvailable)
        {
            playerUI?.ShowNotification($"{animalData.animalName} ещё не готова дать {GetResourceName()}!", 2f);
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

        //  ВЫЗЫВАЕМ ПОВЕДЕНИЕ
        if (behaviour != null)
        {
            behaviour.OnCollect(transform);
        }
        else
        {
            Debug.LogError($"AnimalController: behaviour = NULL для {animalData?.animalName}!");
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

    public void SetCooldown(float time)
    {
        if (animalData != null)
        {
            animalData.cooldownTime = time;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isAvailable ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}