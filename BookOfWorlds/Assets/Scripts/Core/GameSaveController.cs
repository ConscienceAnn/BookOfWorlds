using UnityEngine;
using Zenject;
using System.Collections.Generic;
using System.Linq;

public class GameSaveController : MonoBehaviour
{
    [Inject] private IPlayerInventory inventory;
    [Inject] private UIManager uiManager;
    [Inject] private LevelProgress levelProgress;
    [Inject] private LevelManager levelManager;

    private List<BuildingController> buildings = new List<BuildingController>();
    private string currentLevelName => levelManager?.CurrentLevelData?.levelName ?? "Unknown";

    // ===== СОХРАНЕНИЕ =====

    public void SaveGame()
    {
        Debug.Log("========== СОХРАНЕНИЕ ==========");

        SaveData data = SaveSystem.Load() ?? new SaveData();

        // 1. Текущий уровень
        if (levelManager != null)
        {
            data.currentLevel = levelManager.CurrentLevelIndex;
            Debug.Log($"  - Уровень: {data.currentLevel} ({currentLevelName})");
        }

        // 2. Монеты
        data.coins = uiManager?.GetCoins() ?? 0;
        Debug.Log($"  - Монеты: {data.coins}");

        // 3. Ресурсы
        SaveResources(data);

        // 4. Прогресс зданий
        SaveCurrentLevelProgress(data);

        // 5. Открываем следующий уровень
        if (levelManager != null && levelManager.IsLevelComplete)
        {
            int nextLevelIndex = levelManager.CurrentLevelIndex + 1;
            if (nextLevelIndex < levelManager.GetLevelsCount())
            {
                string nextLevelName = levelManager.GetLevelName(nextLevelIndex);
                if (!string.IsNullOrEmpty(nextLevelName) && !data.openedLevels.Contains(nextLevelName))
                {
                    data.openedLevels.Add(nextLevelName);
                    Debug.Log($"  - Открыт уровень: {nextLevelName}");
                }
            }
        }

        SaveSystem.Save(data);
        Debug.Log($"Сохранено: {data.resources.Count} ресурсов, {data.levelProgress.Count} уровней");
        Debug.Log("========== СОХРАНЕНИЕ ЗАВЕРШЕНО ==========");
    }

    private void SaveResources(SaveData data)
    {
        data.resources = new List<ResourceEntry>();

        string[] resourceNames = { "Дерево", "Камень", "Молоко", "Шерсть" };
        foreach (string name in resourceNames)
        {
            int amount = inventory.GetAmount(name);
            if (amount > 0)
            {
                data.resources.Add(new ResourceEntry
                {
                    resourceName = name,
                    amount = amount
                });
                Debug.Log($"  - {name}: {amount}");
            }
        }
    }

    private void SaveCurrentLevelProgress(SaveData data)
    {
        if (string.IsNullOrEmpty(currentLevelName)) return;

        var levelProgressData = data.levelProgress.FirstOrDefault(p => p.levelName == currentLevelName);
        if (levelProgressData == null)
        {
            levelProgressData = new LevelProgressData { levelName = currentLevelName };
            data.levelProgress.Add(levelProgressData);
        }

        levelProgressData.buildingProgress = new List<BuildingProgressEntry>();
        levelProgressData.restoredBuildings = new List<string>();

        foreach (var building in buildings)
        {
            if (building == null) continue;

            string buildingId = building.GetBuildingId();

            foreach (var cost in building.GetCosts())
            {
                int invested = building.GetInvestedAmount(cost.resourceName);
                if (invested > 0)
                {
                    levelProgressData.buildingProgress.Add(new BuildingProgressEntry
                    {
                        buildingId = buildingId,
                        resourceName = cost.resourceName,
                        investedAmount = invested
                    });
                }
            }

            if (building.IsRestored())
            {
                levelProgressData.restoredBuildings.Add(buildingId);
            }
        }

        bool allRestored = buildings.Count > 0 && buildings.All(b => b != null && b.IsRestored());
        levelProgressData.isCompleted = allRestored;

        Debug.Log($"  - Прогресс уровня '{currentLevelName}': {levelProgressData.restoredBuildings.Count}/{buildings.Count} зданий");
    }

    // ===== ЗАГРУЗКА =====

    public void LoadGame()
    {
        Debug.Log("========== ЗАГРУЗКА ==========");

        SaveData data = SaveSystem.Load();
        if (data == null)
        {
            Debug.Log("  - Нет сохранения");
            Debug.Log("========== ЗАГРУЗКА ЗАВЕРШЕНА ==========");
            return;
        }

        // 1. Загружаем монеты
        uiManager?.SetCoins(data.coins);
        Debug.Log($"  - Монеты: {data.coins}");

        // 2. Загружаем ресурсы
        LoadResources(data);

        // 3. Получаем актуальные здания
        RefreshBuildingsList();

        // 4. Загружаем прогресс зданий
        LoadCurrentLevelProgress(data);

        // 5. Обновляем UI
        uiManager?.ForceRefreshUI();
        levelProgress?.ForceUpdate();

        Debug.Log("========== ЗАГРУЗКА ЗАВЕРШЕНА ==========");
    }

    private void LoadResources(SaveData data)
    {
        // Сначала очищаем инвентарь
        ClearInventory();

        // Потом загружаем сохранённые ресурсы
        if (data.resources != null)
        {
            foreach (var entry in data.resources)
            {
                inventory.SetAmount(entry.resourceName, entry.amount);
                Debug.Log($"  - {entry.resourceName}: {entry.amount}");
            }
        }
    }

    private void LoadCurrentLevelProgress(SaveData data)
    {
        if (string.IsNullOrEmpty(currentLevelName)) return;

        var levelProgressData = data.levelProgress.FirstOrDefault(p => p.levelName == currentLevelName);
        if (levelProgressData == null)
        {
            Debug.Log($"  - Нет сохранённого прогресса для уровня '{currentLevelName}'");
            ResetAllBuildings();
            return;
        }

        var progressMap = new Dictionary<string, Dictionary<string, int>>();
        foreach (var entry in levelProgressData.buildingProgress)
        {
            if (!progressMap.ContainsKey(entry.buildingId))
                progressMap[entry.buildingId] = new Dictionary<string, int>();
            progressMap[entry.buildingId][entry.resourceName] = entry.investedAmount;
        }

        var restoredSet = new HashSet<string>(levelProgressData.restoredBuildings);

        foreach (var building in buildings)
        {
            if (building == null) continue;

            string id = building.GetBuildingId();
            bool isRestored = restoredSet.Contains(id);
            var progress = progressMap.ContainsKey(id) ? progressMap[id] : null;

            building.SyncStateFromSave(isRestored, progress);
            Debug.Log($"  - {id}: restored={isRestored}, progress={progress?.Count ?? 0} ресурсов");
        }

        if (levelProgressData.isCompleted)
        {
            Debug.Log($"  - Уровень '{currentLevelName}' уже был завершён!");
        }
    }

    private void ResetAllBuildings()
    {
        foreach (var building in buildings)
        {
            if (building != null)
            {
                building.SyncStateFromSave(false, null);
            }
        }
        Debug.Log("  - Все здания сброшены в разрушенное состояние");
    }

    // ===== ОЧИСТКА (ЕДИНЫЙ МЕТОД ДЛЯ ПЕРЕХОДА) =====

    /// <summary>
    /// Очищает состояние игрока для нового уровня (монеты + инвентарь)
    /// Вызывается при переходе между уровнями
    /// </summary>
    public void ClearPlayerState()
    {
        // Очищаем в игре
        ClearInventory();
        ClearCoins();

        // Очищаем в сохранении
        ClearCoinsInSave();
        ClearResourcesInSave();

        Debug.Log("Игровое состояние очищено для нового уровня");
    }

    // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ОЧИСТКИ =====

    private void ClearInventory()
    {
        string[] resourceNames = { "Дерево", "Камень", "Молоко", "Шерсть" };
        foreach (string name in resourceNames)
        {
            inventory.SetAmount(name, 0);
        }
        Debug.Log("  - Инвентарь очищен");
        uiManager?.ForceRefreshUI();
    }

    private void ClearCoins()
    {
        uiManager?.SetCoins(0);
        Debug.Log("  - Монеты обнулены");
    }

    private void ClearCoinsInSave()
    {
        SaveData data = SaveSystem.Load() ?? new SaveData();
        data.coins = 0;
        SaveSystem.Save(data);
        Debug.Log("  - Монеты очищены в сохранении");
    }

    private void ClearResourcesInSave()
    {
        SaveData data = SaveSystem.Load() ?? new SaveData();
        data.resources = new List<ResourceEntry>();
        SaveSystem.Save(data);
        Debug.Log("  - Ресурсы очищены в сохранении");
    }

    // ===== УПРАВЛЕНИЕ ЗДАНИЯМИ =====

    public void RefreshBuildingsList()
    {
        buildings.Clear();
        buildings.AddRange(FindObjectsOfType<BuildingController>());
        Debug.Log($"  - Найдено зданий: {buildings.Count}");
    }

    public void RegisterBuilding(BuildingController building)
    {
        if (building != null && !buildings.Contains(building))
        {
            buildings.Add(building);
        }
    }

    public void RefreshAllSystems()
    {
        RefreshBuildingsList();
        uiManager?.ForceRefreshUI();
        levelProgress?.ForceUpdate();
        Debug.Log("Все системы обновлены");
    }

    public void ResetProgress()
    {
        SaveSystem.DeleteSave();
        Debug.Log("Прогресс сброшен");

        if (levelManager != null)
        {
            levelManager.LoadLevel(0);
        }
    }
}