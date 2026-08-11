using UnityEngine;

public class BuildingService
{
    private readonly IPlayerInventory inventory;

    public BuildingService(IPlayerInventory inventory)
    {
        this.inventory = inventory;
        Debug.Log($" BuildingService создан! inventory = {(inventory != null ? "ЕСТЬ" : "НЕТ")}");
    }

    public bool CanRestore(BuildingDataSO building)
    {
        if (building == null || building.costs == null)
        {
            Debug.LogWarning($" building или costs = null");
            return false;
        }

        foreach (var cost in building.costs)
        {
            int current = inventory.GetAmount(cost.resourceName);
            Debug.Log($" Проверка {cost.resourceName}: {current}/{cost.amount}");
            if (current < cost.amount)
                return false;
        }
        return true;
    }

    public bool Restore(BuildingDataSO building)
    {
        Debug.Log($" Restore() вызван для {building.buildingName}");

        if (!CanRestore(building))
        {
            Debug.Log($" Недостаточно ресурсов");
            return false;
        }

        bool allSuccess = true;
        foreach (var cost in building.costs)
        {
            int before = inventory.GetAmount(cost.resourceName);
            Debug.Log($"   {cost.resourceName}: ДО = {before}");

            bool success = inventory.TrySpend(cost.resourceName, cost.amount);
            Debug.Log($"   {cost.resourceName}: ПОСЛЕ = {inventory.GetAmount(cost.resourceName)}");
            Debug.Log($"   {cost.resourceName}: {(success ? " Успешно" : " Ошибка")}");

            if (!success) allSuccess = false;
        }

        return allSuccess;
    }
}