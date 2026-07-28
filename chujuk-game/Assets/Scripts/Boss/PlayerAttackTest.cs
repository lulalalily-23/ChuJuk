using UnityEngine;

public class PlayerAttackTest : MonoBehaviour
{
    public float attackRange = 2f;
    public LayerMask targetLayer;
    private ICombatStats myStats;

    void Start()
    {
        ICombatStats[] myStatsArray = GetComponents<ICombatStats>();
        myStats = myStatsArray.Length > 0 ? myStatsArray[0] : null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Attack();
        }
    }

    void Attack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, targetLayer);
        if (hit == null)
        {
            Debug.Log("공격 범위 내에 대상 없음");
            return;
        }

        ICombatStats[] statsArray = hit.GetComponentsInParent<ICombatStats>();
        ICombatStats targetStats = statsArray.Length > 0 ? statsArray[0] : null;

        if (targetStats == null)
        {
            Debug.Log("대상에 ICombatStats 없음: " + hit.gameObject.name);
            return;
        }

        int damage = DamageCalculator.CalculateDamage(myStats, targetStats);

        GolemArm arm = hit.GetComponent<GolemArm>();
        if (arm != null)
        {
            Debug.Log(hit.gameObject.name + " (팔) 피격 시도, 계산된 데미지: " + damage);
            arm.OnHit(damage);
        }
        else
        {
            HealthManager targetHealth = hit.GetComponent<HealthManager>();
            if (targetHealth == null)
            {
                Debug.Log("대상에 HealthManager 없음: " + hit.gameObject.name);
                return;
            }
            Debug.Log(hit.gameObject.name + " (본체) 피격, 데미지: " + damage);
            targetHealth.TakeDamage(damage);
        }
    }
}