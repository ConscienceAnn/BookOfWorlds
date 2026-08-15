using UnityEngine;
using Zenject;

public class AudioHandler : MonoBehaviour
{
    [Inject] private AudioHelper _audioHelper;
    [Inject(Optional = true)] private GameSaveController gameSaveController;

    private void Awake()
    {
        // Ïîäïèñêà íà ñîáûòèÿ
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

    // ===== ÎÁĞÀÁÎÒ×ÈÊÈ =====

    private void OnResourceCollected(string resourceName, int amount)
    {
        // ===== ÏĞÎÂÅĞÊÀ: ÍÅ ÈÃĞÀÅÌ ÇÂÓÊ ÏĞÈ ÇÀÃĞÓÇÊÅ =====
        if (gameSaveController != null && gameSaveController.IsLoadingGame)
        {
            return;
        }

        string soundId = GetCollectSound(resourceName);
        _audioHelper.PlaySound(soundId);
    }

    private void OnBuildingRestored(BuildingController building)
    {
        // ===== ÏĞÎÂÅĞÊÀ: ÍÅ ÈÃĞÀÅÌ ÇÂÓÊ ÏĞÈ ÇÀÃĞÓÇÊÅ =====
        if (gameSaveController != null && gameSaveController.IsLoadingGame)
        {
            return;
        }

        _audioHelper.PlaySound("building_restore");
    }

    private void OnCoinsCollected()
    {
        // ===== ÏĞÎÂÅĞÊÀ: ÍÅ ÈÃĞÀÅÌ ÇÂÓÊ ÏĞÈ ÇÀÃĞÓÇÊÅ =====
        if (gameSaveController != null && gameSaveController.IsLoadingGame)
        {
            return;
        }

        _audioHelper.PlaySound("sell");
    }

    private void OnError(string message)
    {
        // ===== ÏĞÎÂÅĞÊÀ: ÍÅ ÈÃĞÀÅÌ ÇÂÓÊ ÏĞÈ ÇÀÃĞÓÇÊÅ =====
        if (gameSaveController != null && gameSaveController.IsLoadingGame)
        {
            return;
        }

        _audioHelper.PlaySound("error");
    }

    private void OnSoundRequest(string soundId)
    {
        // ===== ÏĞÎÂÅĞÊÀ: ÍÅ ÈÃĞÀÅÌ ÇÂÓÊ ÏĞÈ ÇÀÃĞÓÇÊÅ =====
        if (gameSaveController != null && gameSaveController.IsLoadingGame)
        {
            return;
        }

        _audioHelper.PlaySound(soundId);
    }

    // ===== ËÎÃÈÊÀ ÂÛÁÎĞÀ ÇÂÓÊÀ =====

    private string GetCollectSound(string resourceName)
    {
        switch (resourceName)
        {
            case "Äåğåâî":
            case "Wood":
                return "collect_wood";
            case "Êàìåíü":
            case "Stone":
                return "collect_stone";
            case "Ìîëîêî":
            case "Milk":
                return "collect_milk";
            case "Øåğñòü":
            case "Wool":
                return "collect_wool";
            default:
                return "collect_wood";
        }
    }
}