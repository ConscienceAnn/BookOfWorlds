using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

public class BuildingController : MonoBehaviour, IInteractable
{
    [Header("Building Data")]
    [SerializeField] private BuildingDataSO buildingData;

    [Header("Visuals")]
    [SerializeField] private GameObject ruinedVisual;
    [SerializeField] private GameObject restoredVisual;
    [SerializeField] private GameObject[] blockedColliders;

    [Inject] private IPlayerInventory inventory;
    [Inject] private LevelProgress levelProgress;
    [Inject] private GameSaveController gameSaveController;
    [Inject] private PlayerUIMediator playerUIMediator;

    private bool isRestored = false;
    private bool hasLoadedData = false;
    private Dictionary<string, int> investedResources = new Dictionary<string, int>();

    // ===== PUBLIC METHODS =====
    public string GetBuildingId() => buildingData?.buildingId ?? "unknown";
    public string GetBuildingName() => buildingData?.buildingName ?? name;
    public bool IsRestored() => isRestored;
    public ResourceCost[] GetCosts() => buildingData?.costs;

    public int GetInvestedAmount(string resourceName)
    {
        return investedResources.ContainsKey(resourceName) ? investedResources[resourceName] : 0;
    }

    public int GetRequiredAmount(string resourceName)
    {
        foreach (var cost in buildingData.costs)
        {
            if (cost.resourceName == resourceName)
                return cost.amount;
        }
        return 0;
    }

    public void UpdateBuildingPrompt()
    {
        if (playerUIMediator != null)
        {
            playerUIMediator?.UpdateBuildingCost();
        }
    }

    // ===== IInteractable =====
    public void Interact()
    {
        TryRestore().Forget();
    }

    // ===== UNITY LIFECYCLE =====
    private void Awake()
    {
        if (buildingData != null && buildingData.costs != null)
        {
            foreach (var cost in buildingData.costs)
            {
                if (!investedResources.ContainsKey(cost.resourceName))
                {
                    investedResources[cost.resourceName] = 0;
                }
            }
        }
    }

    private void Start()
    {
        if (!hasLoadedData)
        {
            UpdateVisual(false);
        }
    }

    // ===== VISUAL =====
    private void UpdateVisual(bool restored)
    {
        isRestored = restored;

        if (ruinedVisual != null) ruinedVisual.SetActive(!restored);
        if (restoredVisual != null) restoredVisual.SetActive(restored);

        foreach (var collider in blockedColliders)
        {
            if (collider != null)
            {
                collider.SetActive(!restored);
            }
        }
    }

    // ===== SAVE / LOAD =====
    public void SyncStateFromSave(bool restored, Dictionary<string, int> savedInvested)
    {
        hasLoadedData = true;

        if (savedInvested != null)
        {
            foreach (var kvp in savedInvested)
            {
                investedResources[kvp.Key] = kvp.Value;
            }
        }

        isRestored = restored;
        UpdateVisual(restored);

        if (restored)
        {
            var trigger = GetComponentInChildren<BuildingTrigger>();
            if (trigger != null)
            {
                trigger.gameObject.SetActive(false);
            }
            EventBus.BuildingRestored(this);
        }
        else
        {
            if (playerUIMediator != null)
            {
                playerUIMediator?.UpdateBuildingCostImmediate();
            }
            EventBus.BuildingProgressChanged(this);
        }
    }

    public void RestoreImmediate()
    {
        UpdateVisual(true);

        var trigger = GetComponentInChildren<BuildingTrigger>();
        if (trigger != null)
        {
            trigger.gameObject.SetActive(false);
        }

        if (playerUIMediator != null)
        {
            playerUIMediator?.HideBuildingPrompt();
        }

        EventBus.BuildingRestored(this);
    }

    public void UpdateVisualFromProgress()
    {
        bool allComplete = true;
        foreach (var cost in buildingData.costs)
        {
            if (GetInvestedAmount(cost.resourceName) < cost.amount)
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
        {
            RestoreImmediate();
        }
        else
        {
            EventBus.BuildingProgressChanged(this);
        }
    }

    public void SyncStartState(bool restored)
    {
        isRestored = restored;
        hasLoadedData = true;
        UpdateVisual(restored);
    }

    public void SetInvestedAmount(string resourceName, int amount)
    {
        if (investedResources.ContainsKey(resourceName))
        {
            investedResources[resourceName] = amount;
        }
        else
        {
            investedResources[resourceName] = amount;
        }
    }

    public string GetInvestedString()
    {
        if (investedResources == null || investedResources.Count == 0)
            return "EMPTY";

        StringBuilder sb = new StringBuilder();
        foreach (var kvp in investedResources)
        {
            sb.Append($"{kvp.Key}={kvp.Value} ");
        }
        return sb.ToString().Trim();
    }

    // ===== TRIGGER METHODS =====
    public void OnPlayerEnter()
    {
        // Игрок вошёл в зону
    }

    public void OnPlayerExit()
    {
        // Игрок вышел из зоны
    }

    // ===== RESTORE LOGIC =====
    public async UniTaskVoid TryRestore()
    {
        if (isRestored)
        {
            playerUIMediator?.ShowNotification($"{GetBuildingName()} уже восстановлено!", 2f);
            return;
        }

        bool anyResourceAdded = false;
        bool anyResourceAvailable = false;
        List<string> missingResources = new List<string>();

        foreach (var cost in buildingData.costs)
        {
            string resourceName = cost.resourceName;
            int required = cost.amount;
            int invested = GetInvestedAmount(resourceName);
            int remaining = required - invested;

            if (remaining <= 0) continue;

            int available = inventory.GetAmount(resourceName);

            if (available > 0)
            {
                anyResourceAvailable = true;
                int toTransfer = Mathf.Min(available, remaining);
                inventory.TrySpend(resourceName, toTransfer);
                investedResources[resourceName] += toTransfer;
                anyResourceAdded = true;
            }
            else if (available == 0 && remaining > 0)
            {
                missingResources.Add(resourceName);
            }
        }

        if (!anyResourceAvailable)
        {
            string missingList = string.Join(", ", missingResources);
            string message = $"Нет ресурсов для восстановления {GetBuildingName()}! Нужно: {missingList}";
            playerUIMediator?.ShowNotification(message, 2.5f);
            return;
        }

        if (!anyResourceAdded)
        {
            playerUIMediator?.ShowNotification($"Все ресурсы для {GetBuildingName()} уже внесены!", 2f);
            return;
        }

        bool allComplete = true;
        foreach (var cost in buildingData.costs)
        {
            if (GetInvestedAmount(cost.resourceName) < cost.amount)
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
        {
            if (playerUIMediator != null)
            {
                playerUIMediator?.UpdateBuildingCost();
            }

            float animationDuration = playerUIMediator != null ? playerUIMediator.GetBuildingPromptAnimationDuration() : 0.5f;

            await UniTask.Delay((int)(animationDuration * 1000) + 200);

            if (playerUIMediator != null)
            {
                playerUIMediator?.UpdateBuildingCostImmediate();
            }

            await UniTask.Delay(500);

            UpdateVisual(true);

            var trigger = GetComponentInChildren<BuildingTrigger>();
            if (trigger != null)
            {
                trigger.gameObject.SetActive(false);
            }

            if (playerUIMediator != null)
            {
                playerUIMediator?.HideBuildingPrompt();
            }

            EventBus.BuildingRestored(this);

            playerUIMediator?.ShowNotification($" {GetBuildingName()} восстановлено!", 2f);
        }
        else
        {
            if (playerUIMediator != null)
            {
                playerUIMediator?.UpdateBuildingCost();
            }
            EventBus.BuildingProgressChanged(this);

            await UniTask.Delay(300);

            playerUIMediator?.ShowNotification($"Внесены ресурсы для {GetBuildingName()}", 1.5f);
        }
    }

    // ===== RESET =====

    /// <summary>
    /// ПОЛНОСТЬЮ СБРАСЫВАЕТ ЗДАНИЕ В ИСХОДНОЕ СОСТОЯНИЕ
    /// </summary>
    public void ResetBuilding()
    {
        // 1. Сбрасываем флаги
        isRestored = false;
        hasLoadedData = false;

        // 2. Очищаем вложенные ресурсы
        if (investedResources != null)
        {
            investedResources.Clear();
        }

        // 3. Инициализируем словарь заново
        if (buildingData != null && buildingData.costs != null)
        {
            foreach (var cost in buildingData.costs)
            {
                if (!investedResources.ContainsKey(cost.resourceName))
                {
                    investedResources[cost.resourceName] = 0;
                }
            }
        }

        // 4. Обновляем визуал
        UpdateVisual(false);

        // 5. Включаем триггер
        var trigger = GetComponentInChildren<BuildingTrigger>();
        if (trigger != null)
        {
            trigger.gameObject.SetActive(true);
        }

        // 6. Обновляем UI
        EventBus.BuildingProgressChanged(this);
    }
}