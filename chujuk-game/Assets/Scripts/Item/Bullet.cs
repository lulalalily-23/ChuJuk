using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f; // 총알 날아가는 속도
    public int damage = 10; // 총알 데미지

    private Rigidbody2D rb;

    public void Setup(Vector2 direction, int damage)
    {
        this.damage = damage;

        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError(
                "[Bullet] Rigidbody2D를 찾을 수 없습니다.",
                this
            );
            return;
        }

        rb.linearVelocity = direction * speed;

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(
                0,
                0,
                angle
            );

        Destroy(gameObject, 2f);
    }

    // 몬스터랑 부딪혔을 때
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            HealthManager health =
                other.GetComponent<HealthManager>();

            if (health != null)
            {
                float finalDamage = damage;

                if (PlayerStat.Instance != null)
                {
                    // 원거리 공격 추가 데미지
                    float rangedDamage =
                        PlayerStat.Instance.GetStat(
                            StatType.RangedDamage
                        );

                    finalDamage *=
                        1f + rangedDamage;

                    // 추격 세트 공격 데미지
                    float chaseDamageBonus =
                        PlayerStat.Instance.GetChaseDamageBonus();

                    finalDamage *=
                        1f + chaseDamageBonus;
                }

                int result =
                    Mathf.Max(
                        1,
                        Mathf.RoundToInt(finalDamage)
                    );

                health.TakeDamage(result);
            }

            // other.GetComponent<Monster>().TakeDamage(damage);

            Debug.Log("몬스터 명중!");

            Destroy(gameObject);
        }
        // 땅이나 벽에 닿으면 총알 삭제
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}