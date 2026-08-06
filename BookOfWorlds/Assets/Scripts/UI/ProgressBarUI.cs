using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ProgressBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private WorldSpaceUIFollower uiFollower;

    private Canvas parentCanvas;

    private void Awake()
    {
        if (uiFollower == null)
            uiFollower = GetComponent<WorldSpaceUIFollower>();

        if (uiFollower == null)
            Debug.LogError("ProgressBarUI: WorldSpaceUIFollower не найден!");

        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(false);
        }
        SetProgress(0f);
    }

    public void Show(Transform target, float initialProgress = 0f)
    {
        if (target == null)
        {
            Debug.LogError("ProgressBarUI: target is NULL!");
            return;
        }

        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(true);
        }

        if (uiFollower != null)
        {
            uiFollower.SetTarget(target);
        }

        SetProgress(initialProgress);
    }

    public void SetProgress(float progress)
    {
        if (progressSlider != null)
        {
            progressSlider.value = Mathf.Clamp01(progress);
        }
    }

    public void Hide()
    {
        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(false);
        }

        if (uiFollower != null)
            uiFollower.ClearTarget();
    }

    public bool IsActive => progressSlider != null && progressSlider.gameObject.activeSelf;
}