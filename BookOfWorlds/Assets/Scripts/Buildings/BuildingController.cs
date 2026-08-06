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
    [Inject] private PlayerUI playerUI;

    private bool isRestored = false;
    private bool hasLoadedData = false;
    private Dictionary<string, int> investedResources = new Dictionary<string, int>();

    public string BuildingId { get; set; }
    public string BuildingName { get; set; }

  
    public void UpdateBuildingPrompt()
    {
        // Вызываем существующий метод обновления UI
        if (playerUI != null)
        {
            playerUI.UpdateBuildingCost();
        }
    }

    // ===== РЕАЛИЗАЦИЯ IInteractable =====
    public void Interact()
    {
        TryRestore().Forget();
    }
    // ===== КОНЕЦ РЕАЛИЗАЦИИ =====

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
        Debug.Log($"BuildingController {buildingData?.buildingName} Awake - словарь инициализирован");
    }

    private void Start()
    {
        if (!hasLoadedData)
        {
            UpdateVisual(false);
        }

        if (gameSaveController != null)
        {
            gameSaveController.OnBuildingReady();
        }

        Debug.Log($"BuildingController {buildingData.buildingName} инициализирован, restored={isRestored}, hasLoadedData={hasLoadedData}");
    }

    private void UpdateVisual(bool restored)
    {
        Debug.Log($"  - UpdateVisual({restored}) called for {buildingData.buildingName}");

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

    public void SyncStateFromSave(bool restored, Dictionary<string, int> savedInvested)
    {
        hasLoadedData = true;

        if (savedInvested != null)
        {
            foreach (var kvp in savedInvested)
            {
                investedResources[kvp.Key] = kvp.Value;
                Debug.Log($"  - Applied: {kvp.Key} = {kvp.Value}");
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
            if (playerUI != null)
            {
                playerUI.UpdateBuildingCostImmediate();
            }
            EventBus.BuildingProgressChanged(this);
        }
    }

    public int GetInvestedAmount(string resourceName)
    {
        return investedResources.ContainsKey(resourceName) ? investedResources[resourceName] : 0;
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

    public ResourceCost[] GetCosts() => buildingData?.costs;
    public bool IsRestored() => isRestored;
    public string GetBuildingName() => buildingData?.buildingName ?? name;
    public int GetRequiredAmount(string resourceName)
    {
        foreach (var cost in buildingData.costs)
        {
            if (cost.resourceName == resourceName)
                return cost.amount;
        }
        return 0;
    }

    // ===== МЕТОДЫ ДЛЯ BuildingTrigger =====
    public void OnPlayerEnter()
    {
        // Можно использовать для логики входа в зону
        Debug.Log($"Игрок вошёл в зону {buildingData?.buildingName}");
    }

    public void OnPlayerExit()
    {
        // Можно использовать для логики выхода из зоны
        Debug.Log($"Игрок вышел из зоны {buildingData?.buildingName}");
    }
    // ===== КОНЕЦ =====

    public async UniTaskVoid TryRestore()
    {
        Debug.Log($"========== [TryRestore] {buildingData.buildingName} ==========");
        Debug.Log($"  - isRestored: {isRestored}");
        Debug.Log($"  - investedResources BEFORE: {GetInvestedString()}");

        if (isRestored)
        {
            playerUI?.ShowNotification($"{buildingData.buildingName} уже восстановлено!", 2f);
            Debug.Log($"  - Здание уже восстановлено");
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

            Debug.Log($"  - {resourceName}: invested={invested}, required={required}, remaining={remaining}");

            if (remaining <= 0) continue;

            int available = inventory.GetAmount(resourceName);
            Debug.Log($"  - {resourceName}: available={available}, toTransfer={Mathf.Min(available, remaining)}");

            if (available > 0)
            {
                anyResourceAvailable = true;
                int toTransfer = Mathf.Min(available, remaining);
                inventory.TrySpend(resourceName, toTransfer);
                investedResources[resourceName] += toTransfer;
                anyResourceAdded = true;
                Debug.Log($"  - {resourceName}: transferred {toTransfer}, now {investedResources[resourceName]}");
            }
            else if (available == 0 && remaining > 0)
            {
                missingResources.Add(resourceName);
            }
        }

        Debug.Log($"  - anyResourceAdded: {anyResourceAdded}");
        Debug.Log($"  - anyResourceAvailable: {anyResourceAvailable}");
        Debug.Log($"  - investedResources AFTER transfers: {GetInvestedString()}");

        //  ЕСЛИ НЕТ РЕСУРСОВ — ПОКАЗЫВАЕМ УВЕДОМЛЕНИЕ
        if (!anyResourceAvailable)
        {
            string missingList = string.Join(", ", missingResources);
            string message = $"Нет ресурсов для восстановления {buildingData.buildingName}! Нужно: {missingList}";
            playerUI?.ShowNotification(message, 2.5f);
            Debug.Log($"  - {message}");
            return;
        }

        if (!anyResourceAdded)
        {
            playerUI?.ShowNotification($"Все ресурсы для {buildingData.buildingName} уже внесены!", 2f);
            Debug.Log($"  - Все ресурсы уже внесены");
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

        Debug.Log($"  - allComplete: {allComplete}");

        if (allComplete)
        {
            if (playerUI != null)
            {
                playerUI.UpdateBuildingCost();
            }

            float animationDuration = playerUI != null ? playerUI.GetBuildingPromptAnimationDuration() : 0.5f;

            await UniTask.Delay((int)(animationDuration * 1000) + 200);

            if (playerUI != null)
            {
                playerUI.UpdateBuildingCostImmediate();
            }

            await UniTask.Delay(500);

            UpdateVisual(true);

            var trigger = GetComponentInChildren<BuildingTrigger>();
            if (trigger != null)
            {
                trigger.gameObject.SetActive(false);
            }

            if (playerUI != null)
            {
                playerUI.HideBuildingPrompt();
            }

            EventBus.BuildingRestored(this);

            playerUI?.ShowNotification($" {buildingData.buildingName} восстановлено!", 2f);
        }
        else
        {
            if (playerUI != null)
            {
                playerUI.UpdateBuildingCost();
            }
            EventBus.BuildingProgressChanged(this);

            await UniTask.Delay(300); 

            playerUI?.ShowNotification($"Внесены ресурсы для {buildingData.buildingName}", 1.5f);
        }

        Debug.Log($"  - investedResources FINAL: {GetInvestedString()}");
        Debug.Log($"========== [TryRestore] END ==========");
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

    public void RestoreImmediate()
    {
        UpdateVisual(true);

        var trigger = GetComponentInChildren<BuildingTrigger>();
        if (trigger != null)
        {
            trigger.gameObject.SetActive(false);
        }

        if (playerUI != null)
        {
            playerUI.HideBuildingPrompt();
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
}