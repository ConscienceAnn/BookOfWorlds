using UnityEngine;

public class ProgressBarFactory : MonoBehaviour
{
    [Header("Progress Bar Prefab")]
    [SerializeField] private GameObject progressBarPrefab;

    /// <summary>
    /// Создаёт прогресс-бар над указанным объектом
    /// </summary>
    public ProgressBarUI CreateProgressBar(Transform target, Vector3 offset)
    {
        if (progressBarPrefab == null)
        {
            Debug.LogError("ProgressBarFactory: progressBarPrefab is NULL!");
            return null;
        }

        if (target == null)
        {
            Debug.LogError("ProgressBarFactory: target is NULL!");
            return null;
        }

        //  Создаём экземпляр префаба на корневом уровне сцены
        GameObject progressBarObj = Instantiate(progressBarPrefab, target.position + offset, Quaternion.identity);

        //  НЕ ДЕЛАЕМ ЕГО ДОЧЕРНИМ target!
        // Просто размещаем в нужной позиции

        ProgressBarUI progressBar = progressBarObj.GetComponent<ProgressBarUI>();
        if (progressBar != null)
        {
            // Настраиваем фолловер на целевой объект
            WorldSpaceUIFollower follower = progressBarObj.GetComponent<WorldSpaceUIFollower>();
            if (follower != null)
            {
                follower.SetTarget(target);
                follower.SetOffset(offset);
            }

            Debug.Log($"ProgressBarFactory: создан ProgressBar для {target.name}");
            return progressBar;
        }

        Debug.LogError($"ProgressBarFactory: ProgressBarUI не найден на {progressBarObj.name}");
        Destroy(progressBarObj);
        return null;
    }
}