using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelProgress : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;

    [Header("Settings")]
    [SerializeField] private string prefix = "Восстановление: ";
    [SerializeField] private string suffix = "%";

    private BuildingController[] buildings;
    private int totalBuildings = 0;
    private int lastProgress = -1;

    private void Start()
    {
        FindBuildings();
        UpdateProgress();

        EventBus.OnBuildingRestored += OnBuildingRestored;
        EventBus.OnBuildingProgressChanged += OnBuildingProgressChanged;
    }

    private void FindBuildings()
    {
        buildings = FindObjectsOfType<BuildingController>();
        totalBuildings = buildings.Length;
        Debug.Log($"Найдено зданий: {totalBuildings}");
    }

    public void UpdateProgress()
    {
        if (buildings == null || buildings.Length == 0)
        {
            FindBuildings();
            if (buildings == null || buildings.Length == 0)
            {
                return;
            }
        }

        int restoredCount = 0;
        foreach (var building in buildings)
        {
            if (building != null && building.IsRestored())
                restoredCount++;
        }

        float progress = totalBuildings > 0 ? (float)restoredCount / totalBuildings * 100f : 0f;
        int progressInt = Mathf.RoundToInt(progress);

        if (progressInt != lastProgress)
        {
            lastProgress = progressInt;

            if (progressSlider != null)
                progressSlider.value = progressInt;

            if (progressText != null)
                progressText.text = $"{prefix}{progressInt}{suffix}";

            Debug.Log($"Прогресс: {progressInt}% ({restoredCount}/{totalBuildings})");
        }
    }

    public void ForceUpdate()
    {
        lastProgress = -1;
        UpdateProgress();
        Debug.Log("LevelProgress принудительно обновлён");
    }

    private void OnDestroy()
    {
        EventBus.OnBuildingRestored -= OnBuildingRestored;
        EventBus.OnBuildingProgressChanged -= OnBuildingProgressChanged;
    }

    private void OnBuildingRestored(BuildingController building)
    {
        lastProgress = -1;
        UpdateProgress();
        Debug.Log($"Прогресс обновлён: восстановлено {building.GetBuildingName()}");
    }

    private void OnBuildingProgressChanged(BuildingController building)
    {
        UpdateProgress();
        Debug.Log($"Прогресс обновлён (частично): {building.GetBuildingName()}");
    }
}