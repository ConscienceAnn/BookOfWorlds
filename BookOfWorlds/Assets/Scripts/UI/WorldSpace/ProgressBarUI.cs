using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ProgressBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private WorldSpaceUIFollower uiFollower;

    private UIManager uiManager; // БЕЗ [Inject]
    private Canvas parentCanvas;
    private bool isHiddenByPanel = false;

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

        // ===== НАХОДИМ UIManager =====
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                Debug.Log($"ProgressBarUI {gameObject.name}: UIManager найден");
                uiManager.OnPanelsOpened += OnPanelsOpened;
                uiManager.OnPanelsClosed += OnPanelsClosed;
                Debug.Log($"ProgressBarUI {gameObject.name}: подписан на события UIManager");
            }
            else
            {
                Debug.LogWarning($"ProgressBarUI {gameObject.name}: UIManager не найден!");
            }
        }
    }

    private void OnDestroy()
    {
        if (uiManager != null)
        {
            uiManager.OnPanelsOpened -= OnPanelsOpened;
            uiManager.OnPanelsClosed -= OnPanelsClosed;
        }
    }

    private void OnPanelsOpened()
    {
        if (progressSlider != null && progressSlider.gameObject.activeSelf)
        {
            isHiddenByPanel = true;
            progressSlider.gameObject.SetActive(false);
            Debug.Log($"ProgressBarUI {gameObject.name}: скрыт из-за открытой панели");
        }
    }

    private void OnPanelsClosed()
    {
        if (isHiddenByPanel)
        {
            isHiddenByPanel = false;
            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(true);
                Debug.Log($"ProgressBarUI {gameObject.name}: показан после закрытия панели");
            }
        }
    }

    public void Show(Transform target, float initialProgress = 0f)
    {
        if (target == null)
        {
            Debug.LogError("ProgressBarUI: target is NULL!");
            return;
        }

        isHiddenByPanel = false;

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
        isHiddenByPanel = false;

        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(false);
        }

        if (uiFollower != null)
            uiFollower.ClearTarget();
    }

    public bool IsActive => progressSlider != null && progressSlider.gameObject.activeSelf;
}