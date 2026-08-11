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

        Debug.Log($"[TreeBehaviour] ====== COLLECTION START ======");

        resource.SetGray();
        Debug.Log($"[TreeBehaviour] STEP 1: resource.SetGray() CALLED");

        await UniTask.Delay(50);
        Debug.Log($"[TreeBehaviour] STEP 2: Gray color applied");

        Debug.Log($"[TreeBehaviour] STEP 3: Shaking tree...");
        await ShakeTree(resource.transform, shakeDuration, shakeMagnitude);
        Debug.Log($"[TreeBehaviour] STEP 3: Shake completed");

        Debug.Log($"[TreeBehaviour] STEP 4: Creating particles...");
        Vector3 position = resource.transform.position + particleOffset;
        if (particleFactory != null)
        {
            particleFactory.CreateWoodParticles(position);
        }
        Debug.Log($"[TreeBehaviour] STEP 4: Particles created");

        if (flyAnimation != null)
        {
            Debug.Log($"[TreeBehaviour] STEP 5: Starting fly animation...");
            await flyAnimation.Play(position, resource.ResourceName);
            Debug.Log($"[TreeBehaviour] STEP 5: Fly animation completed");
        }

        Debug.Log($"[TreeBehaviour] STEP 7: Hiding resource");
        resource.Hide();
        Debug.Log($"[TreeBehaviour] STEP 7: Resource hidden");

        resource.InvokeCollected();
        Debug.Log($"[TreeBehaviour] STEP 8: InvokeCollected called");

        Debug.Log($"[TreeBehaviour] ====== COLLECTION END ======");
    }

    public void OnRespawn(ResourceSource resource)
    {
        if (resource != null)
        {
            resource.SetColored();
            resource.Show();
        }
        Debug.Log($"[TreeBehaviour] Resource respawned");
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