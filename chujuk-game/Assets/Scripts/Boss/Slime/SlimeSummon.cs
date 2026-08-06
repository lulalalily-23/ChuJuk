using UnityEngine;
using System.Collections;

// 슬라임 낙하공격 시 소환되는 장애물형 소환수.
// GolemSummon과 동일한 좌우 왕복 + 접촉 데미지 로직이지만,
// HealthManager를 붙여서 플레이어 공격으로 처치 가능하게 만듦.
[RequireComponent(typeof(HealthManager))]
public class SlimeSummon : MonoBehaviour
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
    private HealthManager healthManager;
    private float direction = 1f;
    private bool isWaiting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        healthManager = GetComponent<HealthManager>();
        healthManager.OnDeath += HandleDeath;
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

    void HandleDeath()
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (healthManager != null)
            healthManager.OnDeath -= HandleDeath;
    }
}