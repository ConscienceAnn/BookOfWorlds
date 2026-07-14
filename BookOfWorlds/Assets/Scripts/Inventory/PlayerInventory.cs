using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceSlot
{
    public string resourceName;
    public int maxCapacity;
    public int currentAmount;
}

public class PlayerInventory : MonoBehaviour, IPlayerInventory
{
    [Header("Настройки инвентаря")]
    [SerializeField] private ResourceSlot[] resourceSlots;

    public event Action OnInventoryChanged;

    public int GetAmount(string resourceName)
    {
        var slot = GetSlot(resourceName);
        return slot?.currentAmount ?? 0;
    }

    public int GetMax(string resourceName)
    {
        var slot = GetSlot(resourceName);
        return slot?.maxCapacity ?? 0;
    }

    public bool CanAdd(string resourceName, int amount = 1)
    {
        var slot = GetSlot(resourceName);
        if (slot == null) return false;
        return slot.currentAmount + amount <= slot.maxCapacity;
    }

    public bool TryAdd(string resourceName, int amount = 1)
    {
        var slot = GetSlot(resourceName);
        if (slot == null) return false;

        if (slot.currentAmount + amount > slot.maxCapacity)
        {
            Debug.Log($" Инвентарь для {resourceName} полон! ({slot.currentAmount}/{slot.maxCapacity})");
            return false;
        }

        slot.currentAmount += amount;
        OnInventoryChanged?.Invoke();
        Debug.Log($" Добавлен {resourceName} (+{amount}) - {slot.currentAmount}/{slot.maxCapacity}");
        return true;
    }

    public bool TrySpend(string resourceName, int amount)
    {
        var slot = GetSlot(resourceName);
        if (slot == null) return false;

        if (slot.currentAmount < amount)
        {
            Debug.Log($" Недостаточно {resourceName}! Есть {slot.currentAmount}, нужно {amount}");
            return false;
        }

        slot.currentAmount -= amount;
        OnInventoryChanged?.Invoke();
        Debug.Log($" Потрачен {resourceName} (-{amount}) - {slot.currentAmount}/{slot.maxCapacity}");
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
        return null;
    }
}