using UnityEngine;
using System;

// 플레이어/몬스터 공용 체력 관리 컴포넌트.
// TakeDamage/Heal로 체력을 조작하고, 체력이 0 이하가 되면 OnDeath 이벤트를 호출.
// OnDeath를 가지는 별도 스크립트(EnemyDeathHandler, PlayerDeathHandler)가 대상에 따라 다른 후속 처리(재화 지급 / 게임오버)를 담당함.
public class HealthManager : MonoBehaviour
{
    public CharacterData data; // 설계값 참조용. 인스펙터에서 CharacterData 에셋 연결 필요
    public int currentHealth; // 실시간 현재 체력
    public event Action OnDeath; // 사망 시 호출되는 이벤트.
    public event Action<int, int> OnHealthChanged; // UI 연동용 이벤트 추가
    private bool isDead; //중복 사망 방지용

    private int cachedMaxHealth; // 현재 최대 체력 저장용
    private bool isInvincible; // 현재 피격 무적 상태인지 확인
    private float invincibilityEndTime; // 무적 종료 시각
    private Rigidbody2D rb; // 넉백 처리

    // 기본 피격 무적시간
    // 세트 능력치의 InvincibilityDuration은 이 값에 추가된다.
    [SerializeField]
    private float baseInvincibilityDuration = 0.15f;

    void Awake()
    {
        isDead = false;
        isInvincible = false;
        invincibilityEndTime = 0f;

        rb = GetComponent<Rigidbody2D>();

        int maxHealth = GetMaxHealth();

        currentHealth = maxHealth;
        cachedMaxHealth = maxHealth;

        NotifyHealthChanged(); // 초기 UI 동기화
    }

    // 플레이어인지 확인
    private bool IsPlayer()
    {
        return CompareTag("Player");
    }

    // 현재 최대 체력 반환
    public int GetMaxHealth()
    {
        if (IsPlayer())
        {
            if (PlayerStat.Instance != null)
            {
                float maxHP =
                    PlayerStat.Instance.GetStat(
                        StatType.MaxHP
                    );

                return Mathf.Max(
                    1,
                    Mathf.RoundToInt(maxHP)
                );
            }

            if (data != null)
            {
                return Mathf.Max(
                    1,
                    data.maxHealth
                );
            }

            return 1;
        }

        if (data != null)
        {
            return Mathf.Max(
                1,
                data.maxHealth
            );
        }

        return 1;
    }

    // 체력 변경 UI 알림
    public void NotifyHealthChanged()
    {
        int maxHealth = GetMaxHealth();

        currentHealth = Mathf.Clamp(
            currentHealth,
            0,
            maxHealth
        );

        cachedMaxHealth = maxHealth;

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );
    }

    // 플레이어 최종 피해량 계산
    private int CalculateDamage(int amount)
    {
        if (amount <= 0)
            return 0;

        // 몬스터는 기존처럼 전달받은 피해량 그대로 적용
        if (!IsPlayer())
            return amount;

        if (PlayerStat.Instance == null)
            return amount;

        float damageReduction =
            PlayerStat.Instance.GetStat(
                StatType.DamageReduction
            );

        damageReduction = Mathf.Clamp01(
            damageReduction
        );

        float finalDamage =
            amount * (1f - damageReduction);

        int result =
            Mathf.RoundToInt(finalDamage);

        // 피해 감소가 100%가 아닌 이상 1 이상의 피해를 보장
        if (amount > 0 &&
            damageReduction < 1f &&
            result <= 0)
        {
            result = 1;
        }

        return Mathf.Max(
            0,
            result
        );
    }

    // 현재 무적 상태인지 확인
    private bool IsCurrentlyInvincible()
    {
        if (!IsPlayer())
            return false;

        if (!isInvincible)
            return false;

        // 무적시간 종료
        if (Time.time >= invincibilityEndTime)
        {
            isInvincible = false;
            invincibilityEndTime = 0f;
            return false;
        }

        return true;
    }

    // 실제 피격 후 무적 시작
    private void StartInvincibility()
    {
        if (!IsPlayer())
            return;

        float duration = baseInvincibilityDuration;

        // 세트 / 아이템에서 제공하는 추가 무적시간
        if (PlayerStat.Instance != null)
        {
            float bonusDuration =
                PlayerStat.Instance.GetStat(
                    StatType.InvincibilityDuration
                );

            duration += Mathf.Max(
                0f,
                bonusDuration
            );
        }

        if (duration <= 0f)
        {
            isInvincible = false;
            invincibilityEndTime = 0f;
            return;
        }

        isInvincible = true;

        float newEndTime =
            Time.time + duration;

        // 기존 무적시간보다 긴 경우에만 갱신
        if (newEndTime > invincibilityEndTime)
        {
            invincibilityEndTime =
                newEndTime;
        }
    }

    // 액티브 스킬 무적
    public void StartActiveInvincibility(float duration)
    {
        if (isDead)
            return;

        if (duration <= 0f)
            return;

        isInvincible = true;

        float newEndTime =
            Time.time + duration;

        if (newEndTime > invincibilityEndTime)
        {
            invincibilityEndTime =
                newEndTime;
        }
    }

    // 외부 시스템 무적
    public void StartExternalInvincibility(float duration)
    {
        if (isDead)
            return;

        if (duration <= 0f)
            return;

        isInvincible = true;

        float newEndTime =
            Time.time + duration;

        if (newEndTime > invincibilityEndTime)
        {
            invincibilityEndTime =
                newEndTime;
        }
    }

    // 외부에서 호출하여 데미지를 적용하는 함수.
    // 이미 사망한 상태거나 무적 상태면 무시. 체력이 0 이하가 되면 OnDeath 이벤트 발행
    public void TakeDamage(int amount)
    {
        TakeDamage(
            amount,
            Vector2.zero
        );
    }

    // 피해 + 넉백
    public void TakeDamage(
        int amount,
        Vector2 knockbackDirection)
    {
        if (isDead)
            return;

        if (amount <= 0)
            return;

        // 이미 무적이면 피해를 받지 않는다.
        if (IsCurrentlyInvincible())
            return;

        // 최종 피해 계산
        int finalDamage =
            CalculateDamage(amount);

        // 피해가 0이면 아무것도 하지 않는다.
        if (finalDamage <= 0)
            return;

        int maxHealth =
            GetMaxHealth();

        currentHealth = Mathf.Clamp(
            currentHealth - finalDamage,
            0,
            maxHealth
        );

        // 데이터 변경 시 알림
        NotifyHealthChanged();

        // 넉백
        if (finalDamage > 0 &&
            rb != null &&
            knockbackDirection != Vector2.zero)
        {
            PlayerController player =
                GetComponent<PlayerController>();

            if (player == null ||
                !player.IsDashing)
            {
                rb.linearVelocity =
                    Vector2.zero;

                rb.AddForce(
                    knockbackDirection,
                    ForceMode2D.Impulse
                );
            }
        }

        // 플레이어가 실제로 피해를 받은 경우 피격 무적 시작
        if (IsPlayer())
        {
            StartInvincibility();
        }

        if (currentHealth <= 0)
        {
            isDead = true;
            isInvincible = false;
            invincibilityEndTime = 0f;

            OnDeath?.Invoke();
        }
    }

    // 외부에서 호출하여 체력을 회복하는 함수.
    // 이미 사망한 상태면 무시. 회복한 후의 체력이 maxHealth를 초과하지 않도록 설계함.
    public void Heal(int amount)
    {
        if (isDead)
            return;

        if (amount <= 0)
            return;

        int maxHealth =
            GetMaxHealth();

        currentHealth =
            Mathf.Min(
                currentHealth + amount,
                maxHealth
            );

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        ); // 데이터 변경 시 알림
    }

    // 최대 체력 갱신
    public void RefreshMaxHealth()
    {
        int oldMaxHealth =
            cachedMaxHealth;

        int newMaxHealth =
            GetMaxHealth();

        if (newMaxHealth <= 0)
            return;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0,
                newMaxHealth
            );

        cachedMaxHealth =
            newMaxHealth;

        NotifyHealthChanged();
    }

    // 현재 체력
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // 현재 체력 비율
    public float GetHealthRatio()
    {
        int maxHealth =
            GetMaxHealth();

        if (maxHealth <= 0)
            return 0f;

        return (float)currentHealth /
               maxHealth;
    }

    // 사망 여부
    public bool IsDead()
    {
        return isDead;
    }

    // 현재 무적 여부
    public bool IsInvincible()
    {
        return IsCurrentlyInvincible();
    }

    // 남은 무적시간
    public float GetRemainingInvincibilityTime()
    {
        if (!IsCurrentlyInvincible())
            return 0f;

        return Mathf.Max(
            0f,
            invincibilityEndTime - Time.time
        );
    }

    // 기본 무적시간 확인
    public float GetBaseInvincibilityDuration()
    {
        return Mathf.Max(
            0f,
            baseInvincibilityDuration
        );
    }

    // 현재 적용되는 피격 무적시간 확인
    public float GetTotalHitInvincibilityDuration()
    {
        float bonus = 0f;

        if (PlayerStat.Instance != null)
        {
            bonus =
                Mathf.Max(
                    0f,
                    PlayerStat.Instance.GetStat(
                        StatType.InvincibilityDuration
                    )
                );
        }

        return Mathf.Max(
            0f,
            baseInvincibilityDuration
        ) + bonus;
    }

    // 체력 초기화
    public void ResetHealth()
    {
        isDead = false;
        isInvincible = false;
        invincibilityEndTime = 0f;

        int maxHealth =
            GetMaxHealth();

        currentHealth =
            maxHealth;

        cachedMaxHealth =
            maxHealth;

        NotifyHealthChanged();
    }
}