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

    [Header("발판 내려가기")]
    [Tooltip("S키로 내려갈 수 있는 발판의 Tag")]
    public string dropThroughPlatformTag = "Platform";

    [Tooltip("발판을 통과해서 내려가는 시간")]
    public float dropThroughDuration = 0.25f;

    [Tooltip("발판에서 내려갈 때 아래로 이동시키는 거리")]
    public float dropThroughDistance = 0.15f;

    private float dashTime;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool jumpRequested;
    private float h;

    private bool isDashing;
    private float originalGravity;
    private float nextDashTime = 0f;
    private float dashDirection;

    private bool isDroppingThrough;

    [Header("Combat")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint; // 총알 생성 위치
    public float attackCooldown = 0.5f; // 총알 공격 속도 제한
    private float nextAttackTime = 0f; // 다음 공격 쿨타임

    [Header("Camera")]
    public Camera mainCam;

    [Header("Cursor")]
    public Texture2D defaultCursor;      // 기본 커서 (비워두면 OS 기본 커서)
    public Texture2D crosshairCursor;    // 총 모드용 조준경 커서
    public Vector2 crosshairHotspot = new Vector2(16, 16); // 크로스헤어 정중앙이 실제 클릭 지점이 되도록

    [Header("Jump Physics")]
    public float fallMultiplier = 2.5f;
    public float gravityTransitionSpeed = 8f; 

    private float currentGravityMultiplier = 1f;   

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
        UpdateCursor();
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
                            bulletSpawnPoint != null
                                ? bulletSpawnPoint.position
                                : transform.position,
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
            UpdateCursor();
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

        // 발판 아래로 내려가기
        // S를 누르면 Platform 태그가 붙은 발판을 통과
        if (Input.GetKeyDown(KeyCode.S) &&
            isGrounded &&
            !isDroppingThrough)
        {
            TryDropThroughPlatform();
        }
        // 점프
        else if (Input.GetKeyDown(KeyCode.Space) &&
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
        
        float targetMultiplier = rb.linearVelocity.y < 0 ? fallMultiplier : 1f;
        currentGravityMultiplier = Mathf.MoveTowards(currentGravityMultiplier, targetMultiplier, gravityTransitionSpeed * Time.fixedDeltaTime);

        if (currentGravityMultiplier > 1f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (currentGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }

        // 캐릭터 방향 전환
        if (h > 0)
            transform.localScale =
                new Vector3(1, 1, 1);
        else if (h < 0)
            transform.localScale =
                new Vector3(-1, 1, 1);
    }

    // 발판 아래로 내려가기
    private void TryDropThroughPlatform()
    {
        Collider2D[] playerColliders =
            GetComponentsInChildren<Collider2D>();

        if (playerColliders == null ||
            playerColliders.Length == 0)
        {
            return;
        }

        HashSet<Collider2D> platforms =
            new HashSet<Collider2D>();

        // Ground Check 주변에서 발판 찾기
        if (groundCheck != null)
        {
            Collider2D[] groundObjects =
                Physics2D.OverlapCircleAll(
                    groundCheck.position,
                    groundCheckRadius + 0.25f
                );

            for (int i = 0; i < groundObjects.Length; i++)
            {
                Collider2D collider =
                    groundObjects[i];

                if (collider == null)
                    continue;

                if (IsPlatform(collider))
                {
                    platforms.Add(collider);
                }
            }
        }

        // 플레이어 Collider 주변에서도 발판 찾기
        for (int i = 0; i < playerColliders.Length; i++)
        {
            Collider2D playerCollider =
                playerColliders[i];

            if (playerCollider == null)
                continue;

            Bounds bounds =
                playerCollider.bounds;

            Vector2 checkPosition =
                new Vector2(
                    bounds.center.x,
                    bounds.min.y
                );

            Vector2 checkSize =
                new Vector2(
                    Mathf.Max(
                        0.1f,
                        bounds.size.x * 0.9f
                    ),
                    0.25f
                );

            Collider2D[] nearbyObjects =
                Physics2D.OverlapBoxAll(
                    checkPosition,
                    checkSize,
                    0f
                );

            for (int j = 0; j < nearbyObjects.Length; j++)
            {
                Collider2D collider =
                    nearbyObjects[j];

                if (collider == null)
                    continue;

                if (IsPlatform(collider))
                {
                    platforms.Add(collider);
                }
            }
        }

        if (platforms.Count == 0)
            return;

        List<Collider2D> ignoredPlatforms =
            new List<Collider2D>();

        foreach (Collider2D platform in platforms)
        {
            if (platform == null)
                continue;

            bool isPlayerCollider = false;

            for (int i = 0; i < playerColliders.Length; i++)
            {
                if (platform == playerColliders[i])
                {
                    isPlayerCollider = true;
                    break;
                }
            }

            if (isPlayerCollider)
                continue;

            for (int i = 0; i < playerColliders.Length; i++)
            {
                Collider2D playerCollider =
                    playerColliders[i];

                if (playerCollider == null)
                    continue;

                Physics2D.IgnoreCollision(
                    playerCollider,
                    platform,
                    true
                );
            }

            ignoredPlatforms.Add(platform);
        }

        if (ignoredPlatforms.Count == 0)
            return;

        isDroppingThrough = true;

        // 발판에 걸리지 않도록 살짝 아래로 이동
        rb.position +=
            Vector2.down *
            dropThroughDistance;

        // 아래로 떨어지는 속도를 줌
        rb.linearVelocity =
            new Vector2(
                rb.linearVelocity.x,
                -2f
            );

        StartCoroutine(
            RestorePlatformCollisions(
                playerColliders,
                ignoredPlatforms
            )
        );
    }

    private bool IsPlatform(
        Collider2D collider)
    {
        if (collider == null)
            return false;

        // Collider가 붙은 오브젝트 자체 확인
        if (collider.CompareTag(
            dropThroughPlatformTag))
        {
            return true;
        }

        // Collider가 자식에 있고
        // 부모 오브젝트에 Platform Tag가 붙어있는 경우 확인
        Transform parent =
            collider.transform.parent;

        while (parent != null)
        {
            if (parent.CompareTag(
                dropThroughPlatformTag))
            {
                return true;
            }

            parent = parent.parent;
        }

        return false;
    }

    private System.Collections.IEnumerator RestorePlatformCollisions(
        Collider2D[] playerColliders,
        List<Collider2D> platforms)
    {
        yield return new WaitForSeconds(
            dropThroughDuration
        );

        if (playerColliders != null)
        {
            for (int i = 0; i < platforms.Count; i++)
            {
                Collider2D platform =
                    platforms[i];

                if (platform == null)
                    continue;

                for (int j = 0; j < playerColliders.Length; j++)
                {
                    Collider2D playerCollider =
                        playerColliders[j];

                    if (playerCollider == null)
                        continue;

                    Physics2D.IgnoreCollision(
                        playerCollider,
                        platform,
                        false
                    );
                }
            }
        }

        isDroppingThrough = false;
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

    private void UpdateCursor()
    {
        if (IsGunMode && crosshairCursor != null)
            Cursor.SetCursor(
                crosshairCursor,
                crosshairHotspot,
                CursorMode.Auto
            );
        else
            Cursor.SetCursor(
                defaultCursor,
                Vector2.zero,
                CursorMode.Auto
            );
    }
}
