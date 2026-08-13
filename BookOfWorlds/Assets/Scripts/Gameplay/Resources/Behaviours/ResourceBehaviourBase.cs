using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// Базовый класс для поведения ресурсов.
/// Содержит общую логику сбора: затемнение, тряска, частицы, полёт в UI.
/// </summary>
public abstract class ResourceBehaviourBase : IResourceBehaviour
{
    protected ParticleFactory particleFactory;
    protected ResourceFlyAnimation flyAnimation;
    protected Vector3 particleOffset;
    protected float shakeDuration;
    protected float shakeMagnitude;

    public ResourceBehaviourBase(ParticleFactory particleFactory, ResourceFlyAnimation flyAnimation)
    {
        this.particleFactory = particleFactory;
        this.flyAnimation = flyAnimation;
    }

    public virtual async void OnCollect(ResourceSource resource)
    {
        if (resource == null || resource.gameObject == null) return;

        // 1. Затемняем ресурс
        resource.SetGray();

        // 2. Небольшая задержка
        await UniTask.Delay(50);

        if (resource == null || resource.gameObject == null) return;

        // 3. Тряска (специфичная для каждого ресурса)
        await Shake(resource.transform);

        if (resource == null || resource.gameObject == null) return;

        // 4. Создаём частицы (специфичные для каждого ресурса)
        Vector3 position = resource.transform.position + particleOffset;
        CreateParticles(position);

        // 5. Анимация полёта к UI
        if (flyAnimation != null)
        {
            await flyAnimation.Play(position, resource.ResourceName);
        }

        // 6. Прячем ресурс
        if (resource != null && resource.gameObject != null)
        {
            resource.Hide();
            resource.InvokeCollected();
        }
    }

    public virtual void OnRespawn(ResourceSource resource)
    {
        if (resource != null && resource.gameObject != null)
        {
            resource.SetColored();
            resource.Show();
        }
    }

    public virtual void OnCollect(Transform target)
    {
        // Не используется для ресурсов
    }

    /// <summary>
    /// Тряска объекта (реализуется в наследниках)
    /// </summary>
    protected abstract UniTask Shake(Transform transform);

    /// <summary>
    /// Создание частиц (реализуется в наследниках)
    /// </summary>
    protected abstract void CreateParticles(Vector3 position);
}