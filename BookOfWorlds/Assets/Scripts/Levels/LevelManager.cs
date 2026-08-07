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

    [Header("UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject nextLevelButton;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Inject] private PlayerUI playerUI;
    [Inject] private GameSaveController gameSaveController;
    [Inject] private UIManager uiManager;

    private int currentLevelIndex = 0;
    private LevelDataSO currentLevelData;
    private bool isLevelComplete = false;
    private bool isLoadingLevel = false;

    public LevelDataSO CurrentLevelData => currentLevelData;
    public int CurrentLevelIndex => currentLevelIndex;
    public bool IsLevelComplete => isLevelComplete;

    public int GetLevelsCount() => levels.Count;
    public bool HasNextLevel() => currentLevelIndex + 1 < levels.Count;

    public string GetLevelName(int index)
    {
        if (index < 0 || index >= levels.Count) return null;
        return levels[index]?.levelName;
    }

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
            Debug.Log(" Все уровни пройдены!");
            ShowGameComplete();
            isLoadingLevel = false;
            return;
        }

        currentLevelIndex = levelIndex;
        currentLevelData = levels[levelIndex];
        isLevelComplete = false;

        // 1. Генерируем уровень (очищает старые объекты)
        if (levelGenerator != null)
        {
            levelGenerator.GenerateLevel(currentLevelData);
        }

        // 2. Загружаем сохранённый прогресс (монеты, ресурсы, здания)
        if (gameSaveController != null)
        {
            gameSaveController.LoadGame();
        }

        // 3. Обновляем UI
        levelCompletePanel?.SetActive(false);

        if (levelProgress != null)
        {
            levelProgress.ForceUpdate();
        }

        // 4. Сохраняем текущий уровень
        SaveCurrentLevel();

        isLoadingLevel = false;
        Debug.Log($" Загружен уровень {currentLevelIndex + 1}: {currentLevelData.levelName}");
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
            playerUI?.ShowNotification(" Это последний уровень!", 2f);
            return;
        }

        // Очищаем состояние игрока (монеты + ресурсы)
        if (gameSaveController != null)
        {
            gameSaveController.ClearPlayerState();
            Debug.Log(" Игровое состояние очищено для нового уровня");
        }

        await UniTask.Delay(300);

        LoadLevel(currentLevelIndex + 1);
    }

    public void RetryLevel()
    {
        Debug.Log($"Перезапуск уровня {currentLevelData?.levelName}");
        levelCompletePanel?.SetActive(false);
        LoadLevel(currentLevelIndex);
    }

    public void GoToMainMenu()
    {
        Debug.Log("Возврат в главное меню");

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

        ShowCompleteUI();
    }

    private void ShowCompleteUI()
    {
        if (levelCompletePanel == null)
        {
            Debug.LogWarning("levelCompletePanel не назначен в LevelManager!");
            return;
        }

        levelCompletePanel.SetActive(true);

        bool hasNextLevel = HasNextLevel();

        if (nextLevelButton != null)
        {
            nextLevelButton.SetActive(true);

            UnityEngine.UI.Button button = nextLevelButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.interactable = hasNextLevel;
            }

            var text = nextLevelButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = hasNextLevel ? "Следующий уровень" : "Все уровни пройдены!";
            }
        }

        if (!hasNextLevel)
        {
            playerUI?.ShowNotification("Все уровни пройдены!", 3f);
        }
        else
        {
            playerUI?.ShowNotification($"Уровень {currentLevelData.levelName} завершён!", 2f);
        }
    }

    private void ShowGameComplete()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.SetActive(true);
            UnityEngine.UI.Button button = nextLevelButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.interactable = false;
            }
            var text = nextLevelButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "Игра пройдена!";
            }
        }

        playerUI?.ShowNotification("Поздравляем! Игра пройдена!", 5f);
    }

    private void LoadProgress()
    {
        SaveData data = SaveSystem.Load();
        if (data != null)
        {
            currentLevelIndex = data.currentLevel;
            Debug.Log($"Загружен прогресс: уровень {currentLevelIndex}");
        }
        else
        {
            currentLevelIndex = 0;
            Debug.Log("Нет сохранения, начинаем с первого уровня");
        }
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
}