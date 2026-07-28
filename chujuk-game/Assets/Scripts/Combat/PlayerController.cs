using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    // �ִϸ��̼�
    private Animator animator;
    // ���� ���� Į ���
    public event Action<bool> OnWeaponChanged;  // �ܺο��� ���� ���� �̺�Ʈ ���� -> UI���� ���

    private bool IsGunMode = false;
    public bool IsUsingGun => IsGunMode; // �ܺο��� �� ��� ��� ���� Ȯ�� -> UI���� ���
    // 애니메이션
    private Animator animator;

    // 시작 상태 칼 모드
    public event Action<bool> OnWeaponChanged; // 외부에서 무기 변경 이벤트 구독 -> UI에서 사용

    private bool IsGunMode = false;
    public bool IsUsingGun => IsGunMode; // 외부에서 총 모드 사용 여부 확인 -> UI에서 사용
    private HealthManager healthManager; // HealthManager 참조

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 4f; // ���� ���� ����
    public float dashSpeed = 15f;
    public float moveTime = 0.2f; // �뽬 ���� �ð�

    [Header("Jump Physics")]
    public float fallMultiplier = 2.5f; // ������ �� ���ӵ�

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private float dashTime;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool jumpRequested;
    private float h;

    private bool isDashing;
    private float originalGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravity = rb.gravityScale; // ������ �� ���� �߷°� ��ο� ���
        currentHp = maxHp;
        animator = GetComponentInChildren<Animator>();
        animator.SetBool("IsGunMode", IsGunMode);
        currentSpeed = moveSpeed;

        animator = GetComponentInChildren<Animator>();
        animator.SetBool("IsGunMode", IsGunMode);

        healthManager = GetComponent<HealthManager>();
        healthManager.OnDeath += Die;
    }

    void Update()
    {
        // �뽬 ���� ���� ����Ű �� �ٸ� �ൿ ����
        if (isDashing) return;

        h = Input.GetAxisRaw("Horizontal");
        animator.SetFloat("Speed", Mathf.Abs(h));

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // ����
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

        // �뽬 (ZŰ ������ �ߵ�)
        if (Input.GetKeyDown(KeyCode.Z) && !isDashing)
        // 대쉬
        if (Input.GetKeyDown(KeyCode.Z) && dashTime <= 0)
        {
            isDashing = true;
            dashTime = moveTime;
            rb.gravityScale = 0f; // �뽬 ���� �� �߷� ����
        }

        // ����
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Attack");
        }

        // ���� ��ü
        if (Input.GetMouseButtonDown(1))
        {
            IsGunMode = !IsGunMode;
            animator.SetBool("IsGunMode", IsGunMode);
            animator.SetTrigger("Change");

            OnWeaponChanged?.Invoke(IsGunMode);
        }

        // �ٴ� ����
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
        // �뽬 ������ ���� ���� ó��
        if (isDashing)
        {
            dashTime -= Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(
            h * currentSpeed,
            rb.linearVelocity.y
        );

            // �ٶ󺸴� �������� �߷� ���� ����
            rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0f);

            if (dashTime <= 0)
            {
                isDashing = false;
                rb.gravityScale = originalGravity; // �뽬 ������ �߷� ����
            }
            return;
        }

        // �Ϲ� �̵�
        rb.linearVelocity = new Vector2(h * moveSpeed, rb.linearVelocity.y);

        // ���� ���� ����
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            rb.AddForce(
                Vector2.up * jumpForce,
                ForceMode2D.Impulse
            );

            jumpRequested = false;
        }

        // ������ �� �߷� ���ӵ� �߰�
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }

        // ĳ���� ���� ��ȯ
        if (h > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (h < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    // ü�� �� �ǰ�
    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        OnHealthChanged?.Invoke(currentHp, maxHp);

        // �ǰ� �� �з���
        if (!isDashing)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDirection, ForceMode2D.Impulse);
        }

        if (currentHp <= 0)
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

    private void Die()
    {
        // 사망 처리
        gameObject.SetActive(false);
        Debug.Log("플레이어 사망");
    }

    void OnDestroy()
    {
        if (healthManager != null)
            healthManager.OnDeath -= Die;
    }
}