using UnityEngine;
using Cysharp.Threading.Tasks;

public class TreeBehaviour : IResourceBehaviour
{
    private ParticleFactory particleFactory;
    private Vector3 particleOffset = new Vector3(0, 3f, 0);
    // private float hideDelay = 5f; //  УВЕЛИЧИЛА ДО 5 СЕКУНД!
    private float shakeDuration = 0.3f;
    private float shakeMagnitude = 0.15f;

    public TreeBehaviour(ParticleFactory particleFactory)
    {
        this.particleFactory = particleFactory;
    }

    public async void OnCollect(ResourceSource resource)
    {
        if (resource == null) return;

        Debug.Log($" [TreeBehaviour] ====== НАЧАЛО СБОРА ======");

        //  СНАЧАЛА ЖДЁМ 0.1 СЕКУНДЫ (чтобы всё стабилизировалось)
        //await UniTask.Delay(100);

        //  ПОТОМ МЕНЯЕМ ЦВЕТ
        resource.SetGray();
        Debug.Log($" [TreeBehaviour] ШАГ 1: resource.SetGray() ВЫЗВАН");

        //  ТЕПЕРЬ ЖДЁМ 1 СЕКУНДУ (дерево стоит серое)
        await UniTask.Delay(450);
        Debug.Log($" [TreeBehaviour] ШАГ 2: Ожидание завершено");

        await ShakeTree(resource.transform, shakeDuration, shakeMagnitude);

        // 3. ПАРТИКЛЫ
        Debug.Log($" [TreeBehaviour] ШАГ 3: Создаём партиклы...");
        Vector3 position = resource.transform.position + particleOffset;
        if (particleFactory != null)
        {
            particleFactory.CreateWoodParticles(position);
        }
         
        // 4. ЖДЁМ 4 СЕКУНДЫ
        //Debug.Log($" [TreeBehaviour] ШАГ 4: Ждём 4 секунды...");
        //await UniTask.Delay(4000);
        //Debug.Log($" [TreeBehaviour] ШАГ 4: Ожидание завершено");

        // 5. СКРЫВАЕМ
        Debug.Log($" [TreeBehaviour] ШАГ 5: Скрываем ресурс");
        resource.Hide();
        Debug.Log($" [TreeBehaviour] ШАГ 5: Ресурс скрыт");

        // 6.  ВЫЗЫВАЕМ СОБЫТИЕ ЧЕРЕЗ ОБЁРТКУ
        resource.InvokeCollected();
        Debug.Log($" [TreeBehaviour] ШАГ 6: InvokeCollected вызван");

        Debug.Log($" [TreeBehaviour] ====== КОНЕЦ СБОРА ======");
    }

    public void OnRespawn(ResourceSource resource)
    {
        if (resource != null)
        {
            resource.SetColored();
            resource.Show();
        }
        Debug.Log($" [TreeBehaviour] Ресурс восстановлен");
    }

    public void OnCollect(Transform target) { }

    /// <summary>
    /// Анимация тряски дерева
    /// </summary>
    private async UniTask ShakeTree(Transform transform, float duration, float magnitude)
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Случайное смещение в пределах magnitude
            float x = Random.Range(-magnitude, magnitude);
            float z = Random.Range(-magnitude, magnitude);

            transform.position = new Vector3(
                originalPosition.x + x,
                originalPosition.y,
                originalPosition.z + z
            );

            await UniTask.Yield();
        }

        // Возвращаем в исходное положение
        transform.position = originalPosition;
    }
}