using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text costText;

    [Header("Settings")]
    [SerializeField] private string title = "Нажмите E для восстановления";
    [SerializeField] private Vector2 screenOffset = new Vector2(0, 80); // Смещение на экране

    private BuildingController currentBuilding;
    private Camera mainCamera;
    private RectTransform canvasRect;

    private void Start()
    {
        mainCamera = Camera.main;
        canvasRect = promptCanvas.GetComponent<RectTransform>();

        if (titleText != null) titleText.text = title;
        if (promptCanvas != null)
            promptCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (promptCanvas == null || !promptCanvas.gameObject.activeSelf) return;
        if (mainCamera == null) return;

        // Позиция персонажа в мире + смещение над головой
        Vector3 worldPosition = transform.position;
        worldPosition.y += 2f; // Над головой

        // Переводим в экранные координаты
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        // Если персонаж за камерой — скрываем
        if (screenPosition.z < 0)
        {
            promptCanvas.gameObject.SetActive(false);
            return;
        }

        // Применяем смещение
        screenPosition += (Vector3)screenOffset;

        // Устанавливаем позицию Canvas на экране
        canvasRect.position = screenPosition;
    }

    public void ShowBuildingPrompt(BuildingController building)
    {
        if (building == null) return;

        currentBuilding = building;
        promptCanvas.gameObject.SetActive(true);
        UpdateCostText();
        Debug.Log($" Показана подсказка для {building.name}");
    }

    public void HideBuildingPrompt()
    {
        promptCanvas.gameObject.SetActive(false);
        currentBuilding = null;
        Debug.Log($" Подсказка скрыта");
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

    // Вызывается при восстановлении здания
    public void OnBuildingRestored()
    {
        HideBuildingPrompt();
    }

    private void OnDestroy()
    {
        // Очищаем ссылки
        if (promptCanvas != null)
            Destroy(promptCanvas.gameObject);
    }
}