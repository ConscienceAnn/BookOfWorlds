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

    [Header("Camera References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private CameraZoom cameraZoom;

    public override void InstallBindings()
    {
        Debug.Log("=== GameInstaller: InstallBindings START ===");

        // 1. ÂÍÅÄÐßÅÌ PLAYER INPUT
        Container.Bind<PlayerInput>()
            .FromInstance(playerInput)
            .AsSingle();

        // 2. ÂÍÅÄÐßÅÌ PLAYER INPUT HANDLER
        Container.Bind<PlayerInputHandler>()
            .FromInstance(playerInputHandler)
            .AsSingle()
            .NonLazy();

        // 3. ÂÍÅÄÐßÅÌ ÊÀÌÅÐÓ
        Container.Bind<Camera>()
            .FromInstance(mainCamera)
            .AsSingle();

        Container.Bind<CinemachineVirtualCamera>()
            .FromInstance(virtualCamera)
            .AsSingle();

        // 4. ÂÍÅÄÐßÅÌ ÈÃÐÎÊÀ
        Container.Bind<PlayerController>()
            .FromInstance(player)
            .AsSingle();

        // 5. ÂÍÅÄÐßÅÌ ÊÎÌÏÎÍÅÍÒÛ ÊÀÌÅÐÛ
        Container.Bind<CameraFollow>()
            .FromInstance(cameraFollow)
            .AsSingle()
            .NonLazy();

        Container.Bind<CameraZoom>()
            .FromInstance(cameraZoom)
            .AsSingle()
            .NonLazy();

        Debug.Log("=== GameInstaller: InstallBindings END ===");
    }
}