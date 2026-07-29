using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;

public class ResourceFlyAnimation : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private RectTransform woodIconTarget;
    [SerializeField] private RectTransform stoneIconTarget;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public async UniTask Play(Vector3 worldStartPosition, string resourceName)
    {
        Debug.Log($"ResourceFlyAnimation.Play() START: {resourceName}, позиция: {worldStartPosition}");

        if (targetCanvas == null)
        {
            Debug.LogWarning($" targetCanvas is NULL!");
            return;
        }

        // 1. Создаём иконку
        Debug.Log($" Создаём иконку для {resourceName}");
        GameObject icon = new GameObject("FlyingResource");
        icon.transform.position = worldStartPosition;

        var text = icon.AddComponent<TextMeshPro>();
        text.text = GetResourceIcon(resourceName);
        text.fontSize = 20;
        text.alignment = TextAlignmentOptions.Center;

        // 2. Получаем целевую позицию
        RectTransform targetRect = GetTargetRect(resourceName);
        if (targetRect == null)
        {
            Debug.LogWarning($" targetRect is NULL для {resourceName}");
            Destroy(icon);
            return;
        }

        Vector3 targetScreenPos = targetRect.position;
        Vector3 startScreenPos = mainCamera.WorldToScreenPoint(worldStartPosition);
        Debug.Log($" startScreenPos: {startScreenPos}, targetScreenPos: {targetScreenPos}");

        // 3. Анимация полёта
        float duration = 0.5f;
        float elapsed = 0f;

        Debug.Log($" Начинаем анимацию полёта...");
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            Vector3 currentPos = Vector3.Lerp(startScreenPos, targetScreenPos, smoothT);
            icon.transform.position = currentPos;

            await UniTask.Yield();
        }

        Debug.Log($" Анимация полёта завершена");

        // 4. Удаляем иконку
        Destroy(icon);
        Debug.Log($" ResourceFlyAnimation.Play() END");
    }

    private RectTransform GetTargetRect(string resourceName)
    {
        switch (resourceName)
        {
            case "Wood": return woodIconTarget;
            case "Stone": return stoneIconTarget;
            default: return null;
        }
    }

    private string GetResourceIcon(string resourceName)
    {
        switch (resourceName)
        {
            case "Wood": return "w";
            case "Stone": return "s";
            default: return "_";
        }
    }
}