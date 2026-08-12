using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using Cinemachine;
using Zenject;

public class LevelProgress : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private GameObject levelCompleteVisualPrefab;
    [SerializeField] private GameObject levelCompletePanel;

    [Header("Camera Animation")]
    [SerializeField] private Camera finalCamera;
    [SerializeField] private Vector3 finalCameraPosition = new Vector3(0, 65, 25);
    [SerializeField] private Vector3 finalCameraRotation = new Vector3(72, 180, 0);
    [SerializeField] private float cameraAnimationDuration = 2.5f;
    [SerializeField] private AnimationCurve cameraCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Timing")]
    [SerializeField] private float pauseAfterCamera = 1f;
    [SerializeField] private float showPanelDelay = 0.5f;

    [Header("Settings")]
    [SerializeField] private string prefix = "Восстановление: ";
    [SerializeField] private string suffix = "%";

    [Inject] private PlayerInputHandlerMy playerInputHandlerMy;
    [Inject] private UIManager uiManager;
    [Inject] private LevelManager levelManager;

    public event System.Action OnLevelComplete;

    private BuildingController[] buildings;
    private int totalBuildings = 0;
    private int lastProgress = -1;
    private bool isLevelComplete = false;
    private GameObject visualInstance;
    private Camera mainCamera;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isCameraAnimating = false;
    private bool isFinalCameraActive = false;
    private CinemachineBrain cinemachineBrain;
    private CinemachineVirtualCamera[] virtualCameras;
    private bool isRetry = false;

    public void SetRetryMode(bool value)
    {
        isRetry = value;
    }

    // ===== LIFECYCLE =====

    private void Start()
    {
        FindBuildings();
        UpdateProgress();

        EventBus.OnBuildingRestored += OnBuildingRestored;
        EventBus.OnBuildingProgressChanged += OnBuildingProgressChanged;

        // ===== ПОДПИСКА НА ОЧИСТКУ УРОВНЯ =====
        LevelGenerator.OnLevelCleared += OnLevelCleared;

        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalPosition = mainCamera.transform.position;
            originalRotation = mainCamera.transform.rotation;
        }

        cinemachineBrain = FindObjectOfType<CinemachineBrain>();
        virtualCameras = FindObjectsOfType<CinemachineVirtualCamera>();

        if (finalCamera != null)
        {
            finalCamera.enabled = false;
        }
    }

    private void OnDestroy()
    {
        EventBus.OnBuildingRestored -= OnBuildingRestored;
        EventBus.OnBuildingProgressChanged -= OnBuildingProgressChanged;

        // ===== ОТПИСКА =====
        LevelGenerator.OnLevelCleared -= OnLevelCleared;
    }

    // ===== PROGRESS =====

    private void FindBuildings()
    {
        buildings = FindObjectsOfType<BuildingController>();
        totalBuildings = buildings.Length;
    }

    public void UpdateProgress()
    {
        if (buildings == null || buildings.Length == 0)
        {
            FindBuildings();
            if (buildings == null || buildings.Length == 0) return;
        }

        int restoredCount = 0;
        foreach (var building in buildings)
        {
            if (building != null && building.IsRestored()) restoredCount++;
        }

        float progress = totalBuildings > 0 ? (float)restoredCount / totalBuildings * 100f : 0f;
        int progressInt = Mathf.RoundToInt(progress);

        if (progressInt != lastProgress)
        {
            lastProgress = progressInt;

            if (progressSlider != null)
                progressSlider.value = progressInt;

            if (progressText != null)
                progressText.text = $"{prefix}{progressInt}{suffix}";
        }

        if (progressInt >= 100 && !isLevelComplete)
        {
            isLevelComplete = true;
            Debug.Log("УРОВЕНЬ ЗАВЕРШЁН!");
            ShowCompleteAsync().Forget();
        }
    }

    public void ForceUpdate()
    {
        if (isRetry)
        {
            return;
        }

        lastProgress = -1;
        isLevelComplete = false;

        FindBuildings();
        UpdateProgress();
    }

    // ===== COMPLETE UI (PUBLIC) =====

    /// <summary>
    /// ПОКАЗЫВАЕТ ВИЗУАЛ + ФИНАЛЬНУЮ КАМЕРУ + ПАНЕЛЬ
    /// </summary>
    private async UniTaskVoid ShowCompleteAsync()
    {
        // 1. Визуал
        if (levelCompleteVisualPrefab != null)
        {
            visualInstance = Instantiate(levelCompleteVisualPrefab, Vector3.zero, Quaternion.identity);
        }

        // 2. Анимация камеры
        await AnimateCameraToFinalPositionAsync();

        // 3. Пауза
        await UniTask.Delay((int)(pauseAfterCamera * 1000));

        // 4. Панель
        await UniTask.Delay((int)(showPanelDelay * 1000));

        if (uiManager != null)
        {
            bool hasNextLevel = levelManager != null && levelManager.HasNextLevel();
            uiManager.ShowLevelComplete(hasNextLevel);
        }
        else
        {
            // Fallback
            if (levelCompletePanel != null)
            {
                levelCompletePanel.SetActive(true);
            }
        }

        OnLevelComplete?.Invoke();
    }

    /// <summary>
    /// СКРЫВАЕТ ВСЁ: визуал, панель, возвращает камеру
    /// </summary>
    public void HideComplete()
    {
        // 1. Уничтожаем визуал
        if (visualInstance != null)
        {
            Destroy(visualInstance);
            visualInstance = null;
        }

        // 2. Скрываем панель через UIManager
        if (uiManager != null)
        {
            uiManager.HideLevelComplete();
        }
        else if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }

        // 3. Возвращаем камеру
        ReturnToGameCamera();
    }

    /// <summary>
    /// ПОЛНЫЙ СБРОС СОСТОЯНИЯ (для перезапуска уровня)
    /// </summary>
    public void ResetState()
    {
        // 1. Сбрасываем флаги
        isLevelComplete = false;
        lastProgress = -1;
        isRetry = false;
        // 2. Очищаем список зданий
        buildings = null;
        totalBuildings = 0;

        // 3. Скрываем всё
        HideComplete();

        // 4. Сбрасываем UI
        if (progressSlider != null)
            progressSlider.value = 0;

        if (progressText != null)
            progressText.text = $"{prefix}0{suffix}";
    }

    // ===== CAMERA (PRIVATE) =====

    private async UniTask AnimateCameraToFinalPositionAsync()
    {
        if (mainCamera == null || finalCamera == null)
        {
            return;
        }

        if (isCameraAnimating) return;
        isCameraAnimating = true;

        // Отключаем Cinemachine
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }

        foreach (var vcam in virtualCameras)
        {
            if (vcam != null) vcam.enabled = false;
        }

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        Quaternion targetRot = Quaternion.Euler(finalCameraRotation);

        finalCamera.transform.position = finalCameraPosition;
        finalCamera.transform.rotation = targetRot;
        finalCamera.enabled = true;
        isFinalCameraActive = true;

        float elapsed = 0f;
        while (elapsed < cameraAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cameraAnimationDuration;
            float smoothT = cameraCurve.Evaluate(t);

            mainCamera.transform.position = Vector3.Lerp(startPos, finalCameraPosition, smoothT);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);

            await UniTask.Yield(this.GetCancellationTokenOnDestroy());
        }

        mainCamera.transform.position = finalCameraPosition;
        mainCamera.transform.rotation = targetRot;

        isCameraAnimating = false;
    }

    public void ReturnToGameCamera()
    {
        if (!isFinalCameraActive)
        {
            return;
        }

        if (finalCamera != null)
            finalCamera.enabled = false;

        if (mainCamera != null)
        {
            mainCamera.transform.position = originalPosition;
            mainCamera.transform.rotation = originalRotation;
        }

        if (cinemachineBrain != null)
            cinemachineBrain.enabled = true;

        foreach (var vcam in virtualCameras)
        {
            if (vcam != null) vcam.enabled = true;
        }

        isFinalCameraActive = false;
        isCameraAnimating = false;
    }

    // ===== EVENTS =====

    private void OnBuildingRestored(BuildingController building)
    {
        lastProgress = -1;
        UpdateProgress();
    }

    private void OnBuildingProgressChanged(BuildingController building)
    {
        UpdateProgress();
    }

    // ===== PUBLIC GETTERS =====

    public float GetProgress() => totalBuildings > 0 ? (float)GetRestoredCount() / totalBuildings : 0f;
    public int GetRestoredCount()
    {
        int count = 0;
        if (buildings != null)
        {
            foreach (var building in buildings)
            {
                if (building != null && building.IsRestored())
                    count++;
            }
        }
        return count;
    }
    public int GetTotalCount() => totalBuildings;
    public bool IsLevelComplete => isLevelComplete;

    private void OnLevelCleared()
    {
        buildings = null;
        totalBuildings = 0;
        isLevelComplete = false;
        lastProgress = -1;
    }
}