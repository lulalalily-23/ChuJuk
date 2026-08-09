using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // 애니메이션
    private Animator animator;

    // 시작 상태 칼 모드
    public event Action<bool> OnWeaponChanged;  // 외부에서 무기 변경 이벤트 구독 -> UI에서 사용

    private bool IsGunMode = false;
    public bool IsUsingGun => IsGunMode; // 외부에서 총 모드 사용 여부 확인 -> UI에서 사용
    public bool IsDashing => isDashing; // 외부에서 대쉬 중인지 확인

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
        PlayerStat.Instance != null
            ? Mathf.RoundToInt(
                PlayerStat.Instance.GetStat(StatType.MaxHP)
            )
            : 100;

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
    private float dashDirection;

    [Header("Combat")]
    public GameObject bulletPrefab;
    public float attackCooldown = 0.5f; // 총알 공격 속도 제한
    private float nextAttackTime = 0f; // 다음 공격 쿨타임

    [Header("Camera")]
    public Camera mainCam;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravity = rb.gravityScale;
        currentHp = MaxHp;
        animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.SetBool("IsGunMode", IsGunMode);
        }

        mainCam = FindAnyObjectByType<Camera>();
    }

    void Update()
    {
        // 세이브 로드 테스트용 임시 코드
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (SaveManager.Instance == null)
                Debug.LogError("SaveManager.Instance가 null");
            else
                SaveManager.Instance.SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            if (SaveManager.Instance == null)
                Debug.LogError("SaveManager.Instance가 null");
            else
                SaveManager.Instance.LoadGame();
        }

        // 1. 공격
        if (Input.GetMouseButtonDown(0) &&
            Time.time >= nextAttackTime)
        {
            nextAttackTime =
                Time.time + attackCooldown; // 쿨타임 돌리기

            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            if (mainCam == null)
                mainCam = FindAnyObjectByType<Camera>();

            if (mainCam != null)
            {
                Vector3 mousePos =
                    mainCam.ScreenToWorldPoint(
                        Input.mousePosition
                    );

                mousePos.z = 0f;

                if (mousePos.x > transform.position.x)
                    transform.localScale =
                        new Vector3(1, 1, 1);
                else if (mousePos.x < transform.position.x)
                    transform.localScale =
                        new Vector3(-1, 1, 1);

                // 추격 세트 공격 효과
                if (PlayerStat.Instance != null)
                {
                    int maxStack =
                        PlayerStat.Instance.GetChaseMaxStack();

                    if (maxStack > 0)
                    {
                        PlayerStat.Instance.AddChaseStack();
                        PlayerStat.Instance.AddChaseMoveSpeedOnAttack();
                    }
                }

                // 총 모드일 경우 총알 발사
                if (IsGunMode)
                {
                    if (bulletPrefab == null)
                        return;

                    Vector2 shootDirection =
                        (mousePos - transform.position).normalized;

                    GameObject bullet =
                        Instantiate(
                            bulletPrefab,
                            transform.position,
                            Quaternion.identity
                        );

                    int damage = 0;

                    if (PlayerStat.Instance != null)
                    {
                        damage =
                            Mathf.RoundToInt(
                                PlayerStat.Instance.GetStat(
                                    StatType.Attack
                                )
                            );
                    }

                    // 매복 액티브
                    // 실제 원거리 공격 1회가 발사되는 순간 소비
                    if (PlayerStat.Instance != null &&
                        PlayerStat.Instance.IsAmbushActive())
                    {
                        PlayerStat.Instance.ConsumeAmbushActive();
                    }

                    Bullet bulletComponent =
                        bullet.GetComponent<Bullet>();

                    if (bulletComponent != null)
                    {
                        bulletComponent.Setup(
                            shootDirection,
                            damage
                        );
                    }
                }
            }
        }

        // 2. 무기 교체
        if (Input.GetMouseButtonDown(1))
        {
            IsGunMode = !IsGunMode;

            if (animator != null)
            {
                animator.SetBool(
                    "IsGunMode",
                    IsGunMode
                );

                animator.SetTrigger("Change");
            }

            OnWeaponChanged?.Invoke(IsGunMode);
        }

        // 대쉬 중일 때는 이동 및 점프 등 다른 행동 무시
        if (isDashing)
            return;

        h = Input.GetAxisRaw("Horizontal");

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (animator != null)
        {
            if (isGrounded)
            {
                animator.SetFloat(
                    "Speed",
                    Mathf.Abs(h)
                );
            }
            else
            {
                animator.SetFloat(
                    "Speed",
                    0f
                );
            }

            animator.SetBool(
                "IsGrounded",
                isGrounded
            );
        }

        // 점프
        if (Input.GetButtonDown("Jump") &&
            isGrounded)
        {
            jumpRequested = true;

            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }

        // 대쉬 (Shift키 누르면 발동)
        if (Input.GetKeyDown(KeyCode.LeftShift) &&
            !isDashing &&
            Time.time >= nextDashTime)
        {
            isDashing = true;
            dashTime = moveTime;
            dashDirection = transform.localScale.x;
            rb.gravityScale = 0f;

            float cooldown = dashCooldown;

            if (PlayerStat.Instance != null)
            {
                cooldown -=
                    PlayerStat.Instance.GetStat(
                        StatType.DashCoolDown
                    );
            }

            cooldown = Mathf.Max(
                0f,
                cooldown
            );

            nextDashTime =
                Time.time + cooldown;
        }

        // 대쉬 (Z키)
        if (Input.GetKeyDown(KeyCode.Z) &&
            !isDashing &&
            Time.time >= nextDashTime)
        {
            isDashing = true;
            dashTime = moveTime;
            dashDirection = transform.localScale.x;
            rb.gravityScale = 0f;

            float cooldown = dashCooldown;

            if (PlayerStat.Instance != null)
            {
                cooldown -=
                    PlayerStat.Instance.GetStat(
                        StatType.DashCoolDown
                    );
            }

            cooldown = Mathf.Max(
                0f,
                cooldown
            );

            nextDashTime =
                Time.time + cooldown;
        }

        // 액티브 스킬 (Q)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (PlayerActiveSkill.Instance != null &&
                Inventory.Instance != null)
            {
                List<ItemInstance> items =
                    Inventory.Instance.GetItems();

                if (items != null)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        ItemInstance item = items[i];

                        if (item == null)
                            continue;

                        if (!item.CanUse())
                            continue;

                        if (item.Use())
                            break;
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            dashTime -= Time.fixedDeltaTime;

            rb.linearVelocity =
                new Vector2(
                    dashDirection * dashSpeed,
                    0f
                );

            if (dashTime <= 0)
            {
                isDashing = false;
                rb.gravityScale = originalGravity;
            }

            return;
        }

        // 일반 이동
        float currentMoveSpeed = moveSpeed;

        if (PlayerStat.Instance != null)
        {
            currentMoveSpeed =
                PlayerStat.Instance.GetStat(
                    StatType.MoveSpeed
                );

            float chaseMoveSpeedBonus =
                PlayerStat.Instance.GetChaseMoveSpeedBonus();

            currentMoveSpeed *=
                1f + chaseMoveSpeedBonus;
        }

        rb.linearVelocity =
            new Vector2(
                h * currentMoveSpeed,
                rb.linearVelocity.y
            );

        // 일정 높이 점프
        if (jumpRequested)
        {
            rb.linearVelocity =
                new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );

            jumpRequested = false;
        }

        // 떨어질 때 중력 가속도 추가
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity +=
                Vector2.up *
                Physics2D.gravity.y *
                (fallMultiplier - 1) *
                Time.fixedDeltaTime;
        }

        // 캐릭터 방향 전환
        if (h > 0)
            transform.localScale =
                new Vector3(1, 1, 1);
        else if (h < 0)
            transform.localScale =
                new Vector3(-1, 1, 1);
    }

    // 체력 및 피격
    public void TakeDamage(
        int damage,
        Vector2 knockbackDirection)
    {
        if (damage <= 0)
            return;

        // 대쉬 중에는 피해를 받지 않음
        if (isDashing)
            return;

        currentHp -= damage;

        currentHp =
            Mathf.Clamp(
                currentHp,
                0,
                MaxHp
            );

        OnHealthChanged?.Invoke(
            currentHp,
            MaxHp
        );

        // 피격 시 밀려남
        if (!isDashing)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.AddForce(
                knockbackDirection,
                ForceMode2D.Impulse
            );
        }

        // 피해를 받으면 추격 세트 이동속도 효과 적용
        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.AddChaseMoveSpeedOnDamage();
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 체력 회복
    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        if (currentHp <= 0)
            return;

        int oldHp = currentHp;

        currentHp =
            Mathf.Clamp(
                currentHp + amount,
                0,
                MaxHp
            );

        if (currentHp != oldHp)
        {
            OnHealthChanged?.Invoke(
                currentHp,
                MaxHp
            );
        }
    }

    // 처치 시 회복
    public void HealFromKill()
    {
        if (PlayerStat.Instance == null)
            return;

        int healAmount =
            Mathf.RoundToInt(
                PlayerStat.Instance.GetStat(
                    StatType.HealOnKill
                )
            );

        if (healAmount <= 0)
            return;

        Heal(healAmount);
    }

    private void Die()
    {
        // 사망 처리
        gameObject.SetActive(false);
        Debug.Log("플레이어 사망");
    }

    // 세이브 파일 불러오기 전용 (데미지 계산 없이 절대값 복원)
    public void LoadHealth(int hp)
    {
        currentHp =
            Mathf.Clamp(
                hp,
                0,
                MaxHp
            );

        OnHealthChanged?.Invoke(
            currentHp,
            MaxHp
        );
    }
}