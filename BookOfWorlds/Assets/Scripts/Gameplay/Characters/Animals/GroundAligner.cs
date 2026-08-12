using UnityEngine;

public class GroundAligner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float raycastDistance = 2f;
    [SerializeField] private float groundOffset = 0f;
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private float smoothSpeed = 8f;

    private float targetY;

    private void Update()
    {
        AlignToGround();
    }

    private void AlignToGround()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            targetY = hit.point.y + groundOffset;
            Vector3 newPos = transform.position;
            newPos.y = Mathf.Lerp(newPos.y, targetY, smoothSpeed * Time.deltaTime);
            transform.position = newPos;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.down * raycastDistance);
    }
}