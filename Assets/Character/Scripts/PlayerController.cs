using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 6f;

    private Rigidbody rb;
    private bool isGrounded; // 바닥에 닿아있는지 체크
    private bool jumpRequested; // 점프 키 눌림여부

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        // 이동처리
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(h, 0f, v).normalized; // normalized를 안 해주면 대각선으로 갈 때 1.4배 빨라지는 버그 생김
        rb.MovePosition(transform.position + movement * moveSpeed * Time.fixedDeltaTime);

        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            jumpRequested = false;
            isGrounded = false;
        }
    }

    // 충돌처리
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.name.Contains("Plane"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name.Contains("Plane"))
        {
            isGrounded = false;
        }
    }
}