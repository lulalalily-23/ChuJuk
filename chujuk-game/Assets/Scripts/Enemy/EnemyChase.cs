using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Tooltip("몬스터 이동 속도(최고 속도)")]
    public float moveSpeed; 
    [Tooltip("가속도값(moveSpeed값을 목표로 가속함)")]
    public float acceleration; 
    private Transform player;
    private float currentSpeed = 0f; // 가속 메커니즘 위해 저장하는 현재속도 변수값

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnemyController controller = GetComponent<EnemyController>();
        player = controller.player;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 direction = (player.position - transform.position).normalized; //방향벡터를 정규화식으로 표현
        currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, Time.deltaTime * acceleration); //Lerp로 부드럽게 속도 변환
        transform.Translate(direction * currentSpeed * Time.deltaTime); //플레이어를 향해 몬스터 이동.
    }
}
