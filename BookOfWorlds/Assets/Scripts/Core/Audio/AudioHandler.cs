using UnityEngine;
using Zenject;

public class AudioHandler : MonoBehaviour
{
    [Inject] private AudioHelper _audioHelper;
    [Inject(Optional = true)] private GameSaveController gameSaveController;

    private void Awake()
    {
        // Подписка на события
        EventBus.OnResourceCollected += OnResourceCollected;
        EventBus.OnBuildingRestored += OnBuildingRestored;
        EventBus.OnCoinsCollected += OnCoinsCollected;
        EventBus.OnError += OnError;
        EventBus.OnSoundRequest += OnSoundRequest;
    }

    private void OnDestroy()
    {
        EventBus.OnResourceCollected -= OnResourceCollected;
        EventBus.OnBuildingRestored -= OnBuildingRestored;
        EventBus.OnCoinsCollected -= OnCoinsCollected;
        EventBus.OnError -= OnError;
        EventBus.OnSoundRequest -= OnSoundRequest;
    }

    // ===== ОБРАБОТЧИКИ =====

    private void OnResourceCollected(string resourceName, int amount)
    {
       
        if (gameSaveController != null && gameSaveController.IsLoadingGame)
        {
            return;
        }

        string soundId = GetCollectSound(resourceName);
        _audioHelper.PlaySound(soundId);
    }

    private void OnBuildingRestored(BuildingController building)
    {
       
        if (gameSaveController != null && gameSaveController.IsLoadingGame)
        {
            return;
        }

        _audioHelper.PlaySound("building_restore");
    }

    private void OnCoinsCollected()
    {
       
        if (gameSaveController != null && gameSaveController.IsLoadingGame)
        {
            return;
        }

        _audioHelper.PlaySound("sell");
    }

    private void OnError(string message)
    {
      
        if (gameSaveController != null && gameSaveController.IsLoadingGame)
        {
            return;
        }

        _audioHelper.PlaySound("error");
    }

    private void OnSoundRequest(string soundId)
    {
        
        if (gameSaveController != null && gameSaveController.IsLoadingGame)
        {
            return;
        }

        _audioHelper.PlaySound(soundId);
    }


    private string GetCollectSound(string resourceName)
    {
        switch (resourceName)
        {
            case "Дерево":
            case "Wood":
                return "collect_wood";
            case "Камень":
            case "Stone":
                return "collect_stone";
            case "Молоко":
            case "Milk":
                return "collect_milk";
            case "Шерсть":
            case "Wool":
                return "collect_wool";
            default:
                return "collect_wood";
        }
    }
}