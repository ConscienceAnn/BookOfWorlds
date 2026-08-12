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

    [Header("Camera References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private CameraZoom cameraZoom;

    [Header("UI References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private LevelProgress levelProgress;
    [SerializeField] private PlayerUI playerUI;
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

    public override void InstallBindings()
    {
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

        // ===== UI =====
        Container.Bind<UIManager>()
            .FromInstance(uiManager)
            .AsSingle();

        Container.Bind<PanelManager>()
            .FromInstance(panelManager)
            .AsSingle();

        Container.Bind<HUDController>()
            .FromInstance(hudController)
            .AsSingle();

        // ===== 3. INVENTORY =====
        Container.Bind<IPlayerInventory>()
            .To<PlayerInventory>()
            .FromInstance(playerInventory)
            .AsSingle();

        // ===== 4. RESOURCES DATA =====
        Container.Bind<ResourceDataSO[]>()
            .FromInstance(allResources)
            .AsSingle();

        // ===== 5. BUILDINGS DATA =====
        Container.Bind<BuildingDataSO[]>()
            .FromInstance(allBuildings)
            .AsSingle();

        // ===== 6. SERVICES =====
        Container.Bind<SellService>()
            .AsSingle()
            .NonLazy();

        Container.Bind<BuildingService>()
            .AsSingle()
            .NonLazy();

        Container.Bind<PauseService>()
            .AsSingle()
            .NonLazy();

        Container.Bind<PlayerUI>()
            .FromInstance(playerUI)
            .AsSingle();

        Container.Bind<LevelProgress>()
           .FromInstance(levelProgress)
           .AsSingle();

        Container.Bind<GameSaveController>()
           .FromInstance(gameSaveController)
           .AsSingle()
           .NonLazy();

        Container.Bind<ResourceFactory>()
            .FromInstance(resourceFactory)
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

        if (levelManager != null)
        {
            Container.Bind<LevelManager>().FromInstance(levelManager).AsSingle();
        }
        else
        {
            Debug.LogWarning("LevelManager не назначен в GameInstaller!");
        }
    }
}