using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float moveSpeed;
    public float jumpForce;
    public float Speed;
    public float dashSpeed;

    public float moveTime;
    private float dashTime;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool jumpRequested;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }

        if (Input.GetKeyDown(KeyCode.X) && dashTime <= 0)
        {
            dashTime = moveTime;
            moveSpeed = dashSpeed;
        }
    }

    void Start()
    {
        moveSpeed = Speed;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");

        rb.linearVelocity = new Vector2(h * moveSpeed, rb.linearVelocity.y);

        if (jumpRequested)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpRequested = false;
            isGrounded = false;
        }

        if (dashTime > 0)
        {
            dashTime -= Time.fixedDeltaTime;
            if (dashTime <= 0 )
            {
                moveSpeed = Speed;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.name.Contains("Square"))
        {
            isGrounded = true;
        }
    }
}