using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Zenject;

public class GameMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;  
    [SerializeField] private Button continueButton;
    [SerializeField] private Button saveAndQuitButton;
    [SerializeField] private Button quitWithoutSaveButton;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Inject] private GameSaveController gameSaveController;

    private bool isPaused = false;

    private void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (saveAndQuitButton != null)
            saveAndQuitButton.onClick.AddListener(SaveAndQuit);

        if (quitWithoutSaveButton != null)
            quitWithoutSaveButton.onClick.AddListener(QuitWithoutSave);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;

        Debug.Log($"Пауза: {(isPaused ? "ВКЛ" : "ВЫКЛ")}");
    }

    public void ContinueGame()
    {
        TogglePause();
        Debug.Log("Продолжить игру");
    }

    public void SaveAndQuit()
    {
        Debug.Log("Сохранение и выход в главное меню");

        if (gameSaveController != null)
        {
            gameSaveController.SaveGame();
            Debug.Log("Игра сохранена");
        }
        else
        {
            Debug.LogWarning("GameSaveController не найден");
        }

        Time.timeScale = 1f; 
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitWithoutSave()
    {
        Debug.Log("Выход в главное меню без сохранения");
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}