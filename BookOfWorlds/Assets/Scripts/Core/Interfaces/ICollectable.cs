using UnityEngine;

/// <summary>
/// Интерфейс для всех объектов, которые можно собирать
/// </summary>
public interface ICollectable
{
    /// <summary>
    /// Доступен ли объект для сбора
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Название ресурса (Wood, Stone, Milk и т.д.)
    /// </summary>
    string GetResourceName();

    /// <summary>
    /// Количество ресурса за один сбор
    /// </summary>
    int GetAmount();

    /// <summary>
    /// Позиция объекта в мире (для поворота игрока)
    /// </summary>
    Transform GetTransform();

    /// <summary>
    /// Попытка собрать ресурс (возвращает true, если успешно)
    /// </summary>
    bool TryCollect();
}