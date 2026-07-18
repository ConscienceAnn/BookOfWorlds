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

    private void Start()
    {
        FindBuildings();
        UpdateProgress();
    }

    private void FindBuildings()
    {
        buildings = FindObjectsOfType<BuildingController>();
        totalBuildings = buildings.Length;
        Debug.Log($" Найдено зданий: {totalBuildings}");
    }

    private void Update()
    {
        // Обновляем прогресс каждый кадр (можно реже)
        UpdateProgress();
    }

    public void UpdateProgress()
    {
        if (buildings == null || buildings.Length == 0)
        {
            FindBuildings();
            return;
        }

        int restoredCount = 0;
        foreach (var building in buildings)
        {
            if (building.IsRestored())
                restoredCount++;
        }

        float progress = totalBuildings > 0 ? (float)restoredCount / totalBuildings * 100f : 0f;
        int progressInt = Mathf.RoundToInt(progress);

        if (progressSlider != null)
            progressSlider.value = progressInt;

        if (progressText != null)
            progressText.text = $"{prefix}{progressInt}{suffix}";
    }

    // Вызывается при восстановлении здания
    public void OnBuildingRestored()
    {
        UpdateProgress();
        Debug.Log($" Прогресс обновлён: {progressSlider?.value ?? 0}%");
    }
}