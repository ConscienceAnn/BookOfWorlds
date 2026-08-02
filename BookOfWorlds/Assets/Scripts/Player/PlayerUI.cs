using UnityEngine;
using TMPro;
using System.Collections;

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

    private BuildingController currentBuilding;
    private bool isPlayerNearBuilding = false;
    private Coroutine showPromptCoroutine;

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
        currentBuilding = building;
        isPlayerNearBuilding = true;

        // —крываем уведомление, если оно висит
        HideNotification();

        if (showPromptCoroutine != null)
        {
            StopCoroutine(showPromptCoroutine);
            showPromptCoroutine = null;
        }

        promptCanvas.gameObject.SetActive(true);
        buildingPromptUI?.Show(building);
    }

    public void HideBuildingPrompt()
    {
        isPlayerNearBuilding = false;
        currentBuilding = null;

        if (showPromptCoroutine != null)
        {
            StopCoroutine(showPromptCoroutine);
            showPromptCoroutine = null;
        }

        buildingPromptUI?.Hide();
        promptCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// ѕринудительно скрыть уведомление
    /// </summary>
    public void HideNotification()
    {
        if (notificationUI != null)
        {
            notificationUI.Hide();
        }
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

        bool wasBuildingPromptVisible = promptCanvas != null && promptCanvas.gameObject.activeSelf;

        if (wasBuildingPromptVisible)
        {
            buildingPromptUI?.Hide();
            promptCanvas.gameObject.SetActive(false);
        }

        notificationUI?.Show(message, duration);

        if (wasBuildingPromptVisible && currentBuilding != null)
        {
            if (showPromptCoroutine != null)
            {
                StopCoroutine(showPromptCoroutine);
            }
            showPromptCoroutine = StartCoroutine(ShowBuildingPromptAfter(duration + 0.1f));
        }
    }

    private IEnumerator ShowBuildingPromptAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        showPromptCoroutine = null;

        if (isPlayerNearBuilding && currentBuilding != null && !currentBuilding.IsRestored())
        {
            promptCanvas.gameObject.SetActive(true);
            buildingPromptUI?.Show(currentBuilding);
            Debug.Log($"Building Prompt восстановлен дл€ {currentBuilding.GetBuildingName()}");
        }
    }

    private void OnBuildingProgressChanged(BuildingController building)
    {
        if (buildingPromptUI != null && buildingPromptUI.IsShowingBuilding(building))
        {
            buildingPromptUI.UpdateCostText();
            Debug.Log($"UI здани€ обновлЄн дл€ {building.GetBuildingName()}");
        }
    }

    private void OnBuildingRestored(BuildingController building)
    {
        if (buildingPromptUI != null && buildingPromptUI.IsShowingBuilding(building))
        {
            isPlayerNearBuilding = false;
            currentBuilding = null;

            if (showPromptCoroutine != null)
            {
                StopCoroutine(showPromptCoroutine);
                showPromptCoroutine = null;
            }

            // —крываем уведомление при восстановлении здани€
            HideNotification();

            buildingPromptUI.Hide();
            promptCanvas.gameObject.SetActive(false);
            Debug.Log($"«дание {building.GetBuildingName()} восстановлено, UI скрыт");
        }
    }

    public void SetPlayerNearBuilding(bool isNear, BuildingController building = null)
    {
        isPlayerNearBuilding = isNear;
        if (isNear)
        {
            currentBuilding = building;
        }
        else if (!isNear && currentBuilding == building)
        {
            currentBuilding = null;
            // ѕри выходе из зоны скрываем уведомление
            HideNotification();
        }
    }
}