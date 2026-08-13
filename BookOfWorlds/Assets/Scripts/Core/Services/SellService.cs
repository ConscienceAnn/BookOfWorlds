using UnityEngine;

public class SellService
{
    private readonly IPlayerInventory inventory;
    private readonly ResourceDataSO[] allResources;
    private readonly UpgradeManager upgradeManager;

    public SellService(IPlayerInventory inventory, ResourceDataSO[] allResources, UpgradeManager upgradeManager)
    {
        this.inventory = inventory;
        this.allResources = allResources;
        this.upgradeManager = upgradeManager;
    }

    public int SellAll()
    {
        var items = inventory.GetAllItems();
        if (items.Count == 0) return 0;

        float bonus = upgradeManager != null ? upgradeManager.GetSellBonus() : 0f;

        int totalCoins = 0;

        foreach (var item in items)
        {
            int price = GetPrice(item.Key);
            int amount = item.Value;
            int earned = Mathf.RoundToInt(price * amount * (1f + bonus));
            totalCoins += earned;
        }

        inventory.ClearAll();
        return totalCoins;
    }

    private int GetPrice(string resourceName)
    {
        foreach (var data in allResources)
        {
            if (string.Equals(data.resourceName, resourceName, System.StringComparison.OrdinalIgnoreCase))
            {
                return data.basePrice;
            }
        }
        Debug.LogWarning($"Цена не найдена для ресурса: {resourceName}");
        return 0;
    }
}