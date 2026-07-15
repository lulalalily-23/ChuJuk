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
    private float lastUseTime = -999f;
    private bool isCharging;

    void Start()
    {
        golemController = GetComponent<GolemController>();
        myStats = GetComponent<ICombatStats>();
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
        isCharging = true;
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

        bool alreadyHit = false; // 돌진 한 번에 중복 판정 방지
        float elapsed = 0f;
        Vector2 moveStart = transform.position;
        float duration = chargeDistance / chargeSpeed;

        while (elapsed < duration)
        {
            transform.position = Vector2.Lerp(moveStart, targetPos, elapsed / duration);

            if (!alreadyHit)
            {
                alreadyHit = CheckHit();   
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isCharging = false;
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
}
