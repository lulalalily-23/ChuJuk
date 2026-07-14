using UnityEngine;

public class CameraFollowClamp : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Map Bounds")]
    [SerializeField] private float mapMinX;
    [SerializeField] private float mapMaxX;
    [SerializeField] private float mapMinY;
    [SerializeField] private float mapMaxY;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("CameraFollowClamp는 Camera 오브젝트에 부착해야 합니다.");
        }
    }

    private void LateUpdate()
    {
        if (target == null || cam == null)
        {
            return;
        }

        Vector3 targetPosition = target.position + offset;

        // Orthographic Size는 카메라 화면 높이의 절반이다.
        float cameraHalfHeight = cam.orthographicSize;
        float cameraHalfWidth =
            cameraHalfHeight * cam.aspect;

        float minCameraX = mapMinX + cameraHalfWidth;
        float maxCameraX = mapMaxX - cameraHalfWidth;

        float minCameraY = mapMinY + cameraHalfHeight;
        float maxCameraY = mapMaxY - cameraHalfHeight;

        float clampedX = Mathf.Clamp(
            targetPosition.x,
            minCameraX,
            maxCameraX
        );

        float clampedY = Mathf.Clamp(
            targetPosition.y,
            minCameraY,
            maxCameraY
        );

        transform.position = new Vector3(
            clampedX,
            clampedY,
            offset.z
        );
    }
}