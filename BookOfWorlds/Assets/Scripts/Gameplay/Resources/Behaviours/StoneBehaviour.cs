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

        Debug.Log($"[StoneBehaviour] ====== COLLECTION START ======");

        resource.SetGray();
        Debug.Log($"[StoneBehaviour] STEP 1: resource.SetGray() CALLED");

        await UniTask.Delay(50);
        Debug.Log($"[StoneBehaviour] STEP 2: Gray color applied");

        Debug.Log($"[StoneBehaviour] STEP 3: Shaking stone...");
        await ShakeStone(resource.transform, shakeDuration, shakeMagnitude);
        Debug.Log($"[StoneBehaviour] STEP 3: Shake completed");

        Debug.Log($"[StoneBehaviour] STEP 4: Creating particles...");
        Vector3 position = resource.transform.position + particleOffset;
        if (particleFactory != null)
        {
            particleFactory.CreateStoneParticles(position);
        }
        Debug.Log($"[StoneBehaviour] STEP 4: Particles created");

        if (flyAnimation != null)
        {
            Debug.Log($"[StoneBehaviour] STEP 5: Starting fly animation...");
            await flyAnimation.Play(position, resource.ResourceName);
            Debug.Log($"[StoneBehaviour] STEP 5: Fly animation completed");
        }

        Debug.Log($"[StoneBehaviour] STEP 6: Hiding resource");
        resource.Hide();
        Debug.Log($"[StoneBehaviour] STEP 6: Resource hidden");

        resource.InvokeCollected();
        Debug.Log($"[StoneBehaviour] STEP 7: InvokeCollected called");

        Debug.Log($"[StoneBehaviour] ====== COLLECTION END ======");
    }

    public void OnRespawn(ResourceSource resource)
    {
        if (resource != null)
        {
            resource.SetColored();
            resource.Show();
        }
        Debug.Log($"[StoneBehaviour] Resource respawned");
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