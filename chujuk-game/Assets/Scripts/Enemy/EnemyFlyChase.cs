using UnityEngine;

public class EnemyFlyChase : EnemyChase
{
    [Tooltip("최고 이동 속도")]
    public float flySpeed = 4f;
    [Tooltip("가속도값")]
    public float flyAcceleration = 3f;
    [Tooltip("플레이어와 이 거리 이내로는 더 접근하지 않고 멈춤 (0이면 계속 파고듦)")]
    public float stopDistance = 1.2f;

    private EnemyController flyController;
    private Rigidbody2D flyRb;
    private SpriteRenderer flySr;
    private Vector2 flyCurrentVelocity;

    private void Awake()
    {
        flyController = GetComponent<EnemyController>();
        flyRb = GetComponent<Rigidbody2D>();
        flySr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (flyRb != null)
            flyRb.gravityScale = 0f; 
    }

    private void FixedUpdate()
    {
        if (flyController.player == null) return;

        Vector2 toPlayer = (Vector2)flyController.player.position - flyRb.position;
        float distance = toPlayer.magnitude;

        if (distance < stopDistance)
        {
            flyCurrentVelocity = Vector2.Lerp(flyCurrentVelocity, Vector2.zero, Time.fixedDeltaTime * flyAcceleration);
            flyRb.MovePosition(flyRb.position + flyCurrentVelocity * Time.fixedDeltaTime);
            return;
        }

        Vector2 direction = toPlayer.normalized;

        if (direction.x != 0)
            flySr.flipX = direction.x < 0;

        flyCurrentVelocity = Vector2.Lerp(flyCurrentVelocity, direction * flySpeed, Time.fixedDeltaTime * flyAcceleration);
        flyRb.MovePosition(flyRb.position + flyCurrentVelocity * Time.fixedDeltaTime);
    }
}