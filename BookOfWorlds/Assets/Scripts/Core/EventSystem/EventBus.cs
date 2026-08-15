using System;

public static class EventBus
{

    public static event Action<string, int> OnResourceCollected;
    public static event Action<int> OnCoinsChanged;
    public static event Action<BuildingController> OnBuildingRestored;
    public static event Action<BuildingController> OnBuildingProgressChanged;
    public static event Action OnRespawnMultiplierChanged;


    public static event Action<string> OnError;
    public static event Action OnCoinsCollected;
    public static event Action<string> OnSoundRequest;
    public static event Action<bool> OnPauseStateChanged;


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

    public static void RespawnMultiplierChanged()
    {
        OnRespawnMultiplierChanged?.Invoke();
    }


    public static void Error(string message)
    {
        OnError?.Invoke(message);
    }

    public static void CoinsCollected()
    {
        OnCoinsCollected?.Invoke();
    }

    public static void PlaySound(string soundId)
    {
        OnSoundRequest?.Invoke(soundId);
    }

    public static void PauseStateChanged(bool isPaused)
    {
        OnPauseStateChanged?.Invoke(isPaused);
    }
}