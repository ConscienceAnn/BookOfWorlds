using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private Vector2 screenOffset = new Vector2(0, 80);

    [Header("Building Prompt")]
    [SerializeField] private BuildingPromptUI buildingPromptUI;

    [Header("Notification")]
    [SerializeField] private NotificationUI notificationUI;

    private Camera mainCamera;
    private RectTransform canvasRect;

    private void Start()
    {
        mainCamera = Camera.main;
        canvasRect = promptCanvas.GetComponent<RectTransform>();

        if (promptCanvas != null)
            promptCanvas.gameObject.SetActive(false);

        EventBus.OnBuildingProgressChanged += OnBuildingProgressChanged;
        EventBus.OnBuildingRestored += OnBuildingRestored;
    }

    private void OnDestroy()
    {
        EventBus.OnBuildingProgressChanged -= OnBuildingProgressChanged;
        EventBus.OnBuildingRestored -= OnBuildingRestored;
    }

    private void Update()
    {
        if (promptCanvas == null || !promptCanvas.gameObject.activeSelf) return;
        if (mainCamera == null) return;

        Vector3 worldPosition = transform.position;
        worldPosition.y += 2f;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        if (screenPosition.z < 0)
        {
            promptCanvas.gameObject.SetActive(false);
            return;
        }

        screenPosition += (Vector3)screenOffset;
        canvasRect.position = screenPosition;
    }

    public void ShowBuildingPrompt(BuildingController building)
    {
        promptCanvas.gameObject.SetActive(true);
        buildingPromptUI?.Show(building);
    }

    public void HideBuildingPrompt()
    {
        buildingPromptUI?.Hide();
        promptCanvas.gameObject.SetActive(false);
    }

    public void UpdateBuildingCost()
    {
        buildingPromptUI?.UpdateCostText();
    }

    public void UpdateBuildingCostImmediate()
    {
        buildingPromptUI?.UpdateCostTextImmediate();
    }

    public float GetBuildingPromptAnimationDuration()
    {
        return buildingPromptUI != null ? buildingPromptUI.AnimationDuration : 0.5f;
    }

    public void ShowNotification(string message, float duration = 2f)
    {
        Debug.Log($"PlayerUI.ShowNotification: {message}");

        // Активируем Canvas для уведомления
        if (promptCanvas != null && !promptCanvas.gameObject.activeSelf)
        {
            promptCanvas.gameObject.SetActive(true);
        }

        notificationUI?.Show(message, duration);
    }

    private void OnBuildingProgressChanged(BuildingController building)
    {
        if (buildingPromptUI != null && buildingPromptUI.IsShowingBuilding(building))
        {
            buildingPromptUI.UpdateCostText();
            Debug.Log($"UI здания обновлён для {building.GetBuildingName()}");
        }
    }

    private void OnBuildingRestored(BuildingController building)
    {
        if (buildingPromptUI != null && buildingPromptUI.IsShowingBuilding(building))
        {
            buildingPromptUI.Hide();
            promptCanvas.gameObject.SetActive(false);
            Debug.Log($"Здание {building.GetBuildingName()} восстановлено, UI скрыт");
        }
    }
}