using UnityEngine;
using Cysharp.Threading.Tasks;

public class StoneBehaviour : ResourceBehaviourBase
{
    public StoneBehaviour(ParticleFactory particleFactory, ResourceFlyAnimation flyAnimation)
        : base(particleFactory, flyAnimation)
    {
        particleOffset = new Vector3(0, 0.8f, 0);
        shakeDuration = 0.2f;
        shakeMagnitude = 0.06f;
    }

    protected override async UniTask Shake(Transform transform)
    {
        if (transform == null) return;

        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            if (transform == null) return;

            float x = Random.Range(-shakeMagnitude, shakeMagnitude);
            float z = Random.Range(-shakeMagnitude, shakeMagnitude);
            float rotY = Random.Range(-shakeMagnitude * 5f, shakeMagnitude * 5f);

            transform.position = new Vector3(
                originalPosition.x + x,
                originalPosition.y,
                originalPosition.z + z
            );

            transform.rotation = originalRotation * Quaternion.Euler(0, rotY, 0);

            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        if (transform != null)
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }
    }

    protected override void CreateParticles(Vector3 position)
    {
        if (particleFactory != null)
        {
            particleFactory.CreateStoneParticles(position);
        }
    }
}