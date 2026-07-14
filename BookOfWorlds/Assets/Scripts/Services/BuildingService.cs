using UnityEngine;
public class BuildingService
{
    private readonly IPlayerInventory inventory;

    public BuildingService(IPlayerInventory inventory)
    {
        this.inventory = inventory;
    }

    public bool CanRestore(BuildingDataSO building)
    {
        if (building == null || building.costs == null) return false;

        foreach (var cost in building.costs)
        {
            if (inventory.GetAmount(cost.resourceName) < cost.amount)
                return false;
        }
        return true;
    }

    public bool Restore(BuildingDataSO building)
    {
        if (!CanRestore(building)) return false;

        foreach (var cost in building.costs)
        {
            inventory.TrySpend(cost.resourceName, cost.amount);
        }

        Debug.Log($"Здание {building.buildingName} восстановлено!");
        return true;
    }
}