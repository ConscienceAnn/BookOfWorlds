using UnityEngine;
using Zenject;
using Cinemachine;
using UnityEngine.InputSystem;

public class GameInstaller : MonoInstaller
{
    [Header("Player References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Camera References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private CameraZoom cameraZoom;

    [Header("UI References")]              
    [SerializeField] private UIManager uiManager;  

    [Header("Resources & Data")]
    [SerializeField] private ResourceDataSO[] allResources;
    [SerializeField] private BuildingDataSO[] allBuildings;

    public override void InstallBindings()
    {
        Debug.Log("=== GameInstaller: InstallBindings START ===");

        // ===== 1. PLAYER & INPUT =====
        Container.Bind<PlayerInput>()
            .FromInstance(playerInput)
            .AsSingle();

        Container.Bind<PlayerInputHandler>()
            .FromInstance(playerInputHandler)
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


        // =====  UI (мнбне) =====
        Container.Bind<UIManager>()
            .FromInstance(uiManager)
            .AsSingle();

        // ===== 3. INVENTORY (мнбне) =====
        Container.Bind<IPlayerInventory>()
            .To<PlayerInventory>()
            .FromInstance(playerInventory)
            .AsSingle();

        // ===== 4. RESOURCES DATA (мнбне) =====
        Container.Bind<ResourceDataSO[]>()
            .FromInstance(allResources)
            .AsSingle();

        // ===== 5. BUILDINGS DATA (мнбне) =====
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

        Debug.Log("=== GameInstaller: InstallBindings END ===");
    }
}