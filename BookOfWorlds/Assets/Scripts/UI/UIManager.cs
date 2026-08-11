using UnityEngine;
using TMPro;
using Zenject;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public event System.Action OnPanelsOpened;
    public event System.Action OnPanelsClosed;

    [Header("UI References")]
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text stoneText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text milkText;
    [SerializeField] private TMP_Text woolText;

    [Header("Inventory Reference")]
    [SerializeField] private PlayerInventory inventory;

    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject levelCompletePanel;

    [Header("Buttons")]
    [SerializeField] private GameObject nextLevelButton;

    [Inject] private PlayerInputHandler playerInputHandler;
    [Inject] private LevelManager levelManager;

    private bool isAnyPanelOpen = false;
    private int coins = 0;

    public GameObject LevelCompletePanel => levelCompletePanel;
    public GameObject PauseMenuPanel => pauseMenuPanel;

    private void Start()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged += UpdateUI;
        }

        EventBus.OnCoinsChanged += OnCoinsChanged;

        CloseAllPanels();
        UpdateUI();

        if (playerInputHandler != null)
        {
            playerInputHandler.OnPauseInput += OnPauseButtonClick;
            Debug.Log("UIManager: Подписан на OnPauseInput");
        }

        Debug.Log("UIManager: Готов к работе!");

    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateUI;
        }

        EventBus.OnCoinsChanged -= OnCoinsChanged;

        if (playerInputHandler != null)
        {
            playerInputHandler.OnPauseInput -= OnPauseButtonClick;
        }
    }

    // ===== UI UPDATE =====

    private void UpdateUI()
    {
        if (inventory != null)
        {
            if (woodText != null)
                woodText.text = $"{inventory.GetAmount("Дерево")}/{inventory.GetMax("Дерево")}";

            if (stoneText != null)
                stoneText.text = $"{inventory.GetAmount("Камень")}/{inventory.GetMax("Камень")}";

            if (milkText != null)
                milkText.text = $"{inventory.GetAmount("Молоко")}/{inventory.GetMax("Молоко")}";

            if (woolText != null)
                woolText.text = $"{inventory.GetAmount("Шерсть")}/{inventory.GetMax("Шерсть")}";
        }

        if (coinsText != null)
            coinsText.text = coins.ToString();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
    }

    public int GetCoins() => coins;

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

    // ===== PANEL MANAGEMENT =====

    public void OpenPanel(GameObject panel)
    {
        if (panel == null) return;

        Debug.Log($"=== UIManager.OpenPanel: {panel.name} ===");
        Debug.Log($"  - panel.activeSelf before: {panel.activeSelf}");

        panel.SetActive(true);
        isAnyPanelOpen = true;

        OnPanelsOpened?.Invoke();
        Debug.Log("OnPanelsOpened вызван");

        Debug.Log($"  - panel.activeSelf after: {panel.activeSelf}");
        Debug.Log($"  - playerInputHandler: {(playerInputHandler != null ? "exists" : "NULL")}");

        if (playerInputHandler != null)
        {
            Debug.Log("  - Calling SetInputEnabled(false)");
            playerInputHandler.SetInputEnabled(false);

            Debug.Log("  - Calling LockCursor(false)");
            playerInputHandler.LockCursor(false);
        }

        Debug.Log($"  - Time.timeScale before: {Time.timeScale}");
        Time.timeScale = 0f;
        Debug.Log($"  - Time.timeScale after: {Time.timeScale}");

        Debug.Log($"UI Panel opened: {panel.name}");
    }

    public void ClosePanel(GameObject panel)
    {
        if (panel == null) return;

        Debug.Log($"=== UIManager.ClosePanel: {panel.name} ===");
        Debug.Log($"  - panel.activeSelf before: {panel.activeSelf}");

        panel.SetActive(false);

        Debug.Log($"  - panel.activeSelf after: {panel.activeSelf}");

        bool hasAnyOpen = false;
        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf) hasAnyOpen = true;
        if (levelCompletePanel != null && levelCompletePanel.activeSelf) hasAnyOpen = true;

        Debug.Log($"  - hasAnyOpen: {hasAnyOpen}");

        if (!hasAnyOpen)
        {
            isAnyPanelOpen = false;
            OnPanelsClosed?.Invoke();
            Debug.Log("OnPanelsClosed вызван");

            if (playerInputHandler != null)
            {
                Debug.Log("  - Calling SetInputEnabled(true)");
                playerInputHandler.SetInputEnabled(true);

                Debug.Log("  - Calling LockCursor(true)");
                playerInputHandler.LockCursor(true);
            }

            Debug.Log($"  - Time.timeScale before: {Time.timeScale}");
            Time.timeScale = 1f;
            Debug.Log($"  - Time.timeScale after: {Time.timeScale}");
        }

        Debug.Log($"UI Panel closed: {panel.name}");
    }

    public void CloseAllPanels()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);

        isAnyPanelOpen = false;

        if (playerInputHandler != null)
        {
            playerInputHandler.SetInputEnabled(true);
            playerInputHandler.LockCursor(true);
        }

        Time.timeScale = 1f;
        Debug.Log("All panels closed");
    }

    public void ShowLevelComplete(bool hasNextLevel)
    {
        Debug.Log("=== UIManager.ShowLevelComplete ===");
        Debug.Log($"  - levelCompletePanel: {(levelCompletePanel != null ? "exists" : "NULL")}");
        Debug.Log($"  - hasNextLevel: {hasNextLevel}");

        if (levelCompletePanel == null)
        {
            Debug.LogError("levelCompletePanel is NULL!");
            return;
        }

        // Проверяем состояние до открытия
        Debug.Log($"  - panel active before: {levelCompletePanel.activeSelf}");
        Debug.Log($"  - panel activeInHierarchy: {levelCompletePanel.activeInHierarchy}");

        // Настраиваем кнопку "Следующий уровень"
        if (nextLevelButton != null)
        {
            nextLevelButton.SetActive(true);
            var button = nextLevelButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.interactable = hasNextLevel;
                Debug.Log($"  - NextLevelButton interactable: {hasNextLevel}");
            }
        }

        // ОТКРЫВАЕМ ПАНЕЛЬ
        OpenPanel(levelCompletePanel);

        // Проверяем состояние после открытия
        Debug.Log($"  - panel active after: {levelCompletePanel.activeSelf}");
        Debug.Log($"  - panel activeInHierarchy after: {levelCompletePanel.activeInHierarchy}");

        // Проверяем кнопки
        Button[] buttons = levelCompletePanel.GetComponentsInChildren<Button>(true);
        Debug.Log($"  - Найдено кнопок: {buttons.Length}");
        foreach (var btn in buttons)
        {
            Debug.Log($"    - {btn.name}: interactable={btn.interactable}, active={btn.gameObject.activeSelf}");
        }

        Debug.Log("=== ShowLevelComplete END ===");
    }

    public void HideLevelComplete()
    {
        if (levelCompletePanel != null)
        {
            ClosePanel(levelCompletePanel);
        }
    }

    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            OpenPanel(pauseMenuPanel);
        }
    }

    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            ClosePanel(pauseMenuPanel);
        }
    }

    public bool IsAnyPanelOpen() => isAnyPanelOpen;

    // ===== BUTTON HANDLERS =====

    public void OnResumeButtonClick()
    {
        Debug.Log($"=== UIManager.OnResumeButtonClick ===");
        HidePauseMenu();
    }

    public void OnNextLevelButtonClick()
    {
        // 1. Сначала скрываем панель (это вернёт камеру и ввод)
        HideLevelComplete();

        // 2. Затем загружаем следующий уровень
        if (levelManager != null)
        {
            levelManager.LoadNextLevel();
        }
    }

    public void OnRetryButtonClick()
    {
        Debug.Log($"=== UIManager.OnRetryButtonClick ===");
        HideLevelComplete();
        HidePauseMenu();

        if (levelManager != null)
        {
            Debug.Log("  - Calling levelManager.RetryLevel()");
            levelManager.RetryLevel();
        }
        else
        {
            Debug.LogError("  - levelManager is NULL!");
        }
    }

    public void OnSaveAndQuitButtonClick()
    {
        HidePauseMenu();

        if (levelManager != null)
        {
            levelManager.GoToMainMenu();
        }
    }

    public void OnQuitWithoutSaveButtonClick()
    {
        HidePauseMenu();

        if (levelManager != null)
        {
            levelManager.GoToMainMenuWithoutSave();
        }
    }

    public void OnPauseButtonClick()
    {
        Debug.Log($"=== UIManager.OnPauseButtonClick ===");
        Debug.Log($"  - isAnyPanelOpen: {isAnyPanelOpen}");

        if (isAnyPanelOpen)
        {
            Debug.Log("  - Calling HidePauseMenu()");
            HidePauseMenu();
        }
        else
        {
            Debug.Log("  - Calling ShowPauseMenu()");
            ShowPauseMenu();
        }
    }

    /// <summary>
    /// В ГЛАВНОЕ МЕНЮ ИЗ LevelCompletePanel
    /// </summary>
    public void OnMainMenuFromCompleteButtonClick()
    {
        // 1. Скрываем панель завершения
        HideLevelComplete();

        // 2. Переходим в главное меню с сохранением
        if (levelManager != null)
        {
            levelManager.GoToMainMenu();
        }
    }
}