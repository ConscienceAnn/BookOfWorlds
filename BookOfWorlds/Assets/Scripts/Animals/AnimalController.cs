using UnityEngine;
using Zenject;

public class AnimalController : MonoBehaviour
{
    [Header("Animal Data")]
    [SerializeField] private AnimalDataSO animalData;

    [Header("Visuals")]
    [SerializeField] private Animator animator;
    [SerializeField] private VisualState visualState; 

    [Inject] private IPlayerInventory inventory;

    private bool isAvailable = true;
    private float cooldownTimer = 0f;

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

    private void Update()
    {
        if (!isAvailable)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                isAvailable = true;

                if (visualState != null)
                {
                    visualState.SetColored();
                }

                Debug.Log($" {animalData.animalName} готова дать {ResourceName}!");
            }
        }
    }

    public void Interact()
    {
        if (!isAvailable)
        {
            Debug.Log($" {animalData.animalName} ещё не готова!");
            return;
        }

        if (!inventory.CanAdd(ResourceName, animalData.resourceAmount))
        {
            Debug.Log($" Нет места для {ResourceName}!");
            return;
        }

        inventory.TryAdd(ResourceName, animalData.resourceAmount);

        isAvailable = false;
        cooldownTimer = animalData.cooldownTime;

        if (visualState != null)
        {
            visualState.SetGray();
        }

        PlayCollectAnimation();

        Debug.Log($"Собрано {ResourceName} (+{animalData.resourceAmount}) от {animalData.animalName}");
    }

    private void PlayCollectAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Collect");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isAvailable ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}