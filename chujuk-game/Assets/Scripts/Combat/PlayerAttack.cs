using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public int damage = 20;
    public float attackRange = 1f;
    public LayerMask enemyLayer;

    [Header("Attack Point")]
    public Transform attackPoint;

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

            EnemyDeath enemyDeath = enemy.GetComponent<EnemyDeath>();

            if (enemyDeath != null)
            {
                enemyDeath.TakeDamage(damage);
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