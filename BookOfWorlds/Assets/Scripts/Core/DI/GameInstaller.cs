using UnityEngine;
using Zenject;
using Cinemachine;
using UnityEngine.InputSystem;

public class GameInstaller : MonoInstaller
{
    [Header("Player References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerInputHandlerMy playerInputHandlerMy;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerCollector playerCollector;

    [Header("Camera References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private CameraZoom cameraZoom;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineVirtualCamera[] virtualCameras;

    [Header("UI References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private LevelProgress levelProgress;
    [SerializeField] private PlayerUIMediator playerUIMediator;
    [SerializeField] private BuildingPromptController buildingPromptController;
    [SerializeField] private NotificationController notificationController;
    [SerializeField] private PanelManager panelManager;
    [SerializeField] private HUDController hudController;

    [Header("Resources & Data")]
    [SerializeField] private ResourceDataSO[] allResources;
    [SerializeField] private BuildingDataSO[] allBuildings;

    [Header("Resource System")]
    [SerializeField] private ResourceFactory resourceFactory;

    [Header("Game Save")]
    [SerializeField] private GameSaveController gameSaveController;

    [Header("Services")]
    [SerializeField] private ParticleFactory particleFactory;
    [SerializeField] private ResourceFlyAnimation flyAnimation;
    [SerializeField] private ProgressBarFactory progressBarFactory;

    [Header("Levels")]
    [SerializeField] private LevelManager levelManager;

    [Header("Debug")]
    [SerializeField] private DebugHotkeyHandler debugHotkeyHandler;

    [Header("Upgrades")]
    [SerializeField] private UpgradeManager upgradeManager;

    public override void InstallBindings()
    {
        Debug.Log("=== GameInstaller: InstallBindings START ===");

        // ===== 1. PLAYER & INPUT =====
        Container.Bind<PlayerInput>()
            .FromInstance(playerInput)
            .AsSingle();

        Container.Bind<PlayerInputHandlerMy>()
            .FromInstance(playerInputHandlerMy)
            .AsSingle()
            .NonLazy();

        Container.Bind<PlayerController>()
            .FromInstance(player)
            .AsSingle();

        Container.Bind<PlayerCollector>()
            .FromInstance(playerCollector)
            .AsSingle();

        // ===== 2. CAMERA =====
        Container.Bind<Camera>()
            .FromInstance(mainCamera)
            .AsSingle();

        Container.Bind<CinemachineVirtualCamera>()
            .FromInstance(virtualCamera)
            .AsSingle();

        Container.Bind<CameraFollow>()
            .FromInstance(cameraFollow)
            .AsSingle()
            .NonLazy();

        Container.Bind<CameraZoom>()
            .FromInstance(cameraZoom)
            .AsSingle()
            .NonLazy();

        Container.Bind<CinemachineBrain>()
            .FromInstance(cinemachineBrain)
            .AsSingle();

        Container.Bind<CinemachineVirtualCamera[]>()
            .FromInstance(virtualCameras)
            .AsSingle();

        // ===== 3. UI =====
        Container.Bind<UIManager>()
            .FromInstance(uiManager)
            .AsSingle();

        Container.Bind<PanelManager>()
            .FromInstance(panelManager)
            .AsSingle();

        Container.Bind<HUDController>()
            .FromInstance(hudController)
            .AsSingle();

        // ===== 4. UI PROMPTS =====
        Container.Bind<BuildingPromptController>()
            .FromInstance(buildingPromptController)
            .AsSingle();

        Container.Bind<NotificationController>()
            .FromInstance(notificationController)
            .AsSingle();

        Container.Bind<PlayerUIMediator>()
            .FromInstance(playerUIMediator)
            .AsSingle();

        // ===== 5. INVENTORY =====
        Container.Bind<IPlayerInventory>()
            .To<PlayerInventory>()
            .FromInstance(playerInventory)
            .AsSingle();

        // ===== 6. RESOURCES DATA =====
        Container.Bind<ResourceDataSO[]>()
            .FromInstance(allResources)
            .AsSingle();

        // ===== 7. BUILDINGS DATA =====
        Container.Bind<BuildingDataSO[]>()
            .FromInstance(allBuildings)
            .AsSingle();

        // ===== UPGRADES =====
        Container.Bind<UpgradeManager>()
            .FromInstance(upgradeManager)
            .AsSingle()
            .NonLazy();


        // ===== 8. SERVICES =====
        Container.Bind<SellService>()
            .AsSingle()
            .NonLazy();

        Container.Bind<BuildingService>()
            .AsSingle()
            .NonLazy();

        Container.Bind<PauseService>()
            .AsSingle()
            .NonLazy();

        // ===== 9. RESOURCE SYSTEM =====
        Container.Bind<ResourceFactory>()
            .FromInstance(resourceFactory)
            .AsSingle();

        Container.Bind<ResourceBehaviourFactory>()  
            .AsSingle();

        Container.Bind<ParticleFactory>()
            .FromInstance(particleFactory)
            .AsSingle();

        Container.Bind<ResourceFlyAnimation>()
            .FromInstance(flyAnimation)
            .AsSingle();

        Container.Bind<ProgressBarFactory>()
            .FromInstance(progressBarFactory)
            .AsSingle();

        // ===== 10. LEVEL & SAVE =====
        Container.Bind<LevelProgress>()
            .FromInstance(levelProgress)
            .AsSingle();

        Container.Bind<LevelManager>()
            .FromInstance(levelManager)
            .AsSingle();

        Container.Bind<GameSaveController>()
            .FromInstance(gameSaveController)
            .AsSingle()
            .NonLazy();

       
        if (debugHotkeyHandler != null)
        {
            Container.Bind<DebugHotkeyHandler>()
                .FromInstance(debugHotkeyHandler)
                .AsSingle()
                .NonLazy();
            Debug.Log("DebugHotkeyHandler зарегистрирован в Zenject");
        }

        Debug.Log("=== GameInstaller: InstallBindings END ===");
    }
}