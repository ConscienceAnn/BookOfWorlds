using UnityEngine;

public interface IResourceBehaviour
{
    void OnCollect(ResourceSource resource);
    void OnCollect(Transform target);
    void OnRespawn(ResourceSource resource);
}