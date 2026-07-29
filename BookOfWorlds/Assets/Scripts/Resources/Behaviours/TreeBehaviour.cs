using UnityEngine;
using Cysharp.Threading.Tasks;

public class TreeBehaviour : IResourceBehaviour
{
    private ParticleFactory particleFactory;
    private Vector3 particleOffset = new Vector3(0, 3f, 0);
    private float grayDelay = 0.2f;      // Задержка перед серым
   // private float particleDelay = 0.3f;  // Задержка перед партиклами
    private float hideDelay = 1.8f;      // Задержка перед скрытием

    public TreeBehaviour(ParticleFactory particleFactory)
    {
        this.particleFactory = particleFactory;
    }

    public async void OnCollect(ResourceSource resource)
    {
        if (resource == null) return;

        Debug.Log($" ========== TreeBehaviour.OnCollect() START ==========");
        Debug.Log($"  - resource: {resource.name}");
        Debug.Log($"   - resource.IsAvailable: {resource.IsAvailable}");

        // 1. СРАЗУ — дерево становится серым
        Debug.Log($"   - ШАГ 1: Вызываем resource.SetGray()");
        resource.SetGray();
        Debug.Log($"   - SetGray() ВЫЗВАН");

        // 2. НЕБОЛЬШАЯ ПАУЗА
        Debug.Log($"  - ШАГ 2: Ждём {grayDelay} сек...");
        await UniTask.Delay((int)(grayDelay * 1000));
        Debug.Log($"  - Задержка завершена");

        // 3. ПАРТИКЛЫ
        Debug.Log($" - ШАГ 3: Создаём партиклы...");
        Vector3 position = resource.transform.position + particleOffset;
        if (particleFactory != null)
        {
            particleFactory.CreateWoodParticles(position);
            Debug.Log($"   - Партиклы созданы в {position}");
        }

        // 4. ЖДЁМ
        Debug.Log($"  - ШАГ 4: Ждём {hideDelay} сек...");
        await UniTask.Delay((int)(hideDelay * 1000));
        Debug.Log($"  - Задержка завершена");

        // 5. СКРЫВАЕМ
        Debug.Log($"  - ШАГ 5: Скрываем ресурс");
        resource.Hide();
        Debug.Log($"   - Ресурс {resource.name} скрыт");

        Debug.Log($" ========== TreeBehaviour.OnCollect() END ==========");
    }

    public void OnRespawn(ResourceSource resource)
    {
        if (resource != null)
        {
            resource.SetColored();
            resource.Show();
        }
        Debug.Log($" Ресурс {resource?.name} восстановлен и цветной");
    }

    public void OnCollect(Transform target) { }
}