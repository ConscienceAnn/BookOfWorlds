using UnityEngine;
using Zenject;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    [Header("Upgrades")]
    [SerializeField] private UpgradeDataSO[] allUpgrades;

    [Header("References")]
    [Inject] private IPlayerInventory inventory;
    [Inject] private UIManager uiManager;

    private Dictionary<string, int> currentLevels = new Dictionary<string, int>();

    public event System.Action OnUpgradesChanged;
    public event System.Action<string, bool> OnUpgradeNotification;

    // ===== ПУБЛИЧНЫЕ МЕТОДЫ =====

    public int GetUpgradeLevel(string upgradeId)
    {
        return currentLevels.ContainsKey(upgradeId) ? currentLevels[upgradeId] : 0;
    }

    public int GetMaxLevel(string upgradeId)
    {
        var data = GetUpgradeData(upgradeId);
        return data != null ? data.levels.Length : 0;
    }

    public int GetNextLevelCost(string upgradeId)
    {
        int currentLevel = GetUpgradeLevel(upgradeId);
        var data = GetUpgradeData(upgradeId);

        if (data == null || currentLevel >= data.levels.Length) return -1;
        return data.levels[currentLevel].cost;
    }

    public float GetNextLevelValue(string upgradeId)
    {
        int currentLevel = GetUpgradeLevel(upgradeId);
        var data = GetUpgradeData(upgradeId);

        if (data == null || currentLevel >= data.levels.Length) return -1;
        return data.levels[currentLevel].value;
    }

    public string GetNextLevelDescription(string upgradeId)
    {
        int currentLevel = GetUpgradeLevel(upgradeId);
        var data = GetUpgradeData(upgradeId);

        if (data == null || currentLevel >= data.levels.Length) return "MAX";
        return data.levels[currentLevel].description;
    }

    public bool CanUpgrade(string upgradeId)
    {
        int currentLevel = GetUpgradeLevel(upgradeId);
        var data = GetUpgradeData(upgradeId);

        if (data == null || currentLevel >= data.levels.Length) return false;

        int cost = data.levels[currentLevel].cost;
        return uiManager.GetCoins() >= cost;
    }

    public void ApplyUpgrade(string upgradeId)
    {
        if (!CanUpgrade(upgradeId))
        {
            int currentLevel = GetUpgradeLevel(upgradeId);
            var data = GetUpgradeData(upgradeId);

            if (data != null && currentLevel < data.levels.Length)
            {
                int cost = data.levels[currentLevel].cost;
                OnUpgradeNotification?.Invoke($"Недостаточно монет! Нужно: {cost}", true);
            }
            else
            {
                OnUpgradeNotification?.Invoke("Улучшение уже максимальное!", true);
            }
            return;
        }

        var data2 = GetUpgradeData(upgradeId);
        int currentLevel2 = GetUpgradeLevel(upgradeId);
        int cost2 = data2.levels[currentLevel2].cost;

        uiManager.AddCoins(-cost2);

        int newLevel = currentLevel2 + 1;
        currentLevels[upgradeId] = newLevel;

        ApplyEffect(upgradeId, newLevel);

        OnUpgradesChanged?.Invoke();

        OnUpgradeNotification?.Invoke($"{data2.upgradeName} улучшено до уровня {newLevel}!", false);

        Debug.Log($"Улучшение {upgradeId} повышено до уровня {newLevel}");
    }

    public void LoadUpgrades(Dictionary<string, int> savedUpgrades)
    {
        if (savedUpgrades == null) return;

        currentLevels = new Dictionary<string, int>(savedUpgrades);

        foreach (var kvp in currentLevels)
        {
            ApplyEffect(kvp.Key, kvp.Value);
        }

        OnUpgradesChanged?.Invoke();
        Debug.Log($"Загружено {currentLevels.Count} улучшений");
    }

    public Dictionary<string, int> GetUpgradesForSave()
    {
        return new Dictionary<string, int>(currentLevels);
    }

    // ===== ПРИВАТНЫЕ МЕТОДЫ =====

    private void ApplyEffect(string upgradeId, int level)
    {
        var data = GetUpgradeData(upgradeId);
        if (data == null || level <= 0 || level > data.levels.Length) return;

        float value = data.levels[level - 1].value;

        switch (upgradeId)
        {
            case "respawn_speed":
                ApplyRespawnSpeed(value);
                break;
            case "inventory_capacity":
                ApplyInventoryCapacity(value);
                break;
            case "sell_bonus":
                ApplySellBonus(value);
                break;
        }
    }

    // ===== ПРИМЕНЕНИЕ ЭФФЕКТОВ =====

    private void ApplyRespawnSpeed(float multiplier)
    {
        // Защита от нулевого значения
        multiplier = Mathf.Max(0.1f, multiplier);

        Debug.Log($"Скорость респавна: {multiplier}x");

        // Сохраняем множитель в глобальных настройках
        RespawnSettings.Multiplier = multiplier;
    }

    private void ApplyInventoryCapacity(float multiplier)
    {
        Debug.Log($"Вместимость инвентаря: {multiplier}x");

        if (inventory is PlayerInventory playerInventory)
        {
            playerInventory.ForceRefreshCapacities();
        }
    }

    private void ApplySellBonus(float bonusPercent)
    {
        Debug.Log($"Бонус к продаже: +{bonusPercent * 100}%");
    }

    private UpgradeDataSO GetUpgradeData(string upgradeId)
    {
        foreach (var data in allUpgrades)
        {
            if (data.upgradeId == upgradeId) return data;
        }
        return null;
    }

    // ===== МЕТОДЫ ДЛЯ ДРУГИХ СИСТЕМ =====

    public float GetRespawnMultiplier()
    {
        // Возвращаем из глобальных настроек
        float multiplier = RespawnSettings.Multiplier;
        return Mathf.Max(0.1f, multiplier);
    }

    public float GetInventoryMultiplier()
    {
        int level = GetUpgradeLevel("inventory_capacity");
        if (level == 0) return 1f;

        var data = GetUpgradeData("inventory_capacity");
        if (data == null || level > data.levels.Length) return 1f;

        return data.levels[level - 1].value;
    }

    public float GetSellBonus()
    {
        int level = GetUpgradeLevel("sell_bonus");
        if (level == 0) return 0f;

        var data = GetUpgradeData("sell_bonus");
        if (data == null || level > data.levels.Length) return 0f;

        return data.levels[level - 1].value;
    }

    public UpgradeDataSO[] GetAllUpgrades()
    {
        return allUpgrades;
    }
}