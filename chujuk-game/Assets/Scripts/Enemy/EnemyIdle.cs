using UnityEngine;

public class EnemyIdle : MonoBehaviour
{
    [Tooltip("순찰 범위, 좌우 합한 기준 (ex. 5인경우 왼쪽으로 2.5 오른쪽으로 2.5)")]
    public float patrolRange; 
    [Tooltip("순찰 속도")]
    public float moveSpeed; 
    private Vector2 basePoint; //순찰을 하게될 기준점
    private float direction = 1f; //1f는 오른쪽 이동, -1f는 왼쪽으로 이동
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    // Idle 상태로 재진입할 때마다 현재 위치를 기준점으로 재설정 (추격하다 멈춘 자리부터 순찰 재개)
    void OnEnable()
    {
        basePoint = transform.position;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()   
    {
        float leftEnd = basePoint.x - patrolRange/2;
        float rightEnd = basePoint.x + patrolRange/2; //순찰 양쪽 끝점, 해당 점에 도달하면 방향을 바꿔서 다시 이동함
        if (transform.position.x <= leftEnd || transform.position.x >= rightEnd)
        {
            direction *= -1; //끝점에 도달하는 경우 방향은 반대로 전환됨
        }

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        sr.flipX = direction < 0;
    }
}