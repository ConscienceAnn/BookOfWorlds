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
        if (building == null || building.costs == null)
        {
            Debug.LogWarning($"building или costs = null");
            return false;
        }

        foreach (var cost in building.costs)
        {
            int current = inventory.GetAmount(cost.resourceName);
            if (current < cost.amount)
                return false;
        }
        return true;
    }

    public bool Restore(BuildingDataSO building)
    {
        if (!CanRestore(building))
        {
            return false;
        }

        bool allSuccess = true;
        foreach (var cost in building.costs)
        {
            bool success = inventory.TrySpend(cost.resourceName, cost.amount);
            if (!success) allSuccess = false;
        }

        return allSuccess;
    }
}