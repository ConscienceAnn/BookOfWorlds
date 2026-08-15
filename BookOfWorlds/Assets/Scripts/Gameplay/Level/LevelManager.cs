using UnityEngine;
using Zenject;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [Header("Levels")]
    [SerializeField] private List<LevelDataSO> levels = new List<LevelDataSO>();
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private LevelProgress levelProgress;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Inject] private PlayerUIMediator playerUIMediator;
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

    public async void LoadLevel(int levelIndex)
    {
        if (isLoadingLevel) return;
        isLoadingLevel = true;

        if (levelIndex >= levels.Count)
        {
            ShowGameComplete();
            isLoadingLevel = false;
            return;
        }

        uiManager?.SetLoadingState(true);

        currentLevelIndex = levelIndex;
        currentLevelData = levels[levelIndex];
        isLevelComplete = false;

        Debug.Log($"Загружается уровень {levelIndex + 1}: {currentLevelData.levelName}");

        // 1. Генерируем уровень АСИНХРОННО и ЖДЁМ
        if (levelGenerator != null)
        {
            await levelGenerator.GenerateLevelAsync(currentLevelData);
        }

        // 2. Теперь всё чисто, загружаем сохранение
        if (gameSaveController != null)
        {
            if (isRetry)
            {
                gameSaveController.ClearLevelProgress();

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

        // 3. Закрываем все панели
        if (uiManager != null)
        {
            uiManager.CloseAllPanels();
        }

        // 4. Обновляем прогресс
        if (levelProgress != null)
        {
            levelProgress.ForceUpdate();
        }

        // 5. Сохраняем текущий уровень
        SaveCurrentLevel();

        uiManager?.SetLoadingState(false);

        isLoadingLevel = false;
    }

    public async void LoadNextLevel()
    {
        if (!isLevelComplete)
        {
            playerUIMediator?.ShowNotification("Восстановите все здания!", 2f);
            return;
        }

        if (!HasNextLevel())
        {
            playerUIMediator?.ShowNotification("Это последний уровень!", 2f);
            return;
        }

        // Закрываем панель через UIManager
        if (uiManager != null)
        {
            uiManager.HideLevelComplete();
        }

        // Очищаем состояние игрока
        if (gameSaveController != null)
        {
            gameSaveController.ClearPlayerState();
        }

        await UniTask.Delay(300);
        isRetry = false;
        LoadLevel(currentLevelIndex + 1);
    }

    public void RetryLevel()
    {
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

        if (uiManager != null)
        {
            uiManager.CloseAllPanels();
        }

        if (gameSaveController != null)
        {
            gameSaveController.SaveGame();
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
        }

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

        playerUIMediator?.ShowNotification("Поздравляем! Игра пройдена!", 5f);
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
    }

    /// <summary>
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

        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    private IEnumerator DelayedLoadComplete()
    {
        yield return null;  // Ждём 1 кадр

        // 3. Загружаем сохранение или сбрасываем при ретрае
        if (gameSaveController != null)
        {
            if (isRetry)
            {
                gameSaveController.ClearLevelProgress();

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

        // 4. Закрываем все панели
        if (uiManager != null)
        {
            uiManager.CloseAllPanels();
        }

        // 5. Обновляем прогресс
        if (levelProgress != null)
        {
            levelProgress.ForceUpdate();
        }

        // 6. Сохраняем текущий уровень
        SaveCurrentLevel();

        isLoadingLevel = false;
    }
}