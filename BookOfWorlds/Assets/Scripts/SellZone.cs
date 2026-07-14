using UnityEngine;
using Zenject;

public class SellZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject promptUI;

    [Inject] private IPlayerInventory inventory;
    [Inject] private SellService sellService;
    [Inject] private UIManager uiManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (promptUI != null) promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (promptUI != null) promptUI.SetActive(false);
        }
    }

    public void Sell()
    {
        if (inventory == null) return;

        var items = inventory.GetAllItems();
        if (items.Count == 0)
        {
            Debug.Log(" Инвентарь пуст! Нечего продавать.");
            return;
        }

        int coins = sellService.SellAll();

        if (coins > 0)
        {
            uiManager.AddCoins(coins);
            Debug.Log($" Продано! Получено {coins} монет.");
        }
    }
}