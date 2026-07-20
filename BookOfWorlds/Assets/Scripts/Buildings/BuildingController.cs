using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

public class BuildingController : MonoBehaviour
{
    [Header("Building Data")]
    [SerializeField] private BuildingDataSO buildingData;

    [Header("Visuals")]
    [SerializeField] private GameObject ruinedVisual;
    [SerializeField] private GameObject restoredVisual;
    [SerializeField] private GameObject blockedCollider;

    [Inject] private IPlayerInventory inventory;
    [Inject] private LevelProgress levelProgress;
    [Inject] private GameSaveController gameSaveController;
    [Inject] private PlayerUI playerUI;

    private bool isRestored = false;
    private bool isPlayerNear = false;
    private bool hasLoadedData = false;
    private Dictionary<string, int> investedResources = new Dictionary<string, int>();

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
        if (blockedCollider != null) blockedCollider.SetActive(!restored);
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
                playerUI.UpdateCostTextForBuilding(this);
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

    public void OnPlayerEnter()
    {
        if (isRestored) return;
        isPlayerNear = true;
    }

    public void OnPlayerExit()
    {
        isPlayerNear = false;
    }

    public async UniTaskVoid TryRestore()
    {
        Debug.Log($"========== [TryRestore] {buildingData.buildingName} ==========");
        Debug.Log($"  - isRestored: {isRestored}");
        Debug.Log($"  - investedResources BEFORE: {GetInvestedString()}");

        if (isRestored) return;

        bool anyResourceAdded = false;

        foreach (var cost in buildingData.costs)
        {
            string resourceName = cost.resourceName;
            int required = cost.amount;
            int invested = GetInvestedAmount(resourceName);
            int remaining = required - invested;

            Debug.Log($"  - {resourceName}: invested={invested}, required={required}, remaining={remaining}");

            if (remaining <= 0) continue;

            int available = inventory.GetAmount(resourceName);
            int toTransfer = Mathf.Min(available, remaining);

            Debug.Log($"  - {resourceName}: available={available}, toTransfer={toTransfer}");

            if (toTransfer > 0)
            {
                inventory.TrySpend(resourceName, toTransfer);
                investedResources[resourceName] += toTransfer;
                anyResourceAdded = true;
                Debug.Log($"  - {resourceName}: transferred {toTransfer}, now {investedResources[resourceName]}");
            }
        }

        Debug.Log($"  - anyResourceAdded: {anyResourceAdded}");
        Debug.Log($"  - investedResources AFTER transfers: {GetInvestedString()}");

        if (!anyResourceAdded) return;

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
        }
        else
        {
            EventBus.BuildingProgressChanged(this);
        }

        Debug.Log($"  - investedResources FINAL: {GetInvestedString()}");
        Debug.Log($"========== [TryRestore] END ==========");
    }

    public void Interact()
    {
        if (!isRestored && isPlayerNear)
        {
            TryRestore().Forget();
        }
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