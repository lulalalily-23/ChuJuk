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


    // 일반 아이템 능력치
    private Dictionary<StatType, float> flatBonusStats = new Dictionary<StatType, float>();
    private Dictionary<StatType, float> percentBonusStats = new Dictionary<StatType, float>();

    // 세트 능력치
    private Dictionary<StatType, float> setFlatBonusStats = new Dictionary<StatType, float>();
    private Dictionary<StatType, float> setPercentBonusStats = new Dictionary<StatType, float>();

    // 런타임 능력치
    private Dictionary<StatType, float> runtimeFlatBonusStats = new Dictionary<StatType, float>();
    private Dictionary<StatType, float> runtimePercentBonusStats = new Dictionary<StatType, float>();

    // 추격 세트
    private int chaseStack;
    private int maxChaseStack;

    private float chaseMoveSpeedBonus;
    private float chaseMoveSpeedBonusFromDamage;

    // 매복 세트 액티브
    private bool ambushActiveRangedBuff;


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


    // 테스트용 로그
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"공격력 : {GetStat(StatType.Attack)}");
            Debug.Log($"방어력 : {GetStat(StatType.Defense)}");
            Debug.Log($"최대 체력 : {GetStat(StatType.MaxHP)}");
            Debug.Log($"공격 속도 : {GetStat(StatType.AttackSpeed)}");
            Debug.Log($"이동 속도 : {GetStat(StatType.MoveSpeed)}");
            Debug.Log($"치명타 확률 : {GetStat(StatType.CriticalChance)}");
            Debug.Log($"치명타 피해 : {GetStat(StatType.CriticalDamage)}");
            Debug.Log($"피격 데미지 감소 : {GetStat(StatType.DamageReduction)}");
            Debug.Log($"피격 무적 시간 증가 : {GetStat(StatType.InvincibilityDuration)}");
            Debug.Log($"원거리 데미지 : {GetStat(StatType.RangedDamage)}");
            Debug.Log($"처치시 회복 : {GetStat(StatType.HealOnKill)}");
            Debug.Log($"추격 이동속도 보너스 : {GetChaseMoveSpeedBonus()}");
            Debug.Log($"추격 스택 : {chaseStack} / {maxChaseStack}");
            Debug.Log($"추격 추가 피해 : {GetChaseDamageBonus()}");
            Debug.Log($"매복 액티브 상태 : {ambushActiveRangedBuff}");
        }
    }


    private void InitializeStats()
    {
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            flatBonusStats[type] = 0f;
            percentBonusStats[type] = 0f;

            setFlatBonusStats[type] = 0f;
            setPercentBonusStats[type] = 0f;

            runtimeFlatBonusStats[type] = 0f;
            runtimePercentBonusStats[type] = 0f;
        }

        chaseStack = 0;
        maxChaseStack = 0;

        chaseMoveSpeedBonus = 0f;
        chaseMoveSpeedBonusFromDamage = 0f;

        ambushActiveRangedBuff = false;
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


        float itemFlat = flatBonusStats.ContainsKey(type) ? flatBonusStats[type] : 0f;
        float itemPercent = percentBonusStats.ContainsKey(type) ? percentBonusStats[type] : 0f;

        float setFlat = setFlatBonusStats.ContainsKey(type) ? setFlatBonusStats[type] : 0f;
        float setPercent = setPercentBonusStats.ContainsKey(type) ? setPercentBonusStats[type] : 0f;

        float runtimeFlat = runtimeFlatBonusStats.ContainsKey(type) ? runtimeFlatBonusStats[type] : 0f;
        float runtimePercent = runtimePercentBonusStats.ContainsKey(type) ? runtimePercentBonusStats[type] : 0f;


        float flat = itemFlat + setFlat + runtimeFlat;
        float percent = itemPercent + setPercent + runtimePercent;


        if (type == StatType.DamageReduction ||
            type == StatType.InvincibilityDuration ||
            type == StatType.DamageIncrease ||
            type == StatType.GoldGain ||
            type == StatType.ActiveCoolDown ||
            type == StatType.DashCoolDown ||
            type == StatType.RangedDamage ||
            type == StatType.HealOnKill)
        {
            return flat + percent;
        }


        return (baseValue + flat) * (1f + percent);
    }


    // 일반 아이템 능력치 추가
    public void AddStat(StatModifier modifier)
    {
        if (modifier == null)
            return;


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


    // 일반 아이템 능력치 제거
    public void RemoveStat(StatModifier modifier)
    {
        if (modifier == null)
            return;


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


    // 세트 능력치 추가
    public void AddSetStat(StatModifier modifier)
    {
        if (modifier == null)
            return;


        if (modifier.modifierType == ModifierType.Flat)
        {
            if (!setFlatBonusStats.ContainsKey(modifier.statType))
                setFlatBonusStats.Add(modifier.statType, 0f);

            setFlatBonusStats[modifier.statType] += modifier.value;
        }
        else
        {
            if (!setPercentBonusStats.ContainsKey(modifier.statType))
                setPercentBonusStats.Add(modifier.statType, 0f);

            setPercentBonusStats[modifier.statType] += modifier.value;
        }


        Debug.Log($"[세트 {modifier.modifierType} 적용] {modifier.statType} +{modifier.value} → 현재 {modifier.statType}: {GetStat(modifier.statType)}");
    }


    // 세트 능력치 제거
    public void RemoveSetStat(StatModifier modifier)
    {
        if (modifier == null)
            return;


        if (modifier.modifierType == ModifierType.Flat)
        {
            if (!setFlatBonusStats.ContainsKey(modifier.statType))
                return;

            setFlatBonusStats[modifier.statType] -= modifier.value;
        }
        else
        {
            if (!setPercentBonusStats.ContainsKey(modifier.statType))
                return;

            setPercentBonusStats[modifier.statType] -= modifier.value;
        }
    }


    // 세트 능력치 전체 초기화
    public void ResetSetStats()
    {
        foreach (StatType type in new List<StatType>(setFlatBonusStats.Keys))
        {
            setFlatBonusStats[type] = 0f;
        }

        foreach (StatType type in new List<StatType>(setPercentBonusStats.Keys))
        {
            setPercentBonusStats[type] = 0f;
        }

        ResetChaseEffects();

        Debug.Log("[세트 초기화] 모든 세트 능력치를 0으로 초기화했습니다.");
    }


    // 세트 능력치 확인
    public float GetSetStat(StatType type)
    {
        float flat = setFlatBonusStats.ContainsKey(type) ? setFlatBonusStats[type] : 0f;
        float percent = setPercentBonusStats.ContainsKey(type) ? setPercentBonusStats[type] : 0f;

        return flat + percent;
    }


    // 런타임 능력치 추가
    public void AddRuntimeStat(StatType statType, ModifierType modifierType, float value)
    {
        if (modifierType == ModifierType.Flat)
        {
            if (!runtimeFlatBonusStats.ContainsKey(statType))
                runtimeFlatBonusStats.Add(statType, 0f);

            runtimeFlatBonusStats[statType] += value;
        }
        else
        {
            if (!runtimePercentBonusStats.ContainsKey(statType))
                runtimePercentBonusStats.Add(statType, 0f);

            runtimePercentBonusStats[statType] += value;
        }
    }


    // 런타임 능력치 확인
    public float GetRuntimeStat(StatType statType)
    {
        float flat = runtimeFlatBonusStats.ContainsKey(statType) ? runtimeFlatBonusStats[statType] : 0f;
        float percent = runtimePercentBonusStats.ContainsKey(statType) ? runtimePercentBonusStats[statType] : 0f;

        return flat + percent;
    }


    // 런타임 능력치 하나 초기화
    public void ResetRuntimeStat(StatType statType)
    {
        if (runtimeFlatBonusStats.ContainsKey(statType))
            runtimeFlatBonusStats[statType] = 0f;

        if (runtimePercentBonusStats.ContainsKey(statType))
            runtimePercentBonusStats[statType] = 0f;
    }


    // 런타임 능력치 전체 초기화
    public void ResetAllRuntimeStats()
    {
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            if (runtimeFlatBonusStats.ContainsKey(type))
                runtimeFlatBonusStats[type] = 0f;

            if (runtimePercentBonusStats.ContainsKey(type))
                runtimePercentBonusStats[type] = 0f;
        }

        ambushActiveRangedBuff = false;
    }


    // 매복 6세트 액티브
    public bool ActivateAmbushActive()
    {
        if (Instance == null)
            return false;


        if (ambushActiveRangedBuff)
        {
            Debug.Log("[매복 액티브] 이미 활성화되어 있습니다.");
            return false;
        }


        if (SetSystem.Instance == null)
        {
            Debug.LogWarning("[매복 액티브] SetSystem.Instance가 없습니다.");
            return false;
        }


        int ambushSetCount = SetSystem.Instance.GetSetCount(ItemTag.Ambush);


        if (ambushSetCount < 6)
        {
            Debug.Log($"[매복 액티브] 매복 6세트가 필요합니다. 현재: {ambushSetCount}세트");
            return false;
        }


        AddRuntimeStat(
            StatType.RangedDamage,
            ModifierType.Percent,
            3.0f
        );


        ambushActiveRangedBuff = true;


        Debug.Log("[매복 액티브] 발동!");
        Debug.Log("[매복 액티브] 다음 원거리 공격 1회 원거리 데미지 +300%");


        return true;
    }


    // 매복 액티브 활성화 여부
    public bool IsAmbushActive()
    {
        return ambushActiveRangedBuff;
    }


    // 매복 액티브 소비
    public bool ConsumeAmbushActive()
    {
        if (!ambushActiveRangedBuff)
            return false;


        AddRuntimeStat(
            StatType.RangedDamage,
            ModifierType.Percent,
            -3.0f
        );


        ambushActiveRangedBuff = false;

        Debug.Log("[매복 액티브] 원거리 공격 1회 사용 완료 → +300% 제거");

        return true;
    }


    // 처치 시 회복
    public void HealFromKill()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();

        if (player == null)
            return;


        int healAmount = Mathf.RoundToInt(
            GetStat(StatType.HealOnKill)
        );


        if (healAmount <= 0)
            return;


        player.Heal(healAmount);

        Debug.Log($"[돌진 세트] 적 처치 → 체력 +{healAmount}");
    }


    // 추격 최대 스택
    public void SetChaseMaxStack(int maxStack)
    {
        maxChaseStack = Mathf.Max(0, maxStack);


        if (chaseStack > maxChaseStack)
            chaseStack = maxChaseStack;
    }


    public int GetChaseStack()
    {
        return chaseStack;
    }


    public int GetChaseMaxStack()
    {
        return maxChaseStack;
    }


    public void AddChaseStack()
    {
        if (maxChaseStack <= 0)
            return;


        chaseStack++;

        chaseStack = Mathf.Clamp(
            chaseStack,
            0,
            maxChaseStack
        );


        Debug.Log($"[추격] 스택 증가 : {chaseStack} / {maxChaseStack}");
    }


    public void ResetChaseStack()
    {
        chaseStack = 0;
    }


    public float GetChaseDamageBonus()
    {
        return chaseStack * 0.10f;
    }


    // 추격 이동속도
    public void AddChaseMoveSpeedOnAttack()
    {
        if (maxChaseStack <= 0)
            return;


        chaseMoveSpeedBonus = GetChaseMoveSpeedOnAttackValue();
    }


    public void AddChaseMoveSpeedOnDamage()
    {
        if (maxChaseStack <= 0)
            return;


        chaseMoveSpeedBonusFromDamage = GetChaseMoveSpeedOnDamageValue();
    }


    public float GetChaseMoveSpeedBonus()
    {
        return chaseMoveSpeedBonus + chaseMoveSpeedBonusFromDamage;
    }


    private float GetChaseMoveSpeedOnAttackValue()
    {
        int count = 0;


        if (Inventory.Instance != null)
        {
            List<ItemTag> tags = Inventory.Instance.GetAllTags();


            foreach (ItemTag tag in tags)
            {
                if (tag == ItemTag.Pursuit)
                    count++;
            }
        }


        if (count >= 4)
            return 0.30f;

        if (count >= 2)
            return 0.10f;


        return 0f;
    }


    private float GetChaseMoveSpeedOnDamageValue()
    {
        int count = 0;


        if (Inventory.Instance != null)
        {
            List<ItemTag> tags = Inventory.Instance.GetAllTags();


            foreach (ItemTag tag in tags)
            {
                if (tag == ItemTag.Pursuit)
                    count++;
            }
        }


        if (count >= 2)
            return 0.20f;


        return 0f;
    }


    private void ResetChaseEffects()
    {
        chaseStack = 0;
        maxChaseStack = 0;

        chaseMoveSpeedBonus = 0f;
        chaseMoveSpeedBonusFromDamage = 0f;
    }


    public float AttackPower => GetStat(StatType.Attack);
    public float Defense => GetStat(StatType.Defense);
    public float AttackMultiplier => 1f;
    public float DefenseMultiplier => 1f;
    public float CriticalChance => GetStat(StatType.CriticalChance);
    public float CriticalDamage => GetStat(StatType.CriticalDamage);
    public float DamageIncrease => GetStat(StatType.DamageIncrease);
}
