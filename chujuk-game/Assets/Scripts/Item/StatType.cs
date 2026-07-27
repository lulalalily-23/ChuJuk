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

    ActiveCoolDown, // 액티브 쿨타임 감소

    CriticalChance,  // 치명타 확률
    CriticalDamage,  // 치명타 피해
}
public enum ModifierType
{
    Flat,       // 고정값 더하기 예) +10
    Percent     // 퍼센트 예) +5%
}