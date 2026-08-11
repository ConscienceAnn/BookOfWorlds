using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

public class BuildingController : MonoBehaviour, IInteractable
{
    [Header("Building Data")]
    [SerializeField] private BuildingDataSO buildingData; //  buildingId внутри

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

    // ===== PUBLIC METHODS =====
    public string GetBuildingId() => buildingData?.buildingId ?? "unknown";
    public string GetBuildingName() => buildingData?.buildingName ?? name;
    public bool IsRestored()
    {
        bool result = isRestored;
        Debug.Log($" {GetBuildingName()}.IsRestored() = {result}, investedResources={GetInvestedString()}");
        return result;
    }
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
        if (playerUI != null)
        {
            playerUI.UpdateBuildingCost();
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
        Debug.Log($"BuildingController {GetBuildingName()} Awake - словарь инициализирован");
    }

    private void Start()
    {
        if (!hasLoadedData)
        {
            UpdateVisual(false);
        }

        Debug.Log($"BuildingController {GetBuildingName()} инициализирован, restored={isRestored}, hasLoadedData={hasLoadedData}");
    }

    // ===== VISUAL =====
    private void UpdateVisual(bool restored)
    {
        Debug.Log($"  - UpdateVisual({restored}) called for {GetBuildingName()}");

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
        Debug.Log($"Игрок вошёл в зону {GetBuildingName()}");
    }

    public void OnPlayerExit()
    {
        Debug.Log($"Игрок вышел из зоны {GetBuildingName()}");
    }

    // ===== RESTORE LOGIC =====
    public async UniTaskVoid TryRestore()
    {
        Debug.Log($"========== [TryRestore] {GetBuildingName()} ==========");
        Debug.Log($"  - isRestored: {isRestored}");
        Debug.Log($"  - investedResources BEFORE: {GetInvestedString()}");

        if (isRestored)
        {
            playerUI?.ShowNotification($"{GetBuildingName()} уже восстановлено!", 2f);
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

        if (!anyResourceAvailable)
        {
            string missingList = string.Join(", ", missingResources);
            string message = $"Нет ресурсов для восстановления {GetBuildingName()}! Нужно: {missingList}";
            playerUI?.ShowNotification(message, 2.5f);
            Debug.Log($"  - {message}");
            return;
        }

        if (!anyResourceAdded)
        {
            playerUI?.ShowNotification($"Все ресурсы для {GetBuildingName()} уже внесены!", 2f);
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

            playerUI?.ShowNotification($" {GetBuildingName()} восстановлено!", 2f);
        }
        else
        {
            if (playerUI != null)
            {
                playerUI.UpdateBuildingCost();
            }
            EventBus.BuildingProgressChanged(this);

            await UniTask.Delay(300);

            playerUI?.ShowNotification($"Внесены ресурсы для {GetBuildingName()}", 1.5f);
        }

        Debug.Log($"  - investedResources FINAL: {GetInvestedString()}");
        Debug.Log($"========== [TryRestore] END ==========");
    }

    // ===== RESET =====

    /// <summary>
    /// ПОЛНОСТЬЮ СБРАСЫВАЕТ ЗДАНИЕ В ИСХОДНОЕ СОСТОЯНИЕ
    /// </summary>
    public void ResetBuilding()
    {
        Debug.Log($" ResetBuilding() для {GetBuildingName()}");

        // 1. Сбрасываем флаги
        isRestored = false;
        hasLoadedData = false;

        // 2. Очищаем вложенные ресурсы
        if (investedResources != null)
        {
            investedResources.Clear();
            Debug.Log($"  - investedResources очищен");
        }

        // 3. Инициализируем словарь заново
        if (buildingData != null && buildingData.costs != null)
        {
            foreach (var cost in buildingData.costs)
            {
                if (!investedResources.ContainsKey(cost.resourceName))
                {
                    investedResources[cost.resourceName] = 0;
                    Debug.Log($"  - {cost.resourceName}: 0");
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
            Debug.Log($"  - Trigger включён");
        }

        // 6. Обновляем UI
        EventBus.BuildingProgressChanged(this);

        Debug.Log($"ResetBuilding() завершён для {GetBuildingName()}, isRestored={isRestored}");
    }

}