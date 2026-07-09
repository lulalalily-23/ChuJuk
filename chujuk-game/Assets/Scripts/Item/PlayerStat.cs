using UnityEngine;
using System.Collections.Generic;


public class PlayerStat : MonoBehaviour
{
    public static PlayerStat Instance;


    [Header("기본 능력치")]
    public float baseAttack = 10f;
    public float baseDefense = 5f;
    public float baseMaxHP = 100f;
    public float baseAttackSpeed = 1f;
    public float baseMoveSpeed = 5f;


    // 아이템 및 버프로 추가되는 능력치
    private Dictionary<StatType, float> bonusStats = new Dictionary<StatType, float>();



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



    // 초기화
    private void InitializeStats()
    {
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            bonusStats[type] = 0f;
        }
    }



    // 최종 능력치 반환
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
        }


        return baseValue + bonusStats[type];
    }



    // 아이템 능력치 추가
    public void AddStat(StatModifier modifier)
    {
        if (!bonusStats.ContainsKey(modifier.statType))
        {
            bonusStats.Add(modifier.statType, 0);
        }


        bonusStats[modifier.statType] += modifier.value;


        Debug.Log(
            $"{modifier.statType} 증가 : {modifier.value}"
        );

        Debug.Log(
            $"현재 공격력 : {GetStat(StatType.Attack)}"
        );
    }



    // 아이템 제거
    public void RemoveStat(StatModifier modifier)
    {
        if (!bonusStats.ContainsKey(modifier.statType))
            return;


        bonusStats[modifier.statType] -= modifier.value;
    }



    // 현재 모든 능력치 초기화
    public void ResetStats()
    {
        foreach (StatType type in bonusStats.Keys)
        {
            bonusStats[type] = 0;
        }
    }
}