using UnityEngine;

public class SellService
{
    private readonly IPlayerInventory inventory;
    private readonly ResourceDataSO[] allResources;

    public SellService(IPlayerInventory inventory, ResourceDataSO[] allResources)
    {
        this.inventory = inventory;
        this.allResources = allResources;
    }

    public int SellAll()
    {
        var items = inventory.GetAllItems();
        if (items.Count == 0)
        {
            return 0;
        }

        int totalCoins = 0;

        foreach (var item in items)
        {
            int price = GetPrice(item.Key);
            totalCoins += price * item.Value;
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