using UnityEngine;

public class EnemyFlyChase : EnemyChase
{
    [Tooltip("최고 이동 속도")]
    public float flySpeed = 4f;
    [Tooltip("가속도값")]
    public float flyAcceleration = 3f;

    private EnemyController controller;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 currentVelocity;

    private void Awake()
    {
        controller = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (rb != null)
            rb.gravityScale = 0f; 
    }

    private new void FixedUpdate()
    {
        if (controller.player == null) return;

        Vector2 direction = ((Vector2)controller.player.position - rb.position).normalized;

        if (direction.x != 0)
            sr.flipX = direction.x < 0;

        currentVelocity = Vector2.Lerp(currentVelocity, direction * flySpeed, Time.fixedDeltaTime * flyAcceleration);
        rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
    }
}