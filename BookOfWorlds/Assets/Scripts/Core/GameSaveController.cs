using UnityEngine;
using Zenject;
using System.Collections.Generic;

public class GameSaveController : MonoBehaviour
{
    [Inject] private IPlayerInventory inventory;
    [Inject] private UIManager uiManager;
    [Inject] private LevelProgress levelProgress;

    private List<BuildingController> buildings = new List<BuildingController>();

  
    private SaveData pendingSaveData;
    private bool isDataPending = false;

    private void Start()
    {
        FindBuildings();
        LoadGame();
    }

    private void FindBuildings()
    {
        buildings.Clear();
        buildings.AddRange(FindObjectsOfType<BuildingController>());
        Debug.Log($"Найдено зданий: {buildings.Count}");
    }

    public void SaveGame()
    {
        Debug.Log("========== СОХРАНЕНИЕ ==========");

        SaveData data = new SaveData();

        data.coins = GetCoins();
        Debug.Log($"Монеты: {data.coins}");

        data.resources = new List<ResourceEntry>();
        string[] resourceNames = { "Wood", "Stone", "Milk", "Wool" };
        foreach (string name in resourceNames)
        {
            data.resources.Add(new ResourceEntry
            {
                resourceName = name,
                amount = inventory.GetAmount(name)
            });
            Debug.Log($"Ресурс {name}: {inventory.GetAmount(name)}");
        }

        data.buildingProgress = new List<BuildingProgressEntry>();
        data.restoredBuildings = new List<string>();

        FindBuildings();

        foreach (var building in buildings)
        {
            if (building == null) continue;

            string buildingName = building.name;

            foreach (var cost in building.GetCosts())
            {
                int invested = building.GetInvestedAmount(cost.resourceName);
                data.buildingProgress.Add(new BuildingProgressEntry
                {
                    buildingName = buildingName,
                    resourceName = cost.resourceName,
                    investedAmount = invested
                });
                Debug.Log($"Прогресс {buildingName} {cost.resourceName}: {invested}/{cost.amount}");
            }

            if (building.IsRestored())
            {
                data.restoredBuildings.Add(buildingName);
                Debug.Log($"Восстановлено: {buildingName}");
            }
        }

        data.currentLevel = 0;
        data.openedLevels = new List<string> { "Level1" };

        SaveSystem.Save(data);
        Debug.Log($"Сохранено: {data.restoredBuildings.Count} зданий");
        Debug.Log("========== СОХРАНЕНИЕ ЗАВЕРШЕНО ==========");
    }

    public void LoadGame()
    {
        SaveData data = SaveSystem.Load();
        if (data == null)
        {
            Debug.Log("Нет сохранения");
            return;
        }

        Debug.Log("========== ЗАГРУЗКА ==========");

        SetCoins(data.coins);
        Debug.Log($"Монеты: {data.coins}");

        if (data.resources != null)
        {
            foreach (var entry in data.resources)
            {
                inventory.SetAmount(entry.resourceName, entry.amount);
                Debug.Log($"Ресурс {entry.resourceName} установлен: {entry.amount}");
            }
        }

        if (uiManager != null)
        {
            uiManager.ForceRefreshUI();
        }

        FindBuildings();

        pendingSaveData = data;
        isDataPending = true;

        bool applied = TryApplyBuildingData();

        if (!applied)
        {
            Debug.Log("Данные зданий будут применены позже");
            Invoke(nameof(DelayedApplyBuildingData), 0.3f);
        }
        else
        {
            isDataPending = false;
            pendingSaveData = null;
        }

        if (levelProgress != null)
        {
            levelProgress.ForceUpdate();
            Debug.Log("Прогресс-бар обновлён");
        }

        Debug.Log("========== ЗАГРУЗКА ЗАВЕРШЕНА ==========");
    }

    private void DelayedApplyBuildingData()
    {
        if (isDataPending && pendingSaveData != null)
        {
            Debug.Log("Повторная попытка применения данных к зданиям...");
            bool applied = TryApplyBuildingData();

            if (!applied)
            {
                Invoke(nameof(SecondDelayedApplyBuildingData), 0.5f);
            }
            else
            {
                isDataPending = false;
                pendingSaveData = null;
            }
        }
    }

    private void SecondDelayedApplyBuildingData()
    {
        if (isDataPending && pendingSaveData != null)
        {
            Debug.Log("Третья попытка применения данных к зданиям...");
            bool applied = TryApplyBuildingData();

            if (applied)
            {
                isDataPending = false;
                pendingSaveData = null;
            }
            else
            {
                Debug.LogWarning("Не удалось применить данные к зданиям после нескольких попыток!");
                isDataPending = false;
                pendingSaveData = null;
            }
        }
    }

    private bool TryApplyBuildingData()
    {
        if (pendingSaveData == null)
        {
            Debug.LogWarning("TryApplyBuildingData: pendingSaveData is NULL!");
            return false;
        }

        FindBuildings();

        if (buildings == null || buildings.Count == 0)
        {
            Debug.Log("TryApplyBuildingData: Здания ещё не найдены");
            return false;
        }

        Debug.Log($"TryApplyBuildingData: Применяем данные к {buildings.Count} зданиям...");
        Debug.Log($"TryApplyBuildingData: buildingProgress entries = {pendingSaveData.buildingProgress?.Count ?? 0}");

        var data = pendingSaveData;
        bool anyDataApplied = false;

        if (data.buildingProgress != null)
        {
            Debug.Log("TryApplyBuildingData: Все данные из сохранения:");
            foreach (var entry in data.buildingProgress)
            {
                Debug.Log($"  - {entry.buildingName} {entry.resourceName} = {entry.investedAmount}");
            }
        }

        var buildingProgressMap = new Dictionary<string, Dictionary<string, int>>();

        if (data.buildingProgress != null)
        {
            foreach (var entry in data.buildingProgress)
            {
                if (!buildingProgressMap.ContainsKey(entry.buildingName))
                {
                    buildingProgressMap[entry.buildingName] = new Dictionary<string, int>();
                }
                buildingProgressMap[entry.buildingName][entry.resourceName] = entry.investedAmount;
                anyDataApplied = true;
            }
        }

        
        string keysString = "";
        foreach (var key in buildingProgressMap.Keys)
        {
            keysString += key + ", ";
        }
        Debug.Log($"TryApplyBuildingData: buildingProgressMap keys: {keysString}");

        foreach (var building in buildings)
        {
            if (building == null) continue;

            string buildingName = building.name;
            Debug.Log($"TryApplyBuildingData: Обработка здания '{buildingName}'");

            bool isRestored = data.restoredBuildings != null &&
                             data.restoredBuildings.Contains(buildingName);

            var progress = buildingProgressMap.ContainsKey(buildingName) ?
                          buildingProgressMap[buildingName] : null;

            if (progress == null)
            {
                Debug.Log($"TryApplyBuildingData: Нет прогресса для '{buildingName}'");
            }
            else
            {
                Debug.Log($"TryApplyBuildingData: Прогресс для '{buildingName}': {DictToString(progress)}");
            }

            Debug.Log($"TryApplyBuildingData: BEFORE Sync - building.IsRestored()={building.IsRestored()}");

            building.SyncStateFromSave(isRestored, progress);

            Debug.Log($"TryApplyBuildingData: AFTER Sync - building.IsRestored()={building.IsRestored()}");

            if (isRestored)
            {
                Debug.Log($"TryApplyBuildingData: Восстановлено: {buildingName}");
            }
        }

        if (anyDataApplied)
        {
            if (levelProgress != null)
            {
                levelProgress.ForceUpdate();
            }

            isDataPending = false;
            pendingSaveData = null;
            Debug.Log("TryApplyBuildingData: Данные зданий применены успешно");
            return true;
        }

        Debug.LogWarning("TryApplyBuildingData: Нет данных для применения к зданиям!");
        return anyDataApplied;
    }

    
    private string DictToString(Dictionary<string, int> dict)
    {
        if (dict == null || dict.Count == 0) return "EMPTY";

        string result = "";
        foreach (var kvp in dict)
        {
            result += $"{kvp.Key}={kvp.Value}, ";
        }
      
        if (result.Length > 2)
            result = result.Substring(0, result.Length - 2);
        return result;
    }

    public void OnBuildingReady()
    {
        if (isDataPending && pendingSaveData != null)
        {
            Debug.Log("Здание готово, пытаемся применить данные...");
            bool applied = TryApplyBuildingData();

            if (applied)
            {
                isDataPending = false;
                pendingSaveData = null;
                CancelInvoke(nameof(DelayedApplyBuildingData));
                CancelInvoke(nameof(SecondDelayedApplyBuildingData));
            }
        }
    }

    private int GetCoins()
    {
        return uiManager != null ? uiManager.GetCoins() : 0;
    }

    private void SetCoins(int coins)
    {
        if (uiManager != null)
        {
            uiManager.SetCoins(coins);
        }
    }

    public void RegisterBuilding(BuildingController building)
    {
        if (!buildings.Contains(building))
        {
            buildings.Add(building);
        }
    }

    public void RefreshAllSystems()
    {
        if (uiManager != null)
            uiManager.ForceRefreshUI();

        if (levelProgress != null)
            levelProgress.ForceUpdate();

        FindBuildings();
        foreach (var building in buildings)
        {
            if (building != null && !building.IsRestored())
            {
                building.UpdateVisualFromProgress();
            }
        }
    }
}