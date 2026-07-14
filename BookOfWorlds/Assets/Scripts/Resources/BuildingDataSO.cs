using System;
using UnityEngine;

[Serializable]
public class ResourceCost
{
    public string resourceName;
    public int amount;
}

[CreateAssetMenu(fileName = "BuildingData", menuName = "Game/Building Data")]
public class BuildingDataSO : ScriptableObject
{
    [Header("Основные параметры")]
    public string buildingName;        // "Мост", "Мельница", "Дом"
    public string description;         // Описание для UI
    public Sprite icon;

    [Header("Стоимость восстановления")]
    public ResourceCost[] costs;       // Что нужно для восстановления

    [Header("Визуал")]
    public GameObject ruinedPrefab;    // Разрушенное здание (серое)
    public GameObject restoredPrefab;  // Восстановленное здание (цветное)
    public float restoreTime = 2f;     // Время анимации восстановления
}