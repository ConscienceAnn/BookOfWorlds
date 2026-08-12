using UnityEngine;
using Zenject;

public class ProgressBarFactory : MonoBehaviour
{
    [Header("Progress Bar Prefab")]
    [SerializeField] private GameObject progressBarPrefab;

    [Inject] private DiContainer diContainer;

    /// <summary>
    /// —оздаЄт прогресс-бар над указанным объектом
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

        // ===== »—ѕќЋ№«”≈ћ DiContainer ƒЋя —ќ«ƒјЌ»я =====
        GameObject progressBarObj = diContainer.InstantiatePrefab(
            progressBarPrefab,
            target.position + offset,
            Quaternion.identity,
            null  // parent = null (корневой уровень)
        );

        ProgressBarUI progressBar = progressBarObj.GetComponent<ProgressBarUI>();
        if (progressBar != null)
        {
            // Ќастраиваем фолловер
            WorldSpaceUIFollower follower = progressBarObj.GetComponent<WorldSpaceUIFollower>();
            if (follower != null)
            {
                follower.SetTarget(target);
                follower.SetOffset(offset);
            }

            return progressBar;
        }

        Debug.LogError($"ProgressBarFactory: ProgressBarUI не найден на {progressBarObj.name}");
        Destroy(progressBarObj);
        return null;
    }
}