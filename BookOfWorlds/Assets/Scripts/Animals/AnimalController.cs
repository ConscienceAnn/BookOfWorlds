using UnityEngine;
using Zenject;

public class AnimalController : MonoBehaviour
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
    private CowBehaviour behaviour;

    public bool IsAvailable => isAvailable;
    public string ResourceName => animalData.resourceData.resourceName;

    private void Awake()
    {
        if (visualState == null)
        {
            visualState = GetComponent<VisualState>();
            if (visualState == null)
            {
                visualState = GetComponentInChildren<VisualState>();
            }
        }

        behaviour = new CowBehaviour(progressBar, animalData.cooldownTime);
        behaviour.OnCowRespawned += OnCowRespawned;
    }

    private void OnDestroy()
    {
        if (behaviour != null)
        {
            behaviour.OnCowRespawned -= OnCowRespawned;
        }
    }

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (visualState != null)
        {
            visualState.SetColored();
        }
    }

    public void Interact()
    {
        if (!isAvailable)
        {
            //  Добавляем уведомление
            playerUI?.ShowNotification($" {animalData.animalName} ещё не готова дать {ResourceName}!", 2f);
            Debug.Log($" {animalData.animalName} ещё не готова!");
            return;
        }

        if (!inventory.CanAdd(ResourceName, animalData.resourceAmount))
        {
            playerUI?.ShowNotification($" Нет места для {ResourceName}!", 2f);
            Debug.Log($" Нет места для {ResourceName}!");
            return;
        }

        inventory.TryAdd(ResourceName, animalData.resourceAmount);

        isAvailable = false;

        if (visualState != null)
        {
            visualState.SetGray();
        }

        behaviour.OnCollect(transform);

        PlayCollectAnimation();

        Debug.Log($" Собрано {ResourceName} (+{animalData.resourceAmount}) от {animalData.animalName}");
    }

    private void OnCowRespawned()
    {
        isAvailable = true;

        if (visualState != null)
        {
            visualState.SetColored();
        }

        Debug.Log($" {animalData.animalName} готова дать {ResourceName}!");
    }

    private void PlayCollectAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Collect");
        }
    }

    public string GetAnimalName()
    {
        return animalData.animalName;
    }

    public string GetResourceName()
    {
        return animalData.resourceData.resourceName;
    }
}