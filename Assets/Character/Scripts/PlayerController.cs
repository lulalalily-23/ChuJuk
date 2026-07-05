using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;

    void Start()
    {
        // 시작할 때 내 캡슐에 달려있는 Rigidbody를 찾아서 가져옴
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // 키보드 방향키나 WASD 입력 받기
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 이동 방향 설정 (x축은 좌우, z축은 앞뒤)
        // normalized를 안 해주면 대각선으로 갈 때 1.4배 빨라지는 버그 생김
        Vector3 movement = new Vector3(h, 0f, v).normalized;

        // 실제 이동 처리
        rb.MovePosition(transform.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}