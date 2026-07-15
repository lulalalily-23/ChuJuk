using UnityEngine;

public interface ICombatStats
{
    float AttackPower { get; }
    float Defense { get; }
    float AttackMultiplier { get; }
    float DefenseMultiplier { get; }
}