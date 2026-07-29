using UnityEngine;

public class ParticleFactory : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem woodParticles;
    [SerializeField] private ParticleSystem stoneParticles;

    public ParticleSystem CreateWoodParticles(Vector3 position)
    {
        if (woodParticles == null)
        {
            Debug.LogWarning("ParticleFactory: woodParticles is NULL!");
            return null;
        }

        Debug.Log($" Создаём WoodParticles в {position}");

        ParticleSystem instance = Instantiate(woodParticles, position, Quaternion.identity, null);

        //  Убираем проверку particleCount (она не нужна)
        instance.Play();

        Destroy(instance.gameObject, instance.main.duration + 0.5f);

        return instance;
    }

    public ParticleSystem CreateStoneParticles(Vector3 position)
    {
        if (stoneParticles == null)
        {
            Debug.LogWarning("ParticleFactory: stoneParticles is NULL!");
            return null;
        }

        Debug.Log($" Создаём StoneParticles в {position}");

        ParticleSystem instance = Instantiate(stoneParticles, position, Quaternion.identity, null);

        instance.Play();

        Destroy(instance.gameObject, instance.main.duration + 0.5f);

        return instance;
    }
}