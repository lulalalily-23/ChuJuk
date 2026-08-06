using UnityEngine;
using System.Collections;

// 분열 후 자식 슬라임 전용 패턴.
// 위로 재빨리 이동해 화면 밖으로 나감 -> 3초 대기 -> 플레이어 현재 위치 위로 재배치 후 강하게 낙하
// -> 착지 충격 데미지 + 소환수 소환.
public class SlimeSkill_FallAttack : MonoBehaviour
{
    public Transform player;

    [Header("이탈/낙하")]
    public float offScreenHeight = 15f;
    public float riseSpeed = 20f;
    public float waitBeforeFall = 3f;
    public float fallSpeed = 25f;

    [Header("착지")]
    public float impactRadius = 2f;
    public int impactDamageFallback = 15; // ICombatStats가 없을 때 쓸 기본 데미지
    public LayerMask targetLayer;
    public LayerMask groundLayer;   // 실제 바닥 판정용
    public GameObject summonPrefab;

    [Tooltip("착지 지점 예고선 표시에만 쓰는 값 - 실제 착지는 이제 충돌로 판정하므로 여기엔 영향 없음")]
    public float groundSurfaceOffset = 0f;

    public float coolTime = 8f;

    [Header("형제가 낙하공격 중일 때 대신 하는 소환")]
    public float quickSummonCoolTime = 4f;

    private SlimeController controller;
    private Collider2D col;
    private Rigidbody2D rb;
    private float lastUseTime = -999f;
    private float lastQuickSummonTime = -999f;

    void Start()
    {
        controller = GetComponent<SlimeController>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public bool CanUse()
    {
        return Time.time - lastUseTime >= coolTime;
    }

    // 형제가 낙하공격 중일 때, 이쪽은 화려한 연출 없이 소환수만 즉시 소환하는 간단한 대체 액션
    public bool CanQuickSummon()
    {
        return summonPrefab != null && Time.time - lastQuickSummonTime >= quickSummonCoolTime;
    }

    public void ExecuteQuickSummon()
    {
        lastQuickSummonTime = Time.time;
        controller.PlaySummon();
        Instantiate(summonPrefab, transform.position + Vector3.left * 1.5f, Quaternion.identity);
        controller.OnPatternExecuted();
    }

    public void Execute()
    {
        StartCoroutine(FallAttackRoutine());
    }

    private IEnumerator FallAttackRoutine()
    {
        lastUseTime = Time.time;
        controller.isPerformingFallAttack = true; // 형제에게 "나 지금 낙하공격 중" 알림

        // 평소엔 Dynamic(중력 적용)이었다가, 이 패턴이 실행되는 동안만 코드가 완전히 제어하도록 Kinematic으로 전환
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        if (col != null) col.enabled = false; // 화면 밖에 있는 동안 피격 불가

        // 위로 재빨리 이동해서 화면 밖으로
        Vector3 riseTarget = transform.position + Vector3.up * offScreenHeight;
        while (Vector3.Distance(transform.position, riseTarget) > 0.2f)
        {
            transform.position = Vector3.MoveTowards(transform.position, riseTarget, riseSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(waitBeforeFall);

        // 낙하 직전 - 플레이어 x좌표 기준으로 위로 재배치
        Vector3 fallStartPos = new Vector3(player.position.x, transform.position.y, transform.position.z);
        transform.position = fallStartPos;

        // 예고선 표시용 - 대략적인 착지 위치 미리보기 (실제 착지 지점은 아래에서 충돌로 확정됨)
        Vector2 rayOrigin = new Vector2(player.position.x, fallStartPos.y);
        RaycastHit2D previewHit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, groundLayer);
        float previewY = previewHit.collider != null ? previewHit.point.y + groundSurfaceOffset : player.position.y;
        Vector2 telegraphPos = new Vector2(player.position.x, previewY);

        TelegraphIndicator.Instance.ShowCircle(telegraphPos, impactRadius, 0.6f);
        yield return new WaitForSeconds(0.6f);

        // 실제 낙하 - 매 이동 스텝 전에 "이번 걸음만큼 앞에 바닥이 있는지" 미리 확인.
        // 물리 충돌 이벤트가 들어오는 타이밍에 의존하지 않기 때문에, 타일 이음새 등으로
        // 이벤트가 씹히더라도 계산상 바닥을 넘어서 이동하는 일 자체가 없음.
        col.enabled = true;

        while (true)
        {
            float stepDistance = fallSpeed * Time.fixedDeltaTime;
            RaycastHit2D groundAhead = Physics2D.Raycast(rb.position, Vector2.down, stepDistance + 0.05f, groundLayer);

            if (groundAhead.collider != null)
            {
                rb.MovePosition(groundAhead.point + Vector2.up * 0.02f); // 바닥 표면 바로 위에 붙임
                break;
            }

            rb.MovePosition(rb.position + Vector2.down * stepDistance);
            yield return new WaitForFixedUpdate();
        }

        controller.PlayFall();

        // 낙하공격 끝났으니 다시 평소 상태(중력 적용)로 복구
        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

        Vector3 landPos = transform.position; // 실제로 멈춘 그 자리 기준

        // 착지 충격 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(landPos, impactRadius, targetLayer);
        ICombatStats myStats = GetComponent<ICombatStats>();

        foreach (Collider2D hit in hits)
        {
            HealthManager targetHealth = hit.GetComponentInParent<HealthManager>();
            if (targetHealth == null) continue;

            ICombatStats targetStats = hit.GetComponentInParent<ICombatStats>();
            int damage = (myStats != null && targetStats != null)
                ? DamageCalculator.CalculateDamage(myStats, targetStats)
                : impactDamageFallback;

            targetHealth.TakeDamage(damage);
        }

        // 소환수 소환
        if (summonPrefab != null)
            Instantiate(summonPrefab, landPos + Vector3.left * 1.5f, Quaternion.identity);

        controller.isPerformingFallAttack = false; // 형제에게 "나 끝났다" 알림
        controller.OnPatternExecuted();
    }
}