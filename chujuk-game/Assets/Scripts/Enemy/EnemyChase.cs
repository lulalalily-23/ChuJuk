using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Tooltip("몬스터 이동 속도(최고 속도)")]
    public float moveSpeed; 
    [Tooltip("가속도값(moveSpeed값을 목표로 가속함)")]
    public float acceleration; 
    private Transform player;
    private Rigidbody2D rb;
    private float currentSpeed = 0f;
    private SpriteRenderer sr;

    void Start()
    {
        EnemyController controller = GetComponent<EnemyController>();
        player = controller.player;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()   
    {
        float distanceX = player.position.x - transform.position.x;

        // 흔들림 수정
        if (Mathf.Abs(distanceX) < 0.3f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            currentSpeed = 0f;
            return;
        }
        Vector2 direction = new Vector2(player.position.x - transform.position.x, 0f).normalized;
        if (direction.x != 0)
        {
            sr.flipX = direction.x < 0;
        }
        currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, Time.fixedDeltaTime * acceleration);

        rb.linearVelocity = new Vector2(direction.x * currentSpeed, rb.linearVelocity.y);
    }
}