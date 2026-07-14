using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text stoneText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text milkText;
    [SerializeField] private TMP_Text woolText;

    [Header("Inventory Reference")]
    [SerializeField] private PlayerInventory inventory; 

    private int coins = 0;

    private void Start()
    {
        // Подписываемся на изменения инвентаря
        if (inventory != null)
        {
            inventory.OnInventoryChanged += UpdateUI;
        }

        UpdateUI();
        Debug.Log(" UIManager: Готов к работе!");
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateUI;
        }
    }

    private void UpdateUI()
    {
        if (inventory != null)
        {
            // БЕРЁМ ДАННЫЕ ИЗ ИНВЕНТАРЯ
            if (woodText != null)
                woodText.text = $"{inventory.GetAmount("Wood")}/{inventory.GetMax("Wood")}";

            if (stoneText != null)
                stoneText.text = $"{inventory.GetAmount("Stone")}/{inventory.GetMax("Stone")}";

            if (milkText != null)
                milkText.text = $"{inventory.GetAmount("Milk")}/{inventory.GetMax("Milk")}";

            if (woolText != null)
                woolText.text = $"{inventory.GetAmount("Wool")}/{inventory.GetMax("Wool")}";
        }

        if (coinsText != null)
            coinsText.text = coins.ToString();
    }

    // Метод для добавления монет (вызывается из SellZone)
    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
    }

    // Метод для установки монет (при загрузке сохранения)
    public void SetCoins(int amount)
    {
        coins = amount;
        UpdateUI();
    }
}