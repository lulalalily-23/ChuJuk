using UnityEngine;

// 몬스터의 근접 기본 공격 담당. (Idle -> Chase -> Attack 상태에서 진행될 근접 기본 공격)
// Physics2D.OverlapCircle로 targetLayer에 해당하는 대상을 감지하고, DamageCalculator로 데미지를 계산해 대상의 HealthManager에 적용.

public class EnemyAttack : MonoBehaviour
{
    [Tooltip("OverlapCircle의 판정 반경. EnemyController의 attackRange와는 별개의 값")]
    public float attackDistance;
    [Tooltip("공격 대상이 속한 레이어. 플레이어 오브젝트의 Layer가 여기 지정된 레이어와 일치해야 감지됨")]
    public LayerMask targetLayer;
    [Tooltip("공격 쿨타임(초)")]
    public float coolTime;
    private ICombatStats stats; //공격자 자기 자신의 능력치
    private float lastAttackTime; //마지막 공격 시점 (쿨타임 계산용)
    private Animator animator;

    void OnEnable()
    {
        lastAttackTime = -coolTime; 
        // Attack 상태로 새로 진입할 때마다 호출됨.
        // attackDistance에 플레이어가 처음 들어오자마자 쿨타임 기다릴 필요없이 바로 공격가능하도록 설계함
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stats = GetComponent<ICombatStats>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {   
        if (Time.time - lastAttackTime < coolTime) return; // 쿨타임 경과되지 않을 시 스킵 처리 구문.

        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackDistance, targetLayer);

        if (hit == null) {
            Debug.Log("공격 대상 없음");
            return; //공격 범위 내에 대상이 없으면 스킵 처리함
        }
        animator.SetTrigger("Attack");
        HealthManager targetHealth = hit.GetComponent<HealthManager>();
        ICombatStats targetStats = hit.GetComponent<ICombatStats>();
        if (targetHealth == null) return; // 대상에 HealthManager가 없으면 스킵함 (안전장치)
        targetHealth.TakeDamage(DamageCalculator.CalculateDamage(stats, targetStats));
        lastAttackTime = Time.time;
    }
}
