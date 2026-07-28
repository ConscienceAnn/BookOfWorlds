using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private Canvas parentCanvas;

    private Coroutine hideCoroutine;

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

        Debug.Log($"NotificationUI.Show() called: {message}");

        if (parentCanvas != null)
        {
            if (!parentCanvas.gameObject.activeSelf)
            {
                parentCanvas.gameObject.SetActive(true);
                Debug.Log("NotificationUI: Canvas was disabled, enabled!");
            }
            else
            {
                Debug.Log("NotificationUI: Canvas already enabled");
            }
        }

        gameObject.SetActive(true);

        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();

        Debug.Log($"  - gameObject.activeSelf: {gameObject.activeSelf}");
        Debug.Log($"  - notificationText.activeSelf: {notificationText.gameObject.activeSelf}");

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfter(duration));
    }

    private IEnumerator HideAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
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