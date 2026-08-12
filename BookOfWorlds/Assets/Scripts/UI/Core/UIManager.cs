using UnityEngine;
using Zenject;

/// <summary>
/// Главный координатор UI.
/// Связывает все UI-компоненты и предоставляет API для игровой логики.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PanelManager panelManager;
    [SerializeField] private HUDController hudController;

    [Header("Level Complete")]
    [SerializeField] private GameObject nextLevelButton;

    [Inject] private LevelManager levelManager;
    [Inject] private PlayerInputHandlerMy playerInputHandlerMy;

    private int coins = 0;

    private void Start()
    {
        // ===== ПОДПИСКА НА ВВОД =====
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
        if (panelManager == null) return;

        var panel = panelManager.GetLevelCompletePanel();
        if (panel != null)
        {
            panelManager.ClosePanel(panel);
        }
    }

    public void ShowPauseMenu()
    {
        if (panelManager == null) return;

        Debug.Log("Показано меню паузы");

        var panel = panelManager.GetPauseMenuPanel();
        if (panel != null)
        {
            panelManager.OpenPanel(panel);
        }
    }

    public void HidePauseMenu()
    {
        if (panelManager == null) return;

        Debug.Log("Скрыто меню паузы");

        var panel = panelManager.GetPauseMenuPanel();
        if (panel != null)
        {
            panelManager.ClosePanel(panel);
        }
    }

    public void CloseAllPanels()
    {
        panelManager?.CloseAllPanels();
    }

    public bool IsAnyPanelOpen() => panelManager != null && panelManager.IsAnyPanelOpen;

    // ===== BUTTON HANDLERS =====

    public void OnResumeButtonClick()
    {
        Debug.Log("Нажата кнопка Продолжить");
        HidePauseMenu();
    }

    public void OnNextLevelButtonClick()
    {
        Debug.Log("Нажата кнопка Следующий уровень");
        HideLevelComplete();

        if (levelManager != null)
        {
            levelManager.LoadNextLevel();
        }
    }

    public void OnRetryButtonClick()
    {
        Debug.Log("Нажата кнопка Повторить уровень");
        HideLevelComplete();
        HidePauseMenu();

        if (levelManager != null)
        {
            levelManager.RetryLevel();
        }
    }

    public void OnSaveAndQuitButtonClick()
    {
        Debug.Log("Нажата кнопка Сохранить и выйти");
        HidePauseMenu();

        if (levelManager != null)
        {
            levelManager.GoToMainMenu();
        }
    }

    public void OnQuitWithoutSaveButtonClick()
    {
        Debug.Log("Нажата кнопка Выйти без сохранения");
        HidePauseMenu();

        if (levelManager != null)
        {
            levelManager.GoToMainMenuWithoutSave();
        }
    }

    public void OnPauseButtonClick()
    {
        if (IsAnyPanelOpen())
        {
            HidePauseMenu();
        }
        else
        {
            ShowPauseMenu();
        }
    }

    public void OnMainMenuFromCompleteButtonClick()
    {
        Debug.Log("Нажата кнопка В главное меню (с панели завершения)");
        HideLevelComplete();

        if (levelManager != null)
        {
            levelManager.GoToMainMenu();
        }
    }
}