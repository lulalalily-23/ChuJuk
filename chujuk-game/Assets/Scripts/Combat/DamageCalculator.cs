using UnityEngine;

public static class DamageCalculator
{
    //데미지 계산용 클래스 및 메서드. DamageCalculator.CalculateDamage()로 실행
    public static int CalculateDamage(ICombatStats attacker, ICombatStats defender, float skillMultiplier = 1f)
    //세번째 인자는 기본값 1로 설정(일반적으로는 앞 두인자만 채워도 작동). 나중에 스킬등으로 공격계수를 추가해야할 일을 위해 임시적으로 만듦.
    {
        float rawDamage = attacker.AttackPower * attacker.AttackMultiplier * skillMultiplier - defender.Defense * defender.DefenseMultiplier;
        //데미지 공식: {공격자의 현재 공격력 * 공격자의 공격 계수 * 스킬의 공격 계수(추후를 위해 임시구현, 기본값은 1로 설정)} - (방어자의 현재방어력 * 방어자의 방어 계수)
        if (rawDamage <= 0) return 1;
        //만약 공격력과 방어력이 너무 차이가 나서 총 데미지가 음수값이 되더라도, 최소한의 데미지 1은 보장되도록 설계함.
        return Mathf.RoundToInt(rawDamage); //총 데미지는 반올림해서 int값으로 반환됨
    }

}