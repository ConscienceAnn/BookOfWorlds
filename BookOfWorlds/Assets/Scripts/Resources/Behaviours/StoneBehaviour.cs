using UnityEngine;
using Cysharp.Threading.Tasks;

public class StoneBehaviour : IResourceBehaviour
{
    private ParticleFactory particleFactory;
    private Vector3 particleOffset = new Vector3(0, 0.8f, 0);
    private float shakeDuration = 0.2f;      // Короткая тряска
    private float shakeMagnitude = 0.06f;    // Слабая тряска
   // private float hideDelay = 1.5f;          // Задержка перед скрытием

    public StoneBehaviour(ParticleFactory particleFactory)
    {
        this.particleFactory = particleFactory;
    }

    public async void OnCollect(ResourceSource resource)
    {
        if (resource == null) return;

        Debug.Log($"[StoneBehaviour] ====== НАЧАЛО СБОРА ======");

        // 1. СТАНОВИМСЯ СЕРЫМ (без текстур)
        resource.SetGray();
        Debug.Log($" [StoneBehaviour] ШАГ 1: resource.SetGray() ВЫЗВАН");

        // 2. ДАЁМ ВРЕМЯ ОТРЕНДЕРИТЬ СЕРЫЙ ЦВЕТ
        await UniTask.Delay(50);
        Debug.Log($" [StoneBehaviour] ШАГ 2: Серый цвет применён");

        // 3. ТРЯСКА КАМНЯ
        Debug.Log($" [StoneBehaviour] ШАГ 3: Трясём камень...");
        await ShakeStone(resource.transform, shakeDuration, shakeMagnitude);
        Debug.Log($" [StoneBehaviour] ШАГ 3: Тряска завершена");

        // 4. ПАРТИКЛЫ (пыль)
        Debug.Log($" [StoneBehaviour] ШАГ 4: Создаём партиклы...");
        Vector3 position = resource.transform.position + particleOffset;
        if (particleFactory != null)
        {
            particleFactory.CreateStoneParticles(position);
        }
        Debug.Log($" [StoneBehaviour] ШАГ 4: Партиклы созданы");

        // 5. ЖДЁМ
        //Debug.Log($" [StoneBehaviour] ШАГ 5: Ждём {hideDelay} сек...");
        //await UniTask.Delay((int)(hideDelay * 1000));
        //Debug.Log($" [StoneBehaviour] ШАГ 5: Ожидание завершено");

        // 6. СКРЫВАЕМ
        Debug.Log($" [StoneBehaviour] ШАГ 6: Скрываем ресурс");
        resource.Hide();
        Debug.Log($" [StoneBehaviour] ШАГ 6: Ресурс скрыт");

        // 7. ВЫЗЫВАЕМ СОБЫТИЕ (возвращаем в пул)
        resource.InvokeCollected();
        Debug.Log($" [StoneBehaviour] ШАГ 7: InvokeCollected вызван");

        Debug.Log($" [StoneBehaviour] ====== КОНЕЦ СБОРА ======");
    }

    /// <summary>
    /// Анимация тряски камня (с вращением)
    /// </summary>
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

        // Возвращаем в исходное положение
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    public void OnRespawn(ResourceSource resource)
    {
        if (resource != null)
        {
            resource.SetColored();
            resource.Show();
        }
        Debug.Log($"[StoneBehaviour] Ресурс восстановлен");
    }

    public void OnCollect(Transform target) { }
}