using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text notificationText;

    private Coroutine hideCoroutine;

    private void Start()
    {
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }

    public void Show(string message, float duration = 2f)
    {
        if (notificationText == null)
        {
            Debug.LogError("NotificationUI: notificationText is NULL!");
            return;
        }

        Debug.Log($"NotificationUI.Show() вызван: {message}");

        //  Активируем родительский объект (если есть)
        Transform parent = transform.parent;
        if (parent != null && !parent.gameObject.activeSelf)
        {
            parent.gameObject.SetActive(true);
            Debug.Log($"  - родитель {parent.name} активирован");
        }

        //  Активируем текущий объект
        gameObject.SetActive(true);

        notificationText.text = $" {message}";
        notificationText.gameObject.SetActive(true);

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