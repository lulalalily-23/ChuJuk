using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attackRange = 1f;
    public LayerMask enemyLayer;

    [Header("Attack Point")]
    public Transform attackPoint;

    private ICombatStats myStats;

    void Start()
    {
        myStats = GetComponent<ICombatStats>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void Attack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer);

        foreach (Collider2D enemy in enemies)
        {
            HealthManager health = enemy.GetComponent<HealthManager>();
            ICombatStats targetStats = enemy.GetComponent<ICombatStats>();

            if (health != null && targetStats != null)
            {
                int finalDamage = DamageCalculator.CalculateDamage(myStats, targetStats);
                health.TakeDamage(finalDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}