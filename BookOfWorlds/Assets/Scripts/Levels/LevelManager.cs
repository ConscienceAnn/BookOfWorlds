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
   

    private int currentLevelIndex = 0;
    private LevelDataSO currentLevelData;
    private bool isLevelComplete = false;
    private bool isLoadingLevel = false;

    public LevelDataSO CurrentLevelData => currentLevelData;
    public int CurrentLevelIndex => currentLevelIndex;
    public int GetLevelsCount() => levels.Count;
    public bool HasNextLevel() => currentLevelIndex + 1 < levels.Count;

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

    /// <summary>
    /// Загрузка уровня по индексу
    /// </summary>
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

        // 2. Применяем сохранённый прогресс к зданиям
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
        Debug.Log($"Загружен уровень {currentLevelIndex + 1}: {currentLevelData.levelName}");
    }

    /// <summary>
    /// Загрузка следующего уровня
    /// </summary>
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

        // Сохраняем прогресс (ресурсы НЕ сохраняем при переходе между уровнями)
        if (gameSaveController != null)
        {
            gameSaveController.SaveGame(false);
            Debug.Log("Прогресс сохранён перед переходом на следующий уровень");
        }

        // Анимация перехода (опционально)
        await UniTask.Delay(300);

        // Загружаем следующий уровень
        LoadLevel(currentLevelIndex + 1);
    }

    /// <summary>
    /// Перезапуск текущего уровня
    /// </summary>
    public void RetryLevel()
    {
        Debug.Log($"Перезапуск уровня {currentLevelData?.levelName}");

        // Скрываем панель завершения
        levelCompletePanel?.SetActive(false);

        // Загружаем уровень заново
        LoadLevel(currentLevelIndex);
    }

    /// <summary>
    /// Переход в главное меню
    /// </summary>
    public void GoToMainMenu()
    {
        Debug.Log("Возврат в главное меню");

        // Сохраняем ВСЁ (с ресурсами)
        if (gameSaveController != null)
        {
            gameSaveController.SaveGame(true);
            Debug.Log("Прогресс сохранён перед выходом в меню");
        }

        // Загружаем главное меню
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Обработчик завершения уровня
    /// </summary>
    private void OnLevelComplete()
    {
        if (isLevelComplete) return;
        isLevelComplete = true;

        Debug.Log($"Уровень {currentLevelData.levelName} завершён!");

        // 1. Открываем следующий уровень в сохранении
        OpenNextLevelInSave();

        // 2. Сохраняем прогресс БЕЗ ресурсов (чтобы при переходе ресурсы остались в инвентаре)
        if (gameSaveController != null)
        {
            gameSaveController.SaveGame(false);
            Debug.Log("Прогресс сохранён после завершения уровня (ресурсы сохранены)");
        }

        // 3. Показываем UI завершения
        ShowCompleteUI();
    }

    /// <summary>
    /// Открытие следующего уровня в сохранении
    /// </summary>
    private void OpenNextLevelInSave()
    {
        SaveData saveData = SaveSystem.Load();
        if (saveData == null)
        {
            saveData = new SaveData();
        }

        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex < levels.Count)
        {
            string nextLevelName = levels[nextLevelIndex].levelName;
            if (!saveData.openedLevels.Contains(nextLevelName))
            {
                saveData.openedLevels.Add(nextLevelName);
                SaveSystem.Save(saveData);
                Debug.Log($" Открыт уровень: {nextLevelName}");
            }
        }
    }

    /// <summary>
    /// Показ UI завершения уровня
    /// </summary>
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
            // Кнопка ВСЕГДА видна, но НЕАКТИВНА если нет следующего уровня
            nextLevelButton.SetActive(true);

            // Получаем компонент Button
            UnityEngine.UI.Button button = nextLevelButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.interactable = hasNextLevel;
            }

            // Меняем текст для обратной связи
            var text = nextLevelButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = hasNextLevel ? " Следующий уровень" : " Все уровни пройдены!";
            }

            Debug.Log($"  - Кнопка NextLevel: {(hasNextLevel ? "АКТИВНА" : "НЕАКТИВНА (все уровни пройдены)")}");
        }

        if (!hasNextLevel)
        {
            playerUI?.ShowNotification(" Все уровни пройдены!", 3f);
        }
        else
        {
            playerUI?.ShowNotification($" Уровень {currentLevelData.levelName} завершён!", 2f);
        }
    }

    /// <summary>
    /// Показ завершения игры
    /// </summary>
    private void ShowGameComplete()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.SetActive(false);
        }

        playerUI?.ShowNotification("Поздравляем! Игра пройдена!", 5f);
    }

    /// <summary>
    /// Загрузка сохранённого прогресса
    /// </summary>
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

    /// <summary>
    /// Сохранение текущего уровня
    /// </summary>
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