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

    public void SaveGame(bool saveResources = true)
    {
        Debug.Log(" ========== СОХРАНЕНИЕ ==========");

        SaveData data = new SaveData();

        //  Гарантируем, что списки не null
        if (data.resources == null) data.resources = new List<ResourceEntry>();
        if (data.buildingProgress == null) data.buildingProgress = new List<BuildingProgressEntry>();
        if (data.restoredBuildings == null) data.restoredBuildings = new List<string>();
        if (data.openedLevels == null) data.openedLevels = new List<string>();

        // 1. Монеты
        data.coins = uiManager?.GetCoins() ?? 0;
        Debug.Log($"  - Монеты: {data.coins}");

        // 2. Ресурсы
        if (saveResources)
        {
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

        // 3. Прогресс зданий
        foreach (var building in buildings)
        {
            if (building == null) continue;

            string buildingId = building.GetBuildingId();

            foreach (var cost in building.GetCosts())
            {
                int invested = building.GetInvestedAmount(cost.resourceName);
                if (invested > 0)
                {
                    data.buildingProgress.Add(new BuildingProgressEntry
                    {
                        buildingId = buildingId,
                        resourceName = cost.resourceName,
                        investedAmount = invested
                    });
                    Debug.Log($"  - {buildingId} {cost.resourceName}: {invested}/{cost.amount}");
                }
            }

            if (building.IsRestored())
            {
                data.restoredBuildings.Add(buildingId);
                Debug.Log($"  - {buildingId} восстановлено");
            }
        }

        // 4. Текущий уровень
        if (levelManager != null)
        {
            data.currentLevel = levelManager.CurrentLevelIndex;
            Debug.Log($"  - Уровень: {data.currentLevel}");
        }
        else
        {
            data.currentLevel = 0;
            Debug.LogWarning("  - LevelManager = NULL, уровень установлен в 0");
        }

        // 5. Открытые уровни
        if (data.openedLevels == null || data.openedLevels.Count == 0)
        {
            data.openedLevels = new List<string> { "Level1" };
            Debug.Log("  - Создан список открытых уровней");
        }

        //  Сохраняем
        SaveSystem.Save(data);
        Debug.Log($" Сохранено: {data.restoredBuildings.Count} зданий, {data.resources.Count} ресурсов");
        Debug.Log(" ========== СОХРАНЕНИЕ ЗАВЕРШЕНО ==========");
    }

    public void LoadGame()
    {
        Debug.Log(" ========== ЗАГРУЗКА ==========");

        SaveData data = SaveSystem.Load();
        if (data == null)
        {
            Debug.Log("  - Нет сохранения");
            Debug.Log(" ========== ЗАГРУЗКА ЗАВЕРШЕНА ==========");
            return;
        }

        uiManager?.SetCoins(data.coins);
        Debug.Log($"  - Монеты: {data.coins}");

        if (data.resources != null)
        {
            foreach (var entry in data.resources)
            {
                inventory.SetAmount(entry.resourceName, entry.amount);
                Debug.Log($"  - {entry.resourceName}: {entry.amount}");
            }
        }

        RefreshBuildingsList();
        ApplyBuildingData(data);

        uiManager?.ForceRefreshUI();
        levelProgress?.ForceUpdate();

        Debug.Log(" ========== ЗАГРУЗКА ЗАВЕРШЕНА ==========");
    }

    private void ApplyBuildingData(SaveData data)
    {
        if (buildings == null || buildings.Count == 0)
        {
            Debug.LogWarning("  - Нет зданий для применения данных");
            return;
        }

        var progressMap = new Dictionary<string, Dictionary<string, int>>();
        if (data.buildingProgress != null)
        {
            foreach (var entry in data.buildingProgress)
            {
                if (!progressMap.ContainsKey(entry.buildingId))
                    progressMap[entry.buildingId] = new Dictionary<string, int>();
                progressMap[entry.buildingId][entry.resourceName] = entry.investedAmount;
            }
        }

        var restoredSet = new HashSet<string>();
        if (data.restoredBuildings != null)
        {
            foreach (var id in data.restoredBuildings)
                restoredSet.Add(id);
        }

        foreach (var building in buildings)
        {
            if (building == null) continue;

            string id = building.GetBuildingId();
            bool isRestored = restoredSet.Contains(id);
            var progress = progressMap.ContainsKey(id) ? progressMap[id] : null;

            building.SyncStateFromSave(isRestored, progress);
            Debug.Log($"  - {id}: restored={isRestored}, progress={progress?.Count ?? 0} ресурсов");
        }
    }

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

    public void ResetProgress()
    {
        SaveSystem.DeleteSave();
        Debug.Log("Прогресс сброшен");

        if (levelManager != null)
        {
            levelManager.LoadLevel(0);
        }
        else
        {
            Debug.LogWarning("  - LevelManager = NULL, уровень не перезагружен");
        }
    }

    public void RefreshAllSystems()
    {
        RefreshBuildingsList();
        uiManager?.ForceRefreshUI();
        levelProgress?.ForceUpdate();
        Debug.Log("Все системы обновлены");
    }
}