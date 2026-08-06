using UnityEngine;

// 발사 시점 목표 위치로 직선 이동하는 투사체. 유도 기능 없음 - 플레이어가 피할 수 있음.
// 프리팹에 Collider2D(Is Trigger 체크) + Rigidbody2D(Body Type: Kinematic)가 있어야 함.
// (정적인 벽 콜라이더와의 트리거 판정을 받으려면 둘 중 하나는 Rigidbody2D가 필요함)
public class SlimeProjectile : MonoBehaviour
{
    public float lifeTime = 5f;
    public LayerMask targetLayer;
    public LayerMask wallLayer;
    public int damage = 8;

    private Vector2 direction;
    private float speed;
    private ICombatStats ownerStats;
    private bool hasHit = false;

    public void Launch(Vector2 origin, Vector2 targetPos, float moveSpeed, ICombatStats stats)
    {
        transform.position = origin;
        direction = (targetPos - origin).normalized;
        speed = moveSpeed;
        ownerStats = stats;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // 벽에 닿으면 데미지 없이 그냥 소멸
        if (((1 << other.gameObject.layer) & wallLayer) != 0)
        {
            hasHit = true;
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & targetLayer) == 0) return;

        HealthManager targetHealth = other.GetComponentInParent<HealthManager>();
        if (targetHealth == null) return;

        ICombatStats targetStats = other.GetComponentInParent<ICombatStats>();
        int finalDamage = (ownerStats != null && targetStats != null)
            ? DamageCalculator.CalculateDamage(ownerStats, targetStats)
            : damage;

        hasHit = true;
        targetHealth.TakeDamage(finalDamage);
        Destroy(gameObject);
    }
}