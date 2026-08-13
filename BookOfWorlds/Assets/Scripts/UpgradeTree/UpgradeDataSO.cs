using UnityEngine;
using System;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Game/Upgrade Data")]
public class UpgradeDataSO : ScriptableObject
{
    public string upgradeId;          // "respawn_speed", "inventory_capacity", "sell_bonus"
    public string upgradeName;        // "Скорость респавна"
    public string description;        // "Ресурсы восстанавливаются быстрее"
    public Sprite icon;

    public UpgradeLevel[] levels;     // Уровни (до 4-5)
}

[Serializable]
public class UpgradeLevel
{
    public int level;
    public int cost;
    public float value;               // Множитель: 1.3, 1.7, 2.0 и т.д.
    public string description;        // "Респавн x1.3"
}