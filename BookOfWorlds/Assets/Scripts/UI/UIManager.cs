using UnityEngine;
using TMPro;

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
        if (inventory != null)
        {
            inventory.OnInventoryChanged += UpdateUI;
        }

        EventBus.OnCoinsChanged += OnCoinsChanged;

        UpdateUI();
        Debug.Log("UIManager: Готов к работе!");
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateUI;
        }

        EventBus.OnCoinsChanged -= OnCoinsChanged;
    }

    private void UpdateUI()
    {
        if (inventory != null)
        {
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

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
    }

    public int GetCoins()
    {
        return coins;
    }

    public void SetCoins(int amount)
    {
        coins = amount;
        UpdateUI();
    }

    public void ForceRefreshUI()
    {
        UpdateUI();
        Debug.Log("UI принудительно обновлён");
    }

    private void OnCoinsChanged(int amount)
    {
        coins = amount;
        UpdateUI();
    }
}