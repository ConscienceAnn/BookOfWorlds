using UnityEngine;
using Zenject;
using UnityEngine.InputSystem;
using System.Collections;

public class SellZone : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private float sellPromptDuration = 3f;

    [Inject] private IPlayerInventory inventory;
    [Inject] private SellService sellService;
    [Inject] private UIManager uiManager;
    [Inject] private PlayerUIMediator playerUIMediator;
    [Inject] private PlayerInputHandlerMy inputHandler;

    private bool isPlayerNear = false;
    private bool isSelling = false;

    // ===== РЕАЛИЗАЦИЯ IInteractable =====
    public void Interact()
    {
        Sell();
    }
    // ===== КОНЕЦ РЕАЛИЗАЦИИ =====

    private void OnEnable()
    {
        // Подписываемся на событие сбора (E)
        if (inputHandler != null)
        {
            inputHandler.OnCollectInput += HandleCollectInput;
        }
    }

    private void OnDisable()
    {
        // Отписываемся от события
        if (inputHandler != null)
        {
            inputHandler.OnCollectInput -= HandleCollectInput;
        }
    }

    private void HandleCollectInput()
    {
        if (isPlayerNear && !isSelling)
        {
            Sell();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerUIMediator?.ShowNotification("Здесь можно продать ресурсы. Нажмите E для продажи", sellPromptDuration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }

    private void Update()
    {
        if (isPlayerNear && !isSelling && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Sell();
        }
    }

    public void Sell()
    {
        if (isSelling) return;
        if (inventory == null)
        {
            Debug.LogError("SellZone: inventory is NULL!");
            return;
        }

        isSelling = true;

        var items = inventory.GetAllItems();
        if (items.Count == 0)
        {
            playerUIMediator?.ShowNotification("Инвентарь пуст! Нечего продавать.", 2.5f);
            isSelling = false;
            return;
        }

        int coins = sellService.SellAll();

        if (coins > 0)
        {
            EventBus.CoinsCollected();
            uiManager.AddCoins(coins);
            playerUIMediator?.ShowNotification($"Продано! Получено {coins} монет.", 2.5f);

            // Анимация монеты (вспышка или увеличение)
            StartCoroutine(CoinFlash());
        }
        else
        {
            playerUIMediator?.ShowNotification("Продажа не принесла монет.", 2.5f);
        }

        Invoke(nameof(ResetSellState), 0.5f);
    }

    private IEnumerator CoinFlash()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.5f;

        // Увеличиваем
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.3f;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // Возвращаем
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.3f;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private void ResetSellState()
    {
        isSelling = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}