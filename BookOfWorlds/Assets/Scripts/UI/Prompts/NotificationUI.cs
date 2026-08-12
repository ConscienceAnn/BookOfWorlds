using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private Canvas parentCanvas;

    private Coroutine hideCoroutine;

    public bool IsActive => gameObject.activeSelf && notificationText != null && notificationText.gameObject.activeSelf;

    private void Awake()
    {
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas == null)
            Debug.LogError("NotificationUI: parentCanvas not found!");
    }

    private void Start()
    {
        if (notificationText != null)
            notificationText.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    public void Show(string message, float duration = 2f)
    {
        if (notificationText == null)
        {
            Debug.LogError("NotificationUI: notificationText is NULL!");
            return;
        }

        if (parentCanvas != null && !parentCanvas.gameObject.activeSelf)
        {
            parentCanvas.gameObject.SetActive(true);
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        StartCoroutine(DelayedHideCoroutine(duration));
    }

    private IEnumerator DelayedHideCoroutine(float duration)
    {
        yield return null;

        hideCoroutine = StartCoroutine(HideAfter(duration));
    }

    private IEnumerator HideAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (notificationText != null)
            notificationText.gameObject.SetActive(false);

        gameObject.SetActive(false);
        hideCoroutine = null;
    }

    public void Hide()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (notificationText != null)
            notificationText.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }
}