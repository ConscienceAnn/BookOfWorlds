using UnityEngine;

public class ResourceBehaviourFactory
{
    public IResourceBehaviour Create(ResourceType resourceType, ParticleFactory particleFactory, ResourceFlyAnimation flyAnimation)
    {
        switch (resourceType)
        {
            case ResourceType.Wood:
                return new TreeBehaviour(particleFactory, flyAnimation);

            case ResourceType.Stone:
                return new StoneBehaviour(particleFactory, flyAnimation);

            //case ResourceType.Crystal:
            //    return new CrystalBehaviour(particleFactory, flyAnimation);

            default:
                Debug.LogWarning($"[ResourceBehaviourFactory] Неизвестный тип: {resourceType}, возвращаем TreeBehaviour");
                return new TreeBehaviour(particleFactory, flyAnimation);
        }
    }
}