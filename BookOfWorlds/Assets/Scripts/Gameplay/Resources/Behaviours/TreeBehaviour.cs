using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;

public class TreeBehaviour : IResourceBehaviour
{
    private ParticleFactory particleFactory;
    private ResourceFlyAnimation flyAnimation;
    private Vector3 particleOffset = new Vector3(0, 3f, 0);
    private float shakeDuration = 0.3f;
    private float shakeMagnitude = 0.15f;

    public TreeBehaviour(ParticleFactory particleFactory, ResourceFlyAnimation flyAnimation)
    {
        this.particleFactory = particleFactory;
        this.flyAnimation = flyAnimation;
    }

    public async void OnCollect(ResourceSource resource)
    {
        if (resource == null) return;

        resource.SetGray();

        await UniTask.Delay(50);

        await ShakeTree(resource.transform, shakeDuration, shakeMagnitude);

        Vector3 position = resource.transform.position + particleOffset;
        if (particleFactory != null)
        {
            particleFactory.CreateWoodParticles(position);
        }

        if (flyAnimation != null)
        {
            await flyAnimation.Play(position, resource.ResourceName);
        }

        resource.Hide();

        resource.InvokeCollected();
    }

    public void OnRespawn(ResourceSource resource)
    {
        if (resource != null)
        {
            resource.SetColored();
            resource.Show();
        }
    }

    public void OnCollect(Transform target) { }

    private async UniTask ShakeTree(Transform transform, float duration, float magnitude)
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-magnitude, magnitude);
            float z = Random.Range(-magnitude, magnitude);

            transform.position = new Vector3(
                originalPosition.x + x,
                originalPosition.y,
                originalPosition.z + z
            );

            await UniTask.Yield();
        }

        transform.position = originalPosition;
    }
}