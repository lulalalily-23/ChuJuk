public enum StatType
{
    None = 0,

    Attack,          // 공격력
    Defense,         // 방어력
    MaxHP,           // 최대 체력

    AttackSpeed,     // 공격 속도
    MoveSpeed,       // 이동 속도

    GoldGain,        // 골드 획득량

    DamageIncrease   // 전체 피해 증가

    // 치명타 추가는 논의 사항 / 전체 데미지에 곱연산?
    // CriticalChance,  // 치명타 확률
    // CriticalDamage,  // 치명타 피해
}