using UnityEngine;
using System.Collections.Generic;


public class PlayerStat : MonoBehaviour, ICombatStats
{
    public static PlayerStat Instance;


    [Header("기본 능력치")]
    public float baseAttack = 10f;
    public float baseDefense = 5f;
    public float baseMaxHP = 100f;
    public float baseAttackSpeed = 1f;
    public float baseMoveSpeed = 5f;
    public float baseCriticalChance = 0.1f;
    public float baseCriticalDamage = 1.5f;


    private Dictionary<StatType, float> flatBonusStats = new Dictionary<StatType, float>();
    private Dictionary<StatType, float> percentBonusStats = new Dictionary<StatType, float>();



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;


        InitializeStats();
    }



    private void InitializeStats()
    {
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            flatBonusStats[type] = 0f;
            percentBonusStats[type] = 0f;
        }
    }



    public float GetStat(StatType type)
    {
        float baseValue = 0;


        switch (type)
        {
            case StatType.Attack:
                baseValue = baseAttack;
                break;

            case StatType.Defense:
                baseValue = baseDefense;
                break;

            case StatType.MaxHP:
                baseValue = baseMaxHP;
                break;

            case StatType.AttackSpeed:
                baseValue = baseAttackSpeed;
                break;

            case StatType.MoveSpeed:
                baseValue = baseMoveSpeed;
                break;

            case StatType.CriticalChance:
                baseValue = baseCriticalChance;
                break;

            case StatType.CriticalDamage:
                baseValue = baseCriticalDamage;
                break;
        }


        float flat = flatBonusStats.ContainsKey(type) ? flatBonusStats[type] : 0f;
        float percent = percentBonusStats.ContainsKey(type) ? percentBonusStats[type] : 0f;

        if (type == StatType.DamageIncrease || type == StatType.GoldGain || type == StatType.ActiveCoolDown)
        {
            return flat + percent;   
        }

        return (baseValue + flat) * (1f + percent);
    }



    public void AddStat(StatModifier modifier)
    {
        if (modifier.modifierType == ModifierType.Flat)
        {
            if (!flatBonusStats.ContainsKey(modifier.statType))
                flatBonusStats.Add(modifier.statType, 0f);

            flatBonusStats[modifier.statType] += modifier.value;
        }
        else
        {
            if (!percentBonusStats.ContainsKey(modifier.statType))
                percentBonusStats.Add(modifier.statType, 0f);

            percentBonusStats[modifier.statType] += modifier.value;
        }


        Debug.Log($"[아이템 적용] {modifier.statType} {modifier.modifierType} +{modifier.value} → 현재 {modifier.statType}: {GetStat(modifier.statType)}");
    }



    public void RemoveStat(StatModifier modifier)
    {
        if (modifier.modifierType == ModifierType.Flat)
        {
            if (!flatBonusStats.ContainsKey(modifier.statType))
                return;

            flatBonusStats[modifier.statType] -= modifier.value;
        }
        else
        {
            if (!percentBonusStats.ContainsKey(modifier.statType))
                return;

            percentBonusStats[modifier.statType] -= modifier.value;
        }
    }



    public void ResetStats()
    {
        foreach (StatType type in new List<StatType>(flatBonusStats.Keys))
        {
            flatBonusStats[type] = 0;
        }
        foreach (StatType type in new List<StatType>(percentBonusStats.Keys))
        {
            percentBonusStats[type] = 0;
        }
    }

    public float AttackPower => GetStat(StatType.Attack);
    public float Defense => GetStat(StatType.Defense);
    public float AttackMultiplier => 1f;   
    public float DefenseMultiplier => 1f;
    public float CriticalChance => GetStat(StatType.CriticalChance);
    public float CriticalDamage => GetStat(StatType.CriticalDamage);
    public float DamageIncrease => GetStat(StatType.DamageIncrease); 
}