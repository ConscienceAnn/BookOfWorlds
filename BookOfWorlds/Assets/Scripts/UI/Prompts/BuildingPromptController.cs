using UnityEngine;
using Zenject;

/// <summary>
/// Управляет отображением подсказки над зданием.
/// Находится на персонаже, управляет Canvas с подсказкой.
/// </summary>
public class BuildingPromptController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private BuildingPromptUI buildingPromptUI;
    [SerializeField] private WorldSpaceUIFollower uiFollower;

    [Header("Settings")]
    [SerializeField] private Vector2 screenOffset = new Vector2(0, 80);
    [SerializeField] private float promptOffsetY = 2f;

    [Inject] private Camera mainCamera;

    private BuildingController currentBuilding;


    private void Start()
    {
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

    /// <summary>
    /// Показать подсказку для здания
    /// </summary>
    public void Show(BuildingController building)
    {
        if (building == null || building.IsRestored())
        {
            Hide();
            return;
        }

        currentBuilding = building;

        // Настраиваем позицию — подсказка следует за игроком,
        // но позиционируется относительно здания через WorldSpaceUIFollower
        if (uiFollower != null)
        {
            uiFollower.SetTarget(building.transform);
            uiFollower.SetOffset(new Vector3(0, promptOffsetY, 0));
        }

        promptCanvas.gameObject.SetActive(true);
        buildingPromptUI?.Show(building);
    }

    /// <summary>
    /// Скрыть подсказку
    /// </summary>
    public void Hide()
    {
        currentBuilding = null;

        buildingPromptUI?.Hide();
        promptCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Обновить стоимость восстановления
    /// </summary>
    public void UpdateCost()
    {
        buildingPromptUI?.UpdateCostText();
    }

    /// <summary>
    /// Обновить стоимость мгновенно
    /// </summary>
    public void UpdateCostImmediate()
    {
        buildingPromptUI?.UpdateCostTextImmediate();
    }

    /// <summary>
    /// Получить длительность анимации
    /// </summary>
    public float GetAnimationDuration()
    {
        return buildingPromptUI != null ? buildingPromptUI.AnimationDuration : 0.5f;
    }

    /// <summary>
    /// Проверить, показывает ли контроллер это здание
    /// </summary>
    public bool IsShowingBuilding(BuildingController building)
    {
        return currentBuilding == building;
    }

    /// <summary>
    /// Проверить, активна ли подсказка
    /// </summary>
    public bool IsActive => promptCanvas != null && promptCanvas.gameObject.activeSelf;

    // ===== ОБРАБОТЧИКИ СОБЫТИЙ =====

    private void OnBuildingProgressChanged(BuildingController building)
    {
        if (IsShowingBuilding(building))
        {
            buildingPromptUI?.UpdateCostText();
        }
    }

    private void OnBuildingRestored(BuildingController building)
    {
        if (IsShowingBuilding(building))
        {
            Hide();
        }
    }
}