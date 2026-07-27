using UnityEngine;
using System.Collections;

// 골렘 2페이즈 진입 시 소환되는 장애물형 소환수.
// 좌우로 이동하다가 벽에 부딪히면 방향 전환, 일정 시간 대기 후 다시 이동을 반복.
// 플레이어와 부딪히면 데미지만 주고(공격 판정 없음), 플레이어 공격으로는 제거되지 않음.
// 3페이즈에서 골렘이 죽을 때까지 계속 존재.
public class GolemSummon : MonoBehaviour
{
    [Tooltip("좌우 이동 속도")]
    public float moveSpeed = 3f;
    [Tooltip("벽에 부딪힌 후 대기하는 시간")]
    public float waitDuration = 1f;
    [Tooltip("플레이어와 부딪혔을 때 주는 데미지")]
    public int touchDamage = 10;
    [Tooltip("벽으로 인식할 레이어")]
    public LayerMask wallLayer;
    [Tooltip("플레이어로 인식할 레이어")]
    public LayerMask playerLayer;

    private Rigidbody2D rb;
    private float direction = 1f;  
    private bool isWaiting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isWaiting) return;  

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 벽에 부딪히면 대기 후 방향 전환
        if (((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            StartCoroutine(WaitAndTurn());
        }

        // 플레이어에 부딪히면 데미지만 주고, 이동은 그대로 유지
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            HealthManager targetHealth = collision.gameObject.GetComponent<HealthManager>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(touchDamage);
            }
        }
    }

    IEnumerator WaitAndTurn()
    {
        isWaiting = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);   // 대기 중엔 정지

        yield return new WaitForSeconds(waitDuration);

        direction *= -1;   // 방향 전환
        isWaiting = false;
    }
}