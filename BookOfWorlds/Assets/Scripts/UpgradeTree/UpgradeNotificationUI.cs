using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UpgradeNotificationUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup notificationGroup;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private Image backgroundImage;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Colors")]
    [SerializeField] private Color successColor = new Color(0.2f, 0.8f, 0.2f, 0.9f);  // Зелёный
    [SerializeField] private Color errorColor = new Color(0.8f, 0.2f, 0.2f, 0.9f);    // Красный
    [SerializeField] private Color warningColor = new Color(0.8f, 0.6f, 0.2f, 0.9f);   // Жёлтый

    private Coroutine currentCoroutine;

    private void Start()
    {
        if (notificationGroup != null)
        {
            notificationGroup.alpha = 0f;
            notificationGroup.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Показать уведомление
    /// </summary>
    /// <param name="message">Текст уведомления</param>
    /// <param name="isError">true = ошибка (красный), false = успех (зелёный)</param>
    public void ShowNotification(string message, bool isError = false)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        ShowNotification(message, isError ? errorColor : successColor);
    }

    /// <summary>
    /// Показать уведомление с произвольным цветом
    /// </summary>
    public void ShowNotification(string message, Color color)
    {
        if (currentCoroutine != null)
        {

            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(ShowNotificationCoroutine(message, color));
    }

    private IEnumerator ShowNotificationCoroutine(string message, Color color)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }

        if (notificationGroup != null)
        {
            notificationGroup.gameObject.SetActive(true);

            // Fade In
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                notificationGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            notificationGroup.alpha = 1f;

            // Ждём
            yield return new WaitForSecondsRealtime(displayDuration);

            // Fade Out
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                notificationGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            notificationGroup.alpha = 0f;
            notificationGroup.gameObject.SetActive(false);
        }

        currentCoroutine = null;
    }

    public void HideImmediate()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        if (notificationGroup != null)
        {
            notificationGroup.alpha = 0f;
            notificationGroup.gameObject.SetActive(false);
        }
    }
}