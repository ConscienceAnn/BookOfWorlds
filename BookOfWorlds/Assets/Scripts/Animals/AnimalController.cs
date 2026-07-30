using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks; 

public class AnimalController : MonoBehaviour, ICollectable, IInteractable
{
    [Header("Animal Data")]
    [SerializeField] private AnimalDataSO animalData;

    [Header("Visuals")]
    [SerializeField] private Animator animator;
    [SerializeField] private VisualState visualState;

    [Header("Progress Bar")]
    [SerializeField] private ProgressBarUI progressBar;

    [Header("Fly Animation")]
    [SerializeField] private ResourceFlyAnimation flyAnimation;

    [Inject] private IPlayerInventory inventory;
    [Inject] private PlayerUI playerUI;

    private bool isAvailable = true;
    private float cooldownTimer = 0f;
    private CowBehaviour behaviour;

    public bool IsAvailable => isAvailable;
    public string GetResourceName() => animalData?.resourceData?.resourceName ?? "Unknown";
    public int GetAmount() => animalData?.resourceAmount ?? 1;
    public Transform GetTransform() => transform;

    public bool TryCollect()
    {
        if (!isAvailable) return false;
        PerformCollect();
        return true;
    }

    public void Interact()
    {
        Debug.Log($"[AnimalController] Interact() called. IsAvailable: {isAvailable}");

        if (!isAvailable)
        {
            string message = $"{animalData.animalName} is not ready to give {GetResourceName()}!";
            playerUI?.ShowNotification(message, 2f);
            return;
        }

        if (!inventory.CanAdd(GetResourceName(), GetAmount()))
        {
            playerUI?.ShowNotification($"No space for {GetResourceName()}!", 2f);
            return;
        }

        PlayerCollector collector = FindObjectOfType<PlayerCollector>();
        if (collector != null)
        {
            collector.StartCollect(this);
        }
        else
        {
            PerformCollect();
        }
    }

    private void PerformCollect()
    {
        Debug.Log($"[AnimalController] PerformCollect() called. Resource: {GetResourceName()}");

        inventory.TryAdd(GetResourceName(), GetAmount());

        isAvailable = false;
        cooldownTimer = animalData?.cooldownTime ?? 8f;

        if (visualState != null)
            visualState.SetGray();

        behaviour.OnCollect(transform);

        PlayCollectAnimation();

        
        if (flyAnimation != null)
        {
            Vector3 flyPosition = transform.position + new Vector3(0, 1.5f, 0);
            flyAnimation.Play(flyPosition, GetResourceName()).Forget();
            Debug.Log($"[AnimalController] Fly animation started for {GetResourceName()}");
        }

        Debug.Log($"[AnimalController] Collected {GetResourceName()} (+{GetAmount()}) from {animalData.animalName}");
    }

    private void Awake()
    {
        if (visualState == null)
            visualState = GetComponent<VisualState>();

        if (visualState == null)
            visualState = GetComponentInChildren<VisualState>();

        behaviour = new CowBehaviour(progressBar, animalData?.cooldownTime ?? 8f);
        behaviour.OnCowRespawned += OnCowRespawned;

        if (flyAnimation == null)
            flyAnimation = FindObjectOfType<ResourceFlyAnimation>();
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

                Debug.Log($"[AnimalController] {animalData.animalName} is ready to give {GetResourceName()}!");
            }
        }
    }

    private void OnCowRespawned()
    {
        isAvailable = true;

        if (visualState != null)
            visualState.SetColored();

        Debug.Log($"[AnimalController] {animalData.animalName} is ready to give {GetResourceName()}!");
    }

    private void PlayCollectAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Collect");
    }

    public string GetAnimalName()
    {
        return animalData?.animalName ?? "Animal";
    }

    public string GetResourceNamePublic()
    {
        return GetResourceName();
    }

}