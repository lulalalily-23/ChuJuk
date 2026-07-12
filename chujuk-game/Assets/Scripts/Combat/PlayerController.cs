using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float jumpForce;
    public float dashSpeed;
    public float moveTime;

    private float dashTime;
    private float currentSpeed;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool jumpRequested;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }

        if (Input.GetKeyDown(KeyCode.X) && dashTime <= 0)
        {
            dashTime = moveTime;
            currentSpeed = dashSpeed;
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(h * currentSpeed, rb.linearVelocity.y);

        if (jumpRequested)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpRequested = false;
        }

        if (dashTime > 0)
        {
            dashTime -= Time.fixedDeltaTime;
            if (dashTime <= 0)
            {
                currentSpeed = moveSpeed;
            }
        }
    }
}