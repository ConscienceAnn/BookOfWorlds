using System;
using System.Collections.Generic;

public interface IPlayerInventory
{
    int GetAmount(string resourceName);
    int GetMax(string resourceName);
    bool CanAdd(string resourceName, int amount = 1);
    bool TryAdd(string resourceName, int amount = 1);
    bool TrySpend(string resourceName, int amount);
    Dictionary<string, int> GetAllItems();
    void ClearAll();
    void SetAmount(string resourceName, int amount);

    event Action OnInventoryChanged;
}