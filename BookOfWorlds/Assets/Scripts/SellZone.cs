using UnityEngine;
using Zenject;
using UnityEngine.InputSystem;

public class SellZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sellPromptDuration = 3f;

    [Inject] private IPlayerInventory inventory;
    [Inject] private SellService sellService;
    [Inject] private UIManager uiManager;
    [Inject] private PlayerUI playerUI;

    private bool isPlayerNear = false;
    private bool isSelling = false;

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
        }
        else
        {
            Debug.LogWarning("SellZone: продажа не принесла монет");
            playerUI?.ShowNotification("Продажа не принесла монет.", 2.5f);
        }

        Invoke(nameof(ResetSellState), 0.5f);
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