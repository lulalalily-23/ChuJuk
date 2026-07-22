using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    // 애니메이션
    private Animator animator;

    // 시작 상태 칼 모드
    public event Action<bool> OnWeaponChanged; // 외부에서 무기 변경 이벤트 구독 -> UI에서 사용

    private bool IsGunMode = false;
    public bool IsUsingGun => IsGunMode; // 외부에서 총 모드 사용 여부 확인 -> UI에서 사용

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
    private float h;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = moveSpeed;
        currentHp = maxHp;

        animator = GetComponentInChildren<Animator>();
        animator.SetBool("IsGunMode", IsGunMode);
    }

    void Update()
    {
        h = Input.GetAxisRaw("Horizontal");

        animator.SetFloat("Speed", Mathf.Abs(h));

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
            animator.SetTrigger("Jump");
        }

        // 대쉬
        if (Input.GetKeyDown(KeyCode.Z) && dashTime <= 0)
        {
            dashTime = moveTime;
            currentSpeed = dashSpeed;
        }

        // 공격
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Attack");
        }

        // 무기 교체
        if (Input.GetMouseButtonDown(1))
        {
            IsGunMode = !IsGunMode;

            animator.SetBool("IsGunMode", IsGunMode);
            animator.SetTrigger("Change");

            // 외부(UI)에 무기 변경 이벤트 전달
            OnWeaponChanged?.Invoke(IsGunMode);
        }

        // 바닥 감지
        animator.SetBool("IsGrounded", isGrounded);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            h * currentSpeed,
            rb.linearVelocity.y
        );

        if (jumpRequested)
        {
            rb.AddForce(
                Vector2.up * jumpForce,
                ForceMode2D.Impulse
            );

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

        // 캐릭터 방향 전환
        if (h > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (h < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
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