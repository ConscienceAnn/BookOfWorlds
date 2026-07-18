using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class BuildingController : MonoBehaviour
{
    [Header("Building Data")]
    [SerializeField] private BuildingDataSO buildingData;

    [Header("Visuals")]
    [SerializeField] private GameObject ruinedVisual;
    [SerializeField] private GameObject restoredVisual;
    [SerializeField] private GameObject blockedCollider;

    [Inject] private PlayerUI playerUI;

    [Inject] private BuildingService buildingService;
    [Inject] private IPlayerInventory inventory;

    private bool isRestored = false;
    private bool isPlayerNear = false;
    private Dictionary<string, int> investedResources = new Dictionary<string, int>();

    private void Start()
    {
        UpdateVisual(false);
        foreach (var cost in buildingData.costs)
        {
            investedResources[cost.resourceName] = 0;
        }

        if (playerUI == null)
        {
           
                Debug.LogWarning($" PlayerUI не найден на сцене! Подсказки работать не будут.");
            
        }
    }

    private void UpdateVisual(bool restored)
    {
        isRestored = restored;
        if (ruinedVisual != null) ruinedVisual.SetActive(!restored);
        if (restoredVisual != null) restoredVisual.SetActive(restored);
        if (blockedCollider != null) blockedCollider.SetActive(!restored);
    }

    public ResourceCost[] GetCosts() => buildingData?.costs;
    public int GetCurrentAmount(string resourceName) => inventory?.GetAmount(resourceName) ?? 0;
    public bool IsRestored() => isRestored;

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

    public void OnPlayerEnter()
    {
        if (isRestored) return;
        isPlayerNear = true;

        if (playerUI != null)
        {
            playerUI.ShowBuildingPrompt(this);
            Debug.Log($" Показана подсказка для {buildingData.buildingName}");
        }
    }

    public void OnPlayerExit()
    {
        isPlayerNear = false;

        if (playerUI != null)
        {
            playerUI.HideBuildingPrompt();
        }
    }

    public async UniTaskVoid TryRestore()
    {
        if (isRestored)
        {
            Debug.Log($" Здание {buildingData.buildingName} уже восстановлено!");
            return;
        }

        bool anyResourceAdded = false;

        foreach (var cost in buildingData.costs)
        {
            string resourceName = cost.resourceName;
            int required = cost.amount;
            int invested = GetInvestedAmount(resourceName);
            int remaining = required - invested;

            if (remaining <= 0) continue;

            int available = inventory.GetAmount(resourceName);
            int toTransfer = Mathf.Min(available, remaining);

            if (toTransfer > 0)
            {
                inventory.TrySpend(resourceName, toTransfer);
                investedResources[resourceName] += toTransfer;
                anyResourceAdded = true;

                Debug.Log($" Передано {toTransfer} {resourceName} (всего: {investedResources[resourceName]}/{required})");
            }
        }

        if (!anyResourceAdded)
        {
            Debug.Log($" У вас нет ресурсов для передачи!");
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
            Debug.Log($" Все ресурсы собраны! Восстанавливаем {buildingData.buildingName}...");
            await UniTask.Delay(500);
            UpdateVisual(true);
            Debug.Log($" {buildingData.buildingName} восстановлен!");

            if (playerUI != null)
            {
                playerUI.HideBuildingPrompt();
            }

            var trigger = GetComponentInChildren<BuildingTrigger>();
            if (trigger != null)
            {
                trigger.gameObject.SetActive(false);
            }
        }
        else
        {
            if (playerUI != null)
            {
                playerUI.UpdateCostText();
            }

            Debug.Log($" Прогресс: {GetProgressString()}");
        }
    }

    private string GetProgressString()
    {
        string result = "";
        foreach (var cost in buildingData.costs)
        {
            int invested = GetInvestedAmount(cost.resourceName);
            result += $"{cost.resourceName}: {invested}/{cost.amount} ";
        }
        return result;
    }

    public void Interact()
    {
        if (!isRestored && isPlayerNear)
        {
            TryRestore().Forget();
        }
    }
}