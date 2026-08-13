using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[Serializable]
public class ResourceSlot
{
    public string resourceName;
    public int baseMaxCapacity;
    public int currentAmount;
    [NonSerialized] public int currentMaxCapacity;
}

public class PlayerInventory : MonoBehaviour, IPlayerInventory
{
    [Header("Настройки инвентаря")]
    [SerializeField] private ResourceSlot[] resourceSlots;

    [Inject] private UpgradeManager upgradeManager;

    public event Action OnInventoryChanged;

    private void Start()
    {
        UpdateAllCapacities();
    }

    public void ForceRefreshCapacities()
    {
        UpdateAllCapacities();

        foreach (var slot in resourceSlots)
        {
            if (slot.currentAmount > slot.currentMaxCapacity)
            {
                slot.currentAmount = slot.currentMaxCapacity;
            }
        }

        OnInventoryChanged?.Invoke();
        Debug.Log("[PlayerInventory] Принудительное обновление ёмкостей выполнено");
    }

    private void UpdateAllCapacities()
    {
        float multiplier = upgradeManager != null ? upgradeManager.GetInventoryMultiplier() : 1f;

        foreach (var slot in resourceSlots)
        {
            slot.currentMaxCapacity = Mathf.RoundToInt(slot.baseMaxCapacity * multiplier);
        }

        Debug.Log($"[PlayerInventory] Ёмкость обновлена с множителем {multiplier}x");
    }

    private void UpdateSlotCapacity(string resourceName)
    {
        var slot = GetSlot(resourceName);
        if (slot == null) return;

        float multiplier = upgradeManager != null ? upgradeManager.GetInventoryMultiplier() : 1f;
        slot.currentMaxCapacity = Mathf.RoundToInt(slot.baseMaxCapacity * multiplier);
    }

    public int GetAmount(string resourceName)
    {
        var slot = GetSlot(resourceName);
        return slot?.currentAmount ?? 0;
    }

    public void SetAmount(string resourceName, int amount)
    {
        var slot = GetSlot(resourceName);
        if (slot != null)
        {
            UpdateSlotCapacity(resourceName);
            slot.currentAmount = Mathf.Clamp(amount, 0, slot.currentMaxCapacity);
            OnInventoryChanged?.Invoke();
        }
    }

    public int GetMax(string resourceName)
    {
        var slot = GetSlot(resourceName);
        if (slot == null) return 0;
        UpdateSlotCapacity(resourceName);
        return slot.currentMaxCapacity;
    }

    public bool CanAdd(string resourceName, int amount = 1)
    {
        var slot = GetSlot(resourceName);
        if (slot == null) return false;
        UpdateSlotCapacity(resourceName);
        return slot.currentAmount + amount <= slot.currentMaxCapacity;
    }

    public bool TryAdd(string resourceName, int amount = 1)
    {
        var slot = GetSlot(resourceName);
        if (slot == null)
        {
            Debug.LogError($"[PlayerInventory] TryAdd: слот {resourceName} НЕ НАЙДЕН!");
            return false;
        }

        UpdateSlotCapacity(resourceName);

        if (slot.currentAmount + amount > slot.currentMaxCapacity)
        {
            Debug.Log($"[PlayerInventory] НЕЛЬЗЯ добавить {resourceName} ({amount}): полон! ({slot.currentAmount}/{slot.currentMaxCapacity})");
            return false;
        }

        slot.currentAmount += amount;
        OnInventoryChanged?.Invoke();
        Debug.Log($"[PlayerInventory] ДОБАВЛЕН {resourceName} (+{amount}) - {slot.currentAmount}/{slot.currentMaxCapacity}");
        return true;
    }

    public bool TrySpend(string resourceName, int amount)
    {
        var slot = GetSlot(resourceName);
        if (slot == null)
        {
            Debug.LogError($"[PlayerInventory] TrySpend: слот {resourceName} НЕ НАЙДЕН!");
            return false;
        }

        UpdateSlotCapacity(resourceName);

        if (slot.currentAmount < amount)
        {
            Debug.Log($"[PlayerInventory] НЕДОСТАТОЧНО {resourceName}! Есть {slot.currentAmount}, нужно {amount}");
            return false;
        }

        slot.currentAmount -= amount;
        OnInventoryChanged?.Invoke();
        Debug.Log($"[PlayerInventory] ПОТРАЧЕН {resourceName} (-{amount}) - {slot.currentAmount}/{slot.currentMaxCapacity}");
        return true;
    }

    public Dictionary<string, int> GetAllItems()
    {
        var items = new Dictionary<string, int>();
        foreach (var slot in resourceSlots)
        {
            if (slot.currentAmount > 0)
                items[slot.resourceName] = slot.currentAmount;
        }
        return items;
    }

    public void ClearAll()
    {
        foreach (var slot in resourceSlots)
            slot.currentAmount = 0;
        OnInventoryChanged?.Invoke();
    }

    private ResourceSlot GetSlot(string resourceName)
    {
        foreach (var slot in resourceSlots)
        {
            if (slot.resourceName == resourceName)
                return slot;
        }
        Debug.LogError($"[PlayerInventory] GetSlot: слот {resourceName} НЕ НАЙДЕН!");
        return null;
    }
}