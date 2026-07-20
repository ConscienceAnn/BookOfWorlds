using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text costText;

    [Header("Settings")]
    [SerializeField] private string title = "Нажмите E для восстановления";
    [SerializeField] private Vector2 screenOffset = new Vector2(0, 80);

    private BuildingController currentBuilding;
    private Camera mainCamera;
    private RectTransform canvasRect;
    private BuildingTrigger currentTrigger;

    private void Start()
    {
        mainCamera = Camera.main;
        canvasRect = promptCanvas.GetComponent<RectTransform>();

        if (titleText != null) titleText.text = title;
        if (promptCanvas != null)
            promptCanvas.gameObject.SetActive(false);

        // Подписываемся на события через EventBus
        EventBus.OnBuildingRestored += OnBuildingRestored;
        EventBus.OnBuildingProgressChanged += OnBuildingProgressChanged;
    }

    private void OnDestroy()
    {
        EventBus.OnBuildingRestored -= OnBuildingRestored;
        EventBus.OnBuildingProgressChanged -= OnBuildingProgressChanged;
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
        if (building == null || building.IsRestored()) return;

        currentBuilding = building;
        promptCanvas.gameObject.SetActive(true);
        UpdateCostText();
        Debug.Log($"Показана подсказка для {building.GetBuildingName()}");
    }

    public void HideBuildingPrompt()
    {
        promptCanvas.gameObject.SetActive(false);
        currentBuilding = null;
        Debug.Log("Подсказка скрыта");
    }

    public void UpdateCostText()
    {
        if (currentBuilding == null || costText == null) return;

        var costs = currentBuilding.GetCosts();
        if (costs == null) return;

        string costString = "";
        foreach (var cost in costs)
        {
            int invested = currentBuilding.GetInvestedAmount(cost.resourceName);
            int required = currentBuilding.GetRequiredAmount(cost.resourceName);
            costString += $"{cost.resourceName}: {invested}/{required}\n";
        }
        costText.text = costString;
    }

    public void UpdateCostTextForBuilding(BuildingController building)
    {
        if (building == null)
        {
            Debug.LogWarning("UpdateCostTextForBuilding: building is NULL!");
            return;
        }

        if (costText == null)
        {
            Debug.LogWarning("UpdateCostTextForBuilding: costText is NULL!");
            return;
        }

        var costs = building.GetCosts();
        if (costs == null)
        {
            Debug.LogWarning($"UpdateCostTextForBuilding: costs is NULL for {building.GetBuildingName()}");
            return;
        }

        Debug.Log($"UpdateCostTextForBuilding: {building.GetBuildingName()}");

        string costString = "";
        foreach (var cost in costs)
        {
            int invested = building.GetInvestedAmount(cost.resourceName);
            int required = building.GetRequiredAmount(cost.resourceName);
            costString += $"{cost.resourceName}: {invested}/{required}\n";
            Debug.Log($"  - {cost.resourceName}: invested={invested}, required={required}");
        }
        costText.text = costString;

        Debug.Log($"UpdateCostTextForBuilding: costText = '{costString.Replace("\n", " ")}'");
    }

    private void OnBuildingRestored(BuildingController building)
    {
        if (currentBuilding == building)
        {
            HideBuildingPrompt();
        }
        Debug.Log($"Здание {building.GetBuildingName()} восстановлено!");
    }

    private void OnBuildingProgressChanged(BuildingController building)
    {
        if (currentBuilding == building)
        {
            UpdateCostText();
        }
    }

    public void SetCurrentBuilding(BuildingController building)
    {
        currentBuilding = building;
    }

    public BuildingController GetCurrentBuilding()
    {
        return currentBuilding;
    }
}