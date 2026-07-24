using UnityEngine;
using Zenject;

public class ResourceFactory : MonoBehaviour
{
    [Header("Pools")]
    [SerializeField] private ResourcePool woodPool;
    [SerializeField] private ResourcePool stonePool;

    [Inject] private DiContainer container; 

    public GameObject CreateWood(Vector3 position, Quaternion rotation)
    {
        return woodPool.Get(position, rotation);
    }

    public GameObject CreateStone(Vector3 position, Quaternion rotation)
    {
        return stonePool.Get(position, rotation);
    }

    public void ReturnWood(GameObject obj)
    {
        woodPool.Return(obj);
    }

    public void ReturnStone(GameObject obj)
    {
        stonePool.Return(obj);
    }
}