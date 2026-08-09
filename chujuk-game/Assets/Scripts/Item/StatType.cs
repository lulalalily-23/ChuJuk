public enum StatType
{
    None = 0,

    Attack,          // 공격력
    Defense,         // 방어력
    MaxHP,           // 최대 체력

    AttackSpeed,     // 공격 속도
    MoveSpeed,       // 이동 속도

    GoldGain,        // 골드 획득량

    DamageIncrease,  // 전체 피해 증가

    ActiveCoolDown,  // 액티브 쿨타임 감소

    CriticalChance,  // 치명타 확률
    CriticalDamage,  // 치명타 피해


    // 피해 / 생존

    DamageReduction,        // 피격 데미지 감소
    InvincibilityDuration,  // 피격 무적 시간 증가


    // 공격 관련

    RangedDamage,           // 원거리 데미지
    DashDamage,             // 대쉬 데미지


    // 대쉬

    DashCoolDown,           // 대쉬 쿨타임 감소


    // 처치 / 회복

    HealOnKill,             // 적 처치시 체력 회복
    AttackSpeedOnKill,      // 적 처치시 공격속도 증가


    // 재화

    GoldAttackBonus,        // 보유 골드에 따른 공격력 증가


    // 액티브

    ActiveInvincibilityDuration, // 액티브 무적 지속시간


    // 기타

    Evasion,                // 회피
    TrapAvoidance           // 함정 회피
}