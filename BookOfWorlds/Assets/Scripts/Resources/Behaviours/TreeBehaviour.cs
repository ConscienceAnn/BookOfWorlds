using UnityEngine;

public class TreeBehaviour : IResourceBehaviour
{
    private ParticleSystem particles;

    public TreeBehaviour(ParticleSystem particles)
    {
        this.particles = particles;
    }

    public void OnCollect(ResourceSource resource)
    {
        if (particles != null && resource != null)
        {
            particles.transform.position = resource.transform.position;
            particles.Play();
        }
    }

    public void OnCollect(Transform target)
    {
        // Для дерева не используется, но нужно для интерфейса
        if (particles != null && target != null)
        {
            particles.transform.position = target.position;
            particles.Play();
        }
    }

    public void OnRespawn(ResourceSource resource)
    {
        // Ничего не делаем
    }
}