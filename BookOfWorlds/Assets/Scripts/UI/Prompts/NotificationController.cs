using UnityEngine;
using Zenject;

/// <summary>
/// Управляет отображением уведомлений.
/// Находится на персонаже.
/// </summary>
public class NotificationController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private NotificationUI notificationUI;

    /// <summary>
    /// Показать уведомление
    /// </summary>
    public void Show(string message, float duration = 2f)
    {
        notificationUI?.Show(message, duration);
    }

    /// <summary>
    /// Скрыть уведомление
    /// </summary>
    public void Hide()
    {
        notificationUI?.Hide();
    }

    /// <summary>
    /// Проверить, активно ли уведомление
    /// </summary>
    public bool IsActive => notificationUI != null && notificationUI.IsActive;
}