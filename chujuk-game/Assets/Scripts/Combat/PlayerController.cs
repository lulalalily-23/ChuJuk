using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 6f;
    public float dashSpeed = 15f;
    public float moveTime = 0.5f;

    [Header("Health")]
    public int maxHp = 100;
    public int currentHp;
    public event Action<int, int> OnHealthChanged;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private float dashTime;
    private float currentSpeed;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool jumpRequested;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = moveSpeed;
        currentHp = maxHp;
    }

    void Update()
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

    // 체력 및 피격

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        OnHealthChanged?.Invoke(currentHp, maxHp);

        // 피격 시 밀려남
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDirection, ForceMode2D.Impulse);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 사망 처리
        gameObject.SetActive(false);
        Debug.Log("플레이어 사망");
    }
}