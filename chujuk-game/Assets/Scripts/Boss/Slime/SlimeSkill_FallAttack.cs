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
    public int impactDamageFallback = 15; 
    public LayerMask targetLayer;
    public LayerMask groundLayer;   
    public GameObject summonPrefab;

    public float groundSurfaceOffset = 0f;

    public float coolTime = 8f;

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

    // 형제 슬라임이 낙하공격 중일 때, 이쪽은 소환수만 즉시 소환
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
        controller.isPerformingFallAttack = true; 

        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        if (col != null) col.enabled = false; 

        Vector3 riseTarget = transform.position + Vector3.up * offScreenHeight;
        while (Vector3.Distance(transform.position, riseTarget) > 0.2f)
        {
            transform.position = Vector3.MoveTowards(transform.position, riseTarget, riseSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(waitBeforeFall);

        Vector3 fallStartPos = new Vector3(player.position.x, transform.position.y, transform.position.z);
        transform.position = fallStartPos;

        Vector2 rayOrigin = new Vector2(player.position.x, fallStartPos.y);
        RaycastHit2D previewHit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, groundLayer);
        float previewY = previewHit.collider != null ? previewHit.point.y + groundSurfaceOffset : player.position.y;
        Vector2 telegraphPos = new Vector2(player.position.x, previewY);

        TelegraphIndicator.Instance.ShowCircle(telegraphPos, impactRadius, 0.6f);
        yield return new WaitForSeconds(0.6f);

        col.enabled = true;

        while (true)
        {
            float stepDistance = fallSpeed * Time.fixedDeltaTime;
            RaycastHit2D groundAhead = Physics2D.Raycast(rb.position, Vector2.down, stepDistance + 0.05f, groundLayer);

            if (groundAhead.collider != null)
            {
                rb.MovePosition(groundAhead.point + Vector2.up * 0.02f);
                break;
            }

            rb.MovePosition(rb.position + Vector2.down * stepDistance);
            yield return new WaitForFixedUpdate();
        }

        controller.PlayFall();

        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

        Vector3 landPos = transform.position; 

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

        controller.isPerformingFallAttack = false; 
        controller.OnPatternExecuted();
    }
}