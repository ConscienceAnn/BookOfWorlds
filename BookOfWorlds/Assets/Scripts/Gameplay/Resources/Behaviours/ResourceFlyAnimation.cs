using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class ResourceFlyAnimation : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private RectTransform woodIconTarget;
    [SerializeField] private RectTransform stoneIconTarget;
    [SerializeField] private RectTransform milkIconTarget;
    [SerializeField] private RectTransform woolIconTarget;

    [Header("Resource Icons (PNG Sprites)")]
    [SerializeField] private Sprite woodSprite;
    [SerializeField] private Sprite stoneSprite;
    [SerializeField] private Sprite milkSprite;
    [SerializeField] private Sprite woolSprite;

    [Header("Animation Settings")]
    [SerializeField] private float flyDuration = 0.6f;
    [SerializeField] private float startScale = 0.5f;
    [SerializeField] private float endScale = 1.2f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.5f, 1, 1.2f);

    private Camera mainCamera;
    private CancellationTokenSource cts;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public async UniTask Play(Vector3 worldStartPosition, string resourceName)
    {
        if (targetCanvas == null)
        {
            Debug.LogWarning($"ResourceFlyAnimation: targetCanvas is NULL!");
            return;
        }

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();

        try
        {
            await PlayInternal(worldStartPosition, resourceName, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log($" Анимация полёта {resourceName} отменена");
        }
    }

    private async UniTask PlayInternal(Vector3 worldStartPosition, string resourceName, CancellationToken token)
    {
        Debug.Log($" [ResourceFlyAnimation] Начинаем полёт: {resourceName}");

        // 1. Получаем спрайт для ресурса
        Sprite iconSprite = GetResourceSprite(resourceName);
        if (iconSprite == null)
        {
            Debug.LogWarning($" Нет спрайта для {resourceName}");
            return;
        }

        // 2. Создаём GameObject с Image в мире (на Canvas)
        GameObject iconObj = new GameObject($"FlyingResource_{resourceName}");
        iconObj.transform.SetParent(targetCanvas.transform, false);

        // Добавляем Image и назначаем спрайт
        var image = iconObj.AddComponent<UnityEngine.UI.Image>();
        image.sprite = iconSprite;
        image.raycastTarget = false;

        // Настраиваем размер
        RectTransform rectTransform = iconObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(48, 48); // Размер иконки

        // Позиция в мире - экран
        Vector3 startScreenPos = mainCamera.WorldToScreenPoint(worldStartPosition);
        rectTransform.position = startScreenPos;

        // 3. Получаем целевую позицию
        RectTransform targetRect = GetTargetRect(resourceName);
        if (targetRect == null)
        {
            Debug.LogWarning($" targetRect is NULL для {resourceName}");
            Destroy(iconObj);
            return;
        }

        Vector3 endScreenPos = targetRect.position;

        // 4. Анимация полёта
        float elapsed = 0f;
        float duration = flyDuration;

        Vector3 startPos = startScreenPos;
        Vector3 endPos = endScreenPos;

        while (elapsed < duration)
        {
            if (token.IsCancellationRequested)
            {
                Destroy(iconObj);
                return;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            // Дуга
            float arcHeight = Mathf.Sin(t * Mathf.PI) * 80f;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, smoothT);
            currentPos.z += arcHeight;

            rectTransform.position = currentPos;

            // Масштаб
            float scale = Mathf.Lerp(startScale, endScale, scaleCurve.Evaluate(t));
            rectTransform.localScale = Vector3.one * scale;

            await UniTask.Yield(token);
        }

        // 5. Финальный pop-эффект
        float popDuration = 0.15f;
        float popElapsed = 0f;
        while (popElapsed < popDuration)
        {
            if (token.IsCancellationRequested)
            {
                Destroy(iconObj);
                return;
            }

            popElapsed += Time.deltaTime;
            float popT = popElapsed / popDuration;
            float scale = Mathf.Lerp(endScale, endScale * 1.5f, popT);
            rectTransform.localScale = Vector3.one * scale;

            await UniTask.Yield(token);
        }

        // 6. Удаляем иконку
        Destroy(iconObj);
        Debug.Log($" [ResourceFlyAnimation] Полёт завершён: {resourceName}");
    }

    private Sprite GetResourceSprite(string resourceName)
    {
        switch (resourceName)
        {
            case "Дерево": return woodSprite;
            case "Камень": return stoneSprite;
            case "Молоко": return milkSprite;
            case "Шерсть": return woolSprite;
            default: return null;
        }
    }

    private RectTransform GetTargetRect(string resourceName)
    {
        switch (resourceName)
        {
            case "Дерево": return woodIconTarget;
            case "Камень": return stoneIconTarget;
            case "Молоко": return milkIconTarget;
            case "Шерсть": return woolIconTarget;
            default: return null;
        }
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}