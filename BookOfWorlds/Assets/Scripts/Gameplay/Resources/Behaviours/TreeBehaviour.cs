using UnityEngine;
using Cysharp.Threading.Tasks;

public class TreeBehaviour : ResourceBehaviourBase
{
    public TreeBehaviour(ParticleFactory particleFactory, ResourceFlyAnimation flyAnimation)
        : base(particleFactory, flyAnimation)
    {
        particleOffset = new Vector3(0, 3f, 0);
        shakeDuration = 0.3f;
        shakeMagnitude = 0.15f;
    }

    protected override async UniTask Shake(Transform transform)
    {
        if (transform == null) return;

        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            if (transform == null) return;

            float x = Random.Range(-shakeMagnitude, shakeMagnitude);
            float z = Random.Range(-shakeMagnitude, shakeMagnitude);

            transform.position = new Vector3(
                originalPosition.x + x,
                originalPosition.y,
                originalPosition.z + z
            );

            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        if (transform != null)
        {
            transform.position = originalPosition;
        }
    }

    protected override void CreateParticles(Vector3 position)
    {
        if (particleFactory != null)
        {
            particleFactory.CreateWoodParticles(position);
        }
    }
}