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
    [Inject] private PlayerUI playerUI;

    private bool isPlayerNear = false;
    private bool isSelling = false;

    // ===== РЕАЛИЗАЦИЯ IInteractable =====
    public void Interact()
    {
        Sell();
    }
    // ===== КОНЕЦ РЕАЛИЗАЦИИ =====

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            playerUI?.ShowNotification("Здесь можно продать ресурсы. Нажмите E для продажи", sellPromptDuration);
            Debug.Log("Игрок вошёл в зону продажи");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            Debug.Log(" Игрок вышел из зоны продажи");
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
            Debug.Log("SellZone: инвентарь пуст");
            playerUI?.ShowNotification("Инвентарь пуст! Нечего продавать.", 2.5f);
            isSelling = false;
            return;
        }

        int coins = sellService.SellAll();

        if (coins > 0)
        {
            uiManager.AddCoins(coins);
            Debug.Log($"SellZone: продано! {coins} монет");
            playerUI?.ShowNotification($"Продано! Получено {coins} монет.", 2.5f);
            

            // Анимация монеты (вспышка или увеличение)
            StartCoroutine(CoinFlash());
        }
        else
        {
            Debug.LogWarning("SellZone: продажа не принесла монет");
            playerUI?.ShowNotification("Продажа не принесла монет.", 2.5f);
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