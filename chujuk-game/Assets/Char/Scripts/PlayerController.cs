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
    private bool isDash;

    void Start()
    {
        moveSpeed = Speed;
        rb = GetComponent<Rigidbody2D>();
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
        float h = Input.GetAxis("Horizontal");

        rb.linearVelocity = new Vector2(h * moveSpeed, rb.linearVelocity.y);

        if (jumpRequested)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpRequested = false;
            isGrounded = false;
        }

        /* if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = 6;
        } else
        {
            moveSpeed = Speed;
        } */

        if (Input.GetKey(KeyCode.X))
        {
            isDash = true;
        }

        if (dashTime <= 0)
        {
            // moveSpeed = Speed;
            if (isDash)
            {
                dashTime = moveTime;
            }
        } else
        {
            dashTime -= Time.deltaTime;
            moveSpeed = dashSpeed;
        }
        isDash = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.name.Contains("Square"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name.Contains("Square"))
        {
            isGrounded = false;
        }
    }
}