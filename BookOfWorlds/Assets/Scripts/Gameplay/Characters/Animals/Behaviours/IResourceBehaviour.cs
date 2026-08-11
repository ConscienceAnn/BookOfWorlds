using UnityEngine;

public interface IResourceBehaviour
{
    /// <summary>
    /// Что делать при сборе ресурса
    /// </summary>
    void OnCollect(ResourceSource resource);

    /// <summary>
    /// Что делать при респавне ресурса
    /// </summary>
    void OnRespawn(ResourceSource resource);

    /// <summary>
    /// Что делать при сборе (перегрузка для Transform)
    /// </summary>
    void OnCollect(Transform target);
}