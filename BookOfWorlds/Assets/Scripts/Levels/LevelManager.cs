using UnityEngine;
using Zenject;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class LevelManager : MonoBehaviour
{
    [Header("Levels")]
    [SerializeField] private List<LevelDataSO> levels = new List<LevelDataSO>();
    [SerializeField] private LevelGenerator levelGenerator;

    [Header("UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject nextLevelButton;

    [Inject] private PlayerUI playerUI;
    [Inject] private GameSaveController gameSaveController;

    private int currentLevelIndex = 0;
    private LevelDataSO currentLevelData;
    private bool isLevelComplete = false;

    public LevelDataSO CurrentLevelData => currentLevelData;
    public int CurrentLevelIndex => currentLevelIndex;
    public int GetLevelsCount() => levels.Count;

    private void Start()
    {
        LoadProgress();
        LoadLevel(currentLevelIndex);

        //  ПРАВИЛЬНАЯ ПОДПИСКА на событие OnBuildingRestored
        EventBus.OnBuildingRestored += OnBuildingRestored;
    }

    private void OnDestroy()
    {
        // ПРАВИЛЬНАЯ ОТПИСКА
        EventBus.OnBuildingRestored -= OnBuildingRestored;
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= levels.Count)
        {
            Debug.Log("Все уровни пройдены!");
            ShowGameComplete();
            return;
        }

        currentLevelIndex = levelIndex;
        currentLevelData = levels[levelIndex];

        if (levelGenerator != null)
        {
            levelGenerator.GenerateLevel(currentLevelData);
        }

        UpdateLevelUI();

        isLevelComplete = false;
        levelCompletePanel?.SetActive(false);

        Debug.Log($" Загружен уровень {currentLevelIndex + 1}: {currentLevelData.levelName}");
    }

    public async void LoadNextLevel()
    {
        if (!isLevelComplete)
        {
            playerUI?.ShowNotification("Восстановите все здания!", 2f);
            return;
        }

        if (gameSaveController != null)
        {
            gameSaveController.SaveGame();
        }

        await UniTask.Delay(500);

        LoadLevel(currentLevelIndex + 1);
    }

    private void OnBuildingRestored(BuildingController building)
    {
        CheckLevelComplete();
    }

    private void CheckLevelComplete()
    {
        if (isLevelComplete) return;

        var buildings = levelGenerator.GetBuildings();
        bool allRestored = true;

        foreach (var building in buildings)
        {
            if (building != null && !building.IsRestored())
            {
                allRestored = false;
                break;
            }
        }

        if (allRestored && buildings.Count > 0)
        {
            isLevelComplete = true;
            OnLevelComplete();
        }
    }

    private void OnLevelComplete()
    {
        Debug.Log($"Уровень {currentLevelData.levelName} завершён!");

        SaveData saveData = SaveSystem.Load();
        if (saveData != null)
        {
            int nextLevelIndex = currentLevelIndex + 1;
            if (nextLevelIndex < levels.Count && !saveData.openedLevels.Contains(levels[nextLevelIndex].levelName))
            {
                saveData.openedLevels.Add(levels[nextLevelIndex].levelName);
                SaveSystem.Save(saveData);
            }
        }

        levelCompletePanel?.SetActive(true);

        bool hasNextLevel = currentLevelIndex + 1 < levels.Count;
        nextLevelButton?.SetActive(hasNextLevel);

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
        levelCompletePanel?.SetActive(true);
        nextLevelButton?.SetActive(false);
        playerUI?.ShowNotification("Поздравляем! Игра пройдена!", 5f);
    }

    private void UpdateLevelUI()
    {
        // Можно добавить отображение названия уровня
    }

    private void LoadProgress()
    {
        SaveData data = SaveSystem.Load();
        if (data != null)
        {
            currentLevelIndex = data.currentLevel;
            Debug.Log($" Загружен прогресс: уровень {currentLevelIndex}");
        }
    }
}