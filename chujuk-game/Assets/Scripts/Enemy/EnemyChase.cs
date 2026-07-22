using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Tooltip("몬스터 이동 속도(최고 속도)")]
    public float moveSpeed; 
    [Tooltip("가속도값(moveSpeed값을 목표로 가속함)")]
    public float acceleration; 
    private Transform player;
    private Rigidbody2D rb;
    private float currentSpeed = 0f;

    void Start()
    {
        EnemyController controller = GetComponent<EnemyController>();
        player = controller.player;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()   
    {
        Vector2 direction = new Vector2(player.position.x - transform.position.x, 0f).normalized; 
        currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, Time.fixedDeltaTime * acceleration);

        rb.linearVelocity = new Vector2(direction.x * currentSpeed, rb.linearVelocity.y); 
    }
}