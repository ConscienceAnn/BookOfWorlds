using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Slider progressSlider;

    private UpgradeManager upgradeManager;
    private string upgradeId;

    public void Initialize(string id, UpgradeDataSO data, UpgradeManager manager)
    {
        upgradeId = id;
        upgradeManager = manager;

        if (nameText != null) nameText.text = data.upgradeName;
        if (iconImage != null) iconImage.sprite = data.icon;

        upgradeButton.onClick.AddListener(OnUpgradeClicked);
        Refresh();
    }

    public void Refresh()
    {
        if (string.IsNullOrEmpty(upgradeId) || upgradeManager == null) return;

        int currentLevel = upgradeManager.GetUpgradeLevel(upgradeId);
        int maxLevel = upgradeManager.GetMaxLevel(upgradeId);
        bool isMaxLevel = currentLevel >= maxLevel;
        bool hasEnoughCoins = upgradeManager.CanUpgrade(upgradeId);

        // Уровень
        if (levelText != null)
        {
            levelText.text = isMaxLevel ? "MAX" : $"{currentLevel + 1}/{maxLevel}";
        }

        // Значение
        if (valueText != null)
        {
            valueText.text = isMaxLevel ? "" : upgradeManager.GetNextLevelDescription(upgradeId);
        }

        // Стоимость
        if (costText != null)
        {
            if (isMaxLevel)
            {
                costText.text = "";
            }
            else
            {
                int cost = upgradeManager.GetNextLevelCost(upgradeId);
                costText.text = cost > 0 ? $"{cost} монет" : "Бесплатно";
            }
        }

        // Прогресс
        if (progressSlider != null)
        {
            progressSlider.maxValue = maxLevel;
            progressSlider.value = currentLevel;
        }

        // ===== КНОПКА: ВСЕГДА АКТИВНА (КРОМЕ MAX) =====
        if (upgradeButton != null)
        {
            // Кнопка НЕАКТИВНА только если MAX уровень
            upgradeButton.interactable = !isMaxLevel;

            // Текст кнопки
            TMP_Text buttonText = upgradeButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                if (isMaxLevel)
                {
                    buttonText.text = "Достигнут максимум";
                }
                else if (!hasEnoughCoins)
                {
                    int cost = upgradeManager.GetNextLevelCost(upgradeId);
                    buttonText.text = $"Нужно {cost} монет";
                }
                else
                {
                    buttonText.text = "Улучшить";
                }
            }
        }
    }

    private void OnUpgradeClicked()
    {
        if (!string.IsNullOrEmpty(upgradeId) && upgradeManager != null)
        {
            // Клик ВСЕГДА вызывает ApplyUpgrade,
            // а там уже будет проверка на монеты и уведомление
            upgradeManager.ApplyUpgrade(upgradeId);
        }
    }
}