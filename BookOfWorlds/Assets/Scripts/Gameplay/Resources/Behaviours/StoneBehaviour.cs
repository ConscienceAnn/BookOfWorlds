using UnityEngine;
using Cysharp.Threading.Tasks;

public class StoneBehaviour : IResourceBehaviour
{
    private ParticleFactory particleFactory;
    private ResourceFlyAnimation flyAnimation;
    private Vector3 particleOffset = new Vector3(0, 0.8f, 0);
    private float shakeDuration = 0.2f;
    private float shakeMagnitude = 0.06f;

    public StoneBehaviour(ParticleFactory particleFactory, ResourceFlyAnimation flyAnimation)
    {
        this.particleFactory = particleFactory;
        this.flyAnimation = flyAnimation;
    }

    public async void OnCollect(ResourceSource resource)
    {
        if (resource == null) return;

        resource.SetGray();

        await UniTask.Delay(50);

        await ShakeStone(resource.transform, shakeDuration, shakeMagnitude);

        Vector3 position = resource.transform.position + particleOffset;
        if (particleFactory != null)
        {
            particleFactory.CreateStoneParticles(position);
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

    private async UniTask ShakeStone(Transform transform, float duration, float magnitude)
    {
        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float x = Random.Range(-magnitude, magnitude);
            float z = Random.Range(-magnitude, magnitude);
            float rotY = Random.Range(-magnitude * 5f, magnitude * 5f);

            transform.position = new Vector3(
                originalPosition.x + x,
                originalPosition.y,
                originalPosition.z + z
            );

            transform.rotation = originalRotation * Quaternion.Euler(0, rotY, 0);

            await UniTask.Yield();
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}