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
        int amount = slot?.currentAmount ?? 0;
        Debug.Log($"[PlayerInventory] GetAmount({resourceName}) = {amount}");
        return amount;
    }

    public void SetAmount(string resourceName, int amount)
    {
        var slot = GetSlot(resourceName);
        if (slot != null)
        {
            int oldAmount = slot.currentAmount;
            slot.currentAmount = Mathf.Clamp(amount, 0, slot.maxCapacity);

            OnInventoryChanged?.Invoke();
            Debug.Log($"[PlayerInventory] SetAmount: {resourceName} = {slot.currentAmount}/{slot.maxCapacity} (было {oldAmount})");
        }
        else
        {
            Debug.LogError($"[PlayerInventory] SetAmount: слот {resourceName} НЕ НАЙДЕН!");
        }
    }

    public int GetMax(string resourceName)
    {
        var slot = GetSlot(resourceName);
        int max = slot?.maxCapacity ?? 0;
        Debug.Log($"[PlayerInventory] GetMax({resourceName}) = {max}");
        return max;
    }

    public bool CanAdd(string resourceName, int amount = 1)
    {
        var slot = GetSlot(resourceName);
        if (slot == null)
        {
            Debug.LogError($"[PlayerInventory] CanAdd: слот {resourceName} НЕ НАЙДЕН!");
            return false;
        }

        bool canAdd = slot.currentAmount + amount <= slot.maxCapacity;
        Debug.Log($"[PlayerInventory] CanAdd({resourceName}, {amount}): current={slot.currentAmount}, max={slot.maxCapacity}, result={canAdd}");
        return canAdd;
    }

    public bool TryAdd(string resourceName, int amount = 1)
    {
        var slot = GetSlot(resourceName);
        if (slot == null)
        {
            Debug.LogError($"[PlayerInventory] TryAdd: слот {resourceName} НЕ НАЙДЕН!");
            return false;
        }

        if (slot.currentAmount + amount > slot.maxCapacity)
        {
            Debug.Log($"[PlayerInventory] НЕЛЬЗЯ добавить {resourceName} ({amount}): полон! ({slot.currentAmount}/{slot.maxCapacity})");
            return false;
        }

        slot.currentAmount += amount;
        OnInventoryChanged?.Invoke();
        Debug.Log($"[PlayerInventory] ДОБАВЛЕН {resourceName} (+{amount}) - {slot.currentAmount}/{slot.maxCapacity}");
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

        if (slot.currentAmount < amount)
        {
            Debug.Log($"[PlayerInventory] НЕДОСТАТОЧНО {resourceName}! Есть {slot.currentAmount}, нужно {amount}");
            return false;
        }

        slot.currentAmount -= amount;
        OnInventoryChanged?.Invoke();
        Debug.Log($"[PlayerInventory] ПОТРАЧЕН {resourceName} (-{amount}) - {slot.currentAmount}/{slot.maxCapacity}");
        return true;
    }

    public Dictionary<string, int> GetAllItems()
    {
        var items = new Dictionary<string, int>();
        foreach (var slot in resourceSlots)
        {
            if (slot.currentAmount > 0)
                items[slot.resourceName] = slot.currentAmount;

            Debug.Log($"[PlayerInventory] GetAllItems: {slot.resourceName} = {slot.currentAmount}/{slot.maxCapacity}");
        }
        return items;
    }

    public void ClearAll()
    {
        foreach (var slot in resourceSlots)
            slot.currentAmount = 0;
        OnInventoryChanged?.Invoke();
        Debug.Log("[PlayerInventory] Все ресурсы очищены");
    }

    private ResourceSlot GetSlot(string resourceName)
    {
        foreach (var slot in resourceSlots)
        {
            if (slot.resourceName == resourceName)
                return slot;
        }
        Debug.Log($"[PlayerInventory] GetSlot: слот {resourceName} НЕ НАЙДЕН!");
        return null;
    }
}