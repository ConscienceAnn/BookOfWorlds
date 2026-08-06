using UnityEngine;
using Zenject;

public class ProgressBarFactory : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject progressBarPrefab;

    [Header("Canvas")]
    [SerializeField] private Canvas targetCanvas;

    [Inject] private DiContainer container;

    public ProgressBarUI CreateProgressBar(Transform target, Vector3 offset)
    {
        if (progressBarPrefab == null)
        {
            Debug.LogError("ProgressBarFactory: progressBarPrefab is NULL!");
            return null;
        }

        if (targetCanvas == null)
        {
            Debug.LogError("ProgressBarFactory: targetCanvas is NULL!");
            return null;
        }

        //  1. ÑÍÀ×ÀËÀ ÑÎÇÄÀ¨Ì İÊÇÅÌÏËßĞ ÁÅÇ ĞÎÄÈÒÅËß
        GameObject instance = container.InstantiatePrefab(
            progressBarPrefab,
            Vector3.zero,
            Quaternion.identity,
            null  
        );

        // 2. ÏÎÒÎÌ ÓÑÒÀÍÀÂËÈÂÀÅÌ ĞÎÄÈÒÅËß (óæå íà ñöåíå)
        instance.transform.SetParent(targetCanvas.transform, false);

        ProgressBarUI progressBar = instance.GetComponent<ProgressBarUI>();
        if (progressBar == null)
        {
            Debug.LogError("ProgressBarFactory: ProgressBarUI component not found!");
            Destroy(instance);
            return null;
        }

        // Íàñòğàèâàåì WorldSpaceUIFollower
        WorldSpaceUIFollower follower = instance.GetComponent<WorldSpaceUIFollower>();
        if (follower != null)
        {
            follower.SetTarget(target);
            follower.SetOffset(offset);
        }

        // Ñêğûâàåì ïî óìîë÷àíèş
        progressBar.Hide();

        Debug.Log($"ProgressBarFactory: ñîçäàí ProgressBar äëÿ {target.name}");

        return progressBar;
    }

    public void DestroyProgressBar(ProgressBarUI progressBar)
    {
        if (progressBar != null)
        {
            Destroy(progressBar.gameObject);
        }
    }
}