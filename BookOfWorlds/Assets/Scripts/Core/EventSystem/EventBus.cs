using System;

public static class EventBus
{
    public static event Action<string, int> OnResourceCollected;
    public static event Action<int> OnCoinsChanged;
    public static event Action<BuildingController> OnBuildingRestored;
    public static event Action<BuildingController> OnBuildingProgressChanged;

    public static void ResourceCollected(string resourceName, int amount)
    {
        OnResourceCollected?.Invoke(resourceName, amount);
    }

    public static void CoinsChanged(int amount)
    {
        OnCoinsChanged?.Invoke(amount);
    }

    public static void BuildingRestored(BuildingController building)
    {
        OnBuildingRestored?.Invoke(building);
    }

    public static void BuildingProgressChanged(BuildingController building)
    {
        OnBuildingProgressChanged?.Invoke(building);
    }
}