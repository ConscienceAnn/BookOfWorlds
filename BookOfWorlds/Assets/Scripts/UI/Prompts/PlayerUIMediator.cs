using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;

/// <summary>
/// Координатор UI игрока.
/// Находится на персонаже, связывает подсказки зданий и уведомления.
/// </summary>
public class PlayerUIMediator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BuildingPromptController buildingPrompt;
    [SerializeField] private NotificationController notification;

    private BuildingController currentBuilding;
    private bool isPlayerNearBuilding = false;

    // ===== ПУБЛИЧНЫЕ МЕТОДЫ =====

    /// <summary>
    /// Показать подсказку здания
    /// </summary>
    public void ShowBuildingPrompt(BuildingController building)
    {
        if (building == null || building.IsRestored())
        {
            HideBuildingPrompt();
            return;
        }

        currentBuilding = building;
        isPlayerNearBuilding = true;

        // Скрываем уведомление, если оно активно
        notification?.Hide();

        buildingPrompt?.Show(building);
    }

    /// <summary>
    /// Скрыть подсказку здания
    /// </summary>
    public void HideBuildingPrompt()
    {
        isPlayerNearBuilding = false;
        currentBuilding = null;
        buildingPrompt?.Hide();
    }

    /// <summary>
    /// Обновить стоимость восстановления
    /// </summary>
    public void UpdateBuildingCost()
    {
        buildingPrompt?.UpdateCost();
    }

    /// <summary>
    /// Обновить стоимость мгновенно
    /// </summary>
    public void UpdateBuildingCostImmediate()
    {
        buildingPrompt?.UpdateCostImmediate();
    }

    /// <summary>
    /// Получить длительность анимации
    /// </summary>
    public float GetBuildingPromptAnimationDuration()
    {
        return buildingPrompt != null ? buildingPrompt.GetAnimationDuration() : 0.5f;
    }

    /// <summary>
    /// Показать уведомление
    /// </summary>
    public void ShowNotification(string message, float duration = 2f)
    {
        bool wasBuildingPromptVisible = buildingPrompt != null && buildingPrompt.IsActive;

        if (wasBuildingPromptVisible)
        {
            buildingPrompt.Hide();
        }

        notification?.Show(message, duration);

        // Если подсказка была активна — показываем её снова после уведомления
        if (wasBuildingPromptVisible && currentBuilding != null && !currentBuilding.IsRestored())
        {
            ShowBuildingPromptAfterDelay(duration + 0.1f).Forget();
        }
    }

    /// <summary>
    /// Скрыть уведомление
    /// </summary>
    public void HideNotification()
    {
        notification?.Hide();
    }

    /// <summary>
    /// Установить состояние игрока рядом со зданием
    /// </summary>
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
            // При выходе из зоны скрываем уведомление
            notification?.Hide();
        }
    }

    // ===== ПРИВАТНЫЕ МЕТОДЫ =====

    private async UniTaskVoid ShowBuildingPromptAfterDelay(float delay)
    {
        await UniTask.Delay((int)(delay * 1000));

        if (isPlayerNearBuilding && currentBuilding != null && !currentBuilding.IsRestored())
        {
            buildingPrompt?.Show(currentBuilding);
        }
    }
}