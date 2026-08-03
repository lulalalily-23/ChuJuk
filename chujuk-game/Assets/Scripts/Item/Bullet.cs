using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f; // 총알 날아가는 속도
    public int damage = 10; // 총알 데미지

    private Rigidbody2D rb;

    public void Setup(Vector2 direction)
    {
        rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = direction * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, 2f);
    }

    // 몬스터랑 부딪혔을 때
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            // other.GetComponent<Monster>().TakeDamage(damage);

            Debug.Log("몬스터 명중!");
            Destroy(gameObject);
        }
    }
}