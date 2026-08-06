using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    // 애니메이션
    private Animator animator;
    // 시작 상태 칼 모드
    public event Action<bool> OnWeaponChanged;  // 외부에서 무기 변경 이벤트 구독 -> UI에서 사용

    private bool IsGunMode = false;
    public bool IsUsingGun => IsGunMode; // 외부에서 총 모드 사용 여부 확인 -> UI에서 사용

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 4f; // 점프 높이 세팅
    public float dashSpeed = 15f;
    public float moveTime = 0.2f; // 대쉬 지속 시간
    public float dashCooldown = 1.5f;

    [Header("Jump Physics")]
    public float fallMultiplier = 2.5f; // 떨어질 때 가속도

    [Header("Health")]
    public int MaxHp =>
    Mathf.RoundToInt(
        PlayerStat.Instance.GetStat(StatType.MaxHP)
    );
    public int currentHp;
    public event Action<int, int> OnHealthChanged;

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
    private float nextDashTime = 0f;

    [Header("Combat")]
    public GameObject bulletPrefab;

    [Header("Camera")]
    public Camera mainCam;

    [Header("Inventory")]
    public int currentMoney = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravity = rb.gravityScale; // 시작할 때 원래 중력값 장부에 기록
        currentHp = MaxHp;
        animator = GetComponentInChildren<Animator>();
        animator.SetBool("IsGunMode", IsGunMode);
        mainCam = FindAnyObjectByType<Camera>();
    }

    void Update()
    {
        // 대쉬 중일 때는 방향키 등 다른 행동 무시
        if (isDashing) return;

        h = Input.GetAxisRaw("Horizontal");

        if (isGrounded)
        {
            animator.SetFloat("Speed", Mathf.Abs(h));
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
            animator.SetTrigger("Jump");
        }

        // 대쉬 (Z키 누르면 발동)
        if (Input.GetKeyDown(KeyCode.Z) && !isDashing && Time.time >= nextDashTime)
        {
            isDashing = true;
            dashTime = moveTime;
            rb.gravityScale = 0f;

            nextDashTime = Time.time + dashCooldown;
        }

        // 공격
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Attack");

            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            if (mousePos.x > transform.position.x)
                transform.localScale = new Vector3(1, 1, 1);
            else if (mousePos.x < transform.position.x)
                transform.localScale = new Vector3(-1, 1, 1);

            if (IsGunMode)
            {
                Vector2 shootDirection = (mousePos - transform.position).normalized;

                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

                int damage = Mathf.RoundToInt(PlayerStat.Instance.GetStat(StatType.Attack));


                bullet.GetComponent<Bullet>().Setup(shootDirection, damage);
            }
        }

        // 무기 교체
        if (Input.GetMouseButtonDown(1))
        {
            IsGunMode = !IsGunMode;
            animator.SetBool("IsGunMode", IsGunMode);
            animator.SetTrigger("Change");

            OnWeaponChanged?.Invoke(IsGunMode);
        }

        // 바닥 감지
        animator.SetBool("IsGrounded", isGrounded);
    }

    void FixedUpdate()
    {
        // 대쉬 상태일 때의 물리 처리
        if (isDashing)
        {
            dashTime -= Time.fixedDeltaTime;

            // 바라보는 방향으로 중력 없이 직진
            rb.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0f);

            if (dashTime <= 0)
            {
                isDashing = false;
                rb.gravityScale = originalGravity; // 대쉬 끝나면 중력 복구
            }
            return;
        }

        // 일반 이동
        float currentMoveSpeed = PlayerStat.Instance.GetStat(StatType.MoveSpeed);

        rb.linearVelocity = new Vector2(h * currentMoveSpeed, rb.linearVelocity.y);

        // 일정 높이 점프
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpRequested = false;
        }

        // 떨어질 때 중력 가속도 추가
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }

        // 캐릭터 방향 전환
        if (h > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (h < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    // 체력 및 피격
    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, MaxHp);

        OnHealthChanged?.Invoke(currentHp, MaxHp);

        // 피격 시 밀려남
        if (!isDashing)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDirection, ForceMode2D.Impulse);
        }

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

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        Debug.Log($"재화 획득! +{amount} (현재 잔액: {currentMoney})");
        // UI 텍스트 업데이트 하는 코드
    }
}