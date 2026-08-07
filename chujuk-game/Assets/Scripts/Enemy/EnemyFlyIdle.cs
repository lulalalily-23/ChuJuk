using UnityEngine;

public class EnemyFlyIdle : EnemyIdle
{
    [Tooltip("위아래로 흔들리는 폭")]
    public float hoverAmplitude = 0.3f;
    [Tooltip("흔들리는 속도")]
    public float hoverSpeed = 2f;

    private Rigidbody2D rb;
    private Vector2 basePoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private new void OnEnable()
    {
        if (rb != null)
            rb.gravityScale = 0f; 

        basePoint = transform.position;
    }

    private new void FixedUpdate()
    {
        float offsetY = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        Vector2 targetPos = new Vector2(basePoint.x, basePoint.y + offsetY);
        rb.MovePosition(targetPos);
    }
}