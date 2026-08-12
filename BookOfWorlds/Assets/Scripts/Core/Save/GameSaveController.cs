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
        SaveData data = SaveSystem.Load() ?? new SaveData();

        // 1. Текущий уровень
        if (levelManager != null)
        {
            data.currentLevel = levelManager.CurrentLevelIndex;
        }

        // 2. Монеты
        data.coins = uiManager?.GetCoins() ?? 0;

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
                }
            }
        }

        SaveSystem.Save(data);
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
    }

    // ===== ЗАГРУЗКА =====

    public void LoadGame()
    {
        SaveData data = SaveSystem.Load();
        if (data == null)
        {
            return;
        }

        // 1. Загружаем монеты
        uiManager?.SetCoins(data.coins);

        // 2. Загружаем ресурсы
        LoadResources(data);

        // 3. Получаем актуальные здания
        RefreshBuildingsList();

        // 4. Загружаем прогресс зданий
        LoadCurrentLevelProgress(data);

        // 5. Обновляем UI
        uiManager?.ForceRefreshUI();
        levelProgress?.ForceUpdate();
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
            }
        }
    }

    private void LoadCurrentLevelProgress(SaveData data)
    {
        if (string.IsNullOrEmpty(currentLevelName)) return;

        var levelProgressData = data.levelProgress.FirstOrDefault(p => p.levelName == currentLevelName);
        if (levelProgressData == null)
        {
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

        ClearLevelProgressInSave();
    }

    // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ОЧИСТКИ =====

    /// <summary>
    /// ОЧИЩАЕТ ПРОГРЕСС ТЕКУЩЕГО УРОВНЯ В СОХРАНЕНИИ
    /// </summary>
    private void ClearLevelProgressInSave()
    {
        SaveData data = SaveSystem.Load();
        if (data != null)
        {
            // Удаляем прогресс текущего уровня
            data.levelProgress.RemoveAll(p => p.levelName == currentLevelName);
            // Также удаляем из openedLevels, чтобы уровень не считался открытым
            data.openedLevels.Remove(currentLevelName);
            SaveSystem.Save(data);
        }
    }

    private void ClearInventory()
    {
        string[] resourceNames = { "Дерево", "Камень", "Молоко", "Шерсть" };
        foreach (string name in resourceNames)
        {
            inventory.SetAmount(name, 0);
        }
        uiManager?.ForceRefreshUI();
    }

    private void ClearCoins()
    {
        uiManager?.SetCoins(0);
    }

    private void ClearCoinsInSave()
    {
        SaveData data = SaveSystem.Load() ?? new SaveData();
        data.coins = 0;
        SaveSystem.Save(data);
    }

    private void ClearResourcesInSave()
    {
        SaveData data = SaveSystem.Load() ?? new SaveData();
        data.resources = new List<ResourceEntry>();
        SaveSystem.Save(data);
    }

    // ===== УПРАВЛЕНИЕ ЗДАНИЯМИ =====

    public void RefreshBuildingsList()
    {
        buildings.Clear();
        buildings.AddRange(FindObjectsOfType<BuildingController>());
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
    }

    public void ResetProgress()
    {
        SaveSystem.DeleteSave();

        if (levelManager != null)
        {
            levelManager.LoadLevel(0);
        }
    }

    /// <summary>
    /// СБРАСЫВАЕТ ПРОГРЕСС ТОЛЬКО ДЛЯ ТЕКУЩЕГО УРОВНЯ (при перезапуске)
    /// </summary>
    public void ClearLevelProgress()
    {
        ClearInventory();
        if (uiManager != null) uiManager.SetCoins(0);

        RefreshBuildingsList();

        foreach (var building in buildings)
        {
            building.ResetBuilding();
        }

        LevelProgress levelProgress = FindObjectOfType<LevelProgress>();
        if (levelProgress != null)
        {
            levelProgress.ResetState();
        }

        uiManager?.ForceRefreshUI();
        levelProgress?.ForceUpdate();

        SaveData data = SaveSystem.Load();
        if (data != null)
        {
            data.levelProgress.RemoveAll(p => p.levelName == currentLevelName);
            data.coins = 0;
            data.resources = new List<ResourceEntry>();
            SaveSystem.Save(data);
        }
    }
}