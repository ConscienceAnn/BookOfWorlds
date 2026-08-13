using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using System.Collections.Generic;

public class UpgradeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject upgradeItemPrefab;
    [SerializeField] private Transform upgradeContainer;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private Button closeButton;
    [SerializeField] private UpgradeNotificationUI upgradeNotification;

    [Inject] private UpgradeManager upgradeManager;
    [Inject] private PlayerInputHandlerMy inputHandler;
    [Inject] private UIManager uiManager;

    private List<UpgradeItemUI> upgradeItems = new List<UpgradeItemUI>();

    private void Start()
    {
        upgradePanel.SetActive(false);

        if (inputHandler != null)
        {
            inputHandler.OnUpgradeInput += Toggle;
        }

        upgradeManager.OnUpgradeNotification += ShowNotification;
        upgradeManager.OnUpgradesChanged += RefreshUI;
        EventBus.OnCoinsChanged += OnCoinsChanged;

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        CreateUpgradeItems();
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
            inputHandler.OnUpgradeInput -= Toggle;

        if (upgradeManager != null)
        {
            upgradeManager.OnUpgradeNotification -= ShowNotification;
            upgradeManager.OnUpgradesChanged -= RefreshUI;
        }
        EventBus.OnCoinsChanged -= OnCoinsChanged;
    }

    private void CreateUpgradeItems()
    {
        if (upgradeItemPrefab == null || upgradeContainer == null) return;

        foreach (Transform child in upgradeContainer)
        {
            Destroy(child.gameObject);
        }
        upgradeItems.Clear();

        var allUpgrades = upgradeManager.GetAllUpgrades();
        if (allUpgrades == null || allUpgrades.Length == 0) return;

        foreach (var data in allUpgrades)
        {
            GameObject itemObj = Instantiate(upgradeItemPrefab, upgradeContainer);
            UpgradeItemUI itemUI = itemObj.GetComponent<UpgradeItemUI>();

            if (itemUI != null)
            {
                itemUI.Initialize(data.upgradeId, data, upgradeManager);
                upgradeItems.Add(itemUI);
            }
        }
    }

    public void RefreshUI()
    {
        if (coinsText != null && uiManager != null)
        {
            coinsText.text = $"Монет: {uiManager.GetCoins()}";
        }

        foreach (var item in upgradeItems)
        {
            item.Refresh();
        }
    }

    private void OnCoinsChanged(int amount)
    {
        RefreshUI();
    }

    private void ShowNotification(string message, bool isError)
    {
        if (upgradeNotification != null)
        {
            upgradeNotification.ShowNotification(message, isError);
        }
    }

    public void Show()
    {
        upgradePanel.SetActive(true);

        Time.timeScale = 0f;

        if (inputHandler != null)
        {
            inputHandler.SetInputEnabled(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshUI();
    }

    public void Hide()
    {
        if (upgradeNotification != null)
        {
            upgradeNotification.HideImmediate();
        }

        upgradePanel.SetActive(false);

        Time.timeScale = 1f;

        if (inputHandler != null)
        {
            inputHandler.SetInputEnabled(true);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Toggle()
    {
        if (upgradePanel.activeSelf)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public bool IsOpen => upgradePanel.activeSelf;
}