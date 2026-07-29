using UnityEngine;

public class WorldSpaceUIFollower : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] private bool followTarget = true;

    private Transform target;
    private Camera mainCamera;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!followTarget || target == null || mainCamera == null)
        {
            return;
        }

        Vector3 worldPos = target.position + worldOffset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        //  Если объект за камерой — просто не обновляем позицию, НО НЕ ВЫКЛЮЧАЕМ
        if (screenPos.z < 0)
        {
            return;
        }

        //  Обновляем позицию (объект остаётся включённым)
        rectTransform.position = screenPos;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void ClearTarget()
    {
        target = null;
    }

    public void SetOffset(Vector3 newOffset)
    {
        worldOffset = newOffset;
    }

    public bool IsFollowing => target != null;
}