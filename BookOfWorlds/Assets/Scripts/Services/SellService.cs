
using UnityEngine;
public class SellService
{
    private readonly IPlayerInventory inventory;
    private readonly ResourceDataSO[] allResources;

    //  Конструктор должен принимать оба параметра
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
            Debug.Log(" Инвентарь пуст! Нечего продавать.");
            return 0;
        }

        int totalCoins = 0;

        foreach (var item in items)
        {
            int price = GetPrice(item.Key);
            totalCoins += price * item.Value;
            Debug.Log($" Продано {item.Key} x{item.Value} по {price} монет");
        }

        inventory.ClearAll();
        return totalCoins;
    }

    private int GetPrice(string resourceName)
    {
        foreach (var data in allResources)
        {
            // Сравниваем без учёта регистра
            if (string.Equals(data.resourceName, resourceName, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($" Найдена цена для {resourceName}: {data.basePrice}");
                return data.basePrice;
            }
        }
        Debug.LogWarning($" Цена не найдена для ресурса: {resourceName}");
        return 0;
    }
}