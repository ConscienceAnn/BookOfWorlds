using UnityEngine;
using Cinemachine;
using Zenject;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float cameraHeight = 6f;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private float rotationSpeed = 5f;

    [Inject] private CinemachineVirtualCamera virtualCamera;
    [Inject] private PlayerController player; 

    private CinemachineTransposer transposer;
    private bool isInitialized = false;

    void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("CameraFollow: Virtual Camera is null!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("CameraFollow: Player is null!");
            return;
        }

        //  Камера следит за Player
        virtualCamera.Follow = player.transform;
        virtualCamera.LookAt = player.transform;

        Debug.Log($"CameraFollow: Follow set to {virtualCamera.Follow?.name ?? "NULL"}");

        
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer == null)
        {
          

            transposer = virtualCamera.AddCinemachineComponent<CinemachineTransposer>();
        }

        if (transposer != null)
        {
         
            transposer.m_BindingMode = CinemachineTransposer.BindingMode.LockToTarget;

           
            transposer.m_FollowOffset = new Vector3(0, cameraHeight, -cameraDistance);

            
            transposer.m_XDamping = followSpeed;
            transposer.m_YDamping = followSpeed;
            transposer.m_ZDamping = followSpeed;

          
            transposer.m_AngularDampingMode = CinemachineTransposer.AngularDampingMode.Euler;
            transposer.m_PitchDamping = rotationSpeed;
            transposer.m_YawDamping = rotationSpeed;
            transposer.m_RollDamping = rotationSpeed;
        }

        isInitialized = true;
    }

    void LateUpdate()
    {
        if (!isInitialized) return;

        
        if (virtualCamera.Follow == null || virtualCamera.LookAt == null)
        {
            virtualCamera.Follow = player?.transform;
            virtualCamera.LookAt = player?.transform;
        }
    }
}