using UnityEngine;
using Zenject;

/// <summary>
/// Управляет открытием и закрытием панелей.
/// Отвечает за блокировку ввода и паузу времени.
/// </summary>
public class PanelManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject levelCompletePanel;

    [Inject] private PlayerInputHandlerMy playerInputHandlerMy;

    [Inject(Optional = true)] private AudioHelper _audioHelper;
    [Inject(Optional = true)] private GameSaveController gameSaveController;

    public event System.Action OnPanelsOpened;
    public event System.Action OnPanelsClosed;

    private bool isAnyPanelOpen = false;

    public bool IsAnyPanelOpen => isAnyPanelOpen;

    public void OpenPanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogError($"PanelManager: panel is NULL!");
            return;
        }

        Debug.Log($"Панель открыта: {panel.name}");

        bool isLoading = gameSaveController != null && gameSaveController.IsLoadingGame;
        if (!isLoading && _audioHelper != null && _audioHelper.IsReady)
        {
            _audioHelper.PlaySound("ui_open");
        }

        panel.SetActive(true);
        isAnyPanelOpen = true;

        OnPanelsOpened?.Invoke();

        // Блокируем ввод и ставим на паузу
        if (playerInputHandlerMy != null)
        {
            playerInputHandlerMy.SetInputEnabled(false);
            playerInputHandlerMy.LockCursor(false);
        }

        Time.timeScale = 0f;
    }

    public void ClosePanel(GameObject panel)
    {
        if (panel == null) return;

        Debug.Log($"Панель закрыта: {panel.name}");

        bool isLoading = gameSaveController != null && gameSaveController.IsLoadingGame;
        if (!isLoading && _audioHelper != null && _audioHelper.IsReady)
        {
            _audioHelper.PlaySound("ui_click");
        }

        panel.SetActive(false);

        // Проверяем, остались ли открытые панели
        bool hasAnyOpen = false;
        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf) hasAnyOpen = true;
        if (levelCompletePanel != null && levelCompletePanel.activeSelf) hasAnyOpen = true;

        if (!hasAnyOpen)
        {
            isAnyPanelOpen = false;
            OnPanelsClosed?.Invoke();

            // Разблокируем ввод и возобновляем время
            if (playerInputHandlerMy != null)
            {
                playerInputHandlerMy.SetInputEnabled(true);
                playerInputHandlerMy.LockCursor(true);
            }

            Time.timeScale = 1f;
        }
    }

    public void CloseAllPanels()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);

        isAnyPanelOpen = false;

        if (playerInputHandlerMy != null)
        {
            playerInputHandlerMy.SetInputEnabled(true);
            playerInputHandlerMy.LockCursor(true);
        }

        Time.timeScale = 1f;
    }

    public GameObject GetPauseMenuPanel() => pauseMenuPanel;
    public GameObject GetLevelCompletePanel() => levelCompletePanel;
}