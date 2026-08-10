using UnityEngine;

public class EnemyFlyIdle : EnemyIdle
{
    [Tooltip("위아래로 흔들리는 폭")]
    public float hoverAmplitude = 0.3f;
    [Tooltip("흔들리는 속도")]
    public float hoverSpeed = 2f;

    private Rigidbody2D flyRb;
    private Vector2 flyBasePoint;

    private void Awake()
    {
        flyRb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (flyRb != null)
            flyRb.gravityScale = 0f; 

        flyBasePoint = transform.position;
    }

    private void FixedUpdate()
    {
        float offsetY = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        Vector2 targetPos = new Vector2(flyBasePoint.x, flyBasePoint.y + offsetY);
        flyRb.MovePosition(targetPos);
    }
}