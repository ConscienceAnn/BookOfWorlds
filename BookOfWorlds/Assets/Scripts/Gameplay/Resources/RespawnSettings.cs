using UnityEngine;

public static class RespawnSettings
{
    private static float _multiplier = 1f;

    public static float Multiplier
    {
        get => _multiplier;
        set
        {
            // Защита от нулевых и отрицательных значений
            float newValue = Mathf.Max(0.1f, value);
            if (Mathf.Approximately(_multiplier, newValue)) return;
            _multiplier = newValue;
            Debug.Log($"[RespawnSettings] Множитель обновлён: {_multiplier}x");
            EventBus.RespawnMultiplierChanged();
        }
    }
}