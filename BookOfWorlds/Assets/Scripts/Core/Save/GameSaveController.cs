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
    [Inject] private UpgradeManager upgradeManager;

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

        // ===== 6. СОХРАНЯЕМ УЛУЧШЕНИЯ =====
        SaveUpgrades(data);

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

    /// <summary>
    /// СОХРАНЯЕТ УЛУЧШЕНИЯ
    /// </summary>
    private void SaveUpgrades(SaveData data)
    {
        if (upgradeManager == null)
        {
            Debug.LogWarning("UpgradeManager не найден, улучшения не сохранены");
            return;
        }

        data.upgrades = new List<UpgradeSaveEntry>();
        var upgrades = upgradeManager.GetUpgradesForSave();

        foreach (var kvp in upgrades)
        {
            data.upgrades.Add(new UpgradeSaveEntry
            {
                upgradeId = kvp.Key,
                level = kvp.Value
            });
        }
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

        // ===== 5. ЗАГРУЖАЕМ УЛУЧШЕНИЯ =====
        LoadUpgrades(data);

        // 6. Обновляем UI
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

    /// <summary>
    /// ЗАГРУЖАЕТ УЛУЧШЕНИЯ
    /// </summary>
    private void LoadUpgrades(SaveData data)
    {
        if (upgradeManager == null)
        {
            Debug.LogWarning("UpgradeManager не найден, улучшения не загружены");
            return;
        }

        if (data.upgrades == null || data.upgrades.Count == 0)
        {
            return;
        }

        var upgrades = new Dictionary<string, int>();
        foreach (var entry in data.upgrades)
        {
            upgrades[entry.upgradeId] = entry.level;
        }

        upgradeManager.LoadUpgrades(upgrades);
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

    // ===== ОЧИСТКА =====

    public void ClearPlayerState()
    {
        ClearInventory();
        ClearCoins();
        ClearCoinsInSave();
        ClearResourcesInSave();
        ClearLevelProgressInSave();
    }

    private void ClearLevelProgressInSave()
    {
        SaveData data = SaveSystem.Load();
        if (data != null)
        {
            data.levelProgress.RemoveAll(p => p.levelName == currentLevelName);
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

    public void ClearLevelProgress()
    {
        ClearInventory();
        if (uiManager != null) uiManager.SetCoins(0);

        RefreshBuildingsList();

        foreach (var building in buildings)
        {
            building.ResetBuilding();
        }

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
            data.upgrades = new List<UpgradeSaveEntry>();
            SaveSystem.Save(data);
        }
    }
}