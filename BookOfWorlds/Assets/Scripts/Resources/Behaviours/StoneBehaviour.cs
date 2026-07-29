using UnityEngine;
using Cysharp.Threading.Tasks;

public class StoneBehaviour : IResourceBehaviour
{
    private ParticleFactory particleFactory;
    private Vector3 particleOffset = new Vector3(0, 0.5f, 0);
    private float hideDelay = 0.8f;

    public StoneBehaviour(ParticleFactory particleFactory)
    {
        this.particleFactory = particleFactory;
    }

    public async void OnCollect(ResourceSource resource)
    {
        if (resource == null) return;

        Debug.Log($" StoneBehaviour.OnCollect() START для {resource.name}");

        Vector3 position = resource.transform.position + particleOffset;

        // 1. ПАРТИКЛЫ — появляются СРАЗУ
        if (particleFactory != null)
        {
            particleFactory.CreateStoneParticles(position);
            Debug.Log($" Партиклы созданы в {position}");
        }

        // 2. ЖДЁМ, ПОКА ИГРОК ВИДИТ АНИМАЦИЮ
        Debug.Log($" Ждём {hideDelay} сек перед скрытием...");
        await UniTask.Delay((int)(hideDelay * 1000));

        // 3. РЕСУРС СКРЫВАЕТСЯ
        resource.Hide();
        Debug.Log($" Ресурс {resource.name} скрыт");

        Debug.Log($" StoneBehaviour.OnCollect() END");
    }

    public void OnRespawn(ResourceSource resource)
    {
        resource?.Show();
        Debug.Log($" Ресурс {resource?.name} показан");
    }

    public void OnCollect(Transform target) { }
}