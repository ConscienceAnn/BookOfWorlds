using UnityEngine;
using TMPro;

public class BuildingPrompt : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text costText;

    [Header("Settings")]
    [SerializeField] private string title = "Нажмите E для восстановления";

    private BuildingController buildingController;

    private void Start()
    {
        buildingController = GetComponentInParent<BuildingController>();
        Debug.Log($" BuildingPrompt.Start(): buildingController = {(buildingController != null ? "НАЙДЕН" : "НЕ НАЙДЕН")}");
        Debug.Log($" BuildingPrompt.Start(): promptCanvas = {(promptCanvas != null ? "ЕСТЬ" : "НЕТ")}");

        if (titleText != null) titleText.text = title;

        if (promptCanvas != null)
        {
            promptCanvas.gameObject.SetActive(false);
            Debug.Log($" Canvas изначально скрыт");
        }
    }

    public void UpdateCostText()
    {
        if (costText == null || buildingController == null) return;

        var costs = buildingController.GetCosts();
        if (costs == null) return;

        string costString = "";
        foreach (var cost in costs)
        {
            int invested = buildingController.GetInvestedAmount(cost.resourceName);
            int required = buildingController.GetRequiredAmount(cost.resourceName);
            costString += $"{cost.resourceName}: {invested}/{required}\n";
        }
        costText.text = costString;
    }

    public void Show()
    {
        Debug.Log($" BuildingPrompt.Show() вызван! promptCanvas = {(promptCanvas != null ? "ЕСТЬ" : "НЕТ")}");

        if (promptCanvas != null)
        {
            promptCanvas.gameObject.SetActive(true);
            Debug.Log($" Canvas активирован!");
            UpdateCostText();
        }
        else
        {
            Debug.LogError($" promptCanvas = NULL! Невозможно показать подсказку!");
        }
    }

    public void Hide()
    {
        Debug.Log($" BuildingPrompt.Hide() вызван!");

        if (promptCanvas != null)
        {
            promptCanvas.gameObject.SetActive(false);
            Debug.Log($" Canvas скрыт!");
        }
    }

    public void SetActive(bool active)
    {
        Debug.Log($" BuildingPrompt.SetActive({active}) вызван!");

        if (promptCanvas != null)
        {
            promptCanvas.gameObject.SetActive(active);
            if (active) UpdateCostText();
            Debug.Log($" Canvas {(active ? "активирован" : "скрыт")}!");
        }
    }
}