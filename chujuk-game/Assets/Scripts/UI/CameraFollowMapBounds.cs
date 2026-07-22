using UnityEngine;

public class CameraFollowMapBounds : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Map Boundary")]
    [SerializeField] private Collider2D mapBoundary;

    [Header("Settings")]
    [SerializeField]
    private Vector3 offset =
        new Vector3(0f, 0f, -10f);

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null ||
            mapBoundary == null ||
            cam == null)
        {
            return;
        }

        Bounds bounds = mapBoundary.bounds;

        float cameraHalfHeight = cam.orthographicSize;
        float cameraHalfWidth =
            cameraHalfHeight * cam.aspect;

        float minX = bounds.min.x + cameraHalfWidth;
        float maxX = bounds.max.x - cameraHalfWidth;

        float minY = bounds.min.y + cameraHalfHeight;
        float maxY = bounds.max.y - cameraHalfHeight;

        Vector3 targetPosition = target.position + offset;

        float clampedX = Mathf.Clamp(
            targetPosition.x,
            minX,
            maxX
        );

        float clampedY = Mathf.Clamp(
            targetPosition.y,
            minY,
            maxY
        );

        transform.position = new Vector3(
            clampedX,
            clampedY,
            offset.z
        );
    }
}