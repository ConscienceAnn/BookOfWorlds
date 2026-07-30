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

    [Header("Movement")]
    [SerializeField] private AnimalMover mover; 

    [Inject] private IPlayerInventory inventory;
    [Inject] private PlayerUI playerUI;

    private bool isAvailable = true;
    private float cooldownTimer = 0f;
    private IResourceBehaviour behaviour;

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
        if (!isAvailable)
        {
            playerUI?.ShowNotification($"{animalData.animalName} is not ready!", 2f);
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
        inventory.TryAdd(GetResourceName(), GetAmount());

        isAvailable = false;
        cooldownTimer = animalData?.cooldownTime ?? 8f;

        if (visualState != null)
            visualState.SetGray();

        behaviour?.OnCollect(transform);

        PlayCollectAnimation();

      
        mover?.StopMoving();

        if (flyAnimation != null)
        {
            Vector3 flyPosition = transform.position + new Vector3(0, 1.5f, 0);
            flyAnimation.Play(flyPosition, GetResourceName()).Forget();
        }

        Debug.Log($"[AnimalController] Collected {GetResourceName()} (+{GetAmount()}) from {animalData.animalName}");
    }

    private void Awake()
    {
        if (visualState == null)
            visualState = GetComponent<VisualState>();

        if (visualState == null)
            visualState = GetComponentInChildren<VisualState>();

        behaviour = AnimalBehaviourFactory.Create(animalData, progressBar);

        if (behaviour is AnimalBehaviourBase animalBehaviour)
        {
            animalBehaviour.OnAnimalRespawned += OnAnimalRespawned;
        }

        if (flyAnimation == null)
            flyAnimation = FindObjectOfType<ResourceFlyAnimation>();

      
        if (mover == null)
            mover = GetComponent<AnimalMover>();
    }

    private void OnDestroy()
    {
        if (behaviour is AnimalBehaviourBase animalBehaviour)
        {
            animalBehaviour.OnAnimalRespawned -= OnAnimalRespawned;
        }
    }

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (visualState != null)
            visualState.SetColored();

        if (mover != null)
        {
            mover.SetStartPosition(transform.position);
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

              
                if (animalData != null && animalData.canMove && mover != null)
                {
                    mover.StartMoving();
                }

                Debug.Log($"[AnimalController] {animalData.animalName} is ready!");
            }
            return;
        }

        if (animalData != null && animalData.canMove && mover != null)
        {
            mover.Tick();
        }
    }

    private void OnAnimalRespawned()
    {
        isAvailable = true;

        if (visualState != null)
            visualState.SetColored();

        if (animalData != null && animalData.canMove && mover != null)
        {
            mover.StartMoving();
        }

        Debug.Log($"[AnimalController] {animalData.animalName} respawned!");
    }

    private void PlayCollectAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Collect");
    }

    public string GetAnimalName() => animalData?.animalName ?? "Animal";

   
}