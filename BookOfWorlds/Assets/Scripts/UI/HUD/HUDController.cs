using UnityEngine;
using TMPro;
using Zenject;

/// <summary>
/// Только отображение ресурсов и монет на HUD.
/// Подписывается на события, обновляет текст.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Resource Texts")]
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text stoneText;
    [SerializeField] private TMP_Text milkText;
    [SerializeField] private TMP_Text woolText;
    [SerializeField] private TMP_Text coinsText;

    [Inject] private IPlayerInventory inventory;
    [Inject] private UIManager uiManager;  

    private void Start()
    {
        // Подписываемся на события
        if (inventory != null)
        {
            inventory.OnInventoryChanged += UpdateUI;
        }

        EventBus.OnCoinsChanged += OnCoinsChanged;

        // Первоначальное обновление
        UpdateUI();
        UpdateCoins();
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
        if (inventory == null) return;

        if (woodText != null)
            woodText.text = $"{inventory.GetAmount("Дерево")}/{inventory.GetMax("Дерево")}";

        if (stoneText != null)
            stoneText.text = $"{inventory.GetAmount("Камень")}/{inventory.GetMax("Камень")}";

        if (milkText != null)
            milkText.text = $"{inventory.GetAmount("Молоко")}/{inventory.GetMax("Молоко")}";

        if (woolText != null)
            woolText.text = $"{inventory.GetAmount("Шерсть")}/{inventory.GetMax("Шерсть")}";
    }

    private void UpdateCoins()
    {
        if (coinsText != null && uiManager != null)
        {
            coinsText.text = uiManager.GetCoins().ToString();
        }
    }

    private void OnCoinsChanged(int amount)
    {
        UpdateCoins();
    }

    public void ForceRefresh()
    {
        UpdateUI();
        UpdateCoins();
    }
}