using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BuildingPromptUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text hintText;

    [Header("Settings")]
    [SerializeField] private string hint = "Нажмите E чтобы вложить ресурсы";
    [SerializeField] private float animationDuration = 0.5f;

    public float AnimationDuration => animationDuration;

    private BuildingController currentBuilding;
    private float currentSliderValue = 0f;
    private float targetSliderValue = 0f;
    private CancellationTokenSource cts; 

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    public void Show(BuildingController building)
    {
        if (building == null || building.IsRestored()) return;

        currentBuilding = building;
        gameObject.SetActive(true);
        UpdateCostTextImmediate();
    }

    public void Hide()
    {
        cts?.Cancel();
        gameObject.SetActive(false);
        currentBuilding = null;
    }

    public void UpdateCostText()
    {
        if (currentBuilding == null) return;

        var costs = currentBuilding.GetCosts();
        if (costs == null || costs.Length == 0) return;

        if (titleText != null)
        {
            titleText.text = "Для восстановления требуется:";
        }

        string costString = "";
        int totalInvested = 0;
        int totalRequired = 0;

        foreach (var cost in costs)
        {
            int invested = currentBuilding.GetInvestedAmount(cost.resourceName);
            int required = currentBuilding.GetRequiredAmount(cost.resourceName);
            costString += $"{cost.resourceName}: {invested}/{required}\n";
            totalInvested += invested;
            totalRequired += required;
        }

        if (costText != null)
            costText.text = costString.TrimEnd('\n');

        float newProgress = totalRequired > 0 ? (float)totalInvested / totalRequired * 100f : 0f;
        targetSliderValue = Mathf.Clamp(newProgress, 0f, 100f);

        if (progressText != null)
        {
            progressText.text = $"{totalInvested}/{totalRequired}";
        }

        if (hintText != null)
        {
            hintText.text = currentBuilding.IsRestored() ? "Здание восстановлено!" : hint;
        }

        AnimateSliderAsync().Forget();
    }

    public void UpdateCostTextImmediate()
    {
        if (currentBuilding == null) return;

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();

        var costs = currentBuilding.GetCosts();
        if (costs == null || costs.Length == 0) return;

        if (titleText != null)
        {
            titleText.text = "Для восстановления требуется:";
        }

        string costString = "";
        int totalInvested = 0;
        int totalRequired = 0;

        foreach (var cost in costs)
        {
            int invested = currentBuilding.GetInvestedAmount(cost.resourceName);
            int required = currentBuilding.GetRequiredAmount(cost.resourceName);
            costString += $"{cost.resourceName}: {invested}/{required}\n";
            totalInvested += invested;
            totalRequired += required;
        }

        if (costText != null)
            costText.text = costString.TrimEnd('\n');

        float progress = totalRequired > 0 ? (float)totalInvested / totalRequired * 100f : 0f;
        currentSliderValue = Mathf.Clamp(progress, 0f, 100f);
        targetSliderValue = currentSliderValue;

        if (progressSlider != null)
        {
            progressSlider.value = currentSliderValue;
        }

        if (progressText != null)
        {
            progressText.text = $"{totalInvested}/{totalRequired}";
        }

        if (hintText != null)
        {
            hintText.text = currentBuilding.IsRestored() ? "Здание восстановлено!" : hint;
        }
    }

    private async UniTaskVoid AnimateSliderAsync()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();

        float startValue = currentSliderValue;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            if (cts.Token.IsCancellationRequested) break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            currentSliderValue = Mathf.Lerp(startValue, targetSliderValue, smoothT);

            if (progressSlider != null)
            {
                progressSlider.value = currentSliderValue;
            }

            await UniTask.Yield(cts.Token);
        }

        if (!cts.Token.IsCancellationRequested)
        {
            currentSliderValue = targetSliderValue;
            if (progressSlider != null)
            {
                progressSlider.value = currentSliderValue;
            }
        }
    }

    public bool IsShowingBuilding(BuildingController building)
    {
        return currentBuilding == building;
    }

    public void Refresh()
    {
        UpdateCostText();
    }
}