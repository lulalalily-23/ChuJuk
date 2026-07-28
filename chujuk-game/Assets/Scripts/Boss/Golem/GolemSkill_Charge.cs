using UnityEngine;
using System.Collections;

// 플레이어 방향(좌우)으로 빠르게 돌진.
// 돌진 방향에 맞는 발(leftFoot/rightFoot)을 시작점으로 삼아 텔레그래프를 표시하고,
// 이동 중 매 프레임 판정하며 한 번 맞으면 더 이상 중복 판정하지 않음.
public class GolemSkill_Charge : MonoBehaviour
{
    [Tooltip("이동 거리")]
    public float chargeDistance = 8f;
    [Tooltip("이동 속도")]
    public float chargeSpeed = 15f;
    [Tooltip("돌진 전 공격 예고 지속시간")]
    public float telegraphDuration = 1f;
    [Tooltip("쿨타임")]
    public float coolTime = 6f;
    [Tooltip("판정 대상 레이어 (Player)")]
    public LayerMask targetLayer;
    public Transform player;
    public Transform leftFoot;
    public Transform rightFoot;

    private GolemController golemController;
    private HealthManager selfHealth;
    private ICombatStats myStats;
    private Rigidbody2D rb;
    private float lastUseTime = -999f;
    private bool isCharging;
    private Vector2 chargeDirection;
    private bool hitWall = false; // 벽 충돌 감지 시 true, while 루프를 즉시 종료시키는 용도
    public LayerMask wallLayer; 

    void Start()
    {
        golemController = GetComponent<GolemController>();
        myStats = GetComponent<ICombatStats>();
        rb = GetComponent<Rigidbody2D>();
    }

    // 물리 이동은 FixedUpdate로 변경
    void FixedUpdate()
    {
        if (isCharging)
        {
            rb.linearVelocity = new Vector2(chargeDirection.x * chargeSpeed, rb.linearVelocity.y);
        }
    }

    public bool CanUse()
    {
        return !isCharging && Time.time - lastUseTime >= coolTime;
    }

    public void Execute()
    {
        if (isCharging) return;
        StartCoroutine(ChargeRoutine());
    }
    
    IEnumerator ChargeRoutine()
    {
        lastUseTime = Time.time;

        // 플레이어 방향(좌우) 계산, 그 방향에 해당하는 발을 시작점으로 사용
        Vector2 direction = new Vector2(player.position.x - transform.position.x, 0f).normalized;
        Transform startFoot = direction.x >= 0 ? rightFoot : leftFoot;
        Vector2 startPos = new Vector2(startFoot.position.x, transform.position.y);
        Vector2 targetPos = startPos + direction * chargeDistance;

        Vector2 telegraphCenter = (startPos + targetPos) / 2f;
        Vector2 telegraphSize = new Vector2(chargeDistance, 1f);
        TelegraphIndicator.Instance.ShowSquare(telegraphCenter, telegraphSize, telegraphDuration);

        yield return new WaitForSeconds(telegraphDuration);

        chargeDirection = direction;
        isCharging = true;   // FixedUpdate가 velocity를 밀어줌

        bool alreadyHit = false;
        float elapsed = 0f;
        float duration = chargeDistance / chargeSpeed;

    // 시간이 다 되거나(elapsed >= duration), 벽에 부딪히면(hitWall) 즉시 루프 종료
       while (elapsed < duration && !hitWall)
        {
            if (!alreadyHit)
            {
                alreadyHit = CheckHit();
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        hitWall = false;
        isCharging = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);   // 돌진 끝나면 멈춤
        golemController.OnPatternExecuted();
    }

    bool CheckHit()
    {
        Collider2D[] allHits = Physics2D.OverlapBoxAll(transform.position, new Vector2(2f, 2f), 0f, targetLayer);
        if (allHits.Length == 0) return false;

        Collider2D hit = allHits[0];
        ICombatStats[] statsArray = hit.GetComponentsInParent<ICombatStats>();
        ICombatStats targetStats = statsArray.Length > 0 ? statsArray[0] : null;
        if (targetStats == null) return false;

        HealthManager targetHealth = hit.GetComponentInParent<HealthManager>();
        if (targetHealth == null) return false;

        int damage = DamageCalculator.CalculateDamage(myStats, targetStats);
        targetHealth.TakeDamage(damage);
        return true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCharging && ((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            hitWall = true;
        }
    }
}