using UnityEngine;
using Zenject;

public class CameraTarget : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
    [SerializeField] private float followSpeed = 10f;

    private Transform playerTransform;
    private bool isInitialized = false;

    public void Initialize(Transform player)
    {
        playerTransform = player;
        isInitialized = true;

        if (playerTransform != null)
        {
            // Мгновенно перемещаемся на позицию игрока
            transform.position = playerTransform.position + offset;
            transform.rotation = Quaternion.identity;
            Debug.Log($"CameraTarget initialized at position: {transform.position}");
        }
    }

    void LateUpdate()
    {
        if (!isInitialized || playerTransform == null)
        {
            Debug.LogWarning("CameraTarget: Not initialized or player is null!");
            return;
        }

        // Целевая позиция
        Vector3 targetPosition = playerTransform.position + offset;

        // Плавное следование
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        // ВАЖНО: ВСЕГДА смотрим строго вверх!
        transform.rotation = Quaternion.identity;
    }

    // Метод для принудительного обновления позиции
    public void ForceUpdatePosition()
    {
        if (playerTransform != null)
        {
            transform.position = playerTransform.position + offset;
        }
    }
}