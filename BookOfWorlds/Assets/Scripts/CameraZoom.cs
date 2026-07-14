using UnityEngine;
using Cinemachine;
using Zenject;
using Cysharp.Threading.Tasks;

public class CameraZoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 15f;
    [SerializeField] private float zoomSpeed = 0.5f;
    [SerializeField] private float smoothingSpeed = 0.3f;
    [SerializeField] private float defaultZoomDistance = 10f;

    [Header("Height Settings")]
    [SerializeField] private float minHeight = 2f;      // Минимальная высота (приближение)
    [SerializeField] private float maxHeight = 6f;      // Максимальная высота (отдаление)
    [SerializeField] private float defaultHeight = 3f;  // Стандартная высота

    [Inject] private CinemachineVirtualCamera virtualCamera;
    [Inject] private PlayerInputHandler inputHandler;

    private CinemachineTransposer transposer;
    private float currentZoomDistance;
    private float targetZoomDistance;
    private float currentHeight;
    private float targetHeight;
    private bool isZooming = false;

    void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera is null in CameraZoom!");
            return;
        }

        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

        if (transposer == null)
        {
            Debug.Log("CameraZoom: Transposer not found, creating...");

            transposer = virtualCamera.AddCinemachineComponent<CinemachineTransposer>();
            transposer.m_BindingMode = CinemachineTransposer.BindingMode.LockToTarget;
            transposer.m_FollowOffset = new Vector3(0, defaultHeight, -defaultZoomDistance);
            transposer.m_XDamping = 8f;
            transposer.m_YDamping = 8f;
            transposer.m_ZDamping = 8f;
        }

        if (transposer != null)
        {
            // Сохраняем текущие значения
            currentZoomDistance = Mathf.Abs(transposer.m_FollowOffset.z);
            targetZoomDistance = currentZoomDistance;
            defaultZoomDistance = currentZoomDistance;

            currentHeight = transposer.m_FollowOffset.y;
            targetHeight = currentHeight;
            defaultHeight = currentHeight;

            Debug.Log($"CameraZoom: Initialized with distance {currentZoomDistance}, height {currentHeight}");
        }

        if (inputHandler != null)
        {
            inputHandler.OnZoomInput += HandleZoom;
            inputHandler.OnResetZoomInput += HandleResetZoom;
        }
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
        {
            inputHandler.OnZoomInput -= HandleZoom;
            inputHandler.OnResetZoomInput -= HandleResetZoom;
        }
    }

    private void HandleZoom(float scrollValue)
    {
        if (transposer == null) return;

        if (Mathf.Abs(scrollValue) > 0.01f)
        {
            //  Изменяем дистанцию
            targetZoomDistance -= scrollValue * zoomSpeed;
            targetZoomDistance = Mathf.Clamp(targetZoomDistance, minZoom, maxZoom);

            // Изменяем высоту пропорционально дистанции
            // Чем дальше камера, тем выше она поднимается
            float zoomProgress = (targetZoomDistance - minZoom) / (maxZoom - minZoom); // 0..1
            targetHeight = Mathf.Lerp(minHeight, maxHeight, zoomProgress);

            Debug.Log($"Zoom target: distance={targetZoomDistance}, height={targetHeight} (scroll: {scrollValue})");

            if (!isZooming)
            {
                SmoothZoomAsync().Forget();
            }
        }
    }

    private void HandleResetZoom()
    {
        ResetZoom();
    }

    private async UniTaskVoid SmoothZoomAsync()
    {
        if (transposer == null) return;

        isZooming = true;
        float elapsedTime = 0f;
        float startDistance = currentZoomDistance;
        float startHeight = currentHeight;

        while (elapsedTime < smoothingSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / smoothingSpeed;
            float smoothedT = Mathf.SmoothStep(0f, 1f, t);

            //  Плавно меняем дистанцию
            currentZoomDistance = Mathf.Lerp(startDistance, targetZoomDistance, smoothedT);

            //  Плавно меняем высоту
            currentHeight = Mathf.Lerp(startHeight, targetHeight, smoothedT);

            //  Применяем оба изменения
            Vector3 offset = transposer.m_FollowOffset;
            offset.z = -currentZoomDistance;
            offset.y = currentHeight;
            transposer.m_FollowOffset = offset;

            if (Mathf.Abs(currentZoomDistance - targetZoomDistance) < 0.01f &&
                Mathf.Abs(currentHeight - targetHeight) < 0.01f)
                break;

            await UniTask.Yield(this.GetCancellationTokenOnDestroy());
        }

        //  Финализируем
        currentZoomDistance = targetZoomDistance;
        currentHeight = targetHeight;

        Vector3 finalOffset = transposer.m_FollowOffset;
        finalOffset.z = -currentZoomDistance;
        finalOffset.y = currentHeight;
        transposer.m_FollowOffset = finalOffset;

        Debug.Log($"Zoom finished: distance={currentZoomDistance}, height={currentHeight}");
        isZooming = false;
    }

    public void ResetZoom()
    {
        targetZoomDistance = defaultZoomDistance;
        currentZoomDistance = defaultZoomDistance;

        targetHeight = defaultHeight;
        currentHeight = defaultHeight;

        if (transposer != null)
        {
            Vector3 offset = transposer.m_FollowOffset;
            offset.z = -defaultZoomDistance;
            offset.y = defaultHeight;
            transposer.m_FollowOffset = offset;
        }

        isZooming = false;
        Debug.Log($"Zoom reset: distance={defaultZoomDistance}, height={defaultHeight}");
    }
}