using UnityEngine;
using Zenject;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PanelManager panelManager;
    [SerializeField] private HUDController hudController;

    [Header("Level Complete")]
    [SerializeField] private GameObject nextLevelButton;

    [Inject(Optional = true)] private LevelManager levelManager;
    [Inject(Optional = true)] private PlayerInputHandlerMy playerInputHandlerMy;
    [Inject(Optional = true)] private PauseService _pauseService;
    [Inject(Optional = true)] private AudioHelper _audioHelper;
    [Inject(Optional = true)] private GameSaveController gameSaveController;

    private int coins = 0;
    private bool isLoadingGame = false;

    private void Start()
    {
        if (playerInputHandlerMy != null)
        {
            playerInputHandlerMy.OnPauseInput += OnPauseButtonClick;
        }
        else
        {
            Debug.LogError("UIManager: playerInputHandler is NULL!");
        }
    }

    private void OnDestroy()
    {
        if (playerInputHandlerMy != null)
        {
            playerInputHandlerMy.OnPauseInput -= OnPauseButtonClick;
        }
    }

    // ===== ПУБЛИЧНЫЙ МЕТОД ДЛЯ УПРАВЛЕНИЯ ЗВУКАМИ ПРИ ЗАГРУЗКЕ =====
    public void SetLoadingState(bool isLoading)
    {
        isLoadingGame = isLoading;
        Debug.Log($"[UIManager] SetLoadingState: {isLoading}");
    }

    // ===== HUD API =====

    public void AddCoins(int amount)
    {
        coins += amount;
        EventBus.CoinsChanged(coins);
        hudController?.ForceRefresh();
    }

    public void SetCoins(int amount)
    {
        coins = amount;
        EventBus.CoinsChanged(coins);
        hudController?.ForceRefresh();
    }

    public int GetCoins() => coins;

    public void ForceRefreshUI()
    {
        hudController?.ForceRefresh();
    }

    // ===== PANEL API =====

    public void ShowLevelComplete(bool hasNextLevel)
    {
        // Не показываем панель при загрузке
        if (isLoadingGame)
        {
            Debug.Log("[UIManager] ShowLevelComplete skipped during load");
            return;
        }

        if (panelManager == null)
        {
            Debug.LogError("UIManager: panelManager is NULL!");
            return;
        }

        Debug.Log($"Показана панель завершения уровня, hasNextLevel={hasNextLevel}");

        if (nextLevelButton != null)
        {
            nextLevelButton.SetActive(true);
            var button = nextLevelButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.interactable = hasNextLevel;
            }
        }

        var panel = panelManager.GetLevelCompletePanel();
        if (panel != null)
        {
            panelManager.OpenPanel(panel);
        }
    }

    public void HideLevelComplete()
    {
        // Не закрываем панель при загрузке
        if (gameSaveController != null && gameSaveController.IsLoadingGame)
        {
            Debug.Log("[UIManager] HideLevelComplete skipped during load");
            return;
        }

        if (isLoadingGame)
        {
            Debug.Log("[UIManager] HideLevelComplete skipped during load (local flag)");
            return;
        }

        if (panelManager == null) return;

        var panel = panelManager.GetLevelCompletePanel();
        if (panel != null)
        {
            panelManager.ClosePanel(panel);
        }
    }

    public void CloseAllPanels()
    {
        // Не закрываем панели при загрузке
        if (isLoadingGame)
        {
            Debug.Log("[UIManager] CloseAllPanels skipped during load");
            return;
        }

        panelManager?.CloseAllPanels();
    }

    public bool IsAnyPanelOpen() => panelManager != null && panelManager.IsAnyPanelOpen;

    // ===== BUTTON HANDLERS =====

    public void OnResumeButtonClick()
    {
        _audioHelper?.PlaySound("ui_click");
        Debug.Log("Нажата кнопка Продолжить");
        _pauseService.TogglePause();
    }

    public void OnNextLevelButtonClick()
    {
        _audioHelper?.PlaySound("ui_click");
        Debug.Log("Нажата кнопка Следующий уровень");

        if (_pauseService.IsPaused)
        {
            _pauseService.TogglePause();
        }

        HideLevelComplete();

        if (levelManager != null)
        {
            levelManager.LoadNextLevel();
        }
    }

    public void OnRetryButtonClick()
    {
        _audioHelper?.PlaySound("ui_click");
        Debug.Log("Нажата кнопка Повторить уровень");

        if (_pauseService.IsPaused)
        {
            _pauseService.TogglePause();
        }

        HideLevelComplete();

        if (levelManager != null)
        {
            levelManager.RetryLevel();
        }
    }

    public void OnSaveAndQuitButtonClick()
    {
        _audioHelper?.PlaySound("ui_click");
        Debug.Log("Нажата кнопка Сохранить и выйти");

        if (_pauseService.IsPaused)
        {
            _pauseService.TogglePause();
        }

        if (levelManager != null)
        {
            levelManager.GoToMainMenu();
        }
    }

    public void OnQuitWithoutSaveButtonClick()
    {
        _audioHelper?.PlaySound("ui_click");
        Debug.Log("Нажата кнопка Выйти без сохранения");

        if (_pauseService.IsPaused)
        {
            _pauseService.TogglePause();
        }

        if (levelManager != null)
        {
            levelManager.GoToMainMenuWithoutSave();
        }
    }

    public void OnPauseButtonClick()
    {
        _pauseService.TogglePause();
    }

    public void OnMainMenuFromCompleteButtonClick()
    {
        _audioHelper?.PlaySound("ui_click");
        Debug.Log("Нажата кнопка В главное меню (с панели завершения)");

        if (_pauseService.IsPaused)
        {
            _pauseService.TogglePause();
        }

        HideLevelComplete();

        if (levelManager != null)
        {
            levelManager.GoToMainMenu();
        }
    }

    // ===== ПОДПИСКА НА СОБЫТИЕ ПАУЗЫ =====
    private void OnEnable()
    {
        EventBus.OnPauseStateChanged += OnPauseStateChanged;
    }

    private void OnDisable()
    {
        EventBus.OnPauseStateChanged -= OnPauseStateChanged;
    }

    private void OnPauseStateChanged(bool isPaused)
    {
        if (isPaused)
        {
            ShowPauseMenuPanel();
        }
        else
        {
            HidePauseMenuPanel();
        }
    }

    private void ShowPauseMenuPanel()
    {
        if (panelManager == null) return;

        Debug.Log("Показано меню паузы (панель)");

        var panel = panelManager.GetPauseMenuPanel();
        if (panel != null)
        {
            panelManager.OpenPanel(panel);
        }
    }

    private void HidePauseMenuPanel()
    {
        if (panelManager == null) return;

        Debug.Log("Скрыто меню паузы (панель)");

        var panel = panelManager.GetPauseMenuPanel();
        if (panel != null)
        {
            panelManager.ClosePanel(panel);
        }
    }
}