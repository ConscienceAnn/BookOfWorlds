using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private WorldSpaceUIFollower uiFollower;
    [SerializeField] private Canvas parentCanvas;

    private void Awake()
    {
        if (uiFollower == null)
            uiFollower = GetComponent<WorldSpaceUIFollower>();

        if (uiFollower == null)
            Debug.LogError("ProgressBarUI: WorldSpaceUIFollower не найден!");

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
        SetProgress(0f);
    }

    public void Show(Transform target, float initialProgress = 0f)
    {
        if (target == null)
        {
            Debug.LogError("ProgressBarUI: target is NULL!");
            return;
        }

        // Активируем родительский Canvas
        if (parentCanvas != null && !parentCanvas.gameObject.activeSelf)
        {
            parentCanvas.gameObject.SetActive(true);
        }

        // Активируем объект
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
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
        if (uiFollower != null)
            uiFollower.ClearTarget();

        gameObject.SetActive(false);
    }

    public bool IsActive => gameObject.activeSelf && uiFollower != null && uiFollower.IsFollowing;
}