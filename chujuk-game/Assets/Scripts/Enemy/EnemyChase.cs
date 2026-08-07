using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Tooltip("몬스터 이동 속도(최고 속도)")]
    public float moveSpeed; 
    [Tooltip("가속도값(moveSpeed값을 목표로 가속함)")]
    public float acceleration; 
    private EnemyController controller; // player를 따로 복사하지 않고, 여기서 실시간으로 읽어옴
    private Rigidbody2D rb;
    private float currentSpeed = 0f;
    private SpriteRenderer sr;

    void Start()
    {
        controller = GetComponent<EnemyController>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()   
    {
        // EnemyController가 스스로 player를 복구하기 전 타이밍에 걸리면 이번 스텝만 조용히 스킵
        // (다음 Update에서 EnemyController가 다시 찾아줄 것이므로 여기서 따로 재탐색하지 않아도 됨)
        if (controller.player == null) return;

        Transform player = controller.player;

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