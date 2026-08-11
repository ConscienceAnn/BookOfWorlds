using UnityEngine;
using Zenject;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class LevelManager : MonoBehaviour
{
    [Header("Levels")]
    [SerializeField] private List<LevelDataSO> levels = new List<LevelDataSO>();
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private LevelProgress levelProgress;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Inject] private PlayerUI playerUI;
    [Inject] private GameSaveController gameSaveController;
    [Inject] private UIManager uiManager;

    private int currentLevelIndex = 0;
    private LevelDataSO currentLevelData;
    private bool isLevelComplete = false;
    private bool isLoadingLevel = false;
    private bool isRetry = false;

    public LevelDataSO CurrentLevelData => currentLevelData;
    public int CurrentLevelIndex => currentLevelIndex;
    public bool IsLevelComplete => isLevelComplete;

    public int GetLevelsCount() => levels.Count;
    public bool HasNextLevel() => currentLevelIndex + 1 < levels.Count;
    public string GetLevelName(int index) => index >= 0 && index < levels.Count ? levels[index]?.levelName : null;

    private void Start()
    {
        LoadProgress();
        LoadLevel(currentLevelIndex);

        if (levelProgress != null)
        {
            levelProgress.OnLevelComplete += OnLevelComplete;
        }
    }

    private void OnDestroy()
    {
        if (levelProgress != null)
        {
            levelProgress.OnLevelComplete -= OnLevelComplete;
        }
    }

    public void LoadLevel(int levelIndex)
    {
        if (isLoadingLevel) return;
        isLoadingLevel = true;

        if (levelIndex >= levels.Count)
        {
            Debug.Log("Все уровни пройдены!");
            ShowGameComplete();
            isLoadingLevel = false;
            return;
        }

        currentLevelIndex = levelIndex;
        currentLevelData = levels[levelIndex];
        isLevelComplete = false;

        // 1. Генерируем уровень
        if (levelGenerator != null)
        {
            levelGenerator.GenerateLevel(currentLevelData);
        }

        // 2. Загружаем сохранение или сбрасываем при ретрае
        if (gameSaveController != null)
        {
            if (isRetry)
            {
                gameSaveController.ClearLevelProgress();
                Debug.Log($"Уровень {currentLevelData.levelName} сброшен (Retry)");

                if (levelProgress != null)
                {
                    levelProgress.SetRetryMode(false);
                }
                isRetry = false;
            }
            else
            {
                gameSaveController.LoadGame();
            }
        }

        // 3. Закрываем все панели через UIManager
        if (uiManager != null)
        {
            uiManager.CloseAllPanels();
        }

        if (levelProgress != null)
        {
            levelProgress.ForceUpdate();
        }

        // 4. Сохраняем текущий уровень
        SaveCurrentLevel();

        isLoadingLevel = false;
        Debug.Log($"Загружен уровень {currentLevelIndex + 1}: {currentLevelData.levelName}");
    }

    public async void LoadNextLevel()
    {
        if (!isLevelComplete)
        {
            playerUI?.ShowNotification("Восстановите все здания!", 2f);
            return;
        }

        if (!HasNextLevel())
        {
            playerUI?.ShowNotification("Это последний уровень!", 2f);
            return;
        }

        // Закрываем панель через UIManager
        if (uiManager != null)
        {
            uiManager.HideLevelComplete();  // Используем метод вместо прямого доступа
        }

        // Очищаем состояние игрока
        if (gameSaveController != null)
        {
            gameSaveController.ClearPlayerState();
            Debug.Log("Игровое состояние очищено для нового уровня");
        }

        await UniTask.Delay(300);
        isRetry = false;
        LoadLevel(currentLevelIndex + 1);
    }

    public void RetryLevel()
    {
        Debug.Log($"Перезапуск уровня {currentLevelData?.levelName}");
        isRetry = true;

        // Сбрасываем прогресс
        if (levelProgress != null)
        {
            levelProgress.ReturnToGameCamera();
            levelProgress.SetRetryMode(true);
            levelProgress.ResetState();
        }

        // Сбрасываем данные игрока
        if (gameSaveController != null)
        {
            gameSaveController.ClearLevelProgress();
        }

        // Закрываем все панели через UIManager
        if (uiManager != null)
        {
            uiManager.CloseAllPanels();
        }

        // Перезагружаем уровень
        LoadLevel(currentLevelIndex);
    }

    public void GoToMainMenu()
    {
        Debug.Log("Возврат в главное меню");

        // Закрываем все панели
        if (uiManager != null)
        {
            uiManager.CloseAllPanels();
        }

        if (gameSaveController != null)
        {
            gameSaveController.SaveGame();
            Debug.Log("Прогресс сохранён");
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnLevelComplete()
    {
        if (isLevelComplete) return;
        isLevelComplete = true;

        Debug.Log($"Уровень {currentLevelData.levelName} завершён!");

        if (gameSaveController != null)
        {
            gameSaveController.SaveGame();
            Debug.Log("Прогресс сохранён");
        }

        // ПОКАЗЫВАЕМ ПАНЕЛЬ ЧЕРЕЗ UIManager
        if (uiManager != null)
        {
            uiManager.ShowLevelComplete(HasNextLevel());
        }
    }

    private void ShowGameComplete()
    {
        if (uiManager != null)
        {
            uiManager.ShowLevelComplete(false);
        }

        playerUI?.ShowNotification("Поздравляем! Игра пройдена!", 5f);
    }

    private void LoadProgress()
    {
        SaveData data = SaveSystem.Load();
        currentLevelIndex = data?.currentLevel ?? 0;
        Debug.Log(currentLevelIndex > 0 ? $"Загружен прогресс: уровень {currentLevelIndex}" : "Нет сохранения, начинаем с первого уровня");
    }

    private void SaveCurrentLevel()
    {
        SaveData data = SaveSystem.Load() ?? new SaveData();
        data.currentLevel = currentLevelIndex;

        if (data.openedLevels == null || data.openedLevels.Count == 0)
        {
            data.openedLevels = new List<string> { levels[0]?.levelName ?? "Level1" };
        }

        SaveSystem.Save(data);
        Debug.Log($"Сохранён текущий уровень: {currentLevelIndex}");
    }

    // <summary>
    /// ПЕРЕХОД В ГЛАВНОЕ МЕНЮ БЕЗ СОХРАНЕНИЯ
    /// </summary>
    public void GoToMainMenuWithoutSave()
    {
        Debug.Log("Возврат в главное меню без сохранения");

        // Закрываем все панели
        if (uiManager != null)
        {
            uiManager.CloseAllPanels();
        }

        // НЕ СОХРАНЯЕМ ИГРУ!
        // Просто переходим в главное меню
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }
}