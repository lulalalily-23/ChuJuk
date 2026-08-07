using UnityEngine;

public class CameraFollowMapBounds : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Map Boundary")]
    [SerializeField] private Collider2D mapBoundary;

    [Header("자동 탐색용")]
    [SerializeField] private string groundObjectName = "Ground";

    [Header("Settings")]
    [SerializeField]
    private Vector3 offset =
        new Vector3(0f, 0f, -10f);

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        EnsureReferences();
    }

    private void LateUpdate()
    {
        // 씬 리로드 등으로 참조가 끊겼으면 다시 찾기 시도
        if (target == null || mapBoundary == null)
        {
            EnsureReferences();
        }

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

    private void EnsureReferences()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                target = playerObj.transform;
        }

        if (mapBoundary == null)
        {
            GameObject groundObj = GameObject.Find(groundObjectName);
            if (groundObj != null)
                mapBoundary = groundObj.GetComponent<Collider2D>();
        }
    }
}