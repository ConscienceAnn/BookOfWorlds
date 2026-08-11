using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Game/Resource Data")]
public class ResourceDataSO : ScriptableObject
{
    [Header("Основные параметры")]
    public string resourceName;        // "Wood", "Stone", "Milk", "Wool"
    public Sprite icon;                // Иконка для UI
    public int basePrice = 1;          // Цена продажи
    public float respawnTime = 5f;     // Время восстановления

    [Header("Визуал")]
    public GameObject prefab;          // Префаб ресурса
    public Color gizmoColor = Color.green;
}